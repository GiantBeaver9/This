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

# HORIZON (px from top) = the baked sky/ground boundary. The 360px strip maps 1:1 to the
# 15wu screen (camera ortho 7.5, centred at y=0): world_y = 7.5 - py/24. horizon_y=216 →
# world -1.5, matching the procedural Backdrop's sky-band bottom, so the walkable ground is
# the lower ~40% and the sky fills the upper ~60% (matches levels 1-4). AddStrip does NOT
# read horizon_y from the json (it places strips 1:1), so this is a pure-art change.
W, H, LANE_H, HORIZON = 512, 360, 96, 216
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


def ground_below(far, horizon, top, bot, rows=None):
    """Fill the far layer BELOW the horizon with a solid ground plane (so horizon-seated
    props stand on visible ground instead of floating over sky). `rows`=(c1,c2) draws
    faint furrow bands for fields."""
    d = ImageDraw.Draw(far)
    vgrad(d, 0, horizon, W, H, top, bot)
    if rows:
        for i, yy in enumerate(range(horizon + 6, H, 12)):
            if i % 2 == 0:
                d.rectangle([0, yy, W, yy + 5], fill=rows[0] + (60,))
            else:
                d.rectangle([0, yy, W, yy + 5], fill=rows[1] + (50,))


def seat_prop(layer, prop, x, ground_y):
    """Paste a prop so its BOTTOM edge sits exactly on ground_y (bottom-centre seating),
    wrap-tiled so it repeats seamlessly across the 512 seam."""
    if prop is None:
        return
    wrap_paste(layer, prop, x, ground_y - prop.height)


def hazed(im, f, haze=(150, 172, 150)):
    """Return a depth-faded copy: blend rgb toward `haze` by (1-f), preserving alpha.
    f=1 keeps the sprite as-is; lower f pushes it into the misty background."""
    out = im.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            px[x, y] = (int(r * f + haze[0] * (1 - f)),
                        int(g * f + haze[1] * (1 - f)),
                        int(b * f + haze[2] * (1 - f)), a)
    return out


def scaled(im, s):
    return im.resize((max(1, int(im.width * s)), max(1, int(im.height * s))), Image.NEAREST)


def dirt_grass_lane(dirt=(150, 120, 84), dirt2=(132, 104, 70), speck=(120, 92, 62),
                    grass=(96, 152, 74), grass2=(74, 128, 58), top_h=14, bot_h=12):
    """NON-URBAN walk surface: brown DIRT with GREEN GRASS verges top & bottom, no road
    markings (creator: 'just brown with some grass around top and bottom'). Seamless:
    every motif uses a period that divides 512."""
    lane = Image.new("RGBA", (W, LANE_H), dirt + (255,))
    d = ImageDraw.Draw(lane)
    # faint dirt mottle / pebbles for texture (period 8 & 16 divide 512 -> seamless)
    for i, x in enumerate(range(0, W, 8)):
        yy = top_h + 4 + ((x * 7) % max(1, LANE_H - top_h - bot_h - 8))
        c = dirt2 if (i % 2 == 0) else speck
        d.rectangle([x, yy, x + 3, yy + 2], fill=c + (255,))
    # top grass verge (dirt meets field) + a few blades poking down into the dirt
    d.rectangle([0, 0, W, top_h], fill=grass + (255,))
    for x in range(0, W, 8):
        hh = 3 + ((x * 5) % 5)
        d.line([(x, top_h), (x, top_h + hh)], fill=grass2 + (255,), width=2)
    # bottom grass verge + a few blades poking up
    d.rectangle([0, LANE_H - bot_h, W, LANE_H], fill=grass + (255,))
    for x in range(4, W, 8):
        hh = 3 + ((x * 3) % 5)
        d.line([(x, LANE_H - bot_h), (x, LANE_H - bot_h - hh)], fill=grass2 + (255,), width=2)
    return lane


def forest_floor_lane(litter=(120, 92, 66), litter2=(98, 74, 52), needle=(86, 64, 44),
                      fern=(74, 116, 60), fern2=(58, 96, 48)):
    """Redwood FOREST FLOOR: dark needle-litter dirt with fern/undergrowth speckle. No road."""
    lane = Image.new("RGBA", (W, LANE_H), litter + (255,))
    d = ImageDraw.Draw(lane)
    for i, x in enumerate(range(0, W, 8)):
        yy = 8 + ((x * 11) % (LANE_H - 16))
        c = [litter2, needle, litter2][i % 3]
        d.line([(x, yy), (x + 5, yy - 2)], fill=c + (255,), width=2)   # scattered needles
    # low ferns along the top edge (undergrowth by the path)
    for x in range(0, W, 12):
        hh = 6 + ((x * 7) % 8)
        col = fern if (x // 12) % 2 == 0 else fern2
        d.line([(x, 12), (x - 3, 12 - hh)], fill=col + (255,), width=2)
        d.line([(x, 12), (x + 3, 12 - hh)], fill=col + (255,), width=2)
        d.line([(x, 12), (x, 12 - hh - 2)], fill=col + (255,), width=2)
    return lane


def fairground_lane(path=(198, 176, 130), path2=(180, 158, 112), seam=(150, 130, 96),
                    grass=(104, 158, 80), grass2=(80, 132, 62)):
    """Six-Flags FAIRGROUND path: light sandy paver walk with grass verges + pennant-bunting
    confetti dots. NOT a car street (no lane dashes)."""
    lane = Image.new("RGBA", (W, LANE_H), path + (255,))
    d = ImageDraw.Draw(lane)
    # paver seams (period 32 divides 512), light so it reads as a plaza not a road
    for x in range(0, W, 32):
        d.line([(x, 16), (x, LANE_H)], fill=seam + (255,), width=1)
    for y in range(28, LANE_H, 20):
        d.line([(0, y), (W, y)], fill=seam + (255,), width=1)
    for i, x in enumerate(range(6, W, 24)):        # scattered dropped confetti
        c = [(210, 80, 70), (70, 130, 190), (230, 200, 80)][i % 3]
        d.rectangle([x, 22 + (x % 40), x + 3, 25 + (x % 40)], fill=c + (255,))
    # grass verges top & bottom
    d.rectangle([0, 0, W, 12], fill=grass + (255,))
    for x in range(0, W, 8):
        d.line([(x, 12), (x, 12 + 3 + (x % 4))], fill=grass2 + (255,), width=2)
    d.rectangle([0, LANE_H - 8, W, LANE_H], fill=grass + (255,))
    return lane


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
    # NON-URBAN farmland: brown dirt path with grass verges, barn/silo seated on green fields.
    horizon = HORIZON
    far = sky((150, 196, 236), (206, 230, 242), (248, 250, 252), 30, 3)
    # GREEN FIELDS fill below the horizon so the barn stands on ground (not floating over sky)
    ground_below(far, horizon, (156, 190, 100), (120, 162, 78), rows=((150, 176, 88), (110, 150, 70)))
    GROUND = H - LANE_H  # 264: the field/path boundary — props seat here
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # distant tree line along the horizon (period 16 divides 512 → seamless)
    for x in range(-16, W + 16, 16):
        d.ellipse([x, horizon - 12, x + 26, horizon + 12], fill=(88, 126, 80, 255))
    # a rail fence receding across the field
    fy = 236
    for x in range(0, W, 30):
        d.rectangle([x, fy - 16, x + 3, fy], fill=(120, 92, 60, 255))
    d.rectangle([0, fy - 13, W, fy - 10], fill=(146, 112, 74, 255))
    d.rectangle([0, fy - 4, W, fy - 1], fill=(146, 112, 74, 255))
    # RED BARN + silo prop, seated ON the ground (one per tile, fully inside → seamless)
    barn = load_prop("area3_props", "barn.png", scale_h=104)
    if barn:
        mid.alpha_composite(barn, (150, GROUND - barn.height))
    near = new_layer()  # foreground is the dirt path itself
    lane = dirt_grass_lane()  # brown dirt + grass verges, NO road markings
    write_theme("area3_farm", far, mid, near, lane)


def build_dixon():
    # URBAN small town (Dixon): a coherent main-street with buildings + water tower, KEEP the road.
    horizon = HORIZON
    far = sky((158, 202, 234), (218, 234, 240), (248, 250, 246), 28, 2)
    ground_below(far, horizon, (168, 176, 150), (140, 148, 128))  # dusty town lots
    GROUND = H - LANE_H
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # low small-town storefronts, seated on the ground (period 128 divides 512 → seamless)
    cols = [(208, 198, 178), (182, 150, 120), (156, 172, 182), (200, 172, 150)]
    for i, x in enumerate(range(0, W, 128)):
        c = cols[i % len(cols)]
        bh = 74 + (i * 11) % 26
        d.rectangle([x + 8, GROUND - bh, x + 8 + 104, GROUND], fill=c + (255,))
        d.rectangle([x + 8, GROUND - bh, x + 8 + 104, GROUND - bh + 8], fill=(120, 82, 66, 255))  # cornice
        for wx in range(x + 16, x + 108, 24):        # storefront windows
            d.rectangle([wx, GROUND - 30, wx + 15, GROUND - 8], fill=(150, 182, 198, 255))
    # WATER TOWER prop seated on the ground, one per tile
    wt = load_prop("area3_props", "water_tower.png", scale_h=120)
    if wt:
        mid.alpha_composite(wt, (338, GROUND - wt.height))
    near = new_layer()
    lane = lane_strip(((150, 150, 156), (110, 110, 116)), (96, 96, 102), (212, 202, 92), "dashes")  # town street
    write_theme("area3_dixon", far, mid, near, lane)


# --- Area 4: the Bay — Vallejo, Marin, Golden Gate, SF -----------------------
def build_vallejo():
    # Six Flags (NON-URBAN fairground): roller coaster + vendor stalls, festive bunting, park path.
    horizon = HORIZON
    far = sky((104, 168, 222), (236, 210, 196), (250, 250, 252), 24, 2)  # warm festive sky
    ground_below(far, horizon, (128, 170, 96), (104, 148, 80))           # park lawn
    GROUND = H - LANE_H
    mid = new_layer(); d = ImageDraw.Draw(mid)
    # ROLLER COASTER on the horizon (one per tile, fully inside → seamless), on a low berm
    rc = load_prop("area4_props", "roller_coaster.png", scale_h=112)
    if rc:
        mid.alpha_composite(rc, (30, (horizon + 8) - rc.height))   # seated on the park lawn at the horizon
    # festive pennant bunting strung across the top (period 24 divides 512)
    for i, x in enumerate(range(0, W, 24)):
        c = [(212, 72, 60), (70, 142, 192), (240, 202, 72), (92, 172, 112)][i % 4]
        d.polygon([(x, 40), (x + 24, 40), (x + 12, 58)], fill=c + (255,))
    d.line([(0, 40), (W, 40)], fill=(60, 60, 66, 255), width=1)
    # VENDOR STALLS seated on the ground (2 per tile → variety, both inside → seamless)
    stall = load_prop("area4_props", "vendor_stall.png", scale_h=66)
    if stall:
        for x in (70, 322):
            mid.alpha_composite(stall, (x, GROUND - stall.height))
    near = new_layer()
    lane = fairground_lane()  # packed-dirt/paver path + grass verges, NOT a street
    write_theme("area4_vallejo", far, mid, near, lane)


def build_marin():
    # TOWERING REDWOODS (NON-URBAN): massive vertical trunks receding through the layers,
    # dappled light, forest-floor ground. NOT hills.
    horizon = HORIZON
    far = new_layer(); d = ImageDraw.Draw(far)
    vgrad(d, 0, 0, W, H, (196, 214, 190), (122, 150, 118))     # hazy green forest depth
    # shafts of dappled light angling down from the canopy
    for x in range(30, W, 118):
        d.polygon([(x, 0), (x + 34, 0), (x + 14, H), (x - 12, H)], fill=(244, 246, 220, 22))
    ground_below(far, horizon + 30, (98, 112, 78), (66, 80, 54))   # forest floor depth
    redwood = load_prop("area4_props", "redwood.png")             # native ~114x340
    GROUND = H - LANE_H
    mid = new_layer()
    if redwood:
        # FAR trunks: small, hazed into the mist, receding (fully inside tile → seamless)
        for (x, s, f) in [(64, 0.55, 0.42), (300, 0.5, 0.36), (430, 0.6, 0.46)]:
            t = hazed(scaled(redwood, s), f)
            mid.alpha_composite(t, (x, (horizon + 40) - t.height))
        # MID trunks: medium, a little haze
        for (x, s, f) in [(150, 0.85, 0.72), (370, 0.78, 0.66)]:
            t = hazed(scaled(redwood, s), f)
            mid.alpha_composite(t, (x, (GROUND - 6) - t.height))
    near = new_layer()
    if redwood:
        # FOREGROUND colossus trunks: taller than the frame (top cut) so they read as GIANT,
        # framing the path. Placed inside the tile so the set repeats seamlessly.
        for (x, s) in [(6, 1.28), (410, 1.18)]:
            t = scaled(redwood, s)
            near.alpha_composite(t, (x, H - t.height + 24))       # base below frame → towering
    lane = forest_floor_lane()                                    # dirt + needles + ferns, NO road
    write_theme("area4_marin", far, mid, near, lane)


def build_goldengate():
    # MAJESTIC Golden Gate (URBAN highlight): tall international-orange towers dominating the
    # frame, sweeping main cables + suspender ropes, the bay + Marin headlands behind. Drawn
    # procedurally so it tiles seamlessly (period 256) — you WALK the endless bridge deck.
    horizon = HORIZON
    far = sky((88, 148, 202), (196, 220, 240), (240, 246, 250), 22, 2, haze=(214, 228, 236))
    ground_below(far, horizon, (120, 160, 186), (66, 108, 150))   # bay water
    d = ImageDraw.Draw(far)
    for cx in range(-64, W + 256, 256):                           # distant Marin headland
        d.ellipse([cx, horizon - 26, cx + 256, horizon + 84], fill=(120, 134, 112, 255))
    ORANGE = (206, 70, 44); ORANGE_D = (156, 48, 30)
    DECK_Y = horizon + 40
    P = 256
    TOP = 22
    mid = new_layer(); dm = ImageDraw.Draw(mid)
    # main cables: catenary between towers (period 256, phase 0 at towers → seamless at the seam)
    for x in range(0, W):
        seg = (x - 128) % P
        dip = 96 * (1 - math.cos(seg / P * 2 * math.pi)) / 2       # 0 at towers → 96 mid-span
        cy = TOP + int(dip)
        dm.line([(x, cy), (x + 1, cy)], fill=ORANGE + (255,), width=3)
        if x % 10 == 0:                                            # vertical suspender ropes
            dm.line([(x, cy), (x, DECK_Y)], fill=(196, 92, 66, 200), width=1)
    def tower(tx):                                                 # tall tapered tower
        dm.rectangle([tx - 8, TOP, tx - 1, DECK_Y], fill=ORANGE + (255,))
        dm.rectangle([tx + 1, TOP, tx + 8, DECK_Y], fill=ORANGE + (255,))
        for by in range(TOP + 20, DECK_Y, 30):                     # cross-braces
            dm.rectangle([tx - 8, by, tx + 8, by + 6], fill=ORANGE + (255,))
        dm.rectangle([tx - 11, TOP - 4, tx + 11, TOP + 6], fill=ORANGE + (255,))  # cap
    for tx in (128, 384):
        tower(tx)
    dm.rectangle([0, DECK_Y, W, DECK_Y + 9], fill=ORANGE_D + (255,))   # roadway deck
    dm.rectangle([0, DECK_Y + 9, W, DECK_Y + 13], fill=(84, 58, 48, 255))
    near = new_layer()
    lane = lane_strip(((150, 150, 156), (110, 110, 116)), (96, 96, 102), (214, 204, 92), "dashes")  # bridge deck road
    write_theme("area4_goldengate", far, mid, near, lane)


def build_sf():
    # SAN FRANCISCO (URBAN): distant downtown skyline, a row of Victorian "painted ladies" on
    # the hill, a cable-car trolley on the near street with cable-car rails. KEEP the road.
    horizon = HORIZON
    far = sky((120, 162, 212), (200, 218, 236), (238, 242, 248), 26, 3)
    skyline_teeth(far, horizon + 22, (148, 166, 190), period=64, minh=56, maxh=140, seed=1)  # downtown
    ground_below(far, horizon + 6, (150, 152, 158), (120, 122, 130))    # city ground haze
    GROUND = H - LANE_H
    mid = new_layer()
    skyline_teeth(mid, horizon + 26, (104, 118, 142), period=64, minh=46, maxh=110, seed=4)
    # ROW of Victorian painted ladies, seated on the ground, near-touching (repeats per tile)
    h1 = load_prop("area4_props", "painted_house1.png", scale_h=118)
    h2 = load_prop("area4_props", "painted_house2.png", scale_h=126)
    houses = [h for h in (h1, h2) if h]
    if houses:
        x = 6; i = 0
        while x < W - 30:
            hh = houses[i % len(houses)]
            mid.alpha_composite(hh, (x, GROUND - hh.height))
            x += hh.width - 2; i += 1
    near = new_layer()
    # TROLLEY (cable car) seated at the ground line so it rides IN FRONT of the houses on the
    # street edge — NOT down in the lane band (the opaque lane strip would hide it there).
    tr = load_prop("area4_props", "trolley.png", scale_h=60)
    if tr:
        near.alpha_composite(tr, (150, GROUND - tr.height))
    lane = lane_strip(((150, 150, 156), (112, 112, 118)), (96, 96, 102), (150, 62, 44), "rails")  # cable-car rails
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
