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
            // LEVEL 1 ONLY for now (creator ruling — per-area hazards come later).
            if (CampaignRunner.Instance == null || CampaignRunner.Instance.CurrentStage != 0) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = Random.Range(MinInterval, MaxInterval);
            StartCoroutine(WarnThenCar());
        }

        private System.Collections.IEnumerator WarnThenCar()
        {
            _busy = true;
            bool fromLeft = Random.value < 0.5f;
            float z = Random.Range(1f, Tuning.ZBandDepth - 1f);

            // TELEGRAPH: a blinking warning marker at the incoming edge on the car's row + horn,
            // so the player always sees it coming.
            Sfx.Play("car_horn");
            var warn = MakeWarn(fromLeft, z);
            float t = WarnSeconds;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                if (warn != null) warn.enabled = ((int)(Time.time * 10f) & 1) == 0;   // blink
                yield return null;
            }
            if (warn != null) Destroy(warn.gameObject);

            float edge = Tuning.ScreenWidthUnits * 0.5f + 3f;
            float x = PlayerController.MidX() + (fromLeft ? -edge : edge);
            float vel = (fromLeft ? 1f : -1f) * CarSpeed;
            CarHazard.Spawn(x, z, vel, CarColors[Random.Range(0, CarColors.Length)]);
            _busy = false;
        }

        // A yellow warning arrow at the screen edge on the car's row, pointing the way it'll come.
        private static SpriteRenderer MakeWarn(bool fromLeft, float z)
        {
            var go = new GameObject("car_warn");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WarnSprite();
            sr.sortingOrder = 950;
            float edgeX = PlayerController.MidX() + (fromLeft ? -(Tuning.ScreenWidthUnits * 0.5f - 1f)
                                                              : (Tuning.ScreenWidthUnits * 0.5f - 1f));
            Playfield.Place(go.transform, edgeX, z, sr);
            go.transform.position += Vector3.up * 1.2f;
            go.transform.localScale = new Vector3(fromLeft ? 1f : -1f, 1f, 1f); // point toward travel
            return sr;
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
    }
}
