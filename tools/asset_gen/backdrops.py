"""Parallax backdrop sets — 3 seamless tiling layers per stage-theme.

LOCKED spec (ASSET_MANIFEST.md §7): far 0.2x / mid 0.5x / near 0.85x, each a
horizontally-tiling strip authored 360 px tall (full internal render height),
none scrolling vertically. Play lane = 1.0x. This first pass ships the Area-1
suburb set — the manifest's validation-gate backdrop — plus its lane floor.
Tiles are 512 px wide and seam-safe (elements repeat on divisors of the width).
"""

from __future__ import annotations

import math

from PIL import Image, ImageDraw

from .common import INTERNAL_H, save_png, write_json
from .palette import rgb, AREA_ACCENTS, BASE_PALETTE_GROUPS, PAPER

TILE_W = 512
H = INTERNAL_H  # 360
HORIZON = int(H * 0.40)  # top 40% is the sky/HUD band

INK = BASE_PALETTE_GROUPS["ink"]
BLUE = BASE_PALETTE_GROUPS["blue"]
GREEN = BASE_PALETTE_GROUPS["green"]
BROWN = BASE_PALETTE_GROUPS["brown"]
ORANGE = BASE_PALETTE_GROUPS["orange"]


def _vgrad(img, y0, y1, top, bot):
    d = ImageDraw.Draw(img)
    for y in range(y0, y1):
        t = (y - y0) / max(1, (y1 - y0))
        c = tuple(int(top[i] + (bot[i] - top[i]) * t) for i in range(3))
        d.line([(0, y), (img.width, y)], fill=c + (255,))


def _suburb_far(acc):
    """Sky gradient + wispy clouds (fills the top-40% sky band and beyond)."""
    img = Image.new("RGBA", (TILE_W, H), (0, 0, 0, 0))
    _vgrad(img, 0, H, rgb(BLUE[3]), rgb(BLUE[4]))
    d = ImageDraw.Draw(img)
    cloud = rgb(acc[5]) + (200,)
    for cx in (60, 240, 400):  # divisors keep the seam clean
        for k in range(4):
            r = 10 + k * 4
            d.ellipse([cx + k * 12, 40 - k * 2, cx + k * 12 + r * 2, 40 - k * 2 + r],
                      fill=cloud)
    return img


def _suburb_mid(acc):
    """Houses + treeline sitting on the horizon."""
    img = Image.new("RGBA", (TILE_W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    base = HORIZON + 24
    period = 128  # 512 / 4 -> seamless
    for i in range(TILE_W // period + 1):
        x = i * period
        # house
        hcol = rgb(acc[1] if i % 2 else acc[0]) + (255,)
        d.rectangle([x + 14, base - 46, x + 74, base], fill=hcol)
        d.polygon([(x + 8, base - 46), (x + 44, base - 74), (x + 80, base - 46)],
                  fill=rgb(acc[2]) + (255,))
        d.rectangle([x + 34, base - 24, x + 46, base], fill=rgb(BROWN[0]) + (255,))  # door
        # tree (mulberry/tall)
        tx = x + 100
        d.rectangle([tx, base - 30, tx + 6, base], fill=rgb(BROWN[1]) + (255,))
        d.ellipse([tx - 16, base - 60, tx + 22, base - 22], fill=rgb(GREEN[1]) + (255,))
    return img


def _suburb_near(acc):
    """Roadside props just behind the lane (hedges, fence, hydrant, mailbox)."""
    img = Image.new("RGBA", (TILE_W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    y = H - 70
    period = 256
    for i in range(TILE_W // period + 1):
        x = i * period
        d.rectangle([x, y + 30, x + period, y + 44], fill=rgb(GREEN[0]) + (255,))  # hedge run
        for fx in range(x + 4, x + period, 10):  # picket fence
            d.rectangle([fx, y + 16, fx + 3, y + 34], fill=rgb(PAPER) + (230,))
        d.rectangle([x + 40, y + 8, x + 48, y + 34], fill=rgb(ORANGE[2]) + (255,))   # hydrant-ish
        d.ellipse([x + 38, y + 4, x + 50, y + 14], fill=rgb(ORANGE[0]) + (255,))
        d.rectangle([x + 150, y + 6, x + 158, y + 34], fill=rgb(INK[2]) + (255,))    # mailbox post
        d.rectangle([x + 146, y + 2, x + 164, y + 10], fill=rgb(BLUE[1]) + (255,))
    return img


def _suburb_lane():
    """The 1.0x play-lane floor strip (sidewalk + road) actors stand on."""
    img = Image.new("RGBA", (TILE_W, 96), (0, 0, 0, 0))
    _vgrad(img, 0, 40, rgb(INK[4]), rgb(INK[3]))   # sidewalk
    d = ImageDraw.Draw(img)
    d.rectangle([0, 40, TILE_W, 96], fill=rgb(INK[2]) + (255,))  # asphalt
    for x in range(16, TILE_W, 48):  # lane dashes
        d.rectangle([x, 66, x + 20, 70], fill=rgb(ORANGE[1]) + (255,))
    d.line([(0, 40), (TILE_W, 40)], fill=rgb(INK[1]) + (255,), width=2)
    return img


def generate(out_dir: str):
    acc = AREA_ACCENTS["area1_suburb"]
    theme = "area1_suburb"
    layers = {
        "far": (_suburb_far(acc), 0.2),
        "mid": (_suburb_mid(acc), 0.5),
        "near": (_suburb_near(acc), 0.85),
    }
    for name, (img, factor) in layers.items():
        save_png(img, f"{out_dir}/{theme}/{theme}_{name}.png")
    save_png(_suburb_lane(), f"{out_dir}/{theme}/{theme}_lane.png")

    write_json({
        "theme": theme,
        "tile_width": TILE_W,
        "height": H,
        "seamless": True,
        "layers": {"far": 0.2, "mid": 0.5, "near": 0.85, "lane": 1.0},
        "horizon_y": HORIZON,
        "note": "Validation-gate backdrop (ASSET_MANIFEST §0/§7). 11 more theme "
                "sets follow at the same structure.",
        "placeholder": True,
    }, f"{out_dir}/{theme}/{theme}.json")

    # A composed preview at 640x360 so the set can be eyeballed at native res.
    # Layers are tiled horizontally to show the seamless wrap.
    def tiled(src, w, h):
        strip = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        for x in range(0, w, src.width):
            strip.alpha_composite(src, (x, 0))
        return strip

    comp = Image.new("RGBA", (640, H), (0, 0, 0, 0))
    comp.alpha_composite(tiled(layers["far"][0], 640, H))
    comp.alpha_composite(tiled(layers["mid"][0], 640, H))
    comp.alpha_composite(tiled(layers["near"][0], 640, H))
    lane = _suburb_lane()
    comp.alpha_composite(tiled(lane, 640, lane.height), (0, H - lane.height))
    save_png(comp, f"{out_dir}/{theme}/{theme}_preview_640x360.png")
    return theme
