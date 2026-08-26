using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Places enemy PODS at invisible, spaced trigger points along the stage instead of
    /// letting them ride inside the wave flood (creator: "pods should be tied to invisible
    /// locations on each stage, not just flood after each wave dies — that way you can get
    /// a break"). As the player advances, each time they cross the next trigger threshold a
    /// single pod rises a little ahead and spits swarmers until destroyed. The generous
    /// spacing (one per ~<see cref="SpacingWu"/> of forward progress) leaves quiet stretches
    /// between pod encounters, and because a locked arena stops forward progress, pods only
    /// trigger out in the TRAVEL between arenas — a hazard you can fight or outrun, not a
    /// relentless in-arena swarm.
    /// </summary>
    public sealed class PodDirector : MonoBehaviour
    {
        /// <summary>World-units of NEW forward progress between pod drops (bigger = more breaks).</summary>
        public float SpacingWu = 250f;
        /// <summary>Don't drop the first pod until the player is this far into the run (past the opener).</summary>
        public float FirstAtWu = 140f;
        /// <summary>A pod this far BEHIND the player has been left behind → it despawns (the break).</summary>
        public float LeashBehindWu = 22f;

        private float _nextTriggerX = float.NaN;
        private readonly System.Collections.Generic.List<Pod> _pods = new();

        private void Update()
        {
            if (!PlayerController.AnyAlive) return;
            var p = PlayerController.Primary;
            if (p == null || !p.Alive) return;

            if (float.IsNaN(_nextTriggerX)) _nextTriggerX = p.WorldX + FirstAtWu;

            if (p.WorldX >= _nextTriggerX)
            {
                DropPodAhead(p);
                _nextTriggerX = p.WorldX + SpacingWu;
            }

            // Leash: a pod the player has walked well past is left behind and despawns, so pods never
            // pile up and you get your break (its swarmers are capped + will chase or be outrun).
            float cutoff = p.WorldX - LeashBehindWu;
            for (int i = _pods.Count - 1; i >= 0; i--)
            {
                var pod = _pods[i];
                if (pod == null) { _pods.RemoveAt(i); continue; }
                if (pod.WorldX < cutoff) { Destroy(pod.gameObject); _pods.RemoveAt(i); }
            }
        }

        private void DropPodAhead(PlayerController p)
        {
            float x = p.WorldX + Random.Range(4f, 8f);                       // just ahead
            float z = Random.Range(1f, Tuning.ZBandDepth - 1f);             // random depth row
            var pod = EnemySpawner.PlacePod(x, z);
            if (pod != null) _pods.Add(pod);
        }
    }
}
