#!/usr/bin/env python3
"""
make_stick_weapons.py — hand-build the "weapons ARE dead stick figures" pickup art.

Core premise (creator): stick figures turn into weapons when they die, so every
weapon is visibly a stick-figure corpse contorted into that weapon shape — head,
arms and legs become recognizable parts of the weapon. Pixellab renders either a
plain weapon OR a plain person, never the hybrid, so these are drawn procedurally
for exact control. Black silhouette + thin outline to match the enemy stick style.

Output: assets/sprites/weapons/<kind>/<kind>_pickup.png (one flat pickup sprite each).
Run:  python tools/make_stick_weapons.py [--preview-dir DIR]
"""
import argparse
import math
import os

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
WEAPONS = os.path.join(ROOT, "assets", "sprites", "weapons")

BLACK = (24, 24, 28, 255)      # limb fill (matches stick enemies)
LINE = (0, 0, 0, 255)          # outline
SKIN = (24, 24, 28, 255)


def canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def limb(d, p0, p1, width, col=BLACK):
    """A thick rounded stick-figure limb (a fat line with round caps)."""
    d.line([p0, p1], fill=col, width=width)
    r = width // 2
    for p in (p0, p1):
        d.ellipse([p[0]-r, p[1]-r, p[0]+r, p[1]+r], fill=col)


def head(d, cx, cy, r, hollow=False):
    if hollow:
        d.ellipse([cx-r, cy-r, cx+r, cy+r], outline=LINE, width=3)
    else:
        d.ellipse([cx-r, cy-r, cx+r, cy+r], fill=BLACK)


# ---- Each weapon = a stick-figure corpse fused into the shape ----------------

def sword():
    # Head = pommel (bottom), arms = crossguard, body+legs = the blade (up).
    im = canvas(28, 64); d = ImageDraw.Draw(im)
    cx = 14
    head(d, cx, 56, 5)                       # pommel
    limb(d, (cx, 52), (cx, 50), 6)           # grip
    limb(d, (4, 47), (24, 47), 5)            # crossguard (arms out)
    # blade: body+legs tapering to a point
    d.polygon([(cx-4, 47), (cx+4, 47), (cx+2, 8), (cx, 3), (cx-2, 8)], fill=BLACK)
    return im


def pistol():
    # Body bent into an L: arm = barrel (out), leg = grip (down), head at the back.
    im = canvas(48, 40); d = ImageDraw.Draw(im)
    head(d, 9, 12, 5)                        # head at the back top
    limb(d, (9, 12), (42, 12), 8)            # arm = barrel
    limb(d, (14, 14), (14, 36), 8)           # leg = grip
    limb(d, (20, 14), (24, 22), 6)           # second leg = trigger guard hint
    return im


def revolver():
    im = pistol()
    d = ImageDraw.Draw(im)
    d.ellipse([18, 6, 30, 18], outline=LINE, width=2)  # cylinder = curled torso
    d.ellipse([21, 9, 27, 15], fill=BLACK)
    return im


def club():
    # Stiffened corpse: head = knob handle (bottom), body+legs = tapering bludgeon.
    im = canvas(24, 64); d = ImageDraw.Draw(im)
    cx = 12
    head(d, cx, 57, 5)                       # knob handle
    d.polygon([(cx-3, 54), (cx+3, 54), (cx+7, 6), (cx-7, 6)], fill=BLACK)  # fat top
    return im


def bat():
    im = canvas(22, 64); d = ImageDraw.Draw(im)
    cx = 11
    head(d, cx, 58, 4)
    d.polygon([(cx-2, 55), (cx+2, 55), (cx+6, 6), (cx-6, 6)], fill=BLACK)
    return im


def staff():
    # Fully straightened corpse = a long rigid staff; head at the top.
    im = canvas(16, 72); d = ImageDraw.Draw(im)
    cx = 8
    head(d, cx, 8, 5)
    limb(d, (cx, 12), (cx, 68), 5)
    limb(d, (cx-5, 20), (cx+5, 20), 3)       # arms pinned to the shaft
    return im


def whip():
    # Corpse stretched limp into a lash: head at the grip, body trailing in an S.
    im = canvas(64, 40); d = ImageDraw.Draw(im)
    head(d, 8, 20, 5)
    pts = [(10, 20)]
    for i in range(1, 24):
        t = i / 23
        x = 10 + t * 50
        y = 20 + math.sin(t * math.pi * 2.2) * 12 * (1 - t)
        pts.append((x, y))
    d.line(pts, fill=BLACK, width=4, joint="curve")
    return im


def boomerang():
    # Corpse bent double at the waist into a V.
    im = canvas(64, 56); d = ImageDraw.Draw(im)
    limb(d, (8, 48), (32, 6), 7)             # arm+torso
    limb(d, (32, 6), (56, 48), 7)            # legs
    head(d, 32, 10, 4)
    return im


def grenade():
    # Corpse curled into a ball; a leg sticks up as the spoon/lever.
    im = canvas(40, 44); d = ImageDraw.Draw(im)
    d.ellipse([8, 14, 34, 40], fill=BLACK)   # curled body
    limb(d, (26, 16), (30, 4), 4)            # leg = lever
    d.ellipse([28, 2, 36, 10], outline=LINE, width=2)  # pin ring = curled hand
    return im


def ballchain():
    # Head+torso = the ball; a stretched leg = the chain, arm loop = handle.
    im = canvas(40, 64); d = ImageDraw.Draw(im)
    limb(d, (10, 58), (24, 24), 3)           # chain (leg)
    d.ellipse([6, 52, 18, 64], outline=LINE, width=2)  # handle loop (hand)
    d.ellipse([18, 6, 38, 26], fill=BLACK)   # ball (head+torso)
    return im


BUILDERS = {
    "sword": sword, "pistol": pistol, "revolver": revolver, "club": club,
    "bat": bat, "staff": staff, "whip": whip, "boomerang": boomerang,
    "grenade": grenade, "ballchain": ballchain,
}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--preview-dir", default=None, help="also write copies here for review")
    ap.add_argument("--only", nargs="*", default=None)
    args = ap.parse_args()

    names = args.only or list(BUILDERS)
    for name in names:
        im = BUILDERS[name]()
        bbox = im.getbbox()
        if bbox:
            im = im.crop(bbox)
        out_dir = os.path.join(WEAPONS, name)
        os.makedirs(out_dir, exist_ok=True)
        out = os.path.join(out_dir, f"{name}_pickup.png")
        im.save(out)
        print(f"[stick_weapons] {name} -> {out} {im.size}")
        if args.preview_dir:
            os.makedirs(args.preview_dir, exist_ok=True)
            im.resize((im.width*4, im.height*4), Image.NEAREST).save(
                os.path.join(args.preview_dir, f"{name}.png"))


if __name__ == "__main__":
    main()
