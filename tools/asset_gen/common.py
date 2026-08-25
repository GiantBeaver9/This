"""Shared helpers for the this.l placeholder-asset generators.

Everything here enforces the LOCKED §0 production conventions so the output
drops into the Unity import with zero further decisions:
  * internal render 640x360, 1 wu = 24 px, 12 fps playback
  * PNG, Point filter, no compression, one atlas per actor, ppu = 24
  * frame naming `actor_action_dir_frame`
Placeholder art is deterministic (seeded per actor) so re-running the pipeline
reproduces byte-identical files.
"""

from __future__ import annotations

import hashlib
import json
import math
import os

from PIL import Image, ImageDraw

# --- LOCKED production constants (ASSET_MANIFEST.md §0) -----------------------
INTERNAL_W, INTERNAL_H = 640, 360      # internal render resolution / 16:9
PIXELS_PER_UNIT = 24                    # 1 world-unit = 24 px
FPS = 12                                # animation playback (anime-on-2s)
PLAYER_H = 48                           # player/regular-enemy base sprite height
SWARMER_H = 24
MINIBOSS_SCALE = 1.2
BOSS_SCALE = 2.0
MAX_ATLAS = 2048                        # power-of-two pages, <= 2048^2


def seed_for(name: str) -> int:
    """Stable integer seed from a name — keeps placeholder art deterministic."""
    return int(hashlib.sha256(name.encode()).hexdigest()[:8], 16)


def new_rng(name: str):
    import random
    return random.Random(seed_for(name))


def blank(w: int, h: int) -> Image.Image:
    """Transparent RGBA canvas."""
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def next_pow2(n: int) -> int:
    return 1 << (max(1, n - 1)).bit_length()


def save_png(img: Image.Image, path: str) -> None:
    """Write a PNG with no compression filtering surprises (Point-filter ready)."""
    os.makedirs(os.path.dirname(path), exist_ok=True)
    # optimize=False keeps output deterministic across Pillow versions.
    img.save(path, format="PNG", optimize=False)


def write_json(obj, path: str) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        json.dump(obj, f, indent=2, sort_keys=True)
        f.write("\n")


def pack_atlas(frames: list[Image.Image], columns: int | None = None):
    """Pack equal-or-varying frames into a single power-of-two atlas page.

    Returns (atlas_image, [rect,...]) where rect = (x, y, w, h) per frame.
    Frames are laid out left-to-right, top-to-bottom in a uniform grid sized to
    the largest frame — the simplest layout a Unity sprite-sheet slicer reads.
    """
    if not frames:
        return blank(2, 2), []
    cell_w = max(f.width for f in frames)
    cell_h = max(f.height for f in frames)
    n = len(frames)
    if columns is None:
        columns = min(n, max(1, MAX_ATLAS // max(1, cell_w)))
        columns = min(columns, 8)  # keep sheets readable
    rows = math.ceil(n / columns)
    page_w = next_pow2(columns * cell_w)
    page_h = next_pow2(rows * cell_h)
    atlas = blank(page_w, page_h)
    rects = []
    for i, fr in enumerate(frames):
        cx = (i % columns) * cell_w
        cy = (i // columns) * cell_h
        # center each frame in its cell (bottom-anchored for actors)
        ox = cx + (cell_w - fr.width) // 2
        oy = cy + (cell_h - fr.height)
        atlas.alpha_composite(fr, (ox, oy))
        rects.append((ox, oy, fr.width, fr.height))
    return atlas, rects


def export_actor(out_dir: str, actor: str, clips: dict, meta_extra: dict | None = None):
    """Export one actor as: individual frame PNGs (actor_action_dir_frame.png),
    a packed atlas PNG, and a JSON describing clips/frames/timing.

    `clips` maps clip_name -> {"dir": str, "frames": [Image, ...]}
    where clip_name is like "attack_side". The emitted frame files follow the
    LOCKED `actor_action_dir_frame` convention.
    """
    frames_dir = os.path.join(out_dir, actor, "frames")
    all_frames: list[Image.Image] = []
    atlas_index = []  # parallel to all_frames
    clip_meta = {}
    for clip_name, data in clips.items():
        direction = data["dir"]
        frames = data["frames"]
        # split "attack_side" -> action="attack", already includes dir in name
        action = clip_name
        clip_meta[clip_name] = {
            "direction": direction,
            "frame_count": len(frames),
            "fps": FPS,
            "loop": data.get("loop", False),
            "atlas_range": [len(all_frames), len(all_frames) + len(frames)],
        }
        for fi, fr in enumerate(frames):
            fname = f"{actor}_{action}_{fi:02d}.png"
            save_png(fr, os.path.join(frames_dir, fname))
            all_frames.append(fr)
            atlas_index.append(fname)

    atlas, rects = pack_atlas(all_frames)
    atlas_path = os.path.join(out_dir, actor, f"{actor}_atlas.png")
    save_png(atlas, atlas_path)

    meta = {
        "actor": actor,
        "ppu": PIXELS_PER_UNIT,
        "fps": FPS,
        "import": {"type": "Sprite2D", "filter": "Point", "compression": "None"},
        "atlas": {
            "file": f"{actor}_atlas.png",
            "size": [atlas.width, atlas.height],
            "frames": [
                {"name": atlas_index[i], "rect": list(rects[i])}
                for i in range(len(all_frames))
            ],
        },
        "clips": clip_meta,
        "placeholder": True,
    }
    if meta_extra:
        meta.update(meta_extra)
    write_json(meta, os.path.join(out_dir, actor, f"{actor}.json"))
    return atlas.size
