using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Airport ambience (creator: "Airport needs a tarmac and landing cessnas and jets"). On the
    /// airport stage only, periodically sends a CESSNA or a JET in to land: it enters high off the
    /// incoming edge, descends onto the far tarmac row, touches down and taxis across, then rolls off
    /// and despawns. Purely cosmetic background traffic (behind the fighters) — the lethal jet-pass
    /// stays with <see cref="HazardDirector"/>. Runs beside the other directors (see GameFlow).
    /// </summary>
    public sealed class AirportTraffic : MonoBehaviour
    {
        public float MinGap = 3.5f, MaxGap = 7f;

        private float _timer = 2f;
        private int _lastStage = -1;

        private void Update()
        {
            if (!PlayerController.AnyAlive || CampaignRunner.Instance == null) return;
            int stage = CampaignRunner.Instance.CurrentStage;
            if (stage != _lastStage) { _lastStage = stage; _timer = 1.5f; }
            if (!IsAirportStage(stage)) return;
            if (BossActive()) return;   // pause landings during the helicopter fight (creator)

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = Random.Range(MinGap, MaxGap);
            SpawnLanding();
        }

        private static bool IsAirportStage(int stage)
        {
            var data = StageDatabase.Get(stage);
            return data != null && data.BackdropTheme == "area2_airport";
        }

        private static bool BossActive()
        {
            foreach (var b in Object.FindObjectsByType<BossController>(FindObjectsInactive.Exclude))
                if (b != null && b.Alive) return true;
            return false;
        }

        private static void SpawnLanding()
        {
            bool jet = Random.value < 0.4f;                 // cessnas common, jets the occasional big arrival
            var spr = jet ? JetSprite() : CessnaSprite();
            if (spr == null) return;

            var cam = Camera.main;
            float camX = cam != null ? cam.transform.position.x : PlayerController.MidX();
            bool toRight = Random.value < 0.5f;
            float dir = toRight ? 1f : -1f;

            float tarmacY = Playfield.FeetY(Tuning.ZBandDepth * 0.5f);   // MIDDLE of the play area (creator)
            float half = Tuning.ScreenWidthUnits * 0.5f;

            var go = new GameObject(jet ? "landing_jet" : "landing_cessna");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = -60;                          // behind the fighters, in front of the backdrop

            float targetW = jet ? 6.6f : 3.4f;
            float natW = spr.rect.width / Tuning.PixelsPerUnit;
            float scale = natW > 0.1f ? targetW / natW : 1f;

            var lp = go.AddComponent<LandingPlane>();
            lp.Init(dir, camX - dir * (half + 6f), tarmacY + 7.5f, tarmacY, scale,
                    landSpeed: jet ? 20f : 15f, taxiSpeed: jet ? 6f : 5f);
        }

        // ---- sprites (real art wins; procedural fallback via HazardDirector's plane) ----
        private static Sprite _cessna, _jet;
        private static Sprite CessnaSprite() => _cessna ??= LoadProp("cessna.png");
        private static Sprite JetSprite()    => _jet    ??= LoadProp("jet.png");

        private static Sprite LoadProp(string file)
        {
            try
            {
                string path = System.IO.Path.Combine(SpriteLibrary.AssetsRoot, "sprites", "props", file);
                if (System.IO.File.Exists(path))
                {
                    var t = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    t.LoadImage(System.IO.File.ReadAllBytes(path)); t.Apply();
                    // bottom-centre pivot so the wheels sit on the tarmac line
                    return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.08f), Tuning.PixelsPerUnit);
                }
            }
            catch { }
            return null;
        }
    }

    /// <summary>One landing aircraft: descend to the tarmac, flare/roll out, taxi, then despawn.</summary>
    public sealed class LandingPlane : MonoBehaviour
    {
        private float _dir, _x, _highY, _tarmacY, _scale, _landSpeed, _taxiSpeed;
        private float _t;
        private const float DescentDur = 1.8f;

        public void Init(float dir, float x0, float highY, float tarmacY, float scale, float landSpeed, float taxiSpeed)
        {
            _dir = dir; _x = x0; _highY = highY; _tarmacY = tarmacY; _scale = scale;
            _landSpeed = landSpeed; _taxiSpeed = taxiSpeed;
            Apply(highY);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _t += dt;

            float y, speed;
            if (_t < DescentDur)
            {
                float p = Mathf.SmoothStep(0f, 1f, _t / DescentDur);   // ease onto the runway
                y = Mathf.Lerp(_highY, _tarmacY, p);
                speed = _landSpeed;
            }
            else
            {
                y = _tarmacY;
                speed = Mathf.Lerp(_landSpeed, _taxiSpeed, Mathf.Clamp01((_t - DescentDur) / 1.6f)); // brake to a taxi
            }
            _x += _dir * speed * dt;
            Apply(y);

            // Despawn once it has rolled well off the far side of the view (or after a safe max life).
            var cam = Camera.main;
            float camX = cam != null ? cam.transform.position.x : _x;
            if (_t > 12f || Mathf.Abs(_x - camX) > Tuning.ScreenWidthUnits * 0.5f + 9f)
                Destroy(gameObject);
        }

        private void Apply(float y)
        {
            transform.position = new Vector3(_x, y, 0f);
            transform.localScale = new Vector3(_dir * _scale, _scale, 1f);  // flip to face the travel direction
        }
    }
}
