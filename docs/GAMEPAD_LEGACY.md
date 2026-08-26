# Legacy Input Manager — 2-pad gamepad support

The New Input System package is **not present** in this project (its editor code hard-errors
on Unity 6000.5's deprecated TreeView APIs, and `activeInputHandler` is back to `0` = legacy
Input Manager only). So gamepads are driven entirely through the **legacy `UnityEngine.Input`**
API — no package required. This note records the approach so it can be re-tuned per controller.

## How legacy input addresses TWO controllers

Legacy input splits how it reaches a *specific* joystick by input kind:

- **Axes (sticks / triggers)** are read by **name**. To bind an axis to one physical pad you
  author an entry in `ProjectSettings/InputManager.asset` with the **`joyNum`** field set:
  `joyNum: 0` = "all joysticks combined", `joyNum: 1` = **Joystick 1**, `joyNum: 2` = **Joystick 2**.
  The `axis` field selects the hardware axis (0-indexed: `0`=1st axis, `1`=2nd, `3`=4th …).
  We therefore add per-pad, per-stick axes: `Joy1X`, `Joy2X`, etc. and read them with
  `Input.GetAxisRaw("Joy1X")`.
- **Buttons** are read **device-specifically by `KeyCode`**, no asset entry needed:
  `KeyCode.Joystick1Button0 … Joystick1Button19` = pad 1's buttons,
  `KeyCode.Joystick2Button0 …` = pad 2's. (`KeyCode.JoystickButton0` = *any* pad — we do NOT
  use that, so P1 and P2 stay isolated.) The enum is contiguous with a **stride of 20** per
  joystick, so `(KeyCode)((int)KeyCode.Joystick1Button0 + padIndex*20 + n)` = button `n` on pad
  `padIndex` (padIndex 0 → Joystick 1). Only Joystick 1..8 have dedicated KeyCodes.
- **Presence**: `Input.GetJoystickNames()` returns one string per slot; an **empty string**
  means that slot is currently unplugged. `GetJoystickNames()[i]` lines up with Joystick `i+1`
  (joyNum `i+1`, `Joystick{i+1}ButtonN`). We gate all reads on a non-empty name so a ghost /
  absent pad contributes nothing.

## Xbox / XInput default mapping (Windows)

Button (`KeyCode.Joystick{n}Button{m}`), the common XInput layout:

| Button # | Xbox | Our verb |
|---------:|------|----------|
| 0 | A / ✕ | **Jump** |
| 1 | B / ○ | **Dash** (always a plain dash, never Shield Rush — PLAYER.md §2) |
| 2 | X / □ | Fire fallback (see below) |
| 3 | Y / △ | **Special** |
| 4 | LB    | Walk fallback (see below) |
| 5 | RB    | **Pick-up (F)** |
| 6 | Back  | (unused) |
| 7 | Start | **Join P2 / pause** |
| 8 | LS click | (unused) |
| 9 | RS click | (unused) |

Axis (Windows XInput; `axis` = 0-indexed InputManager field):

| Hardware axis | `axis` | Role |
|---------------|:------:|------|
| Left stick X  | 0 | Move X |
| Left stick Y  | 1 | Move Z (**invert:1** so up = +1) |
| Right stick X | 3 | Aim X |
| Right stick Y | 4 | Aim Z (**invert:1** so up = +1) |
| Left trigger  | 8 | Walk (held) |
| Right trigger | 9 | Fire / use weapon `E` (edge past threshold) |

This matches PLAYER.md §2 [LOCKED]: left stick = 8-dir move, right stick = 8-dir attack,
A = Jump, B = Dash, Y = Special, **E = right trigger**, **F = right bumper**. Walk (the keyboard
`LeftAlt` modifier) is placed on the left trigger — not locked by design, chosen for symmetry.

## Axes added to InputManager.asset (12 entries)

All are `type: 2` (joystick axis), `gravity: 0`, `dead: 0.001` (dead-zoning is done in code),
`sensitivity: 1`, `snap: 0`. Only `m_Name`, `axis`, `invert`, `joyNum` differ:

| m_Name  | axis | invert | joyNum |
|---------|:----:|:------:|:------:|
| Joy1X   | 0 | 0 | 1 |
| Joy1Y   | 1 | 1 | 1 |
| Joy1RX  | 3 | 0 | 1 |
| Joy1RY  | 4 | 1 | 1 |
| Joy1LT  | 8 | 0 | 1 |
| Joy1RT  | 9 | 0 | 1 |
| Joy2X   | 0 | 0 | 2 |
| Joy2Y   | 1 | 1 | 2 |
| Joy2RX  | 3 | 0 | 2 |
| Joy2RY  | 4 | 1 | 2 |
| Joy2LT  | 8 | 0 | 2 |
| Joy2RT  | 9 | 0 | 2 |

## Joining / assigning players

- **P1** defaults to the **keyboard** (`PlayerController` seeds `new KeyboardInput()`), so
  single-player is byte-identical to before.
- **P2 join** (`CoopJoin`): while a run is `Playing` and only one player exists, pressing
  **Start** on any connected, un-claimed pad calls `GameFlow.TryJoinPlayer2(padIndex)`, which
  builds `new GamepadInput(padIndex)` for P2. Common case: P1 keyboard + one USB pad → that pad
  is Joystick 1 (index 0), so P2 lands on index 0.
- **Pad claiming**: every `GamepadInput` registers its index in a static claim set on
  construction; `CoopJoin` skips already-claimed pads and clears the set whenever the game is not
  `Playing` (returning to title resets it). This is what makes true 2-pad play work.
- **Putting P1 on a controller too** (2-pad, no keyboard): there is no in-game "P1 switches to
  pad" control (that would require editing `PlayerController`/`GameFlow`, which this task does
  not own). Do it with a one-line change where P1 is spawned, e.g. right after the P1
  `PlayerController` is created in `GameFlow.BuildWorld`:
  `p1.SetInput(new ThisL.GamepadInput(0));`
  That claims pad 0 for P1; `CoopJoin` then routes P2's Start to pad 1 automatically.

## Caveats the orchestrator should know

- **Controller layout variance is real.** The axis indices above are the *Windows XInput*
  defaults. DirectInput pads, DualShock, and non-Windows platforms shuffle axis indices (esp.
  the triggers — some expose only a **shared** 3rd axis for LT+RT, some rest triggers at `-1`
  instead of `0`). All button numbers and axis indices live in **one `const` block** at the top
  of `GamepadInput.cs` for quick re-tuning; the axis *names* are fixed but their `axis`/`joyNum`
  can be edited in `InputManager.asset`.
- **Trigger robustness.** Because trigger axes are the flakiest, **Fire** also accepts the
  **X button (2)** and **Walk** also accepts the **LB button (4)** as fallbacks, so a pad whose
  triggers don't register on axes 8/9 can still fire and walk.
- **InputManager.asset is YAML** — the 12 new entries were appended in the exact existing
  `- serializedVersion: 3 / m_Axis:`-style block format with no change to any existing entry.
  **Verify it still parses** (Unity will silently drop all axes if the file is malformed).
