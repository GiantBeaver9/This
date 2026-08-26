namespace ThisL
{
    /// <summary>
    /// One player's control surface. <see cref="PlayerController"/> reads ONLY through
    /// this, so a player can be driven by the keyboard (<see cref="KeyboardInput"/>) or a
    /// USB gamepad (<see cref="GamepadInput"/>) with identical game logic. Local co-op is
    /// just a second <see cref="PlayerController"/> with a second source.
    ///
    /// Axes are −1..1 (a digital keyboard returns exactly −1/0/1; an analog stick returns
    /// the deadzoned magnitude). Edge bools are TRUE only on the frame the control was
    /// freshly pressed. <see cref="Tick"/> is called once per frame BEFORE the getters are
    /// read, so analog sources can latch their "pressed this frame" edges.
    /// </summary>
    public interface IPlayerInput
    {
        // Held movement (left hand / left stick).
        float MoveX { get; }
        float MoveZ { get; }

        // Held attack aim (right hand arrows / right stick); PlayerController resolves
        // the dominant cardinal from these.
        float AimX { get; }
        float AimZ { get; }

        // Walk modifier (LeftAlt / trigger). Pads may hard-code false.
        bool WalkHeld { get; }

        // Edge-triggered actions ("pressed this frame").
        bool JumpDown { get; }
        bool DashDown { get; }
        bool FireDown { get; }
        bool PickupDown { get; }
        bool SpecialDown { get; }
        bool AttackDown { get; }   // any fresh aim press this frame (routes PressAttack)

        // Fresh MOVE presses this frame, for the double-tap-to-dash tracking.
        bool MoveLeftDown { get; }
        bool MoveRightDown { get; }
        bool MoveUpDown { get; }
        bool MoveDownDown { get; }

        /// <summary>Called once per frame before the getters are read (latches analog edges).</summary>
        void Tick();
    }
}
