using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>Time-based special payloads that can't happen in one frame:
    /// the shared 0.45s WIND-UP (a slow-mo charge every special leads with), the
    /// sniper's one-at-a-time slow-mo ricochet, and the werewolf transform that
    /// holds for the timer then fades back to human. Each runs on its own spawned
    /// GameObject so a static special can "start" it.</summary>
    public static class SpecialSequences
    {
        /// <summary>The shared special WIND-UP (creator: "all of them need the 0.45 slow-down"): time
        /// slows to a crawl, a blackish blur spins up at the hands + a charge glow, then after ~0.45s
        /// real-time it restores normal speed and runs <paramref name="payload"/> (the actual effect).</summary>
        public static void Windup(PlayerController p, System.Action payload, float seconds = 0.45f) =>
            new GameObject("fx_special_windup").AddComponent<WindupSeq>().Begin(p, payload, seconds);

        public static void SniperRicochet(Actor from, int maxKills) =>
            new GameObject("fx_sniper_seq").AddComponent<SniperSeq>().Begin(from, maxKills);

        public static void Werewolf(PlayerController p, float duration) =>
            new GameObject("fx_werewolf").AddComponent<WerewolfMode>().Begin(p, duration);

        private static Actor NearestEnemyTo(float x, float z)
        {
            Actor best = null; float bestD = float.MaxValue;
            foreach (var a in Actor.All)
            {
                if (!a.Alive || a.Team != Team.Enemy) continue;
                float dx = a.WorldX - x, dz = a.Z - z, d = dx * dx + dz * dz;
                if (d < bestD) { bestD = d; best = a; }
            }
            return best;
        }

        // ---- Shared wind-up visual (the "blackish blur" spin) ----------------------

        /// <summary>A dark bar pinned at the hands that the caller spins fast → a "blackish blur"
        /// spin-up. Parented to the player so it tracks them during the wind-up.</summary>
        private static GameObject MakeSpinBlur(Actor from)
        {
            if (from == null) return null;
            var go = new GameObject("fx_special_spin");
            if (from.transform != null) { go.transform.SetParent(from.transform, false); go.transform.localPosition = new Vector3(0.4f, 0.9f, 0f); }
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BarSprite();
            sr.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);   // blackish
            sr.sortingOrder = 500;                              // in front of the fighters
            go.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            return go;
        }

        private static Sprite _bar;
        private static Sprite BarSprite()
        {
            if (_bar != null) return _bar;
            var tex = new Texture2D(18, 3, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[18 * 3];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px); tex.Apply();
            _bar = Sprite.Create(tex, new Rect(0, 0, 18, 3), new Vector2(0.5f, 0.5f), 24f);
            return _bar;
        }

        /// <summary>A short bright streak between two ground points (X-Z → screen via Playfield),
        /// fading over ~0.12s real-time — the sniper's visible bullet ricochet from foe to foe.</summary>
        private sealed class Tracer : MonoBehaviour
        {
            public static void Spawn(float x1, float z1, float x2, float z2, Color col)
                => new GameObject("fx_tracer").AddComponent<Tracer>().Init(x1, z1, x2, z2, col);

            private SpriteRenderer _sr; private Color _col; private float _life = 0.12f; private const float Max = 0.12f;

            private Tracer Init(float x1, float z1, float x2, float z2, Color col)
            {
                float y1 = Playfield.FeetY(z1) + 0.9f, y2 = Playfield.FeetY(z2) + 0.9f; // ~chest height
                Vector2 a = new(x1, y1), b = new(x2, y2), mid = (a + b) * 0.5f;
                float len = Vector2.Distance(a, b);
                float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
                _sr = gameObject.AddComponent<SpriteRenderer>();
                _sr.sprite = BarSprite();
                _sr.color = _col = col;
                _sr.sortingOrder = 600;
                transform.position = new Vector3(mid.x, mid.y, 0f);
                transform.rotation = Quaternion.Euler(0f, 0f, ang);
                transform.localScale = new Vector3(len / (18f / 24f), 1.4f, 1f); // stretch the 18px bar to span the gap
                return this;
            }

            private void Update()
            {
                _life -= Time.unscaledDeltaTime;
                if (_sr != null) { var c = _col; c.a = Mathf.Clamp01(_life / Max); _sr.color = c; }
                if (_life <= 0f) Destroy(gameObject);
            }
        }

        // ---- The shared wind-up runner --------------------------------------------

        private sealed class WindupSeq : MonoBehaviour
        {
            private PlayerController _p; private System.Action _payload; private float _sec;
            public void Begin(PlayerController p, System.Action payload, float sec)
            { _p = p; _payload = payload; _sec = sec; StartCoroutine(Run()); }

            private IEnumerator Run()
            {
                Time.timeScale = 0.4f;                     // the slow-down beat (real-time waits below)
                Sfx.Play("sniper_timeslow_enter");         // shared slow-mo whoosh
                var blur = MakeSpinBlur(_p);
                if (_p != null) SpecialFx.Glow(_p, new Color(1f, 0.95f, 0.6f), _sec);
                float t = 0f;
                while (t < _sec)
                {
                    t += Time.unscaledDeltaTime;
                    if (blur != null) blur.transform.Rotate(0f, 0f, -1500f * Time.unscaledDeltaTime);
                    yield return null;
                }
                if (blur != null) Destroy(blur);
                Time.timeScale = 1f;                       // restore before the payload runs its own timing
                _payload?.Invoke();
                Destroy(gameObject);
            }
        }

        // ---- Sniper ricochet (payload; the wind-up is the shared Windup above) -----

        /// <summary>The sniper special: time slows, and the shot caroms enemy-to-enemy,
        /// one-shot-killing each with a beat between (drops nothing).</summary>
        private sealed class SniperSeq : MonoBehaviour
        {
            private Actor _from; private int _max;
            public void Begin(Actor from, int max) { _from = from; _max = max; StartCoroutine(Run()); }

            private IEnumerator Run()
            {
                Time.timeScale = 0.28f;                 // time slows (real-time waits below)
                float x = _from != null ? _from.WorldX : 0f, z = _from != null ? _from.Z : 3f;
                int kills = 0;
                while (kills < _max)
                {
                    var t = NearestEnemyTo(x, z);
                    if (t == null) break;
                    Tracer.Spawn(x, z, t.WorldX, t.Z, new Color(1f, 0.95f, 0.6f)); // the bullet's visible ricochet streak
                    Vfx.FinisherFlash(t.WorldX, t.Z);
                    Sfx.Play("hit_spark");
                    if (t is ISpecialKillable k) k.KillBySpecial(_from); else t.TakeDamage(9999f, _from);
                    x = t.WorldX; z = t.Z; kills++;
                    yield return new WaitForSecondsRealtime(0.13f);
                }
                Sfx.Play("time_resume_whoosh");
                Time.timeScale = 1f;
                Destroy(gameObject);
            }
        }

        /// <summary>Werewolf transform: hold the bigger, glowing wolf form for the timer
        /// (full i-frames), auto-slashing anything on screen, then fade back to human.</summary>
        private sealed class WerewolfMode : MonoBehaviour
        {
            private PlayerController _p; private float _dur, _t; private float _baseScale;
            private float _slashTimer;

            public void Begin(PlayerController p, float dur)
            {
                _p = p; _dur = dur;
                _baseScale = p.ScaleMult;
                p.ScaleMult = _baseScale * 1.25f;        // hunched, bigger
                p.SetInvuln(dur + 1.0f);                  // i-frames through the whole window + fade
                SpecialFx.Glow(p, new Color(1f, 0.8f, 0.3f), dur); // sustained power glow
                StartCoroutine(Run());
            }

            private IEnumerator Run()
            {
                while (_t < _dur)
                {
                    _t += Time.deltaTime;
                    // auto-slash: cull the field a few times a second (1HKO, keeps drops)
                    _slashTimer -= Time.deltaTime;
                    if (_slashTimer <= 0f && _p != null)
                    {
                        _slashTimer = 0.25f;
                        foreach (var a in new System.Collections.Generic.List<Actor>(Actor.All))
                            if (a.Alive && a.Team == Team.Enemy && _p.DistanceTo(a) <= 2.5f)
                            { Vfx.FinisherFlash(a.WorldX, a.Z); a.TakeDamage(9999f, _p); }
                    }
                    yield return null;
                }
                // Fade back to human over ~1s.
                float f = 0f;
                while (f < 1f && _p != null)
                {
                    f += Time.deltaTime / 1.0f;
                    _p.ScaleMult = Mathf.Lerp(_baseScale * 1.25f, _baseScale, f);
                    yield return null;
                }
                if (_p != null) _p.ScaleMult = _baseScale;
                Destroy(gameObject);
            }
        }
    }
}
