#!/usr/bin/env python3
"""make_backdrop.py — generate distinct, seamless parallax strip sets for the
REAL StageData.BackdropTheme stems, matching area1_suburb's exact shape so a
load-by-stem Backdrop picks them up unchanged:

  assets/backdrops/<stem>/<stem>_{far,mid,near,lane}.png  (+ _preview_640x360.png + .json)

Stems (must equal StageDatabase.cs BackdropTheme values — the 12-stem NorCal run):
  area1_suburb  area1_mall  area2_sacramento  area2_airport  area3_causeway
  area3_farm    area3_dixon area4_vallejo     area4_marin    area4_goldengate
  area4_sf      finale_rooftop

Layer shape (identical across all stems):
  far  512x360 opaque   — sky gradient + clouds/haze (behind everything)
  mid  512x360 transp   — midground silhouettes/buildings around the horizon
  near 512x360 transp   — foreground band at the bottom
  lane 512x096 opaque   — the ground strip the player walks on
Parallax far .2 / mid .5 / near .85 / lane 1.0 ; horizon_y 144 ; tile_width 512.

Everything horizontally TILEABLE: gradients are vertical-only; repeating motifs use
a period P that divides 512, and any element crossing the right edge is also drawn
shifted by -512 (wrap) so the seam matches. Optional Pixellab landmark PNGs from
assets/backdrops/<propdir>/ are composited as periodic hero elements.
"""
import json, math, os
from PIL import Image, ImageDraw

W, H, LANE_H, HORIZON = 512, 360, 96, 144
ROOT = os.path.join("assets", "backdrops")


def vgrad(draw, x0, y0, x1, y1, top, bot):
    n = max(1, y1 - y0)
    for i in range(n):
        t = i / n
        c = tuple(int(top[k] + (bot[k] - top[k]) * t) for k in range(3))
        draw.line([(x0, y0 + i), (x1, y0 + i)], fill=c + (255,))


def wrap_paste(base, sprite, x, y):
    """paste sprite at x (and x-W, x+W) so it tiles seamlessly across the seam."""
    for dx in (-W, 0, W):
        base.alpha_composite(sprite, (x + dx, y))


def load_prop(propdir, name, scale_h=None):
    p = os.path.join(ROOT, propdir, name)
    if not os.path.exists(p):
        return None
    im = Image.open(p).convert("RGBA")
    if scale_h and im.height != scale_h:
        w = max(1, round(im.width * scale_h / im.height))
        im = im.resize((w, scale_h), Image.NEAREST)
    return im


def new_layer(opaque_fill=None):
    if opaque_fill:
        return Image.new("RGBA", (W, H), opaque_fill + (255,))
    return Image.new("RGBA", (W, H), (0, 0, 0, 0))


def clouds(img, color, y, n=3, w=70, h=22):
    d = ImageDraw.Draw(img)
    for i in range(n):
        cx = int((i + 0.5) * W / n)
        for (ox, oy, rw, rh) in [(-w//2, 0, w, h), (-w//4, -h//3, w//2, h), (w//6, -h//5, w//2, h)]:
            d.ellipse([cx+ox, y+oy, cx+ox+rw, y+oy+rh], fill=color + (255,))


def building_row(img, y_base, period, specs):
    """specs: list of (width, height, color, roof_color_or_None) drawn every `period`px."""
    for start in range(-period, W + period, period):
        x = start
        for (bw, bh, col, roof) in specs:
            top = y_base - bh
            seg = Image.new("RGBA", (W, H), (0, 0, 0, 0))
            dd = ImageDraw.Draw(seg)
            dd.rectangle([x, top, x + bw, y_base], fill=col + (255,))
            if roof:
                dd.polygon([(x - 3, top), (x + bw + 3, top), (x + bw // 2, top - bh // 4)], fill=roof + (255,))
            img.alpha_composite(seg)
            x += bw + 6


def skyline_teeth(img, y_base, color, period=64, minh=40, maxh=120, seed=0):
    d = ImageDraw.Draw(img)
    n = W // period  # teeth per tile — pattern MUST repeat every n so the seam matches
    x = -period
    while x < W + period:
        i = ((x % W) // period) % n  # tile-periodic index (identical across tiles)
        h = minh + (i * 37 + seed * 13) % (maxh - minh)
        w = period - 8
        d.rectangle([x, y_base - h, x + w, y_base], fill=color + (255,))
        for wy in range(y_base - h + 6, y_base - 4, 12):
            for wx in range(x + 4, x + w - 3, 10):
                if (wx - x + wy - (y_base - h) + i) % 3 == 0:
                    d.rectangle([wx, wy, wx + 3, wy + 5], fill=(255, 226, 130, 255))
        x += period


def lane_strip(top_band, asphalt, marking=None, style="dashes", rail=None):
    lane = Image.new("RGBA", (W, LANE_H), asphalt + (255,))
    d = ImageDraw.Draw(lane)
    vgrad(d, 0, 0, W, 18, top_band[0], top_band[1])
    if style == "dashes":
        for x in range(2, W, 32):
            d.rectangle([x, LANE_H - 22, x + 16, LANE_H - 18], fill=marking + (255,))
    elif style == "rails":
        d.rectangle([0, 30, W, 34], fill=marking + (255,))
        for x in range(0, W, 32):
            d.rectangle([x, 30, x + 4, LANE_H], fill=(60, 60, 66, 255))
    elif style == "planks":
        for x in range(0, W, 32):
            d.line([(x, 20), (x, LANE_H)], fill=marking + (255,), width=2)
    elif style == "tile":
        for x in range(0, W, 32):
            d.line([(x, 18), (x, LANE_H)], fill=marking + (255,), width=1)
        d.line([(0, LANE_H//2), (W, LANE_H//2)], fill=marking + (255,), width=1)
    elif style == "water":
        for yy in range(24, LANE_H, 10):
            for x in range((yy*8) % 32, W, 32):
                d.line([(x, yy), (x + 12, yy)], fill=marking + (255,), width=2)
    if rail:
        d.rectangle([0, 6, W, 10], fill=rail + (255,))
    return lane


def near_ground(strip_color, band_h=54, extras=None, top_line=None):
    near = new_layer()
    d = ImageDraw.Draw(near)
    d.rectangle([0, H - band_h, W, H], fill=strip_color + (255,))
    if top_line:
        d.rectangle([0, H - band_h, W, H - band_h + 4], fill=top_line + (255,))
    if extras:
        extras(near, d)
    return near


def preview(far, mid, near, lane):
    base = far.copy()
    base.alpha_composite(mid)
    base.alpha_composite(near)
    base.alpha_composite(lane, (0, H - LANE_H))
    return base.resize((640, 360), Image.NEAREST)


def write_theme(stem, far, mid, near, lane):
    outdir = os.path.join(ROOT, stem)
    os.makedirs(outdir, exist_ok=True)
    far.convert("RGBA").save(os.path.join(outdir, f"{stem}_far.png"))
    mid.save(os.path.join(outdir, f"{stem}_mid.png"))
    near.save(os.path.join(outdir, f"{stem}_near.png"))
    lane.save(os.path.join(outdir, f"{stem}_lane.png"))
    preview(far, mid, near, lane).convert("RGBA").save(os.path.join(outdir, f"{stem}_preview_640x360.png"))
    doc = {
        "height": H, "horizon_y": HORIZON,
        "layers": {"far": 0.2, "lane": 1.0, "mid": 0.5, "near": 0.85},
        "note": "Distinct per-stage strip set (procedural, seamless) matching area1_suburb structure.",
        # Intended working art for load-by-stem — NOT a skip-me placeholder. Procedural, so
        # a later hand-art pass can replace the PNGs in place without touching the wiring.
        "placeholder": False, "procedural": True, "seamless": True, "theme": stem, "tile_width": W,
    }
    with open(os.path.join(outdir, f"{stem}.json"), "w") as f:
        json.dump(doc, f, indent=2, sort_keys=True)
    print(f"WROTE {stem}: far/mid/near/lane + preview + json")


# ---------------------------------------------------------------- themes -----
def sky(far_top, far_bot, cloud_c=None, cloud_y=30, cloud_n=3, haze=None):
    far = new_layer()
    d = ImageDraw.Draw(far)
    vgrad(d, 0, 0, W, H, far_top, far_bot)
    if haze:
        d.rectangle([0, HORIZON - 20, W, HORIZON + 30], fill=haze + (120,))
    if cloud_c:
        clouds(far, cloud_c, cloud_y, cloud_n)
    return far


# --- Area 1: suburbia (stage 1 = first thing the creator tests) --------------
def build_suburb():
    far = sky((150, 196, 232), (210, 230, 244), (244, 246, 248), 34, 3)
    mid = new_layer()
    # rows of pitched-roof tract houses along the horizon
    building_row(mid, HORIZON + 20, 128, [
        (54, 46, (212, 196, 168), (150, 80, 66)),
        (48, 40, (196, 206, 196), (110, 96, 84)),
    ])
    def hedge(img, dd):
        dd.rectangle([0, H - 52, W, H - 46], fill=(70, 120, 70, 255))
    near = near_ground((104, 156, 92), band_h=52, extras=hedge, top_line=(80, 130, 78))  # front lawns
    lane = lane_strip(((198, 196, 190), (172, 170, 164)), (184, 182, 176), (150, 148, 142), "tile")  # sidewalk
    write_theme("area1_suburb", far, mid, near, lane)


def build_mall():
    far = new_layer((232, 224, 208)); d = ImageDraw.Draw(far)  # interior wall
    for x in range(0, W, 64):
        d.rectangle([x, 0, x + 2, H], fill=(214, 204, 186, 255))
    d.rectangle([0, 0, W, 40], fill=(245, 240, 230, 255))  # ceiling with lights
    for x in range(20, W, 64):
        d.rectangle([x, 14, x + 24, 20], fill=(255, 250, 220, 255))
    mid = new_layer(); dm = ImageDraw.Draw(mid)
    cols = [(200, 90, 90), (90, 150, 170), (210, 170, 80), (120, 160, 110)]
    for i, x in enumerate(range(-128, W + 128, 128)):
        c = cols[i % len(cols)]
        dm.rectangle([x, HORIZON - 40, x + 116, HORIZON + 30], fill=(245, 240, 232, 255))
        dm.rectangle([x + 6, HORIZON - 34, x + 110, HORIZON - 22], fill=c + (255,))  # sign
        dm.rectangle([x + 12, HORIZON - 6, x + 104, HORIZON + 30], fill=(180, 205, 220, 255))  # glass
    def rail(img, dd):
        dd.rectangle([0, H - 44, W, H - 40], fill=(150, 150, 160, 255))
    near = near_ground((214, 210, 202), band_h=46, extras=rail, top_line=(180, 178, 172))
    lane = lane_strip(((232, 228, 220), (214, 210, 202)), (222, 218, 210), (190, 186, 178), "tile")
    write_theme("area1_mall", far, mid, near, lane)


# --- Area 2: Sacramento + airport --------------------------------------------
def build_sacramento():
    far = sky((120, 158, 205), (200, 218, 236), (236, 240, 246), 28, 3)
    skyline_teeth(far, HORIZON + 30, (150, 166, 188), period=64, minh=50, maxh=140, seed=5)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    skyline_teeth(mid, HORIZON + 34, (100, 112, 136), period=64, minh=40, maxh=110, seed=7)
    # State Capitol dome, centered per tile
    cx = 256
    d.rectangle([cx - 30, HORIZON - 8, cx + 30, HORIZON + 20], fill=(238, 236, 228, 255))
    d.ellipse([cx - 26, HORIZON - 40, cx + 26, HORIZON + 6], fill=(244, 242, 235, 255))
    d.rectangle([cx - 3, HORIZON - 52, cx + 3, HORIZON - 38], fill=(214, 196, 120, 255))  # cupola
    def rail(img, dd):
        dd.rectangle([0, H - 40, W, H - 36], fill=(120, 120, 130, 255))
    near = near_ground((72, 74, 82), band_h=42, extras=rail, top_line=(150, 150, 160))
    lane = lane_strip(((120, 122, 128), (80, 82, 88)), (70, 72, 78), (220, 216, 70), "dashes")
    write_theme("area2_sacramento", far, mid, near, lane)


def build_airport():
    far = sky((150, 200, 235), (205, 228, 245), (238, 240, 244), 34, 3)
    tower = load_prop("area2_props", "control_tower.png", scale_h=150)
    plane = load_prop("area2_props", "plane.png", scale_h=70)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    for x in range(-40, W + 40, 128):
        d.rectangle([x, HORIZON - 26, x + 96, HORIZON + 18], fill=(196, 202, 210, 255))
        d.rectangle([x, HORIZON - 26, x + 96, HORIZON - 22], fill=(120, 150, 190, 255))
    if tower:
        wrap_paste(mid, tower, 360, HORIZON + 24 - tower.height)
    if plane:
        wrap_paste(mid, plane, 70, HORIZON + 6)
    near = near_ground((70, 72, 78), band_h=40, top_line=(210, 210, 60))  # tarmac edge
    lane = lane_strip(((120, 124, 130), (86, 88, 94)), (74, 76, 82), (214, 214, 60), "dashes")
    write_theme("area2_airport", far, mid, near, lane)


# --- Area 3: causeway + farm country (Yolo / Dixon) --------------------------
def build_causeway():
    far = sky((196, 214, 226), (224, 232, 224), (240, 242, 236), 40, 2, haze=(210, 220, 205))
    deck = load_prop("area3_props", "causeway.png", scale_h=80)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    for x in range(-16, W + 16, 16):
        d.ellipse([x, HORIZON - 14, x + 24, HORIZON + 8], fill=(96, 130, 92, 255))
    if deck:
        wrap_paste(mid, deck, 0, HORIZON - 6)
    def reeds(img, dd):
        for x in range(0, W, 8):
            hh = 30 + (x * 13) % 26
            dd.line([(x, H), (x + 2, H - hh)], fill=(86, 120, 70, 255), width=2)
    near = near_ground((104, 132, 96), band_h=30, extras=reeds, top_line=(70, 104, 66))
    lane = lane_strip(((150, 170, 150), (96, 120, 110)), (74, 100, 96), (150, 175, 165), "water")
    write_theme("area3_causeway", far, mid, near, lane)


def build_farm():
    far = sky((150, 194, 230), (216, 232, 236), (244, 246, 244), 32, 3)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # distant tree line (period 16 divides 512 → seamless)
    for x in range(-16, W + 16, 16):
        d.ellipse([x, HORIZON - 12, x + 26, HORIZON + 10], fill=(92, 128, 84, 255))
    # red barn + silo, one per tile (fully inside the tile, no seam cross)
    bx = 208
    d.rectangle([bx, HORIZON - 30, bx + 70, HORIZON + 16], fill=(168, 60, 50, 255))
    d.polygon([(bx - 4, HORIZON - 30), (bx + 74, HORIZON - 30), (bx + 35, HORIZON - 48)], fill=(120, 44, 38, 255))
    d.rectangle([bx + 78, HORIZON - 40, bx + 98, HORIZON + 16], fill=(200, 196, 186, 255))  # silo
    d.ellipse([bx + 78, HORIZON - 48, bx + 98, HORIZON - 34], fill=(150, 148, 140, 255))
    def field(img, dd):
        for i, yy in enumerate(range(H - 52, H, 8)):
            c = (150, 140, 90) if i % 2 == 0 else (120, 150, 80)
            dd.rectangle([0, yy, W, yy + 8], fill=c + (255,))
    near = near_ground((150, 140, 90), band_h=52, extras=field, top_line=(110, 130, 74))
    lane = lane_strip(((150, 130, 96), (120, 104, 76)), (134, 116, 84), (100, 86, 62), "planks")  # dirt road
    write_theme("area3_farm", far, mid, near, lane)


def build_dixon():
    far = sky((156, 198, 230), (220, 234, 236), (246, 248, 244), 30, 2)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # low small-town buildings (period 128 divides 512)
    for x in range(-40, W + 40, 128):
        d.rectangle([x, HORIZON - 22, x + 70, HORIZON + 16], fill=(206, 198, 180, 255))
        d.rectangle([x, HORIZON - 22, x + 70, HORIZON - 17], fill=(150, 90, 70, 255))  # roof line
    # water tower, one per tile
    wx = 300
    d.rectangle([wx + 4, HORIZON - 20, wx + 8, HORIZON + 16], fill=(90, 96, 100, 255))
    d.rectangle([wx + 24, HORIZON - 20, wx + 28, HORIZON + 16], fill=(90, 96, 100, 255))
    d.ellipse([wx - 4, HORIZON - 42, wx + 36, HORIZON - 16], fill=(120, 128, 132, 255))
    d.polygon([(wx - 4, HORIZON - 40), (wx + 36, HORIZON - 40), (wx + 16, HORIZON - 52)], fill=(96, 102, 106, 255))
    def field(img, dd):
        for i, yy in enumerate(range(H - 48, H, 10)):
            c = (156, 146, 96) if i % 2 == 0 else (128, 154, 84)
            dd.rectangle([0, yy, W, yy + 10], fill=c + (255,))
    near = near_ground((150, 142, 92), band_h=48, extras=field, top_line=(112, 132, 76))
    lane = lane_strip(((150, 134, 100), (122, 108, 80)), (136, 120, 88), (104, 90, 66), "dashes")
    write_theme("area3_dixon", far, mid, near, lane)


# --- Area 4: the Bay — Vallejo, Marin, Golden Gate, SF -----------------------
def build_vallejo():
    far = sky((150, 165, 178), (196, 206, 210), (222, 226, 228), 32, 2)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # gantry cranes
    for x in range(-30, W + 30, 128):
        d.rectangle([x, HORIZON - 60, x + 6, HORIZON + 20], fill=(200, 120, 40, 255))
        d.rectangle([x + 60, HORIZON - 60, x + 66, HORIZON + 20], fill=(200, 120, 40, 255))
        d.rectangle([x - 10, HORIZON - 64, x + 80, HORIZON - 58], fill=(200, 120, 40, 255))
        d.line([(x + 3, HORIZON - 58), (x + 40, HORIZON - 20)], fill=(120, 120, 128, 255), width=2)
    cols = [(180, 70, 60), (70, 130, 160), (200, 170, 60), (90, 150, 90)]
    for i, x in enumerate(range(-32, W + 32, 32)):
        c = cols[i % len(cols)]
        d.rectangle([x, HORIZON + 4, x + 28, HORIZON + 26], fill=c + (255,))
    def water(img, dd):
        for yy in range(H - 30, H, 8):
            for x in range((yy) % 32, W, 32):
                dd.line([(x, yy), (x + 14, yy)], fill=(120, 150, 165, 255), width=2)
    near = near_ground((86, 110, 122), band_h=32, extras=water)
    lane = lane_strip(((120, 100, 78), (96, 78, 58)), (110, 90, 66), (80, 62, 44), "planks")
    write_theme("area4_vallejo", far, mid, near, lane)


def build_marin():
    far = sky((140, 186, 224), (206, 226, 240), (240, 244, 246), 30, 2)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # rolling headland hills (period 256 divides 512 → seamless), golden-green layers
    for (yc, col) in [(HORIZON + 40, (150, 168, 96)), (HORIZON + 64, (120, 148, 82)), (HORIZON + 92, (96, 128, 70))]:
        for cx in range(-64, W + 128, 256):
            d.ellipse([cx, yc - 70, cx + 256, yc + 120], fill=col + (255,))
    near = near_ground((92, 124, 68), band_h=54, top_line=(120, 150, 88))
    lane = lane_strip(((150, 140, 110), (120, 112, 88)), (130, 120, 94), (100, 92, 70), "planks")  # trail
    write_theme("area4_marin", far, mid, near, lane)


def build_goldengate():
    far = sky((120, 158, 200), (200, 220, 236), (238, 242, 246), 28, 2, haze=(214, 222, 226))
    gg = load_prop("area4_props", "golden_gate.png", scale_h=170)
    mid = new_layer(); d = ImageDraw.Draw(mid)
    def tower(x):
        d.rectangle([x, HORIZON - 96, x + 12, HORIZON + 24], fill=(190, 66, 46, 255))
        d.rectangle([x - 4, HORIZON - 70, x + 16, HORIZON - 64], fill=(190, 66, 46, 255))
        d.rectangle([x - 4, HORIZON - 40, x + 16, HORIZON - 34], fill=(190, 66, 46, 255))
    t1, t2 = 128, 384
    tower(t1); tower(t2)
    for x in range(0, W, 4):
        seg = (x - t1) % 256
        sag = 60 * (1 - math.cos(seg / 256 * 2 * math.pi)) / 2
        y = HORIZON - 90 + int(sag)
        d.line([(x, y), (x + 4, y)], fill=(170, 60, 42, 255), width=2)
        d.line([(x, y), (x, HORIZON + 8)], fill=(180, 80, 60, 120), width=1)
    if gg:
        wrap_paste(mid, gg, 40, HORIZON + 20 - gg.height)
    def guard(img, dd):
        dd.rectangle([0, H - 40, W, H - 36], fill=(180, 70, 50, 255))
        for x in range(0, W, 16):
            dd.rectangle([x, H - 40, x + 3, H - 6], fill=(158, 60, 42, 255))
    near = near_ground((80, 82, 88), band_h=40, extras=guard, top_line=(180, 70, 50))
    lane = lane_strip(((120, 122, 128), (86, 88, 94)), (74, 76, 82), (220, 216, 70), "dashes")
    write_theme("area4_goldengate", far, mid, near, lane)


def build_sf():
    far = sky((120, 160, 210), (198, 216, 234), (236, 240, 246), 28, 3)
    skyline_teeth(far, HORIZON + 30, (150, 168, 190), period=64, minh=60, maxh=150, seed=1)
    tower = load_prop("area4_props", "skyline_tower.png", scale_h=180)
    mid = new_layer()
    if tower:
        wrap_paste(mid, tower, 300, HORIZON + 30 - tower.height)
    skyline_teeth(mid, HORIZON + 34, (96, 110, 134), period=64, minh=50, maxh=120, seed=4)
    def rail(img, dd):
        dd.rectangle([0, H - 40, W, H - 36], fill=(150, 60, 40, 255))
        for x in range(0, W, 32):
            dd.rectangle([x, H - 40, x + 4, H], fill=(120, 48, 32, 255))
    near = near_ground((70, 72, 80), band_h=42, extras=rail, top_line=(150, 60, 40))
    lane = lane_strip(((120, 122, 128), (80, 82, 88)), (70, 72, 78), (220, 216, 70), "dashes")
    write_theme("area4_sf", far, mid, near, lane)


# --- Finale: SF rooftop showdown (Phil) --------------------------------------
def build_finale_rooftop():
    far = new_layer(); d = ImageDraw.Draw(far)
    vgrad(d, 0, 0, W, H, (12, 14, 34), (54, 40, 66))
    for bx in range(0, W, 64):  # tiling star field (period 64 divides 512)
        for (sx, sy) in [(11, 18), (37, 9), (52, 40), (24, 60), (6, 78), (45, 70)]:
            if (bx + sx + sy) % 3 == 0:
                d.point((bx + sx, sy), fill=(240, 240, 220))
    skyline_teeth(far, HORIZON + 40, (30, 34, 54), period=64, minh=70, maxh=170, seed=2)
    mid = new_layer()
    skyline_teeth(mid, HORIZON + 46, (18, 20, 34), period=64, minh=50, maxh=130, seed=6)
    dm = ImageDraw.Draw(mid)
    for x in range(30, W, 128):  # water towers
        dm.rectangle([x, HORIZON - 6, x + 22, HORIZON + 20], fill=(40, 30, 26, 255))
        dm.polygon([(x - 2, HORIZON - 6), (x + 24, HORIZON - 6), (x + 11, HORIZON - 18)], fill=(46, 34, 28, 255))
    def para(img, dd):
        dd.rectangle([0, H - 40, W, H - 30], fill=(30, 30, 40, 255))  # rooftop parapet
    near = near_ground((22, 24, 32), band_h=40, extras=para, top_line=(48, 50, 62))
    lane = lane_strip(((44, 44, 54), (30, 30, 38)), (26, 26, 34), (60, 60, 72), "tile")
    write_theme("finale_rooftop", far, mid, near, lane)


# Ordered to match the campaign's stage progression (StageDatabase.cs).
BUILDERS = [
    build_suburb, build_mall, build_sacramento, build_airport, build_causeway,
    build_farm, build_dixon, build_vallejo, build_marin, build_goldengate,
    build_sf, build_finale_rooftop,
]

if __name__ == "__main__":
    for b in BUILDERS:
        b()
    print("done: 12 theme strip sets (real StageData.BackdropTheme stems)")
