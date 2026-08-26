using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Orthographic follow camera framing the 640x360 / 24ppu playfield. Tracks
    /// the player's X with a soft lag and clamps to the stage bounds.
    ///
    /// The bounds double as the encounter GATE: EnemySpawner drops <see cref="MaxX"/>
    /// onto a gate's world-X to hard-lock scrolling until the wave is cleared, then
    /// raises it to let the player advance. <see cref="MinX"/> trails behind so the
    /// player can't backtrack past cleared ground. To make that lock felt (the
    /// player literally can't walk off the locked screen), the rig also WALLS the
    /// player to the currently reachable view — the classic beat-'em-up edge stop.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        public Transform Target;
        public float MinX = -3f;
        public float MaxX = 40f;
        public float FollowLerp = 6f;

        /// <summary>Wall the player to the reachable screen so a locked camera gates them (ENCOUNTERS-style).</summary>
        public bool WallPlayerToView = true;
        /// <summary>How far inside each visible edge the player is stopped (wu).</summary>
        public float EdgeMargin = 1f;

        private Camera _cam;

        // Half of the intended visible world width (matches EnemySpawner.EdgeX spawn edges).
        private static float HalfView => Tuning.ScreenWidthUnits * 0.5f;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = Playfield.CameraOrthoSize;
            _cam.backgroundColor = new Color(0.36f, 0.62f, 0.80f); // placeholder sky
            _cam.clearFlags = CameraClearFlags.SolidColor;
            transform.position = new Vector3(0f, 0f, -10f);
        }

        private void LateUpdate()
        {
            if (Target == null && PlayerController.Primary != null)
                Target = PlayerController.Primary.transform;

            var players = PlayerController.All;

            // Wall EACH living player to the reachable view BEFORE reading their X, so a
            // gate lock (MaxX pinned to the gate) is an actual wall for BOTH players, not
            // just a frozen camera. Bounds come from the clamp range, not the lagged camera
            // position, so players and camera never deadlock. (Single-player: identical to
            // the old behavior — one player, one wall.)
            float leftWall = MinX - HalfView + EdgeMargin;
            float rightWall = MaxX + HalfView - EdgeMargin;
            if (rightWall < leftWall) rightWall = leftWall;

            float minPX = float.MaxValue, maxPX = float.MinValue;
            for (int i = 0; i < players.Count; i++)
            {
                var pl = players[i];
                if (pl == null || !pl.Alive) continue;
                if (WallPlayerToView) pl.WorldX = Mathf.Clamp(pl.WorldX, leftWall, rightWall);
                if (pl.WorldX < minPX) minPX = pl.WorldX;
                if (pl.WorldX > maxPX) maxPX = pl.WorldX;
            }

            // Nobody alive (all downed): hold on the primary player if present, else the
            // last target — never snap.
            if (minPX == float.MaxValue)
            {
                var pr = PlayerController.Primary;
                if (pr != null) { minPX = maxPX = pr.WorldX; }
                else if (Target != null) { minPX = maxPX = Target.position.x; }
                else return;
            }

            // Follow the MIDPOINT of the living players, still clamped to the stage gate.
            float mid = (minPX + maxPX) * 0.5f;
            float x = Mathf.Clamp(mid, MinX, MaxX);
            var p = transform.position;
            p.x = Mathf.Lerp(p.x, x, 1f - Mathf.Exp(-FollowLerp * Time.deltaTime));
            transform.position = p;
        }
    }
}
