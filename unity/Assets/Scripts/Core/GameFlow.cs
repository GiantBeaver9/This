using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Top-level screen flow: Title (home) -> Character Select -> Playing. Owns the
    /// run lifecycle — building the gameplay world for the chosen character and
    /// tearing it down to return to the menu. The Title/Select screens are a
    /// functional IMGUI placeholder (the bespoke UI art, UI.md, replaces them);
    /// the character framework (CharacterDef) they feed is real.
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        public enum State { Title, Menu, PlayerSetup, CharacterSelect, HowToPlay, Playing }
        public static GameFlow Instance { get; private set; }

        /// <summary>How a player is driven — picked on the PlayerSetup screen.</summary>
        public enum InputKind { Keyboard, Controller1, Controller2 }
        private static readonly string[] InputNames = { "KEYBOARD", "CONTROLLER 1", "CONTROLLER 2" };

        public State Current { get; private set; } = State.Title;

        // Player-setup selections (chosen before the run instead of random Start-to-join).
        private int _playerCount = 1;
        private InputKind _p1Input = InputKind.Keyboard;
        private InputKind _p2Input = InputKind.Controller1;

        private CharacterDef _selected;
        private CharacterDef _selectedP2;  // P2's pick (MVP: reuses P1's character)
        private bool _weaponPromptShown;   // one-time "press E to fire" teach on first pickup
        private bool _gameOver;            // latched when the shared life pool empties out
        private bool _endlessPending;      // character-select is feeding an Endless run, not the campaign
        private int _menuIndex;            // highlighted row on the main menu
        private GameObject _worldRoot;
        private CharacterDef[] _roster;
        private GUIStyle _title, _label, _btn, _special;
        private GUIStyle _titleBig, _titleMid, _titleSmall; // stacked, left-justified gag title
        private GUIStyle _menuItem, _menuItemSel, _hint, _keyCap, _keyDesc; // menu + how-to-play

        // Main-menu rows (index order drives keyboard nav + selection).
        private static readonly string[] MenuItems = { "CAMPAIGN", "ENDLESS MODE", "HOW TO PLAY" };

        private void Awake()
        {
            Instance = this;
            _roster = CharacterDef.Roster();
        }

        private void Start() => GoTitle();

        // ---- Transitions -----------------------------------------------------
        public void GoTitle()
        {
            CancelInvoke(nameof(GoTitle)); // cancel a pending game-over return
            _gameOver = false;
            _endlessPending = false;
            TeardownWorld();
            Current = State.Title;
            Music.PlayTitle();
        }

        /// <summary>Title -> main menu (Campaign / Endless / How to Play). Any key on the title lands here.</summary>
        public void GoMenu()
        {
            _menuIndex = 0;
            Current = State.Menu;
        }

        /// <summary>Menu -> character select. <paramref name="endless"/> routes SELECT to <see cref="StartEndlessRun"/>.</summary>
        public void GoCharacterSelect(bool endless)
        {
            _endlessPending = endless;
            Current = State.CharacterSelect;
        }

        public void GoHowToPlay() => Current = State.HowToPlay;

        public void StartRun(CharacterDef def)
        {
            _selected = def;
            _selectedP2 = null;        // MVP: P2 duplicates P1's character on join
            _gameOver = false;
            Lives.Reset(Tuning.StartingLives); // fresh shared life pool for the run
            BuildWorld();
            Current = State.Playing;
        }

        /// <summary>
        /// Endless Mode (STAGES.md §7b): same world-build as <see cref="StartRun"/> — chosen character,
        /// Backdrop, Hud, CoopJoin — but the field is driven by a <see cref="StageDirector"/> running in
        /// endless mode (full-roster refill + scaling waves) instead of the linear <see cref="CampaignRunner"/>.
        /// Skips the intro vignette + tutorial chain: Endless drops you straight into the fight.
        /// </summary>
        public void StartEndlessRun(CharacterDef def)
        {
            _selected = def;
            _selectedP2 = null;
            _gameOver = false;
            Lives.Reset(Tuning.StartingLives);
            BuildEndlessWorld();
            Current = State.Playing;
        }

        /// <summary>
        /// The shared life pool emptied and the last player went down. Brief beat on the
        /// death, then back to the title (the existing game-over path). Guarded so the many
        /// enemies that may all land killing blows the same frame only trigger it once.
        /// </summary>
        public void TriggerGameOver()
        {
            if (_gameOver || Current != State.Playing) return;
            _gameOver = true;
            Sfx.Play("death");
            Invoke(nameof(GoTitle), 2.0f);
        }

        // ---- World lifecycle -------------------------------------------------

        /// <summary>
        /// Build the shared gameplay world for the chosen character — player (keyboard P1),
        /// Backdrop, Hud, CoopJoin — and return the "Systems" object so the caller can attach
        /// the mode-specific director (CampaignRunner for a campaign, StageDirector for Endless).
        /// The EnemySpawner/director is deliberately NOT added here so nothing spawns before the
        /// caller decides how the field runs.
        /// </summary>
        private GameObject BuildWorldCore()
        {
            TeardownWorld();
            _worldRoot = new GameObject("World");

            var pGo = new GameObject("Player");
            pGo.transform.SetParent(_worldRoot.transform);
            pGo.AddComponent<SpriteRenderer>();
            pGo.AddComponent<SpriteAnimator>();
            var player = pGo.AddComponent<PlayerController>();
            player.Configure(_selected);
            player.WorldX = 0f;
            player.Z = 2.5f;
            player.Init();
            player.SetInput(MakeInput(_p1Input));   // P1's control surface, chosen on the setup screen

            // First weapon pickup pops a one-time "press E to fire" teach (TutorialController
            // exposes the self-dismissing overlay; it outlives the tutorial component).
            _weaponPromptShown = false;
            player.WeaponEquipped += kind =>
            {
                if (_weaponPromptShown || kind == WeaponKind.Fists) return;
                _weaponPromptShown = true;
                TutorialController.ShowWeaponPrompt("press E to fire");
            };

            var sys = new GameObject("Systems");
            sys.transform.SetParent(_worldRoot.transform);
            sys.AddComponent<Backdrop>();
            sys.AddComponent<Hud>();
            sys.AddComponent<CoopJoin>();   // press START on a gamepad to also drop in P2 mid-run

            // 2-player was chosen on the setup screen → P2 is already in from the start.
            if (_playerCount == 2) SpawnPlayer2(MakeInput(_p2Input));
            return sys;
        }

        private void BuildWorld()
        {
            var sys = BuildWorldCore();
            // NOTE: the EnemySpawner is deliberately NOT added here — it's added at the
            // end of the opening chain below, so no wave spawns during the intro
            // cinematic or the tutorial.
            RunOpeningSequence(sys);
        }

        /// <summary>
        /// Endless world: the shared core plus a <see cref="StageDirector"/> put straight into endless
        /// mode (<see cref="StageDirector.StartEndless"/>). No CampaignRunner, no opening vignette/tutorial
        /// chain — the director's LateUpdate drives the full-roster refill + scaling waves immediately.
        /// </summary>
        private void BuildEndlessWorld()
        {
            var sys = BuildWorldCore();
            var director = sys.AddComponent<StageDirector>(); // AutoStartStage stays -1, so it idles until told
            director.StartEndless();
        }

        /// <summary>
        /// The opening chain, in order: intro vignette -> short tutorial -> stage-1
        /// opener vignette -> start the level (add the <see cref="EnemySpawner"/>, which
        /// drives the stage-driven horde and its own per-stage music). Each step gates
        /// the next through its completion callback, so the waves only begin once the
        /// player has actually been through the intro + the directional-attack tutorial.
        /// </summary>
        private void RunOpeningSequence(GameObject sys)
        {
            VignettePlayer.Instance.Play(VignetteScripts.Intro, () =>
            {
                if (sys == null) return;
                var tutorial = sys.AddComponent<TutorialController>();
                tutorial.Run(() =>
                {
                    VignettePlayer.Instance.Play(VignetteScripts.A1LincolnOpener, () =>
                    {
                        // Launch the full 13-stage campaign runner (STAGES.md §2/§4.1). It owns a
                        // StageDirector and chains every stage — ENCOUNTERS.md spine + seeded filler
                        // + gates + act-end bosses (via Bosses.Spawn) — from Lincoln to Phil. Each
                        // stage's per-stage vignette/teaching beat is authored as its opening wave in
                        // StageDatabase, so those play in sequence without a separate hook here.
                        // HazardDirector self-gates the car to Level 1 only (creator ruling);
                        // per-area hazards come later.
                        if (sys != null) { sys.AddComponent<CampaignRunner>(); sys.AddComponent<HazardDirector>(); }
                    });
                });
            });
        }

        /// <summary>
        /// Drop a second player into the running game on gamepad <paramref name="padIndex"/>
        /// (called by <see cref="CoopJoin"/> when START is pressed). MVP: P2 duplicates P1's
        /// character, spawns beside them with a cool tint, and is driven by the pad. P1 keeps
        /// the keyboard, so single-player is never disturbed. No-op if a run isn't active or
        /// P2 already exists.
        /// </summary>
        public void TryJoinPlayer2(int padIndex)
        {
            if (Current != State.Playing || _worldRoot == null) return;
            if (PlayerController.All.Count >= 2) return;
            SpawnPlayer2(new GamepadInput(padIndex));
            Sfx.Play("confirm");
            Debug.Log($"[Coop] Player 2 joined on gamepad {padIndex}.");
        }

        /// <summary>Create the second fighter with the given control surface (from the setup screen
        /// up-front, or from a mid-run Start-to-join). No-op if the world is gone or P2 already exists.</summary>
        private void SpawnPlayer2(IPlayerInput input)
        {
            if (_worldRoot == null || PlayerController.All.Count >= 2) return;

            var p2Go = new GameObject("Player2");
            p2Go.transform.SetParent(_worldRoot.transform);
            p2Go.AddComponent<SpriteRenderer>();
            p2Go.AddComponent<SpriteAnimator>();
            var p2 = p2Go.AddComponent<PlayerController>();
            p2.Configure(_selectedP2 ?? _selected);

            var anchor = PlayerController.Primary;
            p2.WorldX = (anchor != null ? anchor.WorldX : 0f) + 1.5f;
            p2.Z = 2.5f;
            p2.Init();
            p2.SetInput(input);
            if (p2.Sr != null) p2.Sr.color = new Color(0.62f, 0.80f, 1f); // cool tint = P2 (until distinct art)
        }

        private void TeardownWorld()
        {
            // Sweep gameplay objects that live outside the world root (spawned
            // enemies, projectiles, loot) so a return to the menu leaves a clean field.
            // Actor.All includes any live tutorial PassiveDummy; the TutorialController
            // component itself rides on the Systems object under _worldRoot and so is
            // disposed when _worldRoot is destroyed below.
            var kill = new List<GameObject>();
            foreach (var a in Actor.All) if (a != null) kill.Add(a.gameObject);
            foreach (var p in Object.FindObjectsByType<Projectile>(FindObjectsInactive.Exclude)) kill.Add(p.gameObject);
            foreach (var p in Object.FindObjectsByType<Pickup>(FindObjectsInactive.Exclude)) kill.Add(p.gameObject);
            foreach (var go in kill) if (go != null) Destroy(go);

            if (_worldRoot != null) Destroy(_worldRoot);
            _worldRoot = null;
        }

        // ---- Placeholder IMGUI screens --------------------------------------
        private void OnGUI()
        {
            if (Current == State.Playing) return;

            EnsureStyles();
            float scale = Screen.height / 360f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.width / scale, h = 360f;

            GUI.color = new Color(0.05f, 0.06f, 0.09f, 0.96f);
            GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;

            switch (Current)
            {
                case State.Title: TitleGUI(w, h); break;
                case State.Menu: MenuGUI(w, h); break;
                case State.PlayerSetup: PlayerSetupGUI(w, h); break;
                case State.CharacterSelect: CharacterSelectGUI(w, h); break;
                case State.HowToPlay: HowToPlayGUI(w, h); break;
            }
        }

        private void TitleGUI(float w, float h)
        {
            // The gag title, stacked + centered, biggest-to-smallest (it started as a
            // hand-drawn paper game — the escalating tiers ARE the joke).
            GUI.Label(new Rect(0, 34, w, 56), "THIS!:", _titleBig);
            GUI.Label(new Rect(0, 92, w, 40), "The Game:", _titleMid);
            GUI.Label(new Rect(0, 132, w, 30), "The Video Game", _titleSmall);

            // Pulsing "press any key to continue" (sine on unscaled time so it breathes even paused).
            float pulse = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.2f));
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, pulse);
            GUI.Label(new Rect(0, 244, w, 24), "press any key to continue", _label);
            GUI.color = prev;

            // ANY key or button (or a click) advances to the menu. Event-driven so one physical
            // press fires exactly once (Input.anyKeyDown would bleed across the frame's GUI passes).
            var e = Event.current;
            if (e.type == EventType.KeyDown || e.type == EventType.MouseDown)
            {
                Sfx.Play("confirm");
                GoMenu();
            }
        }

        // 1P/2P + per-player input selection, shown before character select (replaces the old
        // "press Start on a pad to join" guesswork).
        private void PlayerSetupGUI(float w, float h)
        {
            GUI.Label(new Rect(0, 30, w, 40), _endlessPending ? "ENDLESS — SETUP" : "SETUP", _title);
            float cx = w / 2f;

            if (OptionRow(cx, 110f, "PLAYERS", new[] { "1", "2" }, _playerCount - 1, out int pc)) _playerCount = pc + 1;
            if (OptionRow(cx, 180f, "PLAYER 1", InputNames, (int)_p1Input, out int p1)) _p1Input = (InputKind)p1;
            if (_playerCount == 2 && OptionRow(cx, 250f, "PLAYER 2", InputNames, (int)_p2Input, out int p2))
                _p2Input = (InputKind)p2;

            if (Button(new Rect(cx - 130f, 330f, 260f, 40f), "CONTINUE  →"))
            {
                Sfx.Play("confirm");
                GoCharacterSelect(_endlessPending);
            }

            GUI.Label(new Rect(0, h - 30, w, 24), "click to choose  ·  Esc to go back", _hint);
            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape) { Sfx.Play("cancel"); GoMenu(); }
        }

        /// <summary>A centered "LABEL  [opt][opt][opt]" chooser row; returns true (with the clicked
        /// index) when an option was clicked this frame.</summary>
        private bool OptionRow(float cx, float y, string label, string[] opts, int sel, out int clicked)
        {
            clicked = -1;
            GUI.color = Color.white;
            GUI.Label(new Rect(cx - 160f, y, 320f, 20f), label, _hint);
            float ow = opts.Length <= 2 ? 90f : 150f, gap = 8f;
            float total = opts.Length * ow + (opts.Length - 1) * gap;
            float ox = cx - total / 2f;
            var ev = Event.current;
            for (int i = 0; i < opts.Length; i++)
            {
                var r = new Rect(ox + i * (ow + gap), y + 22f, ow, 32f);
                GUI.color = i == sel ? new Color(0.22f, 0.5f, 0.85f) : new Color(0.16f, 0.19f, 0.27f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(r.x, r.y + 6f, r.width, r.height), opts[i], _label);
                if (ev.type == EventType.MouseDown && r.Contains(ev.mousePosition)) clicked = i;
            }
            return clicked >= 0;
        }

        private void MenuGUI(float w, float h)
        {
            GUI.Label(new Rect(0, 40, w, 40), "THIS.L", _title);

            const float bw = 260f, bh = 40f, gap = 12f;
            float x = (w - bw) / 2f, y0 = 120f;
            var e = Event.current;

            for (int i = 0; i < MenuItems.Length; i++)
            {
                var r = new Rect(x, y0 + i * (bh + gap), bw, bh);
                // Hover selects the row (mouse), matching the keyboard highlight.
                if (e.type == EventType.MouseMove && r.Contains(e.mousePosition)) _menuIndex = i;

                bool sel = _menuIndex == i;
                GUI.color = sel ? new Color(0.22f, 0.5f, 0.85f, 1f) : new Color(0.16f, 0.19f, 0.27f, 1f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(r.x, r.y + 9, r.width, r.height), MenuItems[i], sel ? _menuItemSel : _menuItem);

                if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) { _menuIndex = i; ActivateMenu(i); return; }
            }

            GUI.Label(new Rect(0, h - 30, w, 24), "arrows / W-S to move  ·  Enter to select  ·  Esc for title", _hint);

            // Keyboard nav — event-driven so each keypress moves exactly one row.
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                    case KeyCode.W:
                        _menuIndex = (_menuIndex + MenuItems.Length - 1) % MenuItems.Length;
                        Sfx.Play("menu_move");
                        break;
                    case KeyCode.DownArrow:
                    case KeyCode.S:
                        _menuIndex = (_menuIndex + 1) % MenuItems.Length;
                        Sfx.Play("menu_move");
                        break;
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                    case KeyCode.Space:
                        ActivateMenu(_menuIndex);
                        break;
                    case KeyCode.Escape:
                        Sfx.Play("cancel");
                        GoTitle();
                        break;
                }
            }
        }

        private void ActivateMenu(int i)
        {
            Sfx.Play("confirm");
            switch (i)
            {
                case 0: GoPlayerSetup(false); break;     // Campaign → 1P/2P setup → character select
                case 1: GoPlayerSetup(true); break;      // Endless Mode
                case 2: GoHowToPlay(); break;            // How to Play
            }
        }

        private void GoPlayerSetup(bool endless)
        {
            _endlessPending = endless;
            Current = State.PlayerSetup;
        }

        /// <summary>Build the control surface a player picked on the setup screen.</summary>
        private static IPlayerInput MakeInput(InputKind kind) => kind switch
        {
            InputKind.Controller1 => new GamepadInput(0),
            InputKind.Controller2 => new GamepadInput(1),
            _ => new KeyboardInput(),
        };

        private void DiffButton(Rect r, Difficulty d, string label)
        {
            bool sel = DifficultySettings.Current == d;
            GUI.color = sel ? new Color(0.95f, 0.85f, 0.25f) : new Color(0.20f, 0.26f, 0.38f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = sel ? Color.black : Color.white;
            GUI.Label(new Rect(r.x, r.y + 5, r.width, r.height), label, _label);
            GUI.color = Color.white;
            var e = Event.current;
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
            {
                Sfx.Play("menu_move");
                DifficultySettings.Current = d;
            }
        }

        private void CharacterSelectGUI(float w, float h)
        {
            GUI.Label(new Rect(0, 20, w, 40), _endlessPending ? "ENDLESS — CHOOSE YOUR FIGHTER" : "CHOOSE YOUR FIGHTER", _title);

            // Difficulty picker lives here now (feeds both Campaign and Endless). Hard is the
            // baseline; Easy/Medium pare it down.
            GUI.Label(new Rect(0, 66, w, 16), "DIFFICULTY", _label);
            const float dbw = 92f, dbg = 10f;
            float dtotal = 3 * dbw + 2 * dbg, dbx = (w - dtotal) / 2f;
            DiffButton(new Rect(dbx, 84, dbw, 24), Difficulty.Easy, "EASY");
            DiffButton(new Rect(dbx + dbw + dbg, 84, dbw, 24), Difficulty.Medium, "MEDIUM");
            DiffButton(new Rect(dbx + 2 * (dbw + dbg), 84, dbw, 24), Difficulty.Hard, "HARD");

            int n = _roster.Length;
            float cw = 150f, gap = 14f;
            float totalW = n * cw + (n - 1) * gap;
            float x0 = (w - totalW) / 2f;
            for (int i = 0; i < n; i++)
            {
                var c = _roster[i];
                // Taller card so the special-move name gets its own line clear of the
                // stat block and the SELECT button (was overlapping the button before).
                var r = new Rect(x0 + i * (cw + gap), 110, cw, 176);
                GUI.color = new Color(0.14f, 0.16f, 0.22f, 1f);
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(r.x, r.y + 8, r.width, 24), c.DisplayName, _label);

                // In-game hero SPRITE (idle frame) — NOT the anime portrait (creator: looks off).
                var set = SpriteLibrary.Load(c.SpriteDir, c.SpriteActor);
                var sp = set != null ? (set.FirstOf("idle") ?? set.First) : null;
                if (sp != null && sp.texture != null)
                {
                    var tex = sp.texture; var rr = sp.rect;
                    bool bert = c.SpriteActor == "player_underdog";   // barely-visible forehead gag
                    float frac = bert ? 0.30f : 1f;
                    var uv = new Rect(rr.x / tex.width, (rr.y + rr.height * (1f - frac)) / tex.height,
                                      rr.width / tex.width, rr.height * frac / tex.height);
                    float aspect = rr.width / (rr.height * frac);
                    float dh = bert ? 34f : 92f, dw = dh * aspect;
                    if (dw > r.width - 12) { dw = r.width - 12; dh = dw / aspect; }
                    float top = r.y + 30f, boxH = 92f;
                    float dy = bert ? top + boxH - dh : top + (boxH - dh);
                    GUI.DrawTextureWithTexCoords(new Rect(r.x + 6f + ((r.width - 12f) - dw) / 2f, dy, dw, dh), tex, uv);
                }
                else
                    GUI.Label(new Rect(r.x + 10, r.y + 34, r.width - 20, 76),
                        $"Spd x{c.MoveSpeedMult:0.00}\nFist x{c.PunchDmgMult:0.00}\nMeter x{c.MeterFillMult:0.00}\nWpn x{c.WeaponDmgMult:0.00}");
                // Special-move name on its own line, clearly ABOVE the button.
                GUI.Label(new Rect(r.x + 6, r.y + r.height - 58, r.width - 12, 20), c.Special?.Name ?? "", _special);
                if (Button(new Rect(r.x + 20, r.y + r.height - 34, r.width - 40, 26), "SELECT") ||
                    Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Sfx.Play("confirm");
                    if (_endlessPending) StartEndlessRun(c); else StartRun(c);
                    return;
                }
            }
            GUI.Label(new Rect(0, h - 30, w, 24), "click SELECT or press 1-4  ·  Esc to go back", _label);
            if (Input.GetKeyDown(KeyCode.Escape)) { Sfx.Play("cancel"); GoMenu(); }
        }

        private void HowToPlayGUI(float w, float h)
        {
            GUI.Label(new Rect(0, 20, w, 34), "HOW TO PLAY", _title);

            // Two-column control list: key cap on the left, what it does on the right.
            (string key, string desc)[] rows =
            {
                ("WASD", "Move"),
                ("Arrow Keys", "Attack — directional (8 ways; up/down strike into depth)"),
                ("Q", "Special — when the meter's full"),
                ("E", "Use ranged weapon / fire item"),
                ("F", "Pick up / swap weapon"),
                ("Double-tap dir / LeftShift", "Dash"),
                ("Dash into an enemy", "Bullet-shield dash — grab it as a shield and plow forward"),
                ("Start (2nd USB pad)", "Player 2 drops in"),
            };

            float y = 70f, rowH = 26f;
            float keyX = w * 0.10f, keyW = w * 0.34f;
            float descX = w * 0.46f, descW = w * 0.44f;
            foreach (var (key, desc) in rows)
            {
                GUI.Label(new Rect(keyX, y, keyW, rowH), key, _keyCap);
                GUI.Label(new Rect(descX, y, descW, rowH), desc, _keyDesc);
                y += rowH;
            }

            const float bw = 160f, bh = 34f;
            var back = new Rect((w - bw) / 2f, h - 54, bw, bh);
            if (Button(back, "BACK") || Input.GetKeyDown(KeyCode.Escape))
            {
                Sfx.Play("cancel");
                GoMenu();
            }
            GUI.Label(new Rect(0, h - 16, w, 14), "Esc or BACK to return to the menu", _hint);
        }

        private bool Button(Rect r, string label)
        {
            GUI.color = new Color(0.22f, 0.5f, 0.85f, 1f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(r.x, r.y + 8, r.width, r.height), label, _label);
            var e = Event.current;
            return e.type == EventType.MouseDown && r.Contains(e.mousePosition);
        }

        private void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label) { fontSize = 42, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleBig = new GUIStyle(GUI.skin.label) { fontSize = 54, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleMid = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleSmall = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _label = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            _btn = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            _special = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _special.normal.textColor = new Color(0.95f, 0.82f, 0.45f); // warm accent for the special name

            _menuItem = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _menuItem.normal.textColor = new Color(0.82f, 0.86f, 0.94f);
            _menuItemSel = new GUIStyle(_menuItem);
            _menuItemSel.normal.textColor = Color.white;
            _hint = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            _hint.normal.textColor = new Color(0.65f, 0.70f, 0.80f);
            _keyCap = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            _keyCap.normal.textColor = new Color(0.95f, 0.82f, 0.45f); // warm accent = the key(s)
            _keyDesc = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleLeft };
            _keyDesc.normal.textColor = new Color(0.86f, 0.89f, 0.94f);
        }
    }
}
