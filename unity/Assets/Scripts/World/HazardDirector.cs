using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Periodically launches environmental hazards across the lane to crank the
    /// chaos (AREAS.md §7c). Right now: cars barreling through on a random depth
    /// row from a random side — you have to watch the field AND the road.
    /// </summary>
    public sealed class HazardDirector : MonoBehaviour
    {
        public float MinInterval = 7f;
        public float MaxInterval = 13f;
        public float CarSpeed = 21f;

        private static readonly Color[] CarColors =
        {
            new(0.90f, 0.24f, 0.22f), new(0.25f, 0.45f, 0.85f),
            new(0.95f, 0.80f, 0.25f), new(0.85f, 0.85f, 0.90f),
        };

        public float WarnSeconds = 1.3f;   // telegraph time before the car crosses

        private float _timer;
        private bool _busy;

        private void Start() => _timer = Random.Range(MinInterval * 0.5f, MinInterval);

        private void Update()
        {
            if (!PlayerController.AnyAlive || _busy) return;
            if (CampaignRunner.Instance == null) return;
            var kind = KindForStage(CampaignRunner.Instance.CurrentStage);
            if (kind == HazardKind.None) return;   // per-level nuisance (creator decides which per level)

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = Random.Range(MinInterval, MaxInterval);
            StartCoroutine(WarnThenHazard(kind));
        }

        /// <summary>Which nuisance haunts which level (creator: "we decide on hazards per level").
        /// Theme-driven, NOT by stage index — L2 is stashed, so an index-keyed table put the guard
        /// hazard on the skipped stage and left the playable mall bare. Keying off the backdrop theme
        /// maps each of the four playable levels (Streets / Mall / Sac / Airport) to its hazard.</summary>
        private enum HazardKind { None, Car, Guard, Plane, CrossProp }
        private static HazardKind KindForStage(int stage)
        {
            var data = StageDatabase.Get(stage);
            return (data != null ? data.BackdropTheme : null) switch
            {
                "area1_suburb"    => HazardKind.Car,       // Streets — cars barrel through
                "area4_goldengate"=> HazardKind.Car,       // Golden Gate — bridge traffic
                "area1_mall"      => HazardKind.Guard,     // Mall — security guards charge the concourse
                "area2_airport"   => HazardKind.Plane,     // Airport — a jet screams across the tarmac
                "area3_farm"      => HazardKind.CrossProp, // Farm — a charging bull
                "area3_dixon"     => HazardKind.CrossProp, // Dixon — a tumbleweed rolls through
                "area4_vallejo"   => HazardKind.CrossProp, // Six Flags — a roller-coaster car on the top rail
                "area4_marin"     => HazardKind.CrossProp, // Redwoods — a rolling log
                "area4_sf"        => HazardKind.CrossProp, // SF — a runaway trolley
                _ => HazardKind.None,                      // Sac (whip fight) + causeway (gap platformer): no hazard
            };
        }

        /// <summary>A themed lane-crossing hazard: which prop, how fast, how it hurts. Reused by the
        /// Area-3/4 levels through <see cref="CrossHazard"/> (sprite art faces -X / left).</summary>
        private struct CrossSpec { public string Sprite, Sfx; public float Speed, ScaleH, Dmg, EnemyDmg, Push, Stagger; public bool FarRow; }
        private static CrossSpec SpecForTheme(string theme) => theme switch
        {
            "area3_farm"    => new CrossSpec { Sprite = "bull",       Sfx = "whoosh_heavy", Speed = 14f, ScaleH = 2.4f, Dmg = 18f,   EnemyDmg = 40f, Push = 2f,   Stagger = 0.9f, FarRow = false },
            "area3_dixon"   => new CrossSpec { Sprite = "tumbleweed", Sfx = "dash_whoosh",  Speed = 11f, ScaleH = 1.7f, Dmg = 0f,    EnemyDmg = 0f,  Push = 1.5f, Stagger = 0.4f, FarRow = false },
            "area4_vallejo" => new CrossSpec { Sprite = "coaster",    Sfx = "car_horn",     Speed = 30f, ScaleH = 2.6f, Dmg = 9999f, EnemyDmg = 60f, Push = 0f,   Stagger = 0f,   FarRow = true  },
            "area4_marin"   => new CrossSpec { Sprite = "log",        Sfx = "whoosh_heavy", Speed = 17f, ScaleH = 1.8f, Dmg = 15f,   EnemyDmg = 40f, Push = 2.5f, Stagger = 0.7f, FarRow = false },
            "area4_sf"      => new CrossSpec { Sprite = "trolley",    Sfx = "car_horn",     Speed = 22f, ScaleH = 3.2f, Dmg = 9999f, EnemyDmg = 60f, Push = 0f,   Stagger = 0f,   FarRow = false },
            _               => new CrossSpec { Sprite = "log",        Sfx = "whoosh_heavy", Speed = 14f, ScaleH = 1.8f, Dmg = 15f,   EnemyDmg = 40f, Push = 2f,   Stagger = 0.6f, FarRow = false },
        };

        private System.Collections.IEnumerator WarnThenHazard(HazardKind kind)
        {
            _busy = true;
            bool fromLeft = Random.value < 0.5f;

            CrossSpec spec = default;
            bool cross = kind == HazardKind.CrossProp;
            if (cross)
            {
                var d = CampaignRunner.Instance != null ? StageDatabase.Get(CampaignRunner.Instance.CurrentStage) : null;
                spec = SpecForTheme(d != null ? d.BackdropTheme : null);
            }

            // Planes / a coaster on the top rail scream across a FAR row; the rest use a random near-ish row.
            float z = (kind == HazardKind.Plane || (cross && spec.FarRow))
                ? Tuning.ZBandDepth - 0.5f : Random.Range(1f, Tuning.ZBandDepth - 1f);

            // TELEGRAPH: a blinking warning arrow at the incoming edge on the hazard's row + a cue,
            // so the player always sees it coming.
            Sfx.Play(cross ? spec.Sfx : kind == HazardKind.Plane ? "jet_pass" : kind == HazardKind.Guard ? "guard_whistle" : "car_horn");
            var warn = MakeWarn(fromLeft, z);
            float t = WarnSeconds;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                if (warn != null)
                {
                    warn.enabled = ((int)(Time.time * 10f) & 1) == 0;   // blink
                    PinWarnToCameraEdge(warn.transform, fromLeft, z);   // stay glued to the screen edge
                }
                yield return null;
            }
            if (warn != null) Destroy(warn.gameObject);

            float edge = Tuning.ScreenWidthUnits * 0.5f + 3f;
            float x = PlayerController.MidX() + (fromLeft ? -edge : edge);
            float dir = fromLeft ? 1f : -1f;
            switch (kind)
            {
                case HazardKind.Car:
                    CarHazard.Spawn(x, z, dir * CarSpeed, CarColors[Random.Range(0, CarColors.Length)]);
                    break;
                case HazardKind.Guard:   // a security guard JOGS across and shoulder-checks the row
                {
                    var gs = GuardSprite();
                    float sc = 2.6f / Mathf.Max(0.3f, gs.rect.height / Tuning.PixelsPerUnit); // ~person height, any sprite
                    // The guard KILLS NO ONE — it just barges through and shoves everyone (player AND
                    // enemies) back along its path (creator: "the car should kill them, the guard just
                    // pushes everyone back"). Cars keep their own lethal CarHazard.
                    var guard = CrossHazard.Spawn(gs, x, z, dir * 9f, 0f, new Vector2(sc, sc), 0.8f, 0f);
                    guard.EnemyDamage = 0f;       // no kill
                    guard.PushX = 3.0f;           // firm shove for everyone it clips
                    guard.StaggerSeconds = 1.0f;  // brief stumble, not a long knockdown
                    break;
                }
                case HazardKind.Plane:   // a jet roars across the far tarmac row — big, fast, deadly
                    CrossHazard.Spawn(PlaneSprite(), x, z, dir * 34f, 26f, new Vector2(4.5f, 2.2f), 2.4f, 0.4f);
                    break;
                case HazardKind.CrossProp:   // themed lane-crosser (bull / tumbleweed / coaster / log / trolley)
                {
                    var spr = CrossSprite(spec.Sprite);
                    float sc = spec.ScaleH / Mathf.Max(0.3f, spr.rect.height / Tuning.PixelsPerUnit);
                    float halfLen = Mathf.Max(0.7f, spr.rect.width / Tuning.PixelsPerUnit * sc * 0.42f);
                    var h = CrossHazard.Spawn(spr, x, z, dir * spec.Speed, spec.Dmg, new Vector2(sc, sc), halfLen, 0f);
                    h.EnemyDamage = spec.EnemyDmg;
                    h.PushX = spec.Push;
                    h.StaggerSeconds = spec.Stagger;
                    break;
                }
            }
            _busy = false;
        }

        // A yellow warning arrow pinned to the screen EDGE on the hazard's row, pointing the way it'll come.
        private static SpriteRenderer MakeWarn(bool fromLeft, float z)
        {
            var go = new GameObject("hazard_warn");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WarnSprite();
            sr.sortingOrder = 950;
            PinWarnToCameraEdge(go.transform, fromLeft, z);
            return sr;
        }

        /// <summary>Glue the warn arrow to the CAMERA's screen edge (not a fixed world X) on row z, so it
        /// stays on-screen as the camera scrolls during the telegraph (creator: "bound to the camera,
        /// not x/y/z"). Recomputed every frame from the live camera position.</summary>
        private static void PinWarnToCameraEdge(Transform tr, bool fromLeft, float z)
        {
            var cam = Camera.main;
            float camX = cam != null ? cam.transform.position.x : PlayerController.MidX();
            float edgeX = camX + (fromLeft ? -1f : 1f) * (Tuning.ScreenWidthUnits * 0.5f - 1f);
            Playfield.Place(tr, edgeX, z, null);                 // world x/z for the row; camera pins the x
            tr.position += Vector3.up * 1.2f;
            tr.localScale = new Vector3(fromLeft ? 1f : -1f, 1f, 1f); // point toward travel (overrides Place's depth scale)
        }

        private static Sprite _warn;
        private static Sprite WarnSprite()
        {
            if (_warn != null) return _warn;
            const int W = 16, H = 12;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[W * H];
            var yellow = new Color32(255, 210, 40, 255);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    // a right-pointing arrow (triangle head + shaft)
                    bool head = x >= 8 && Mathf.Abs(y - H / 2) <= (W - 1 - x);
                    bool shaft = x < 9 && Mathf.Abs(y - H / 2) <= 2;
                    px[y * W + x] = (head || shaft) ? yellow : new Color32(0, 0, 0, 0);
                }
            tex.SetPixels32(px); tex.Apply();
            _warn = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), Tuning.PixelsPerUnit);
            return _warn;
        }

        // ---- Plane / guard hazard sprites (real art in assets/sprites/props/ wins; else procedural) --
        private static Sprite _plane, _guard;
        private static Sprite PlaneSprite() => _plane ??= (LoadProp("plane.png") ?? MakePlane());
        private static Sprite GuardSprite() => _guard ??= (LoadProp("guard.png") ?? MakeGuard());

        // Themed cross-prop sprites (bull/tumbleweed/coaster/log/trolley), cached by name.
        private static readonly System.Collections.Generic.Dictionary<string, Sprite> _crossCache = new();
        private static Sprite CrossSprite(string name)
        {
            if (_crossCache.TryGetValue(name, out var s) && s != null) return s;
            s = LoadProp(name + ".png") ?? MakeGuard();   // fallback keeps CrossHazard non-null
            _crossCache[name] = s;
            return s;
        }

        private static Sprite LoadProp(string file)
        {
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", file);
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.12f), Tuning.PixelsPerUnit);
                }
            }
            catch { }
            return null;
        }

        private static Sprite MakePlane()
        {
            const int W = 52, H = 20;
            var px = new Color32[W * H];
            Color32 body = new(220, 224, 230, 255), stripe = new(200, 40, 40, 255),
                    win = new(40, 60, 90, 255), wing = new(180, 184, 192, 255);
            void Set(int x, int y, Color32 c) { if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = c; }
            for (int x = 2; x <= 47; x++)                       // fuselage, tapered nose (left) + tail (right)
            {
                int half = 3;
                if (x < 8) half = 3 - (8 - x) / 2;
                if (x > 42) half = 3 - (x - 42) / 2;
                for (int y = 10 - half; y <= 10 + half; y++) Set(x, y, body);
            }
            for (int x = 8; x <= 44; x++) Set(x, 10, stripe);   // red stripe
            for (int x = 10; x <= 40; x += 3) Set(x, 12, win);  // window row
            Set(5, 11, win); Set(6, 11, win);                   // cockpit
            for (int i = 0; i < 14; i++) Set(24 + i, 7 - i / 2, wing); // swept wing
            for (int y = 12; y <= 18; y++) for (int x = 44; x <= 47 + (y - 12) / 2; x++) Set(x, y, stripe); // tail fin
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.15f), Tuning.PixelsPerUnit);
        }

        private static Sprite MakeGuard()
        {
            const int W = 16, H = 28;
            var px = new Color32[W * H];
            Color32 navy = new(35, 50, 95, 255), skin = new(224, 180, 150, 255),
                    cap = new(20, 28, 55, 255), belt = new(230, 200, 60, 255), leg = new(30, 34, 48, 255);
            void Set(int x, int y, Color32 c) { if (x >= 0 && x < W && y >= 0 && y < H && c.a != 0) px[y * W + x] = c; }
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    Color32 c = new(0, 0, 0, 0);
                    if (y < 4 && x >= 4 && x <= 11) c = leg;                    // legs
                    else if (y >= 4 && y < 19 && x >= 3 && x <= 12) c = navy;   // torso
                    else if (y >= 19 && y < 25 && x >= 5 && x <= 10) c = skin;  // head
                    else if (y >= 24 && y < 27 && x >= 4 && x <= 11) c = cap;   // cap
                    if (y == 11 && x >= 3 && x <= 12) c = belt;                 // belt
                    Set(x, y, c);
                }
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.05f), Tuning.PixelsPerUnit);
        }
    }
}
