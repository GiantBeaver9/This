using System.Collections.Generic;
using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// A USB gamepad control surface built on the LEGACY <see cref="UnityEngine.Input"/>
    /// (the New Input System package is absent in this project). One instance = one physical
    /// pad, selected by <paramref name="index"/> (0 = Joystick 1, 1 = Joystick 2). See
    /// <c>docs/GAMEPAD_LEGACY.md</c> for the axis-name / joyNum wiring and the button map.
    ///
    /// TWIN-STICK scheme (mirrors the keyboard's WASD-move / arrows-attack split, PLAYER.md §2):
    ///   • LEFT STICK = move.
    ///   • RIGHT STICK = the 8-directional attack aim (flick to strike; the dominant cardinal
    ///     is resolved by <see cref="PlayerController"/> exactly like the arrow keys).
    ///   • A / South (button 0) = jump · B / East (1) = dash · Y / North (3) = special.
    ///   • Right trigger = fire/use weapon (E) · Right bumper (5) = pickup (F).
    ///   • Left trigger held = walk. Start (7) = join / pause (handled by <see cref="CoopJoin"/>).
    ///
    /// Axis reads go through per-pad axes authored in InputManager.asset ("Joy1X"/"Joy2X"…);
    /// buttons go through pad-specific <see cref="KeyCode"/>s so P1 and P2 never cross-talk.
    /// </summary>
    public sealed class GamepadInput : IPlayerInput
    {
        // ---- Tunables (see docs/GAMEPAD_LEGACY.md) -----------------------------------------
        private const float MoveDeadzone   = 0.35f; // stick component below this reads as 0
        private const float AttackThreshold = 0.5f; // right-stick magnitude that counts as an attack flick
        private const float MoveEdge       = 0.6f;  // fresh directional push for the double-tap dash
        private const float TriggerPress   = 0.5f;  // trigger pull that counts as "pressed"

        // ---- Xbox / XInput button numbers (KeyCode.Joystick{n}Button{m}) --------------------
        private const int BtnJump    = 0; // A
        private const int BtnDash    = 1; // B  (always a plain dash, never Shield Rush)
        private const int BtnFireAlt = 2; // X  (fallback fire, in case the right trigger axis is dead)
        private const int BtnSpecial = 3; // Y
        private const int BtnWalkAlt = 4; // LB (fallback walk)
        private const int BtnPickup  = 5; // RB
        // Button 7 (Start) is read by CoopJoin, not here.

        // ---- Static pad-claim registry (so CoopJoin knows which pads are taken) -------------
        private static readonly HashSet<int> _claimed = new();
        /// <summary>True if some player already drives this pad slot.</summary>
        public static bool IsClaimed(int index) => _claimed.Contains(index);
        /// <summary>Release all pad claims (called when a run ends / returns to menu).</summary>
        public static void ClearClaims() => _claimed.Clear();

        // ---- Instance --------------------------------------------------------------------------
        private readonly int _index;             // 0 or 1
        private readonly int _joy;               // 1 or 2 (joyNum + KeyCode block)
        private readonly string _ax, _ay, _arx, _ary, _alt, _art; // cached axis names
        private readonly KeyCode _btnBase;       // KeyCode.Joystick{joy}Button0

        // Latched edges (computed in Tick from analog state).
        private bool _attackDown;
        private bool _aimActive, _prevAimActive;
        private bool _left, _right, _up, _down;
        private bool _leftDown, _rightDown, _upDown, _downDown;
        private bool _fireDown, _fireHeld;

        /// <param name="claim">True (P2) marks the pad slot taken so CoopJoin won't reuse it.
        /// P1's soft pad passes false so a single controller can still be handed to P2 via Start.</param>
        public GamepadInput(int index, bool claim = true)
        {
            _index = index < 0 ? 0 : index;
            _joy = _index + 1;
            _ax  = "Joy" + _joy + "X";
            _ay  = "Joy" + _joy + "Y";
            _arx = "Joy" + _joy + "RX";
            _ary = "Joy" + _joy + "RY";
            _alt = "Joy" + _joy + "LT";
            _art = "Joy" + _joy + "RT";
            _btnBase = (KeyCode)((int)KeyCode.Joystick1Button0 + _index * 20);
            if (claim) _claimed.Add(_index);
        }

        /// <summary>The pad slot this surface drives (0 = Joystick 1).</summary>
        public int PadIndex => _index;

        /// <summary>The physical pad slot is currently plugged in.</summary>
        private bool Live
        {
            get
            {
                string[] n = Input.GetJoystickNames();
                return _index >= 0 && _index < n.Length && !string.IsNullOrEmpty(n[_index]);
            }
        }

        // ---- Held axes -------------------------------------------------------------------------
        public float MoveX => Live ? Dead(Axis(_ax)) : 0f;
        public float MoveZ => Live ? Dead(Axis(_ay)) : 0f;
        public float AimX  => Live ? Dead(Axis(_arx)) : 0f;
        public float AimZ  => Live ? Dead(Axis(_ary)) : 0f;

        // Walk = left trigger held (or LB as a fallback for pads without working trigger axes).
        public bool WalkHeld => Live && (Axis(_alt) > TriggerPress || Btn(BtnWalkAlt));

        // ---- Edge-triggered buttons ("pressed this frame") ------------------------------------
        public bool JumpDown    => Live && BtnDown(BtnJump);
        public bool DashDown    => Live && BtnDown(BtnDash);
        public bool PickupDown  => Live && BtnDown(BtnPickup);
        public bool SpecialDown => Live && BtnDown(BtnSpecial);

        // Fire = right trigger crossing its threshold this frame (latched in Tick), OR a fresh X press.
        public bool FireDown => _fireDown || (Live && BtnDown(BtnFireAlt));

        public bool AttackDown   => _attackDown;
        public bool MoveLeftDown  => _leftDown;
        public bool MoveRightDown => _rightDown;
        public bool MoveUpDown    => _upDown;
        public bool MoveDownDown  => _downDown;

        public void Tick()
        {
            if (!Live)
            {
                _attackDown = _fireDown = false;
                _leftDown = _rightDown = _upDown = _downDown = false;
                _aimActive = _left = _right = _up = _down = _fireHeld = false;
                return;
            }

            // Right-stick attack: a fresh flick past the threshold = one attack press.
            float rx = Dead(Axis(_arx));
            float ry = Dead(Axis(_ary));
            _prevAimActive = _aimActive;
            _aimActive = Mathf.Sqrt(rx * rx + ry * ry) >= AttackThreshold;
            _attackDown = _aimActive && !_prevAimActive;

            // Left-stick directional edges for the double-tap dash.
            float lx = Dead(Axis(_ax));
            float lz = Dead(Axis(_ay));
            bool l = lx <= -MoveEdge, r = lx >= MoveEdge;
            bool u = lz >= MoveEdge, d = lz <= -MoveEdge;
            _leftDown = l && !_left; _rightDown = r && !_right;
            _upDown = u && !_up; _downDown = d && !_down;
            _left = l; _right = r; _up = u; _down = d;

            // Right-trigger fire edge.
            bool fire = Axis(_art) > TriggerPress;
            _fireDown = fire && !_fireHeld;
            _fireHeld = fire;
        }

        // ---- Helpers ---------------------------------------------------------------------------
        private static float Dead(float v) => Mathf.Abs(v) < MoveDeadzone ? 0f : v;
        private static float Axis(string name) => Input.GetAxisRaw(name);
        private bool Btn(int n) => Input.GetKey(_btnBase + n);
        private bool BtnDown(int n) => Input.GetKeyDown(_btnBase + n);
    }
}
