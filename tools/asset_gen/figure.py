"""Articulated stick-figure rig for placeholder actor art.

this.l's cast are stick figures — "ink on paper" (ASSET_MANIFEST.md §0), so the
placeholders are real articulated stick figures posed per animation frame, not
colored blobs. A build swaps these for bespoke art later; until then the walk
cycles, swings and deaths actually read at 48 px.
"""

from __future__ import annotations

import math

from PIL import Image, ImageDraw

from .common import blank
from .palette import rgb, INK_BLACK, PAPER


def _pt(root, length, ang_deg):
    """Endpoint `length` px from `root` at `ang_deg` (0 = straight down, +CW)."""
    a = math.radians(ang_deg)
    return (root[0] + length * math.sin(a), root[1] + length * math.cos(a))


def _limb(draw, root, upper, lower, a1, a2, width, color):
    """Two-segment limb; returns the tip point."""
    mid = _pt(root, upper, a1)
    tip = _pt(mid, lower, a1 + a2)
    draw.line([root, mid], fill=color, width=width)
    draw.line([mid, tip], fill=color, width=width)
    return tip


def draw_figure(frame_w, frame_h, ground_y, style, pose):
    """Draw one stick-figure frame.

    style: {height, ink, accent, bulk, hair, hunch, head_hollow, weapon}
    pose:  joint angles/offsets for this frame (see sprites.py pose builders).
    Returns an RGBA image (frame_w x frame_h), figure bottom-anchored at ground_y.
    """
    img = blank(frame_w, frame_h)
    d = ImageDraw.Draw(img)
    H = style["height"]
    ink = rgb(style.get("ink", INK_BLACK)) + (255,)
    accent = rgb(style["accent"]) + (255,)
    bulk = style.get("bulk", 1.0)
    lw = max(2, round(H / 22 * bulk))          # heavy stroke, scales with size
    cx = frame_w / 2 + pose.get("dx", 0)
    gy = ground_y + pose.get("dy", 0)

    # Vertical landmarks (proportional to figure height H).
    hip_y = gy - H * 0.42
    sh_y = gy - H * 0.74
    head_r = H * 0.11
    head_cy = gy - H * 0.86 - head_r
    lean = pose.get("lean", 0.0)                 # torso lean, px at shoulders
    hunch = style.get("hunch", 0.0) * H * 0.12

    hip = (cx, hip_y)
    shoulder = (cx + lean + hunch, sh_y)
    head_c = (cx + lean * 1.3 + hunch, head_cy)

    # Torso.
    d.line([hip, shoulder], fill=ink, width=lw)
    if bulk > 1.15:  # bulky/boss: fill a chest wedge for a top-heavy silhouette
        cw = H * 0.22 * bulk
        d.polygon(
            [
                (shoulder[0] - cw, sh_y),
                (shoulder[0] + cw, sh_y),
                (hip[0] + cw * 0.4, hip_y),
                (hip[0] - cw * 0.4, hip_y),
            ],
            fill=ink,
        )

    # Legs.
    ul, ll = H * 0.24, H * 0.22
    _limb(d, hip, ul, ll, pose["l_hip"], pose["l_knee"], lw, ink)
    _limb(d, hip, ul, ll, pose["r_hip"], pose["r_knee"], lw, ink)

    # Arms (from shoulder). Weapon hand = right arm tip.
    ua, la = H * 0.20, H * 0.20
    _limb(d, shoulder, ua, la, pose["l_sh"], pose["l_el"], lw, ink)
    r_hand = _limb(d, shoulder, ua, la, pose["r_sh"], pose["r_el"], lw, ink)

    # Head — hollow (outline) for the zombie hollow-head state, else filled ink.
    hb = [head_c[0] - head_r, head_c[1] - head_r,
          head_c[0] + head_r, head_c[1] + head_r]
    if style.get("head_hollow"):
        d.ellipse(hb, outline=ink, width=lw)
    else:
        d.ellipse(hb, fill=ink)

    # Hair accent (Shotgunner redhead, etc.).
    if style.get("hair"):
        hc = rgb(style["hair"]) + (255,)
        d.arc([hb[0] - 1, hb[1] - 1, hb[2] + 1, hb[3] + 1], 180, 360, fill=hc, width=lw)

    # Accent band across the chest — the per-character identifier.
    bx = shoulder[0]
    d.line([(bx - head_r, sh_y + lw), (bx + head_r, sh_y + lw)], fill=accent, width=lw)

    # Optional in-hand weapon stub drawn from the right hand.
    weapon = style.get("weapon")
    if weapon and "wpn_ang" in pose:
        wcol = rgb(weapon.get("color", INK_BLACK)) + (255,)
        wlen = weapon.get("len", H * 0.5)
        tip = _pt(r_hand, wlen, pose["wpn_ang"])
        d.line([r_hand, tip], fill=wcol, width=max(2, lw - 1))

    return img
