#!/usr/bin/env python3
"""
forehead_peek.py — the Bert gag. Take a normal character portrait and produce a
card image where only the TOP of the head (the forehead) pokes up from the bottom
edge — as if he's too short to reach the frame. Drawn with ScaleToFit on the
character-select card, so the output uses the card's ~3:2 aspect with the forehead
anchored bottom-center and empty space above.

Usage:
    python tools/forehead_peek.py INPUT.png OUTPUT.png [--peek 0.18] [--rise 0.34]
        [--bg-tol 40] [--aspect 1.5]

  --peek F    fraction of the subject's height (from the top) to keep = the forehead band (default 0.18)
  --rise F    how far up the card the forehead reaches, as a fraction of card height (default 0.34)
  --bg-tol T  background-key tolerance (default 40)
  --aspect A  card width:height ratio (default 1.5 = the fighter card)
"""
import argparse
import sys
from PIL import Image, ImageDraw
import numpy as np


def key_background(img, tol):
    img = img.convert("RGBA")
    w, h = img.size
    if (np.array(img)[:, :, 3] < 250).mean() > 0.05:
        return img
    for corner in [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]:
        if img.getpixel(corner)[3] != 0:
            ImageDraw.floodfill(img, corner, (0, 0, 0, 0), thresh=tol)
    return img


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("input")
    ap.add_argument("output")
    ap.add_argument("--peek", type=float, default=0.18)
    ap.add_argument("--rise", type=float, default=0.34)
    ap.add_argument("--bg-tol", type=int, default=40)
    ap.add_argument("--aspect", type=float, default=1.5)
    args = ap.parse_args()

    img = key_background(Image.open(args.input), args.bg_tol)
    bbox = img.getbbox()
    if not bbox:
        print("[forehead_peek] input is empty after background key", file=sys.stderr)
        return 1
    img = img.crop(bbox)
    sw, sh = img.size

    # The forehead band = the top `peek` fraction of the subject.
    band = img.crop((0, 0, sw, max(1, int(sh * args.peek))))

    # Card canvas at the target aspect, sized off the band width for resolution.
    card_w = max(band.width, 256)
    card_h = int(round(card_w / args.aspect))
    canvas = Image.new("RGBA", (card_w, card_h), (0, 0, 0, 0))

    # Scale the band to a comfortable width, anchor it bottom-center so it rises only
    # `rise` of the way up the card (the rest is empty = "he's too short").
    target_w = int(card_w * 0.72)
    scale = target_w / band.width
    band = band.resize((target_w, max(1, int(band.height * scale))), Image.LANCZOS)
    x = (card_w - band.width) // 2
    y = card_h - band.height                     # forehead sits ON the bottom edge
    top_limit = int(card_h * (1.0 - args.rise))   # don't rise past `rise` of the card
    y = max(y, top_limit)
    canvas.alpha_composite(band, (x, y))

    canvas.save(args.output)
    print(f"[forehead_peek] {args.input} -> {args.output}  ({canvas.width}x{canvas.height}); forehead peeking bottom-center")
    return 0


if __name__ == "__main__":
    sys.exit(main())
