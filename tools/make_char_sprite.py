#!/usr/bin/env python3
"""
make_char_sprite.py — turn a character portrait into an in-game sprite + atlas.
Downscales + palette-quantizes the art to a pixel sprite, then writes a
single-frame atlas (every animation clip points at that one pose) so the real
character shows in-game right away. Refine with real multi-frame side art later.

Usage:
    python tools/make_char_sprite.py PORTRAIT.png ACTOR_ID [--height 64]
e.g. python tools/make_char_sprite.py assets/portraits/tactical.png player_tactical --height 64
"""
import argparse
import json
import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
from sprite_from_art import key_background, quantize_to_palette  # noqa: E402
from PIL import Image  # noqa: E402

CLIPS = ["idle", "walk", "dash", "jump", "land",
         "attack_side", "attack_up", "attack_down", "air_side", "hurt", "death"]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("portrait")
    ap.add_argument("actor")
    ap.add_argument("--height", type=int, default=64)
    ap.add_argument("--outdir", default=None)
    args = ap.parse_args()

    img = key_background(Image.open(args.portrait), 50)
    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
    w, h = img.size
    new_h = max(1, args.height)
    new_w = max(1, round(w * new_h / h))
    img = img.resize((new_w, new_h), Image.LANCZOS)
    img = quantize_to_palette(img)

    outdir = args.outdir or os.path.join("assets", "sprites", "characters", args.actor)
    os.makedirs(outdir, exist_ok=True)

    # Back up any existing placeholder atlas/json once.
    for name in (f"{args.actor}_atlas.png", f"{args.actor}.json"):
        p = os.path.join(outdir, name)
        bak = p + ".stick"
        if os.path.exists(p) and not os.path.exists(bak):
            os.replace(p, bak)

    atlas_name = f"{args.actor}_atlas.png"
    img.save(os.path.join(outdir, atlas_name))

    W, H = img.size
    frames = [{"name": f"{args.actor}_{c}_00.png", "rect": [0, 0, W, H]} for c in CLIPS]
    doc = {
        "actor": args.actor,
        "atlas": {"file": atlas_name, "frames": frames, "size": [W, H]},
        "ppu": 24, "fps": 12, "placeholder": True, "from_concept_art": True,
    }
    with open(os.path.join(outdir, f"{args.actor}.json"), "w") as f:
        json.dump(doc, f, indent=2)

    print(f"[make_char_sprite] {args.actor}: {W}x{H} sprite + {len(CLIPS)}-clip atlas -> {outdir}")


if __name__ == "__main__":
    sys.exit(main())
