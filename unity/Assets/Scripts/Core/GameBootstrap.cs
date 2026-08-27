using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Boots the game from code at play/build start, so the project needs no
    /// hand-authored .unity scene (which would mean fragile GUID wiring against
    /// un-imported assets). Creates the persistent camera + ground, then hands off
    /// to <see cref="GameFlow"/>, which drives Title -> Character Select -> Playing
    /// and builds/tears down the gameplay world per run.
    /// </summary>
    public static class GameBootstrap
    {
        private static bool _built;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (_built) return;
            _built = true;

            Application.targetFrameRate = 60;

            // A hand-saved / default scene can leave a stock "Main Camera" (with its OWN AudioListener)
            // sitting next to the code camera we're about to create — that collides: TWO cameras fight
            // for the view and TWO AudioListeners spam "there are 2 audio listeners" every frame (999+
            // warnings that bury real errors). This is code-first, so strip every pre-existing camera and
            // audio listener before we build ours, leaving exactly one of each.
            foreach (var stray in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.Destroy(stray.gameObject);
            foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.Destroy(listener);

            // --- Persistent camera + rig ---
            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            camGo.AddComponent<AudioListener>(); // required for any SFX/music to be audible
            camGo.AddComponent<CameraRig>();

            // --- Persistent ground plane (depth-reference strip across the play band) ---
            CreateGround();

            // --- Screen flow (Title -> Select -> Playing) owns the gameplay world ---
            new GameObject("GameFlow").AddComponent<GameFlow>();

            Debug.Log("[GameBootstrap] this.l booted. Assets root: " + SpriteLibrary.AssetsRoot);
        }

        private static void CreateGround()
        {
            // Dark ground filling the bottom play band, plus a lighter near-edge line.
            var go = new GameObject("Ground");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SolidSprite(new Color(0.62f, 0.62f, 0.60f)); // neutral pavement gray (the Backdrop draws the real ground over this; gray blends if it ever peeks — was a jarring dark green)
            sr.sortingOrder = -1000;
            // Play band spans y in [-7.5, 1.5]; center it and make it VERY wide so it still covers
            // the (now ~8x longer) lane behind the tiling backdrop.
            go.transform.position = new Vector3(0f, (-7.5f + 1.5f) / 2f, 0f);
            go.transform.localScale = new Vector3(4000f, 9f, 1f);

            // A near-edge horizon line for depth read.
            var line = new GameObject("GroundFarLine");
            var lsr = line.AddComponent<SpriteRenderer>();
            lsr.sprite = SolidSprite(new Color(0.28f, 0.32f, 0.26f));
            lsr.sortingOrder = -999;
            line.transform.position = new Vector3(0f, Playfield.FeetY(Tuning.ZBandDepth), 0f);
            line.transform.localScale = new Vector3(4000f, 0.1f, 1f);
        }

        private static Sprite _solid;
        private static Sprite SolidSprite(Color c)
        {
            // Shared 1x1 white sprite; tint via renderer where needed. For distinct
            // colours we bake a tiny texture per call (only two callers here).
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
