using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Watches for a second player joining local co-op: while a run is playing and only P1
    /// exists, pressing START (button 7) on any connected, un-claimed gamepad spawns P2 on that
    /// pad (via <see cref="GameFlow.TryJoinPlayer2"/>). P1 stays on the keyboard by default, so
    /// single-player is never disturbed — the join only ADDS a controller-driven second fighter.
    /// If P1 has been switched onto a pad, that pad is "claimed" (see <see cref="GamepadInput"/>)
    /// and skipped here, so P2 lands on the other controller.
    ///
    /// Legacy Input Manager only (no New Input System). Added to the world's "Systems" object by
    /// <see cref="GameFlow.BuildWorld"/>. See <c>docs/GAMEPAD_LEGACY.md</c>.
    /// </summary>
    public sealed class CoopJoin : MonoBehaviour
    {
        private const int StartButton = 7;         // Xbox "Start"
        private const int MaxJoysticks = 8;        // KeyCodes exist for Joystick 1..8 only

        private void Update()
        {
            var flow = GameFlow.Instance;
            if (flow == null || flow.Current != GameFlow.State.Playing)
            {
                // Not in a run: drop any stale pad claims so the next run starts fresh.
                GamepadInput.ClearClaims();
                return;
            }
            if (PlayerController.All.Count >= 2) return; // already 2 players

            string[] names = Input.GetJoystickNames();
            int count = Mathf.Min(names.Length, MaxJoysticks);
            for (int i = 0; i < count; i++)
            {
                if (string.IsNullOrEmpty(names[i])) continue; // no pad in this slot
                if (GamepadInput.IsClaimed(i)) continue;      // already drives a player (e.g. P1)

                var startKey = (KeyCode)((int)KeyCode.Joystick1Button0 + i * 20 + StartButton);
                if (Input.GetKeyDown(startKey))
                {
                    flow.TryJoinPlayer2(i);
                    break;
                }
            }
        }
    }
}
