"""VFX, ground pickups, in-hand weapons, and HUD/UI placeholder sprites.

Small single- or few-frame sprites that round out the P0 tier (VFX.md, UI.md,
WEAPONS.md, TUNING.md §6). All draw from the LOCKED palette; gore uses the one
fixed gore hue #B31E2B.
"""

from __future__ import annotations

import math

from PIL import Image, ImageDraw

from .common import blank, save_png, write_json, export_actor, PIXELS_PER_UNIT
from .pixelfont import render_text
from .palette import (
    rgb, GORE_RED, INK_BLACK, INK_MID, INK_LIGHT, PAPER,
    BASE_PALETTE_GROUPS,
)

R = BASE_PALETTE_GROUPS["red"]
O = BASE_PALETTE_GROUPS["orange"]
G = BASE_PALETTE_GROUPS["green"]
B = BASE_PALETTE_GROUPS["blue"]
BR = BASE_PALETTE_GROUPS["brown"]
INK = BASE_PALETTE_GROUPS["ink"]


def _burst(size, n, colors, rng, r0, r1, dot=1):
    img = blank(size, size)
    d = ImageDraw.Draw(img)
    c = size / 2
    for i in range(n):
        a = rng.uniform(0, 2 * math.pi)
        rr = rng.uniform(r0, r1)
        x, y = c + rr * math.cos(a), c + rr * math.sin(a)
        col = rgb(rng.choice(colors)) + (255,)
        d.rectangle([x, y, x + dot, y + dot], fill=col)
    return img


# --- VFX --------------------------------------------------------------------
def _vfx_clips(rng):
    from PIL import ImageDraw
    def puff(prog, col, sz=16):
        img = blank(sz, sz)
        d = ImageDraw.Draw(img)
        c = sz / 2
        r = 2 + prog * (sz / 2 - 2)
        a = int(220 * (1 - prog))
        d.ellipse([c - r, c - r, c + r, c + r], outline=rgb(col) + (a,), width=1)
        return img

    def spark(prog, cols, sz=16):
        return _burst(sz, 8, cols, rng, prog * 2, 2 + prog * (sz / 2 - 2))

    def gust(prog, sz=20):
        img = blank(sz, sz)
        d = ImageDraw.Draw(img)
        cy = sz / 2
        for k in range(3):
            x = 2 + prog * (sz - 6) - k * 3
            d.arc([x, cy - 4 - k, x + 8, cy + 4 + k], -60, 60,
                  fill=rgb(INK[3]) + (int(200 * (1 - prog)),), width=1)
        return img

    def flash(prog, col, sz=24):
        img = blank(sz, sz)
        d = ImageDraw.Draw(img)
        c = sz / 2
        r = prog * (sz / 2)
        d.ellipse([c - r, c - r, c + r, c + r], fill=rgb(col) + (int(230 * (1 - prog)),))
        return img

    return {
        "gust":         {"dir": "side", "frames": [gust(i / 4) for i in range(4)]},
        "dash_dust":    {"dir": "side", "frames": [puff(i / 4, BR[3]) for i in range(4)]},
        "jump_puff":    {"dir": "none", "frames": [puff(i / 3, INK[4]) for i in range(3)]},
        "land_puff":    {"dir": "none", "frames": [puff(i / 4, INK[4], 20) for i in range(4)]},
        "hit_spark":    {"dir": "none", "frames": [spark(i / 4, [O[1], PAPER, O[0]]) for i in range(4)]},
        "finisher_flash": {"dir": "none", "frames": [flash(i / 5, PAPER) for i in range(5)]},
        "death_burst":  {"dir": "none", "frames": [_burst(24, 24, [GORE_RED, R[1], R[3]], rng, i * 2, 2 + i * 4) for i in range(5)]},
        "muzzle_flash": {"dir": "side", "frames": [flash(i / 3, O[1], 14) for i in range(3)]},
    }


def generate_vfx(out_dir, rng):
    export_actor(out_dir, "vfx", _vfx_clips(rng), meta_extra={"tier": "P0", "kind": "vfx"})

    # Blob ground-shadow / Z-marker — the single most-instanced sprite (TUNING §1).
    for name, w in [("shadow_small", 16), ("shadow_regular", 28), ("shadow_boss", 52)]:
        img = blank(w, w // 2 + 2)
        d = ImageDraw.Draw(img)
        d.ellipse([1, 1, w - 2, w // 2], fill=rgb(INK_BLACK) + (110,))
        save_png(img, f"{out_dir}/blob_shadow/{name}.png")
    write_json({"note": "blob ground-shadow / Z-marker, 1 per actor (TUNING.md §1)",
                "opacity": 110, "placeholder": True},
               f"{out_dir}/blob_shadow/blob_shadow.json")


# --- Pod enemy (P0 spawner, HP 50 — ENEMIES.md / TUNING.md §4) ---------------
def generate_pod(out_dir):
    """Destroyable spawner: idle/pulse -> spit -> destroyed. Not a figure."""
    def pod(prog, cracked=False):
        img = blank(32, 40); d = ImageDraw.Draw(img)
        cx = 16
        pulse = int(2 * math.sin(prog * math.pi))
        # fleshy egg-sac body (gore-adjacent reds + a sickly green rim)
        d.ellipse([4 - pulse, 8 - pulse, 28 + pulse, 39], fill=rgb(R[3]) + (255,))
        d.ellipse([7, 12, 25, 36], fill=rgb(R[0]) + (255,))
        d.ellipse([11, 16 - pulse, 21, 28 - pulse], fill=rgb(O[0]) + (255,))  # glowing core
        d.arc([4, 4, 28, 30], 200, 340, fill=rgb(G[1]) + (255,), width=2)     # rim
        if cracked:
            d.line([16, 8, 12, 24], fill=rgb(INK_BLACK) + (255,), width=1)
            d.line([16, 8, 20, 22], fill=rgb(INK_BLACK) + (255,), width=1)
        return img

    idle = {"dir": "none", "frames": [pod(i / 4) for i in range(4)], "loop": True}
    spit = {"dir": "none", "frames": [pod(0.5), pod(0.9), pod(1.0),
                                      _burst_over(pod(1.0), 12)] , "loop": False}
    dead = {"dir": "none", "frames": [pod(1.0, cracked=True), pod(0.6, cracked=True),
                                      pod(0.2, cracked=True),
                                      _burst_over(pod(0.2, cracked=True), 24)],
            "loop": False}
    export_actor(out_dir, "enemy_pod", {"idle": idle, "spit": spit, "death": dead},
                 meta_extra={"tier": "P0", "kind": "enemy", "hp": 50})


def _burst_over(base, n):
    import random
    img = base.copy(); d = ImageDraw.Draw(img)
    rng = random.Random(n)
    for _ in range(n):
        x, y = rng.randint(2, base.width - 2), rng.randint(2, base.height - 2)
        d.point((x, y), fill=rgb(GORE_RED) + (255,))
    return img


# --- Ground pickups & tokens (TUNING.md §6, UI.md §3.4) ----------------------
def generate_pickups(out_dir):
    def coin(sz, face, rimc, label=None):
        img = blank(sz, sz)
        d = ImageDraw.Draw(img)
        d.ellipse([0, 0, sz - 1, sz - 1], fill=rgb(rimc) + (255,))
        d.ellipse([1, 1, sz - 2, sz - 2], fill=rgb(face) + (255,))
        if label:
            t = render_text(label, rgb(rimc) + (255,))
            img.alpha_composite(t, ((sz - t.width) // 2, (sz - t.height) // 2))
        return img

    save_png(coin(12, O[1], O[2]), f"{out_dir}/pickup_coin.png")           # 1¢
    save_png(coin(14, INK[4], INK[2]), f"{out_dir}/pickup_dime.png")       # 10¢

    # Heal pickup — a distinct positive-read first-aid mote (green + cross).
    hp = blank(14, 14); d = ImageDraw.Draw(hp)
    d.rounded_rectangle([1, 1, 12, 12], radius=3, fill=rgb(G[1]) + (255,))
    d.rectangle([6, 3, 7, 10], fill=rgb(PAPER) + (255,))
    d.rectangle([3, 6, 10, 7], fill=rgb(PAPER) + (255,))
    save_png(hp, f"{out_dir}/pickup_heal.png")

    # Merc-claim token (dropped by a killed Monkey).
    mt = blank(14, 14); d = ImageDraw.Draw(mt)
    d.ellipse([1, 1, 12, 12], outline=rgb(BR[1]) + (255,), width=2)
    d.ellipse([4, 4, 9, 9], fill=rgb(BR[2]) + (255,))
    save_png(mt, f"{out_dir}/pickup_merc_token.png")

    # Sniper's dropped rifle (+100 meter) — distinct from held rifle.
    rf = blank(24, 10); d = ImageDraw.Draw(rf)
    d.line([2, 6, 20, 5], fill=rgb(INK_BLACK) + (255,), width=2)
    d.rectangle([14, 3, 18, 6], fill=rgb(BR[1]) + (255,))
    d.rectangle([6, 5, 8, 9], fill=rgb(INK_MID) + (255,))
    save_png(rf, f"{out_dir}/pickup_sniper_rifle.png")

    write_json({"items": ["pickup_coin", "pickup_dime", "pickup_heal",
                           "pickup_merc_token", "pickup_sniper_rifle"],
                "ground_lifetime_s": 12, "placeholder": True},
               f"{out_dir}/pickups.json")


# --- Weapons (P0: Sword, Shotgun) -------------------------------------------
def generate_weapons(out_dir):
    def sword(wear=0):
        img = blank(28, 10); d = ImageDraw.Draw(img)
        blade = rgb(INK_LIGHT) + (255,)
        d.line([6, 5, 25, 4], fill=blade, width=2)          # blade
        d.line([3, 3, 3, 7], fill=rgb(INK[2]) + (255,), width=2)  # guard
        d.rectangle([0, 4, 3, 6], fill=rgb(BR[1]) + (255,))       # grip
        # wear: chip pixels off the edge as wear rises (0..2)
        for k in range(wear):
            d.point((25 - k * 6, 4), fill=(0, 0, 0, 0))
        return img

    def shotgun():
        img = blank(30, 10); d = ImageDraw.Draw(img)
        d.rectangle([4, 4, 26, 6], fill=rgb(INK[2]) + (255,))    # barrel
        d.rectangle([0, 5, 6, 9], fill=rgb(BR[1]) + (255,))      # stock
        d.rectangle([10, 6, 13, 9], fill=rgb(BR[2]) + (255,))    # grip
        return img

    save_png(sword(0), f"{out_dir}/sword/sword_fresh.png")
    save_png(sword(1), f"{out_dir}/sword/sword_worn.png")
    save_png(sword(2), f"{out_dir}/sword/sword_chipped.png")
    save_png(shotgun(), f"{out_dir}/shotgun/shotgun.png")
    # spine-magazine segment (WEAPONS.md — shotgun spine ammo).
    seg = blank(6, 8); d = ImageDraw.Draw(seg)
    d.rectangle([1, 1, 4, 6], fill=rgb(R[0]) + (255,))
    save_png(seg, f"{out_dir}/shotgun/shotgun_spine_segment.png")
    # ground pickups.
    save_png(sword(0), f"{out_dir}/sword/sword_pickup.png")
    save_png(shotgun(), f"{out_dir}/shotgun/shotgun_pickup.png")
    write_json({"P0": ["sword", "shotgun"],
                "sword_wear_states": ["fresh", "worn", "chipped"],
                "placeholder": True}, f"{out_dir}/weapons.json")


# --- UI / HUD (P0) ----------------------------------------------------------
def generate_ui(out_dir):
    ink = rgb(INK_BLACK) + (255,)

    def bar(w, h, fillcol, frac, bg=INK[1]):
        img = blank(w, h); d = ImageDraw.Draw(img)
        d.rectangle([0, 0, w - 1, h - 1], fill=rgb(bg) + (255,))
        d.rectangle([1, 1, w - 2, h - 2], fill=rgb(INK[0]) + (255,))
        fw = int((w - 4) * frac)
        if fw > 0:
            d.rectangle([2, 2, 2 + fw, h - 3], fill=rgb(fillcol) + (255,))
        return img

    # Health bar — green/yellow/red states (UI.md §5).
    save_png(bar(64, 8, G[2], 1.0), f"{out_dir}/health_green.png")
    save_png(bar(64, 8, O[1], 0.55), f"{out_dir}/health_yellow.png")
    save_png(bar(64, 8, R[1], 0.2), f"{out_dir}/health_red.png")
    # Special meter — yellow/blue/green tiers + armed pulse.
    save_png(bar(48, 6, O[1], 0.5, bg=INK[2]), f"{out_dir}/special_yellow.png")
    save_png(bar(48, 6, B[2], 0.75, bg=INK[2]), f"{out_dir}/special_blue.png")
    save_png(bar(48, 6, G[2], 1.0, bg=INK[2]), f"{out_dir}/special_green.png")
    armed = bar(48, 6, G[3], 1.0, bg=INK[2])
    ImageDraw.Draw(armed).rectangle([0, 0, 47, 5], outline=rgb(PAPER) + (255,))
    save_png(armed, f"{out_dir}/special_armed.png")

    # Combo popup "N HIT!" (uses the bundled font).
    for n in (3, 8, 15):
        t = render_text(f"{n} HIT!", rgb(O[1]) + (255,), scale=2)
        pad = blank(t.width + 4, t.height + 4)
        pad.alpha_composite(t, (2, 2))
        save_png(pad, f"{out_dir}/combo_{n}hit.png")

    # Weapon-type icons (small silhouettes).
    for name, drawer in [("fist", lambda d: d.ellipse([4, 4, 11, 11], fill=ink)),
                         ("sword", lambda d: d.line([3, 12, 12, 3], fill=ink, width=2)),
                         ("gun", lambda d: (d.rectangle([2, 6, 12, 8], fill=ink),
                                            d.rectangle([9, 8, 11, 12], fill=ink)))]:
        ic = blank(16, 16); drawer(ImageDraw.Draw(ic))
        save_png(ic, f"{out_dir}/icon_weapon_{name}.png")

    # "BARRAGE INCOMING" warning banner.
    warn = render_text("BARRAGE INCOMING", rgb(R[1]) + (255,), scale=2)
    wb = blank(warn.width + 8, warn.height + 8)
    ImageDraw.Draw(wb).rectangle([0, 0, wb.width - 1, wb.height - 1], fill=rgb(INK[0]) + (200,))
    wb.alpha_composite(warn, (4, 4))
    save_png(wb, f"{out_dir}/warn_barrage.png")

    # Execute prompt "▶ SPECIAL".
    ex = render_text("▶ SPECIAL", rgb(O[1]) + (255,), scale=2)
    save_png(ex, f"{out_dir}/prompt_execute.png")

    write_json({"P0": ["health_green/yellow/red", "special_yellow/blue/green/armed",
                       "combo_popup", "weapon_icons"],
                "P1": ["warn_barrage", "prompt_execute"],
                "placeholder": True}, f"{out_dir}/ui.json")
