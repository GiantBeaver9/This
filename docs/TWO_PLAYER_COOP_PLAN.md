# Local 2-Player Co-op — Implementation Plan

**Game:** `this.l` — Unity 6 (6000.5.9f1), code-first 2.5D beat-'em-up, project root `unity/`.
**Goal:** Two people, side-by-side, on two USB gamepads (classic beat-'em-up co-op), one shared camera. Eventually a shared **life counter** (game is very hard → co-op + lives make it survivable).
**Scope of this doc:** investigation + concrete, file-specific plan. No gameplay code was changed writing it — the only file written is this doc.

---

## 1. Current single-player architecture (what a 2P build has to break)

### 1.1 Input — 100% legacy Input Manager, keyboard-only, static
`PlayerController` (`unity/Assets/Scripts/Actors/PlayerController.cs`) reads input **directly** from the legacy `UnityEngine.Input` through two static helpers at the very bottom of the file:

```csharp
private static bool Key(KeyCode k)     => Input.GetKey(k);      // line 1087
private static bool KeyDown(KeyCode k) => Input.GetKeyDown(k);  // line 1088
```

Every control funnels through those:
- **Move:** `MoveX()`/`MoveZ()` — `Key(KeyCode.A/D/W/S)` (lines 196–197), consumed in `Move()` and in `UpdateAnimation()` (line 1001).
- **Dash:** `ReadDashTaps()` double-taps A/D/W/S + `KeyCode.LeftShift` (lines 218–225); Shield-Rush "hold forward" re-reads `Key(KeyCode.D/A)` (line 420).
- **Walk modifier:** `Key(KeyCode.LeftAlt)` (line 212).
- **Attacks:** `HandleActionInput()` reads arrow keys, `ResolveAttackDir()` reads them again (lines 504–521).
- **Actions:** Space=jump, E=fire weapon, F=pickup, Q=special (lines 498–501).
- **Debug:** I/O/K (lines 161–167) + an `OnGUI` legend/GOD-MODE banner (lines 1068–1085).

Because `Key`/`KeyDown` are **static and parameterless**, there is exactly one implicit input source. This is the single biggest structural change for 2P: input must become an **injected, per-instance source**.

### 1.2 The singleton — `PlayerController.Instance`
```csharp
public static PlayerController Instance { get; private set; }   // line 26
Awake()      → Instance = this;                                 // line 115
OnDestroy()  → if (Instance == this) Instance = null;           // line 142
```
This is assumed by ~18 systems (full enumeration in §5). Second player = second `PlayerController`, so `Instance` becomes ambiguous. This is the second structural change.

Note: the **special meter is already per-instance** (`public readonly SpecialMeter Meter` line 29) and `Character`/`CurrentWeapon` are instance fields — so most *per-player state* is already isolated. The blockers are purely (a) the static input and (b) the static `Instance`.

### 1.3 Camera — follows `Instance`, walls `Instance`
`unity/Assets/Scripts/World/CameraRig.cs`:
- Target auto-binds to `PlayerController.Instance.transform` (lines 45–46).
- `LateUpdate` (line 43) reads `PlayerController.Instance` and **walls the player** to the reachable view (`player.WorldX = Clamp(...)`, lines 55–61) *before* moving the camera toward `player.WorldX` (lines 63–66).
- Clamp range is `[MinX, MaxX]`; `HalfView = Tuning.ScreenWidthUnits * 0.5f`. **`MaxX` is the encounter gate** — `StageDirector`/`EnemySpawner` pin `MaxX` to a gate's world-X to lock scrolling until the wave clears.

For 2P this must frame the **midpoint** of both players and wall **both** to the shared view.

### 1.4 Flow / spawn — one player, hard-coded
`unity/Assets/Scripts/Core/GameFlow.cs`:
- States `Title → CharacterSelect → Playing`; `_selected` is a single `CharacterDef`.
- `BuildWorld()` (line 51) creates **one** `GameObject("Player")`, adds `SpriteRenderer`+`SpriteAnimator`+`PlayerController`, calls `Configure(_selected)`, sets `WorldX=0, Z=2.5`, `Init()` (lines 56–65).
- `CharacterSelectGUI` picks one def and calls `StartRun(c)` (lines 215–219).
- Roster: `CharacterDef.Roster()` → 4 chars (`unity/Assets/Scripts/Core/Characters.cs`).

### 1.5 HUD — draws `Instance` only
`unity/Assets/Scripts/UI/Hud.cs` `OnGUI` reads `PlayerController.Instance`, draws HP bar + special meter top-left, and "YOU DIED" when `!p.Alive`. Single fixed layout.

### 1.6 Enemies / stage — all target `Instance`
- `Actor` (`unity/Assets/Scripts/Actors/Actor.cs`): base with `static List<Actor> All`, `Team`, `WorldX/Z`, `TakeDamage`. Enemies are `Team.Enemy`, players `Team.Player`.
- **Every enemy AI targets `PlayerController.Instance` directly:** `EnemyController.cs:61`, `RangedEnemyController.cs:40`, `NinjaController.cs:61`, `SnapperController.cs:56`, `AntiAircraftController.cs:46`, `BossController.cs:123/268/276`. None pick "nearest of N".
- **Stage clear does NOT assume one player** — good news: `StageDirector.CountLiveEnemies()`/`CountPursuers()` (lines 374–395) and `EnemySpawner` equivalents (lines 266–294) watch `Actor.All` filtered by `Team.Enemy`. Field-clear gating is player-count-agnostic already.
- **Stage origin/anchor DOES assume one player:** `StageDirector` reads `PlayerController.Instance.WorldX` for `_originX` (lines 67, 315) and the spawn anchor (line 353); `EnemySpawner` for `_originX`/leash/spawn edges (lines 105, 132, 297) and gates advancing off `player.WorldX` (line 154).

### 1.7 Package state — legacy Input Manager ONLY
`unity/Packages/manifest.json` has **no `com.unity.inputsystem`**. The new Input System is *not* installed; the project is on the legacy Input Manager. This is the pivotal fact for §2.

---

## 2. Input approach — RECOMMENDATION (demo is tomorrow afternoon)

Two viable paths:

### Option A — Legacy Input Manager multi-joystick (no package change)
- Buttons are free: `KeyCode.Joystick1Button0…19` / `Joystick2Button0…` work with **zero setup** via the existing `Input.GetKey`.
- **Analog sticks are the problem:** `Input.GetAxis("...")` needs named axis entries in `ProjectSettings/InputManager.asset`, and to separate pad 1 from pad 2 you must duplicate each axis bound to "Joystick 1" vs "Joystick 2" (`Joy1 Horizontal`, `Joy2 Horizontal`, …). That file in a code-first project is minimal/default; you'd hand-author several axis entries.
- Per-device quirk: which physical button/stick maps to which `JoystickNButtonM` index and which axis number **varies by USB pad model** (Xbox vs DualShock vs generic) — exactly the "grab any two controllers" case the creator wants. High chance of a "why won't P2 move" moment on unknown hardware.
- **Pros:** no package, no editor restart, no project-setting change, smoke test/CI untouched. **Cons:** fiddly axis authoring, brittle across unknown pads, the worst-case demo failure mode.

### Option B — New Input System package (`com.unity.inputsystem`)  ★ RECOMMENDED
- `Gamepad.all[0]` / `Gamepad.all[1]` index the two pads directly; `.leftStick.ReadValue()`, `.buttonSouth.wasPressedThisFrame`, `.dpad`, `.rightStick` etc. are **device-normalized** — an Xbox pad and a generic USB pad both "just work" with no per-device axis authoring. This is the robust path for "let people rip on it with whatever controllers."
- **Setup cost (one-time, ~30 min):**
  1. Add `"com.unity.inputsystem": "1.11.2"` (or the 6000.5-bundled version) to `unity/Packages/manifest.json`; let the editor resolve.
  2. **Project Settings → Player → Active Input Handling = "Both"** (writes `activeInputHandler: 2` in `ProjectSettings/ProjectSettings.asset`). Requires an **editor restart**.
  3. `using UnityEngine.InputSystem;` in the new input classes.
- **Why "Both" de-risks it:** with Active Input Handling = **Both**, every existing `Input.GetKey(KeyCode.*)` call — the entire keyboard path, the IMGUI menus in `GameFlow`/`Hud`, the debug keys — keeps working **untouched**. We only *add* a gamepad source; we don't rip out the legacy source. So P1-on-keyboard is byte-for-byte the current behavior, and the blast radius of the package add is near zero.
- **Pros:** trivial, robust multi-pad; device-agnostic; keyboard code path unchanged under "Both". **Cons:** package add + one project-setting flip + one editor restart; the smoke test build must still compile (it will — legacy code is untouched under "Both").

### Recommendation
**Go with Option B (New Input System, Active Input Handling = "Both").** For a same-day demo where two people grab two arbitrary USB pads, device-agnostic mapping is the difference between "it works" and "P2's stick is on axis 6 on this pad." The setup is a well-trodden 30-minute operation and, in **Both** mode, cannot break the existing keyboard/IMGUI path. Keep Option A documented as the fallback if the package fails to resolve on the demo machine (in that case: buttons via `KeyCode.JoystickNButtonM`, and author `Joy1/Joy2 Horizontal|Vertical` axes in `InputManager.asset`).

> One caveat to verify at setup time: the project's IMGUI menus use `Event.current`/`Input.GetKeyDown` and `Texture2D.whiteTexture`; under "Both" these are unaffected. If the team ever moves to Active Input Handling = "Input System (New)" **only**, the legacy `Input.GetKey`/`Input.GetKeyDown`/`Input.mousePosition` calls throughout `GameFlow`, `Hud`, and the debug keys would throw — so **do not** pick "New only". Stay on "Both".

### 2.1 Device-flexibility (creator requirement: "the game everyone plays")
The creator wants **any** of these to work: (a) P1 keyboard + P2 controller, (b) both on controllers, (c) either combo. This is a *device-assignment* concern, and it's exactly why the `IPlayerInput` abstraction (§3) matters — the player logic doesn't care what a source is, only that it produces Move/Attack/etc.

**Which approach makes each combo easiest:**

| Combo | New Input System (Option B) | Legacy (Option A) |
|-------|------------------------------|-------------------|
| **(a) keyboard + 1 pad** | Trivial: P1 = `KeyboardInput` (or a keyboard source), P2 = `GamepadInput(0)`. `Keyboard.current` and `Gamepad.all[0]` are independent devices — no conflict. | **Easiest case for legacy too:** P1 keyboard via current `Input.GetKey`, P2 via `KeyCode.Joystick1Button*` + a couple of `Joy1` axes. Only ONE joystick to configure, so the fiddly axis-authoring is minimal. |
| **(b) both on pads** | Trivial: `GamepadInput(0)` + `GamepadInput(1)`; device-normalized, no per-pad axis authoring. | **The painful case:** need `Joy1 *` **and** `Joy2 *` axis sets in `InputManager.asset`, and per-device button/axis indices differ across USB pad models. |
| **(c) either / auto** | A tiny `CompositeInput` (keyboard OR pad-0, whichever moved last) makes P1 "just work" with whatever's plugged in; P2 claims the next free pad on join. | Doable but manual — you hard-code which device is P1 vs P2. |

**Conclusion:** Option B (New Input System) is the clear winner for the flexibility the creator wants — it makes **all three combos** near-free because keyboard and each gamepad are independent devices addressed the same way through `IPlayerInput`. Legacy is only comparably easy for combo (a) (keyboard + **one** pad); combo (b) "two arbitrary USB pads" is legacy's worst case. **This flexibility requirement strengthens the Option B recommendation.**

**MVP fallback — CONTROLLER-ONLY is acceptable.** If the timeline is tight, ship **both players on USB gamepads** as the MVP (`GamepadInput(0)` + `GamepadInput(1)`) and treat keyboard-P1 as a fast follow. Because `KeyboardInput` is already written as the P1 default (§3.2), combo (a) essentially falls out for free once the gamepad path works — so "controller-only" is really just "don't spend demo time polishing the keyboard+pad device-assignment UX," not a code amputation. Call this out to the creator: **2-pad co-op is the committed MVP; keyboard-P1 + pad-P2 is a near-free extension on the same abstraction.**

### 2.2 Timeline note
A 2-pad MVP is roughly a full focused day (§9). It's **plausibly demo-ready tomorrow afternoon but tight**; a **Thursday target is the safer commitment** if the input refactor (the riskiest piece) needs breathing room. Either way the plan below is concrete enough to execute step-by-step.

---

## 3. Per-player input refactor (the abstraction)

Introduce an input interface so `PlayerController` reads from an **injected source** instead of static `Input`.

### 3.1 New file — `unity/Assets/Scripts/Actors/IPlayerInput.cs`
```csharp
namespace ThisL
{
    /// One player's control surface. PlayerController reads ONLY through this.
    public interface IPlayerInput
    {
        // Held axes, -1..1 (digital keyboard returns -1/0/1)
        float MoveX { get; }
        float MoveZ { get; }
        // Attack aim as held cardinals (digital); PlayerController resolves the dominant cardinal
        float AimX { get; }     // arrows / right stick / dpad, -1..1
        float AimZ { get; }
        bool  WalkHeld { get; } // LeftAlt / trigger — optional, can hard-code false for pads

        // Edge-triggered "pressed this frame"
        bool JumpDown { get; }      // Space / South
        bool DashDown { get; }      // LeftShift / West or shoulder
        bool FireDown { get; }      // E / East
        bool PickupDown { get; }    // F / North
        bool SpecialDown { get; }   // Q / trigger or shoulder
        bool AttackDown { get; }    // any fresh aim press this frame (for PressAttack routing)

        // Per-frame "aim newly pressed" for the 4 arrow directions is folded into AttackDown+AimX/AimZ.
        void Tick();  // called once/frame if the source needs to latch wasPressedThisFrame
    }
}
```

### 3.2 New file — `unity/Assets/Scripts/Actors/KeyboardInput.cs`
Wraps the **exact current bindings** so P1-on-keyboard is unchanged:
```csharp
MoveX => (Key(D)?1:0)-(Key(A)?1:0);   MoveZ => (Key(W)?1:0)-(Key(S)?1:0);
AimX  => (Key(RightArrow)?1:0)-(Key(LeftArrow)?1:0);  AimZ => (Key(UpArrow)?1:0)-(Key(DownArrow)?1:0);
WalkHeld => Key(LeftAlt);
JumpDown => KeyDown(Space);  FireDown => KeyDown(E);  PickupDown => KeyDown(F);
SpecialDown => KeyDown(Q);   DashDown => KeyDown(LeftShift);
AttackDown => KeyDown(Left|Right|Up|DownArrow);
// uses UnityEngine.Input.GetKey/GetKeyDown internally — legacy path, still valid under "Both"
```
Also keep the **double-tap-WASD** dash: `ReadDashTaps()` logic stays in `PlayerController` but reads `input.MoveX/MoveZ` edge changes, OR — simpler — expose `KeyboardInput.TapX/TapZ` edges. Cleanest: keep the double-tap tracking in `PlayerController` but feed it `bool leftTapped/rightTapped/...` from the source. Minimal change: keep `ReadDashTaps` but replace `KeyDown(KeyCode.A)` with `_input.MoveLeftDown` etc. (add 4 edge bools to the interface) — see §4.

### 3.3 New file — `unity/Assets/Scripts/Actors/GamepadInput.cs`
```csharp
using UnityEngine.InputSystem;
public sealed class GamepadInput : IPlayerInput
{
    private readonly int _index;   // 0 or 1
    public GamepadInput(int index) => _index = index;
    private Gamepad Pad => (_index >= 0 && _index < Gamepad.all.Count) ? Gamepad.all[_index] : null;

    public float MoveX => Pad == null ? 0 : Deadzone(Pad.leftStick.x.ReadValue()) + Dpad(Pad.dpad.x);
    public float MoveZ => Pad == null ? 0 : Deadzone(Pad.leftStick.y.ReadValue()) + Dpad(Pad.dpad.y);
    public float AimX  => Pad == null ? 0 : Sign(Pad.rightStick.x.ReadValue());   // or reuse leftStick + face button scheme
    public float AimZ  => Pad == null ? 0 : Sign(Pad.rightStick.y.ReadValue());
    public bool JumpDown    => Pad?.buttonSouth.wasPressedThisFrame ?? false;   // A
    public bool FireDown    => Pad?.buttonEast.wasPressedThisFrame  ?? false;   // B
    public bool PickupDown  => Pad?.buttonNorth.wasPressedThisFrame ?? false;   // Y
    public bool DashDown    => Pad?.leftShoulder.wasPressedThisFrame?? false;   // LB
    public bool SpecialDown => Pad?.rightShoulder.wasPressedThisFrame?? false;  // RB (or rightTrigger)
    public bool AttackDown  => /* right-stick flicked or face button X */ ...;
}
```
**Control-scheme decision to confirm with the creator:** the keyboard uses two-handed WASD-move + arrows-attack. On a pad the natural mapping is **left stick = move, face buttons = the 4 attacks** (X=side, Y=up, B=down, or a single "attack" button + stick aim), **A=jump, shoulder=dash, trigger=special, other face=pickup/fire**. Right-stick-as-attack-aim is an alternative. Pick one for the demo; the interface supports either.

### 3.4 Exact edits to `PlayerController.cs`
1. **Add an injected field + setter:**
   `private IPlayerInput _input;`  and  `public void SetInput(IPlayerInput src) => _input = src;` (call from `GameFlow.BuildWorld` right after `Configure`).
   Default in `Awake`: `_input ??= new KeyboardInput();` so nothing breaks if unset.
2. **Delete/redirect the static helpers** (lines 1087–1088). Replace all `Key(KeyCode.X)`/`KeyDown(KeyCode.X)` gameplay reads with `_input.*`:
   - `MoveX()/MoveZ()` (196–197) → `_input.MoveX/_input.MoveZ` (make them instance, not static).
   - `Move()` walk modifier (212) → `_input.WalkHeld`.
   - `ReadDashTaps()` (218–225) → `_input.MoveLeftDown/RightDown/UpDown/DownDown` + `_input.DashDown`.
   - Shield-Rush hold-forward (420) → `_input.MoveX` sign.
   - `HandleActionInput()` (498–507) → `_input.JumpDown/FireDown/PickupDown/SpecialDown` + `_input.AttackDown`.
   - `ResolveAttackDir()` (514–521) → `_input.AimX/_input.AimZ`.
   - `UpdateAnimation()` (1001) → `_input.MoveX/MoveZ`.
3. **Debug keys** (161–167) and the **debug `OnGUI`** (1068–1085): leave on keyboard (`Input.GetKeyDown(KeyCode.I/O/K)` directly) — they are dev-only and P1-convenience; fine to keep global. `GodMode` is `static` and already applies to all players via the `if (GodMode)` block — acceptable.
4. Keep `Instance` writes for now but **also register into `All`** (see §5) — do them both in `Awake`/`OnDestroy`.

Result: P1 defaults to `KeyboardInput` (unchanged behavior) OR gets `GamepadInput(0)`; P2 gets `GamepadInput(1)`. "Keep P1 on keyboard-or-pad-1": in `GameFlow`, if `Gamepad.all.Count >= 1` give P1 `GamepadInput(0)`, else `KeyboardInput`. A nice-to-have "hybrid" P1 source (keyboard OR pad-0, whichever is active) is a trivial `CompositeInput` wrapping both — optional.

---

## 4. Killing the singleton assumption (support N players)

### 4.1 Add a roster to `PlayerController`
```csharp
public static readonly List<PlayerController> All = new();          // add
public static PlayerController Instance => All.Count > 0 ? All[0] : null;  // keep as alias = P1 (Primary)

Awake():     if (!All.Contains(this)) All.Add(this);   // replace "Instance = this"
OnDestroy(): All.Remove(this);                          // replace the guarded null-out
```
Add helpers:
```csharp
public static PlayerController Primary => All.Count > 0 ? All[0] : null;
public static PlayerController Nearest(float worldX, float z) { /* min dist over All where Alive */ }
public static bool AnyAlive { get { foreach (var p in All) if (p != null && p.Alive) return true; return false; } }
```
Keeping `Instance` as an **alias for Primary** means every reader below still compiles; we then upgrade the ones that need true multi-target behavior.

### 4.2 Every `PlayerController.Instance` reader — what it should do with 2 players

| # | File:line | Current use | 2-player action |
|---|-----------|-------------|-----------------|
| 1 | `Actors/EnemyController.cs:61` | melee AI target | **`Nearest(WorldX, Z)`** — enemies target closest player |
| 2 | `Actors/RangedEnemyController.cs:40` | gunner aim target | **`Nearest`** |
| 3 | `Actors/NinjaController.cs:61` | ninja target | **`Nearest`** |
| 4 | `Actors/SnapperController.cs:56` | snapper target | **`Nearest`** |
| 5 | `Actors/AntiAircraftController.cs:46` | AA target | **`Nearest`** (or target the airborne player) |
| 6 | `Bosses/BossController.cs:123` | face the player | **`Nearest`** |
| 7 | `Bosses/BossController.cs:268,276` | boss attack aim | **`Nearest`** (pass into `BossUpdate(dt, player)`; each boss already takes `player`) |
| 8 | `World/CameraRig.cs:45,46,49` | follow + wall | **midpoint of `All`; wall EACH player** (see §6) |
| 9 | `World/EnemySpawner.cs:105` | stage `_originX` | **`Primary.WorldX`** (P1 defines the lane origin) |
| 10 | `World/EnemySpawner.cs:132` | `player` for tick/spawn edges | use **`Primary`** for lane logic; **`AnyAlive`** for the alive-gate; spawn edges off `Primary` or midpoint |
| 11 | `World/EnemySpawner.cs:297 EdgeX` | spawn just off-screen | anchor to **midpoint** (so enemies enter the shared frame) |
| 12 | `World/HazardDirector.cs:28` | hazard placement/target | **`Nearest`** (or place relative to midpoint) |
| 13 | `Stages/StageDirector.cs:67,315` | stage `_originX` | **`Primary.WorldX`** |
| 14 | `Stages/StageDirector.cs:353` | spawn anchor | **midpoint** (or `Primary`) so units enter the frame |
| 15 | `UI/Hud.cs:18` | draw HP/special | **loop over `All`**, draw a bar block per player (see §7) |
| 16 | `Core/TutorialController.cs:302,402,447` | tutorial actor | **`Primary`** — tutorial is P1-only for Phase 1/2 (see risks) |
| 17 | `Story/VignetteStaging.cs:308` | cinematic anchor X | **`Primary.WorldX`** |
| 18 | `Loot/Pickup.cs:103` | auto-grab when empty-handed | **`Nearest`** (nearest empty-handed player grabs) |
| 19 | `Loot/HealPickup.cs:22` | low-HP drop chance | base on **lowest-HP player**, or `Primary` for simplicity |
| 20 | `Loot/HealPickup.cs:42` | heal grab | **`Nearest` alive player** |
| 21 | `VFX/ComboHud.cs:62` | combo popup source | **`Primary.Meter.Combo`** (combo/juice HUD shows P1) — Phase 2 could sum/or-max |
| 22 | `Loot/WeaponProjectiles.cs:135` | thrown-weapon self-damage | give the projectile an **owner `Actor`** at spawn; self-damage the owner, not `Instance` |
| 23 | `Editor/SmokeTest.cs:66,68,75,77,91,93,115` | editor smoke test | keep on **`Instance`/Primary** alias — non-shipping, no change needed |
| — | `Bosses/_INTEGRATION.md`, `VFX/ComboHud.cs:11` (comment) | docs/comments | no code change |

Most sites (9,13,16,17,19,21,23) are fine pointing at **Primary** via the alias — so they need **no edit at all**. The ones that genuinely need work are the **enemy/boss targeting** (1–7,12), the **camera** (8), the **spawn anchors** (10,11,14), the **HUD** (15), **pickups** (18,20), and the **projectile owner** (22).

---

## 5. Shared camera for 2 players

Rewrite `CameraRig.LateUpdate` (`World/CameraRig.cs`) to frame both:
```csharp
var players = PlayerController.All;             // living players
if (no living players) return;
float minPX = min WorldX over living, maxPX = max WorldX over living;
float mid   = (minPX + maxPX) * 0.5f;

// Wall EACH player to the reachable view (unchanged formula, applied per player):
float leftWall  = MinX - HalfView + EdgeMargin;
float rightWall = MaxX + HalfView - EdgeMargin;
foreach (living p) p.WorldX = Clamp(p.WorldX, leftWall, rightWall);

// Follow the midpoint, still clamped to the stage gate [MinX, MaxX]:
float x = Clamp(mid, MinX, MaxX);
transform.position.x = Lerp(..., x, ...);
```
- **Gate interplay (`MaxX`):** unchanged. `StageDirector`/`EnemySpawner` still pin `MaxX` to the gate; the wall now stops **both** players at `rightWall = MaxX + HalfView - EdgeMargin`. So neither player can push past a locked gate — exactly the current single-player feel, extended.
- **Gate advance:** `EnemySpawner.TickAdvancing` checks `player.WorldX >= gateX` (line 154). Change to **`Primary.WorldX`** OR "the leading player reached the gate" (`maxPX >= gateX`) — recommend **leading player** so co-op advances when *either* reaches the gate. Low-risk either way for the demo (use `Primary`).
- **Beat-'em-up spread cap (optional, Phase 2):** classic games also stop players from separating past one screen width. The per-player wall already bounds them to the *view*, so extreme separation is naturally limited; no extra clamp needed for the MVP. (True "rubber-band tether" is a polish item.)
- The single-screen framing does **not** need dynamic zoom for the MVP — the fixed ortho size + per-player walls keep both on screen because both are walled into the same reachable band.

---

## 6. P2 join + character select

**Phase-1 MVP (fastest, "press Start on pad 2 to join"):**
- In `GameFlow.BuildWorld`, spawn P1 as today; assign its input by device (§3.4).
- Add a lightweight `CoopJoinWatcher` (new component on the `Systems` object, or a few lines in `GameFlow`): each frame during `State.Playing`, if `Gamepad.all.Count >= 2` and there is no P2 yet and `Gamepad.all[1].startButton.wasPressedThisFrame`, spawn P2:
  ```csharp
  var p2go = new GameObject("Player2");  // parent under _worldRoot
  p2go.AddComponent<SpriteRenderer>(); p2go.AddComponent<SpriteAnimator>();
  var p2 = p2go.AddComponent<PlayerController>();
  p2.Configure(_selectedP2 ?? _selected);      // reuse P1's char for MVP, or a default
  p2.WorldX = Primary.WorldX + 1.5f; p2.Z = 2.5f;
  p2.Init();
  p2.SetInput(new GamepadInput(1));
  ```
  Because `PlayerController.All` is append-order, P2 becomes `All[1]`; enemies/camera/HUD pick it up automatically.
- Give **P2 a distinct tint** (set `SpriteRenderer.color`) so players tell themselves apart until distinct P2 sprites exist.

**Phase-2 (proper P2 select):** extend `GameFlow` character-select to a two-column pick. Store `_selectedP2`. Options: (a) P2 confirms on pad 2 in the same select screen (each pad has a cursor), or (b) a simple sequential "P1 pick → P2 pick → Start." `Configure(_selectedP2)` on the P2 spawn. Keep the IMGUI placeholder; just add a second selection index driven by `Gamepad.all[1]`.

**Drop-in/drop-out (Phase 2/3):** on P2 pad disconnect or a "leave" press, `Destroy(p2.gameObject)` → `All.Remove` handles cleanup; camera/HUD collapse back to one player automatically.

---

## 7. HUD for P2

`UI/Hud.cs` `OnGUI` currently draws one block for `Instance`. Change to iterate `PlayerController.All`:
```csharp
for (int i = 0; i < All.Count; i++) {
    var p = All[i];
    float baseX = (i == 0) ? 12f : (Screen.width/scale - 192f);  // P1 top-left, P2 top-right
    DrawBar(baseX, 12, 180,16, p.Hp/p.MaxHp, HealthColor(...), i==0?"P1":"P2");
    DrawBar(baseX, 36, 180,12, p.Meter.Fraction01, MeterColor(p.Meter.FullTier), "SPECIAL ...");
    if (!p.Alive) /* small "P2 DOWN" tag near their block */;
}
```
- P1 top-left (current), P2 mirrored top-right. Keeps the "bottom half sacred for playfield" rule.
- The big center "YOU DIED" becomes meaningful only when **all** players are down (ties into §8 lives). For Phase 1, show a per-player "DOWN" tag; wire the shared game-over to the life counter in Phase 3.

---

## 8. Life counter (shared) — DESIGN (Phase 3)

**Recommendation: a shared pool of lives** (classic arcade co-op + matches "co-op + lives to survive a very hard game").

- New static/service `Lives` (e.g. `unity/Assets/Scripts/Core/Lives.cs`), owned by `GameFlow` for the run:
  ```csharp
  public static class Lives { public static int Count; public static void Reset(int n); }
  ```
  Initialize in `GameFlow.StartRun` (e.g. `Lives.Reset(Tuning.StartingLives)` — add a tuning knob, suggest 3–5 given the difficulty).
- **On a player death** (`PlayerController.TakeDamage` → dead branch, line 989): don't destroy the player; play the death anim, mark `!Alive`, and enter a **downed** state. Then:
  - If `Lives.Count > 0`: decrement, **respawn/revive** that player after a short delay at the other player's position (or at a safe screen edge) with partial HP and brief i-frames (`SetInvuln`). Reuse the existing i-frame path.
  - If `Lives.Count == 0` **and all players are down**: game over → `GameFlow.GoTitle()` (or a proper game-over screen).
- **Revive-in-place co-op nicety (optional):** a living player standing over a downed teammate for N seconds revives them without spending a life (classic "help up"). Design only; not MVP.
- **Where it lives:** the counter in `GameFlow`/`Lives`; the death→respawn logic in `PlayerController` (a new `Downed`/`Respawn` path) reading `Lives`. HUD shows the shared life count centered-top.
- **Single-player benefit:** the same lives pool makes solo play more forgiving — ship it for 1P too.

Per-player lives is the alternative (each has own stock) but shared-pool is simpler, more arcade-authentic, and better matches "help each other survive." Recommend shared.

---

## 9. Phased plan, effort, and risk

### Phase 1 — MVP: two pads, two players, one shared camera ("rip on it")
**Deliverable:** P2 presses Start on pad 2 → a second fighter appears; both control independently on separate USB pads; camera frames both; enemies attack whoever's closest; two HP/meter bars.
**Tasks & effort (one experienced Unity dev):**
- Add Input System package + set Active Input Handling = "Both" + restart — **0.5 h** (mostly editor/restart).
- `IPlayerInput` + `KeyboardInput` + `GamepadInput` — **1.5 h**.
- Refactor `PlayerController` static input → injected `_input` (§3.4) — **2–3 h** (mechanical but touches every control; the double-tap dash needs care).
- `PlayerController.All` + `Instance` alias + `Nearest/Primary/AnyAlive` — **0.5 h**.
- Enemy/boss targeting → `Nearest` (7 files, one-line each) — **1 h**.
- Shared camera rewrite (§5) — **1–1.5 h** (the walls + gate interplay need a play-test).
- P2 join watcher + spawn (§6 MVP) + P2 tint — **1 h**.
- HUD loop over `All` (§7) — **0.5 h**.
- Spawn anchors → Primary/midpoint (§4.2 #10,11,14) — **0.5 h**.
- Playtest/tune (deadzones, camera lerp, spread feel) — **1–2 h**.

**Total ≈ 10–13 h of focused work.**
**Demo-ready by tomorrow afternoon?** **Plausibly yes, but tight** — it's roughly a full focused day and hinges on the input refactor going cleanly. To de-risk for the demo, consider a **reduced MVP**: P2 shares P1's character, no fancy control scheme (left stick move + face buttons attack), skip HUD polish, keep camera midpoint-follow simple. That trims 2–4 h. If the package add stalls on the demo machine, fall back to the legacy-buttons path for a keyboard-P1 + one-pad-P2 config to still show co-op.

### Phase 2 — P2 character select, second HUD polish, multi-target correctness
- Two-column/sequential character select driving `_selectedP2` (§6) — **2–3 h**.
- Distinct P2 sprite/tint pipeline, HUD P1/P2 layout polish — **1–2 h**.
- Audit all `Nearest` targeting under real 2P crowds; gate-advance on leading player; ComboHud per-player if desired — **2–3 h**.
- Drop-in/drop-out on connect/disconnect — **1–2 h**.
**Total ≈ 6–10 h.**

### Phase 3 — Shared life counter + respawn/revive
- `Lives` service + `GameFlow` init + tuning knob — **1 h**.
- `PlayerController` downed/respawn path + i-frames + all-down game-over — **3–4 h**.
- Revive-in-place (optional) + HUD life display + audio — **2–3 h**.
**Total ≈ 6–8 h.**

### Riskiest parts (flagged)
1. **The `PlayerController` input refactor** — highest-touch change; every control routes through it, and the double-tap dash + Shield-Rush "hold forward" read input in non-obvious spots (lines 420, 218–225). A mistake here breaks *all* movement. **Mitigate:** keep `KeyboardInput` byte-identical to current bindings and regression-test P1-on-keyboard first, before adding pads.
2. **Active Input Handling project-setting flip** — needs an editor restart and must stay on **"Both"** (never "New only", or the legacy IMGUI menus/debug keys throw). Verify the smoke-test build (`SmokeTest.cs`) still compiles/runs after the flip.
3. **Camera walls vs. gate lock** — the per-player wall + `MaxX` gate must not deadlock two separating players (one at each wall). Playtest the framing; the fixed ortho size means extreme separation just pins both to the shared band, which is acceptable but must be *felt* to confirm.
4. **Unknown USB pad mapping** — the reason to prefer the New Input System; still, right-stick-vs-face-button attack scheme should be chosen and quickly tested on the actual demo pads.

---

## 10. File-change summary (execution checklist)

**New files**
- `unity/Assets/Scripts/Actors/IPlayerInput.cs`
- `unity/Assets/Scripts/Actors/KeyboardInput.cs`
- `unity/Assets/Scripts/Actors/GamepadInput.cs`
- `unity/Assets/Scripts/Core/CoopJoin.cs` (P2 join watcher) — or fold into `GameFlow`
- `unity/Assets/Scripts/Core/Lives.cs` (Phase 3)

**Edited files**
- `unity/Packages/manifest.json` — add `com.unity.inputsystem`.
- `ProjectSettings/ProjectSettings.asset` — Active Input Handling = Both (via editor).
- `unity/Assets/Scripts/Actors/PlayerController.cs` — injected input; `All` list + `Instance` alias + `Nearest/Primary/AnyAlive`; (Phase 3) downed/respawn.
- `unity/Assets/Scripts/World/CameraRig.cs` — midpoint follow + per-player wall.
- `unity/Assets/Scripts/Core/GameFlow.cs` — assign P1 input; P2 spawn/select; (Phase 3) `Lives.Reset`.
- `unity/Assets/Scripts/UI/Hud.cs` — loop over `All`.
- Enemy/boss targeting → `Nearest`: `EnemyController.cs`, `RangedEnemyController.cs`, `NinjaController.cs`, `SnapperController.cs`, `AntiAircraftController.cs`, `Bosses/BossController.cs`.
- Spawn anchors → Primary/midpoint: `World/EnemySpawner.cs`, `Stages/StageDirector.cs`, `World/HazardDirector.cs`.
- Pickups → `Nearest`: `Loot/Pickup.cs`, `Loot/HealPickup.cs`.
- `Loot/WeaponProjectiles.cs` — carry owner Actor instead of `Instance`.

**No change needed** (compile fine via the `Instance`→Primary alias): `Editor/SmokeTest.cs`, `Story/VignetteStaging.cs`, `Core/TutorialController.cs` (P1-only), `VFX/ComboHud.cs` (P1 juice).
