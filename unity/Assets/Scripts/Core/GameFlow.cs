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
        public enum State { Title, CharacterSelect, Playing }
        public static GameFlow Instance { get; private set; }

        public State Current { get; private set; } = State.Title;

        private CharacterDef _selected;
        private bool _weaponPromptShown;   // one-time "press E to fire" teach on first pickup
        private GameObject _worldRoot;
        private CharacterDef[] _roster;
        private GUIStyle _title, _label, _btn, _special;

        private void Awake()
        {
            Instance = this;
            _roster = CharacterDef.Roster();
        }

        private void Start() => GoTitle();

        // ---- Transitions -----------------------------------------------------
        public void GoTitle()
        {
            TeardownWorld();
            Current = State.Title;
            Music.PlayTitle();
        }

        public void GoCharacterSelect() => Current = State.CharacterSelect;

        public void StartRun(CharacterDef def)
        {
            _selected = def;
            BuildWorld();
            Current = State.Playing;
        }

        // ---- World lifecycle -------------------------------------------------
        private void BuildWorld()
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
            // NOTE: the EnemySpawner is deliberately NOT added here — it's added at the
            // end of the opening chain below, so no wave spawns during the intro
            // cinematic or the tutorial.

            RunOpeningSequence(sys);
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
                        if (sys != null) { sys.AddComponent<CampaignRunner>(); sys.AddComponent<HazardDirector>(); }
                    });
                });
            });
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

            if (Current == State.Title) TitleGUI(w, h);
            else CharacterSelectGUI(w, h);
        }

        private void TitleGUI(float w, float h)
        {
            GUI.Label(new Rect(0, 60, w, 60), "this.l", _title);
            GUI.Label(new Rect(0, 128, w, 24), "a spacing brawler", _label);

            // Difficulty picker (Hard is the baseline; Easy/Medium pare it down).
            GUI.Label(new Rect(0, 162, w, 18), "DIFFICULTY", _label);
            const float bw = 92f, bg = 10f;
            float total = 3 * bw + 2 * bg, bx = (w - total) / 2f;
            DiffButton(new Rect(bx, 182, bw, 28), Difficulty.Easy, "EASY");
            DiffButton(new Rect(bx + bw + bg, 182, bw, 28), Difficulty.Medium, "MEDIUM");
            DiffButton(new Rect(bx + 2 * (bw + bg), 182, bw, 28), Difficulty.Hard, "HARD");

            if (Button(new Rect(w / 2f - 90, 238, 180, 40), "PRESS START") ||
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                Sfx.Play("confirm");
                GoCharacterSelect();
            }
            GUI.Label(new Rect(0, h - 28, w, 22), "pick difficulty · Enter / click to begin", _label);
        }

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
            GUI.Label(new Rect(0, 40, w, 40), "CHOOSE YOUR FIGHTER", _title);
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

                // Portrait art if present (assets/portraits/<id>.png); else the stat block.
                var portrait = PortraitLibrary.Get(c.Id);
                if (portrait != null)
                    GUI.DrawTexture(new Rect(r.x + 6, r.y + 30, r.width - 12, 92), portrait, ScaleMode.ScaleToFit);
                else
                    GUI.Label(new Rect(r.x + 10, r.y + 34, r.width - 20, 76),
                        $"Spd x{c.MoveSpeedMult:0.00}\nFist x{c.PunchDmgMult:0.00}\nMeter x{c.MeterFillMult:0.00}\nWpn x{c.WeaponDmgMult:0.00}");
                // Special-move name on its own line, clearly ABOVE the button.
                GUI.Label(new Rect(r.x + 6, r.y + r.height - 58, r.width - 12, 20), c.Special?.Name ?? "", _special);
                if (Button(new Rect(r.x + 20, r.y + r.height - 34, r.width - 40, 26), "SELECT") ||
                    Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Sfx.Play("confirm");
                    StartRun(c);
                    return;
                }
            }
            GUI.Label(new Rect(0, h - 30, w, 24), "click SELECT or press 1-4  ·  Esc to go back", _label);
            if (Input.GetKeyDown(KeyCode.Escape)) { Sfx.Play("cancel"); GoTitle(); }
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
            _label = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            _btn = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            _special = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _special.normal.textColor = new Color(0.95f, 0.82f, 0.45f); // warm accent for the special name
        }
    }
}
