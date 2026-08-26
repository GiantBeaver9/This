#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThisL
{
    /// <summary>
    /// A USB gamepad control surface (New Input System). Device-normalized, so an Xbox
    /// pad and a generic USB pad both "just work" with no per-pad axis authoring.
    ///
    /// TWIN-STICK scheme (mirrors the keyboard's WASD-move / arrows-attack split):
    ///   • LEFT STICK (+ D-pad) = move.
    ///   • RIGHT STICK = the 8-directional attack aim (flick to strike; the dominant
    ///     cardinal is resolved by <see cref="PlayerController"/> exactly like the arrows).
    ///   • A / South = jump, B / East = fire weapon, Y / North = pickup.
    ///   • Left shoulder = dash (double-tap the stick also dashes), Right shoulder = special.
    ///   • Left trigger held = walk.
    ///
    /// The class stores the resolved <see cref="Gamepad"/> device (not a live index) so a
    /// later hot-plug of another pad doesn't reassign this player.
    /// </summary>
    public sealed class GamepadInput : IPlayerInput
    {
        private const float MoveDeadzone = 0.35f;
        private const float AttackThreshold = 0.5f;  // right-stick magnitude that counts as an attack flick
        private const float MoveEdge = 0.6f;         // fresh directional push for the double-tap dash

        private readonly Gamepad _pad;

        // Latched edges (computed in Tick from analog state).
        private bool _attackDown;
        private bool _aimActive, _prevAimActive;
        private bool _left, _right, _up, _down;
        private bool _leftDown, _rightDown, _upDown, _downDown;

        public GamepadInput(int index)
        {
            var all = Gamepad.all;
            _pad = (index >= 0 && index < all.Count) ? all[index] : null;
        }

        private bool Live => _pad != null && _pad.added;

        private Vector2 LeftCombined()
        {
            if (!Live) return Vector2.zero;
            Vector2 ls = _pad.leftStick.ReadValue();
            Vector2 dp = _pad.dpad.ReadValue();
            float x = Mathf.Clamp(Deadzone(ls.x) + dp.x, -1f, 1f);
            float z = Mathf.Clamp(Deadzone(ls.y) + dp.y, -1f, 1f);
            return new Vector2(x, z);
        }

        private Vector2 RightStick()
        {
            if (!Live) return Vector2.zero;
            Vector2 rs = _pad.rightStick.ReadValue();
            return new Vector2(Deadzone(rs.x), Deadzone(rs.y));
        }

        public float MoveX => LeftCombined().x;
        public float MoveZ => LeftCombined().y;
        public float AimX => RightStick().x;
        public float AimZ => RightStick().y;
        public bool WalkHeld => Live && _pad.leftTrigger.ReadValue() > 0.5f;

        public bool JumpDown => Live && _pad.buttonSouth.wasPressedThisFrame;
        public bool FireDown => Live && _pad.buttonEast.wasPressedThisFrame;
        public bool PickupDown => Live && _pad.buttonNorth.wasPressedThisFrame;
        public bool DashDown => Live && _pad.leftShoulder.wasPressedThisFrame;
        public bool SpecialDown => Live && _pad.rightShoulder.wasPressedThisFrame;

        public bool AttackDown => _attackDown;
        public bool MoveLeftDown => _leftDown;
        public bool MoveRightDown => _rightDown;
        public bool MoveUpDown => _upDown;
        public bool MoveDownDown => _downDown;

        public void Tick()
        {
            // Right-stick attack: a fresh flick past the threshold = one attack press.
            Vector2 rs = RightStick();
            _prevAimActive = _aimActive;
            _aimActive = rs.magnitude >= AttackThreshold;
            _attackDown = _aimActive && !_prevAimActive;

            // Left-stick/D-pad directional edges for the double-tap dash.
            Vector2 lc = LeftCombined();
            bool l = lc.x <= -MoveEdge, r = lc.x >= MoveEdge;
            bool u = lc.y >= MoveEdge, d = lc.y <= -MoveEdge;
            _leftDown = l && !_left; _rightDown = r && !_right;
            _upDown = u && !_up; _downDown = d && !_down;
            _left = l; _right = r; _up = u; _down = d;
        }

        private static float Deadzone(float v) => Mathf.Abs(v) < MoveDeadzone ? 0f : v;
    }
}
#else
namespace ThisL
{
    /// <summary>
    /// Stub used when the New Input System backend is not enabled (Active Input Handling
    /// still "Input Manager (Old)"). Keeps the whole project compiling with zero gamepad
    /// support so the legacy keyboard path (single-player) is never at risk.
    /// </summary>
    public sealed class GamepadInput : IPlayerInput
    {
        public GamepadInput(int index) { }
        public float MoveX => 0f;
        public float MoveZ => 0f;
        public float AimX => 0f;
        public float AimZ => 0f;
        public bool WalkHeld => false;
        public bool JumpDown => false;
        public bool DashDown => false;
        public bool FireDown => false;
        public bool PickupDown => false;
        public bool SpecialDown => false;
        public bool AttackDown => false;
        public bool MoveLeftDown => false;
        public bool MoveRightDown => false;
        public bool MoveUpDown => false;
        public bool MoveDownDown => false;
        public void Tick() { }
    }
}
#endif
