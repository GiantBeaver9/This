#!/usr/bin/env python3
"""
frames_to_atlas.py — pack per-clip animation frames into the game's atlas format.

Input: a folder of PNG frames named  <clip>_<NN>.png  (e.g. walk_00.png, walk_01.png,
attack_side_00.png, ...). Frames are grouped by clip and packed into one atlas PNG,
with a <actor>.json listing every frame's rect + clip grouping that SpriteLibrary reads.
Meant for Pixellab output (already clean, transparent pixel art) — no quantize/resize.

Usage:
    python tools/frames_to_atlas.py FRAMES_DIR ACTOR_ID [--outdir DIR] [--cols 8]
e.g. python tools/frames_to_atlas.py build/adam_frames player_tactical
"""
import argparse
import glob
import json
import os
import re
from PIL import Image

FRAME_RE = re.compile(r"^(?P<clip>.+)_(?P<idx>\d+)\.png$", re.IGNORECASE)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("frames_dir")
    ap.add_argument("actor")
    ap.add_argument("--outdir", default=None)
    ap.add_argument("--cols", type=int, default=8)
    ap.add_argument("--no-trim", action="store_true",
                    help="keep each frame's full canvas (no per-frame bbox crop) so a fixed, "
                         "symmetric canvas keeps the body anchored and long props aren't clipped")
    args = ap.parse_args()

    # Gather frames grouped by clip, ordered by index.
    clips = {}
    for path in sorted(glob.glob(os.path.join(args.frames_dir, "*.png"))):
        m = FRAME_RE.match(os.path.basename(path))
        if not m:
            continue
        clips.setdefault(m["clip"], []).append((int(m["idx"]), path))
    if not clips:
        raise SystemExit(f"no <clip>_<NN>.png frames found in {args.frames_dir}")
    for c in clips:
        clips[c].sort()

    # Load all images, TRIMMED to their content so the bottom-align seats feet on the
    # ground line (Pixellab frames carry transparent padding that would otherwise float
    # the character). Cell size = max trimmed dims.
    imgs = []  # (clip, idx, Image)
    cw = ch = 0
    for clip, frames in clips.items():
        for idx, path in frames:
            im = Image.open(path).convert("RGBA")
            if not args.no_trim:
                bbox = im.getbbox()
                if bbox:
                    im = im.crop(bbox)
            cw = max(cw, im.width); ch = max(ch, im.height)
            imgs.append((clip, idx, im))

    cols = max(1, args.cols)
    rows = (len(imgs) + cols - 1) // cols
    atlas = Image.new("RGBA", (cols * cw, rows * ch), (0, 0, 0, 0))

    frames_json = []
    for i, (clip, idx, im) in enumerate(imgs):
        cx, cy = (i % cols) * cw, (i // cols) * ch
        # center each frame horizontally in its cell, bottom-align (feet on cell bottom)
        ox = cx + (cw - im.width) // 2
        oy = cy + (ch - im.height)
        atlas.alpha_composite(im, (ox, oy))
        frames_json.append({"name": f"{args.actor}_{clip}_{idx:02d}.png",
                            "rect": [ox, oy, im.width, im.height]})

    outdir = args.outdir or os.path.join("assets", "sprites", "characters", args.actor)
    os.makedirs(outdir, exist_ok=True)
    # back up any existing atlas once
    for name in (f"{args.actor}_atlas.png", f"{args.actor}.json"):
        p = os.path.join(outdir, name)
        if os.path.exists(p) and not os.path.exists(p + ".prev"):
            os.replace(p, p + ".prev")

    atlas_name = f"{args.actor}_atlas.png"
    atlas.save(os.path.join(outdir, atlas_name))
    doc = {
        "actor": args.actor,
        "atlas": {"file": atlas_name, "frames": frames_json, "size": [atlas.width, atlas.height]},
        "ppu": 24, "fps": 12, "source": "pixellab",
    }
    with open(os.path.join(outdir, f"{args.actor}.json"), "w") as f:
        json.dump(doc, f, indent=2)

    print(f"[frames_to_atlas] {args.actor}: {len(imgs)} frames / {len(clips)} clips "
          f"({', '.join(sorted(clips))}) -> {outdir}  atlas {atlas.width}x{atlas.height}")


if __name__ == "__main__":
    main()
