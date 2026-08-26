using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Player 1's default control surface: the EXACT current keyboard bindings, so
    /// single-player feels byte-identical to before the co-op refactor.
    ///
    /// LEFT HAND = WASD move (double-tap or LeftShift to dash, LeftAlt to walk).
    /// RIGHT HAND = arrow keys for the 8-directional attacks. Space jump, E fire,
    /// F pickup, Q special. Uses the legacy <see cref="UnityEngine.Input"/> — still
    /// valid under Active Input Handling = "Both".
    /// </summary>
    public sealed class KeyboardInput : IPlayerInput
    {
        public float MoveX => (K(KeyCode.D) ? 1 : 0) - (K(KeyCode.A) ? 1 : 0);
        public float MoveZ => (K(KeyCode.W) ? 1 : 0) - (K(KeyCode.S) ? 1 : 0);
        public float AimX => (K(KeyCode.RightArrow) ? 1 : 0) - (K(KeyCode.LeftArrow) ? 1 : 0);
        public float AimZ => (K(KeyCode.UpArrow) ? 1 : 0) - (K(KeyCode.DownArrow) ? 1 : 0);
        public bool WalkHeld => K(KeyCode.LeftAlt);

        public bool JumpDown => KD(KeyCode.Space);
        public bool DashDown => KD(KeyCode.LeftShift);
        public bool FireDown => KD(KeyCode.E);
        public bool PickupDown => KD(KeyCode.F);
        public bool SpecialDown => KD(KeyCode.Q);
        public bool AttackDown => KD(KeyCode.LeftArrow) || KD(KeyCode.RightArrow)
                               || KD(KeyCode.UpArrow) || KD(KeyCode.DownArrow);

        public bool MoveLeftDown => KD(KeyCode.A);
        public bool MoveRightDown => KD(KeyCode.D);
        public bool MoveUpDown => KD(KeyCode.W);
        public bool MoveDownDown => KD(KeyCode.S);

        public void Tick() { }

        private static bool K(KeyCode k) => Input.GetKey(k);
        private static bool KD(KeyCode k) => Input.GetKeyDown(k);
    }
}
