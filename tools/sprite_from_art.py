#!/usr/bin/env python3
"""
sprite_from_art.py — turn a character illustration (e.g. a Stable Diffusion render)
into a game-ready pixel sprite: key out the flat background, trim, downscale, and
quantize to the game's locked 32-color palette (ASSET_MANIFEST.md §0).

Usage:
    python tools/sprite_from_art.py INPUT.png OUTPUT.png [--height 64] [--pad WxH]
        [--bg-tol 40] [--no-quantize] [--facing right]

  --height N     target sprite height in px (default 64; game draws ~48px tall)
  --pad WxH      place the sprite in a transparent WxH frame, feet at bottom-center
  --bg-tol T     background-key tolerance from the corner color (default 40)
  --no-quantize  skip palette quantization (keep full color)
  --facing L/R   flip so the character faces this way (default: leave as-is)

One image = one pose (an idle). For animation, run per pose or see the design brief.
"""
import argparse
import sys
from PIL import Image, ImageDraw
import numpy as np

# The locked 32-color base palette (ASSET_MANIFEST.md §0), as RGB tuples.
PALETTE_HEX = [
    "0D0B0E", "2A2A33", "4A4A57", "7A7A88", "B8B8C4", "F4F2EC",
    "B31E2B", "E8433F", "F5794F", "7A1420",
    "F2A03D", "FFD24A", "C77A2A", "FFF2B0",
    "234D2C", "3A7D44", "6CBF5A", "A8D98B",
    "1B3A5C", "2E6FB0", "4AA3D8", "9FD6EF", "CFEAF7",
    "6A3D8A", "C86FA8",
    "3F2A17", "6B4423", "9C6B3F", "C99A6A",
    "8A5A3A", "D99A6C", "E6B88A",
]
PALETTE = np.array([[int(h[i:i+2], 16) for i in (0, 2, 4)] for h in PALETTE_HEX], dtype=np.int16)


def key_background(img, tol):
    """Flood-fill the flat background from each corner to transparent."""
    img = img.convert("RGBA")
    w, h = img.size
    # If the image already has meaningful alpha, trust it.
    alpha = np.array(img)[:, :, 3]
    if (alpha < 250).mean() > 0.05:
        return img
    for corner in [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]:
        seed = img.getpixel(corner)
        if seed[3] == 0:
            continue
        ImageDraw.floodfill(img, corner, (0, 0, 0, 0), thresh=tol)
    return img


def quantize_to_palette(img):
    """Map every opaque pixel to the nearest palette color."""
    arr = np.array(img)
    rgb = arr[:, :, :3].astype(np.int16)
    a = arr[:, :, 3]
    flat = rgb.reshape(-1, 3)
    # nearest palette color by squared Euclidean distance
    d = ((flat[:, None, :] - PALETTE[None, :, :]) ** 2).sum(axis=2)
    idx = d.argmin(axis=1)
    out = PALETTE[idx].reshape(rgb.shape).astype(np.uint8)
    result = np.dstack([out, a]).astype(np.uint8)
    result[a < 8] = (0, 0, 0, 0)  # fully clear the transparent pixels
    return Image.fromarray(result, "RGBA")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("input")
    ap.add_argument("output")
    ap.add_argument("--height", type=int, default=64)
    ap.add_argument("--pad", default=None, help="WxH frame, feet at bottom-center")
    ap.add_argument("--bg-tol", type=int, default=40)
    ap.add_argument("--no-quantize", action="store_true")
    ap.add_argument("--portrait", action="store_true",
                    help="portrait mode: only key the background (full res, no downscale/quantize)")
    ap.add_argument("--facing", choices=["left", "right"], default=None)
    args = ap.parse_args()

    img = Image.open(args.input)
    img = key_background(img, args.bg_tol)

    if args.portrait:
        if args.facing == "left":
            img = img.transpose(Image.FLIP_LEFT_RIGHT)
        img.save(args.output)
        print(f"[sprite_from_art] portrait {args.input} -> {args.output}  ({img.width}x{img.height}); background keyed")
        return 0

    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)

    # Downscale preserving aspect to the target height.
    w, h = img.size
    new_h = max(1, args.height)
    new_w = max(1, round(w * new_h / h))
    img = img.resize((new_w, new_h), Image.LANCZOS)

    if not args.no_quantize:
        img = quantize_to_palette(img)

    if args.facing == "left":
        img = img.transpose(Image.FLIP_LEFT_RIGHT)

    if args.pad:
        pw, ph = (int(x) for x in args.pad.lower().split("x"))
        frame = Image.new("RGBA", (pw, ph), (0, 0, 0, 0))
        x = (pw - img.width) // 2
        y = ph - img.height  # feet on the bottom edge
        frame.alpha_composite(img, (max(0, x), max(0, y)))
        img = frame

    img.save(args.output)
    print(f"[sprite_from_art] {args.input} -> {args.output}  ({img.width}x{img.height})")


if __name__ == "__main__":
    sys.exit(main())
