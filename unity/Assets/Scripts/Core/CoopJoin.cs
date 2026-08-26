using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ThisL
{
    /// <summary>
    /// Watches for a second player joining local co-op: while a run is playing and only P1
    /// exists, pressing START on any connected gamepad spawns P2 on that pad (via
    /// <see cref="GameFlow.TryJoinPlayer2"/>). P1 stays on the keyboard, so single-player is
    /// never disturbed — the join only ever ADDS a controller-driven second fighter.
    ///
    /// Added to the world's "Systems" object by <see cref="GameFlow.BuildWorld"/>. Compiles
    /// to a harmless no-op watcher when the New Input System backend is not enabled.
    /// </summary>
    public sealed class CoopJoin : MonoBehaviour
    {
        private void Update()
        {
            var flow = GameFlow.Instance;
            if (flow == null || flow.Current != GameFlow.State.Playing) return;
            if (PlayerController.All.Count >= 2) return; // already 2 players

#if ENABLE_INPUT_SYSTEM
            var pads = Gamepad.all;
            for (int i = 0; i < pads.Count; i++)
            {
                var pad = pads[i];
                if (pad != null && pad.startButton.wasPressedThisFrame)
                {
                    flow.TryJoinPlayer2(i);
                    break;
                }
            }
#endif
        }
    }
}
