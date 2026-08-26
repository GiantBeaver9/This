using System.Collections;
using UnityEngine;

namespace ThisL
{
    /// <summary>Time-based special payloads that can't happen in one frame:
    /// the sniper's one-at-a-time slow-mo ricochet, and the werewolf transform
    /// that holds for the timer then fades back to human. Each runs on its own
    /// spawned GameObject so a static special can "start" it.</summary>
    public static class SpecialSequences
    {
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

        /// <summary>The sniper special: time slows, and the shot caroms enemy-to-enemy,
        /// one-shot-killing each with a beat between (drops nothing).</summary>
        private sealed class SniperSeq : MonoBehaviour
        {
            private Actor _from; private int _max;
            public void Begin(Actor from, int max) { _from = from; _max = max; StartCoroutine(Run()); }

            private IEnumerator Run()
            {
                Time.timeScale = 0.28f;                 // time slows (real-time waits below)
                Sfx.Play("sniper_timeslow_enter");
                float x = _from != null ? _from.WorldX : 0f, z = _from != null ? _from.Z : 3f;
                int kills = 0;
                while (kills < _max)
                {
                    var t = NearestEnemyTo(x, z);
                    if (t == null) break;
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
