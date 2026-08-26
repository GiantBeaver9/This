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
            if (Target == null && PlayerController.Instance != null)
                Target = PlayerController.Instance.transform;
            if (Target == null) return;

            var player = PlayerController.Instance;

            // Wall the player to the reachable view BEFORE the camera reads their X,
            // so a gate lock (MaxX pinned to the gate) is an actual wall, not just a
            // frozen camera. Bounds are derived from the clamp range — not the lagged
            // camera position — so player and camera never deadlock each other.
            if (WallPlayerToView && player != null)
            {
                float leftWall = MinX - HalfView + EdgeMargin;
                float rightWall = MaxX + HalfView - EdgeMargin;
                if (rightWall < leftWall) rightWall = leftWall;
                player.WorldX = Mathf.Clamp(player.WorldX, leftWall, rightWall);
            }

            float px = player != null ? player.WorldX : Target.position.x;
            float x = Mathf.Clamp(px, MinX, MaxX);
            var p = transform.position;
            p.x = Mathf.Lerp(p.x, x, 1f - Mathf.Exp(-FollowLerp * Time.deltaTime));
            transform.position = p;
        }
    }
}
