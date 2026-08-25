#!/usr/bin/env python3
"""this.l — placeholder asset generator (first pass).

Deterministic, re-runnable. Emits spec-compliant PLACEHOLDER assets into
`assets/` following the LOCKED §0 production specs in ASSET_MANIFEST.md so the
Unity import and core-loop prototype can proceed with zero further decisions.
Everything here is programmer-art / synth placeholder to be swapped for bespoke
art + creator-produced audio later; dimensions, palette, naming and layout are
already final.

Usage:  python3 tools/generate_assets.py [--out assets]

Covered this pass:
  * Foundation: 32-color palette export, chunky-arcade bitmap font (ui_font.png)
  * P0 sprites: 4 player characters, 4 enemies (Regular/Swarmer/Zombie + Pod),
    2 weapons (Sword, Shotgun), P0 VFX, blob shadow, ground pickups, P0 UI/HUD
  * Area-1 suburb parallax backdrop set (the manifest's validation gate)
  * Full SFX shot-list (95) + music/ambient beds as procedural placeholders
"""

from __future__ import annotations

import argparse
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from tools.asset_gen import (
    audio, backdrops, palette, pixelfont, props, sprites,
)
from tools.asset_gen.common import new_rng, save_png, write_json
from tools.asset_gen.palette import BASE_PALETTE, AREA_ACCENTS, hex_to_rgb

from PIL import Image, ImageDraw


def export_palette(out_dir):
    """Palette as a swatch PNG, a GIMP .gpl, and JSON — the single source."""
    os.makedirs(out_dir, exist_ok=True)
    # swatch strip (16 px per color)
    sw = 16
    img = Image.new("RGBA", (sw * len(BASE_PALETTE), sw), (0, 0, 0, 255))
    d = ImageDraw.Draw(img)
    for i, h in enumerate(BASE_PALETTE):
        d.rectangle([i * sw, 0, (i + 1) * sw - 1, sw - 1], fill=hex_to_rgb(h) + (255,))
    save_png(img, f"{out_dir}/base_palette.png")

    with open(f"{out_dir}/base_palette.gpl", "w") as f:
        f.write("GIMP Palette\nName: this.l base 32\nColumns: 8\n#\n")
        for h in BASE_PALETTE:
            r, g, b = hex_to_rgb(h)
            f.write(f"{r:3d} {g:3d} {b:3d}\t{h}\n")

    write_json({"base_32": BASE_PALETTE, "area_accents": AREA_ACCENTS,
                "gore_red": "#B31E2B", "placeholder": False,
                "note": "LOCKED — ASSET_MANIFEST.md §0. Source of truth."},
               f"{out_dir}/palette.json")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default="assets")
    args = ap.parse_args()
    out = args.out
    summary = {}

    print("[1/8] palette")
    export_palette(f"{out}/palette")

    print("[2/8] font")
    pixelfont.generate(f"{out}/fonts")

    print("[3/8] characters + enemies")
    summary["actors"] = {k: list(v) for k, v in sprites.generate(f"{out}/sprites").items()}
    props.generate_pod(f"{out}/sprites/enemies")

    print("[4/8] vfx + pickups + weapons + ui")
    rng = new_rng("vfx")
    props.generate_vfx(f"{out}/sprites/vfx", rng)
    props.generate_pickups(f"{out}/sprites/pickups")
    props.generate_weapons(f"{out}/sprites/weapons")
    props.generate_ui(f"{out}/ui")

    print("[5/8] backdrops (Area 1 suburb — validation gate)")
    summary["backdrop"] = backdrops.generate(f"{out}/backdrops")

    print("[6/8] sfx")
    n_sfx, sfx_idx = audio.generate_sfx(f"{out}/audio/sfx")
    summary["sfx_count"] = n_sfx

    print("[7/8] music + ambient")
    summary["music"] = audio.generate_music(f"{out}/audio/music")

    print("[8/8] index")
    # Count files produced.
    counts = {}
    for root, _, files in os.walk(out):
        for fn in files:
            ext = os.path.splitext(fn)[1].lower()
            counts[ext] = counts.get(ext, 0) + 1
    write_json({
        "generated_by": "tools/generate_assets.py",
        "status": "first-pass placeholders",
        "specs": "LOCKED ASSET_MANIFEST.md §0 (640x360, 1wu=24px, 12fps, "
                 "32-color palette, actor_action_dir_frame, ppu=24)",
        "file_counts_by_ext": counts,
        "sfx_index": sfx_idx,
        "summary": summary,
    }, f"{out}/asset_index.json")

    total = sum(counts.values())
    print(f"\nDone. {total} files across {len(counts)} types: "
          + ", ".join(f"{k or '(none)'}:{v}" for k, v in sorted(counts.items())))


if __name__ == "__main__":
    main()
