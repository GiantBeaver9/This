#!/usr/bin/env python3
"""
make_phil.py — hand-build PHIL, the finale boss, as animated frames.

Pixellab can't render his iconic look (it always fills the head), so Phil is drawn
procedurally: a stick-figure body with a HOLLOW RING head (empty circle, no face), a
black TOP HAT, and a big pencil sharpened to a point on BOTH ends (no eraser). Black
silhouette + thin outline to sit with the stick-figure cast, boss-scale.

Emits frames for idle/walk/attack_side/hurt/death (east-facing) -> frames_to_atlas ->
assets/sprites/bosses/phil/phil_atlas.png + phil.json.

Run:  python tools/make_phil.py [--preview-dir DIR]
"""
import argparse
import math
import os
import shutil
import subprocess
import sys

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)

INK = (22, 22, 26, 255)      # body silhouette
PENCIL = (240, 205, 90, 255) # pencil shaft (the one warm accent)
LEAD = (40, 40, 44, 255)     # pencil tips/lead
W, H = 128, 120              # WIDE symmetric canvas: the double-pencil extends freely on
                             # both sides without clipping, and (with --no-trim) Phil's body
                             # stays centered so it never shifts frame-to-frame.


def _p(draw, a, b, width):
    draw.line([a, b], fill=INK, width=width)
    r = width // 2
    for q in (a, b):
        draw.ellipse([q[0]-r, q[1]-r, q[0]+r, q[1]+r], fill=INK)


def phil_frame(lean=0.0, arm=0.0, legphase=0.0, pencilang=-0.5, bob=0.0, fall=0.0, hurt=0.0, sharpen=-1):
    """One Phil pose. Angles in radians; lean/fall shift the whole figure."""
    im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)

    cx = W // 2 + int(lean * 10)
    ground = H - 6
    # A full fall lays him flat near the ground.
    tilt = fall * (math.pi / 2)

    def rot(px, py, ox, oy, ang):
        s, c = math.sin(ang), math.cos(ang)
        dx, dy = px - ox, py - oy
        return (ox + dx * c - dy * s, oy + dx * s + dy * c)

    hipy = ground - 34 + int(bob)
    hip = (cx, hipy)
    neck = (cx + int(lean * 6), hipy - 34)
    headc = (neck[0] + int(lean * 4), neck[1] - 12)

    # Apply a whole-body tilt around the hip for the death fall.
    def T(pt):
        return rot(pt[0], pt[1], hip[0], hip[1], tilt) if tilt else pt

    hipT, neckT, headcT = T(hip), T(neck), T(headc)

    # Torso
    _p(d, hipT, neckT, 7)

    # Legs (walk cycle)
    la = 0.5 * math.sin(legphase)
    ra = 0.5 * math.sin(legphase + math.pi)
    lknee = T((hip[0] + math.sin(la) * 8, hip[1] + 14))
    lfoot = T((hip[0] + math.sin(la) * 14, ground))
    rknee = T((hip[0] + math.sin(ra) * 8, hip[1] + 14))
    rfoot = T((hip[0] + math.sin(ra) * 14, ground))
    _p(d, hipT, lknee, 6); _p(d, lknee, lfoot, 6)
    _p(d, hipT, rknee, 6); _p(d, rknee, rfoot, 6)

    # Sharpen pose: raise the pencil vertical and rock it while the Holy Sharpener works.
    if sharpen >= 0:
        pencilang = -math.pi / 2 + 0.14 * math.sin(sharpen * 1.3)
        arm = 0.0

    # Back arm (planted-ish) + front arm holding the pencil
    backhand = T((neck[0] - 12, neck[1] + 14 + hurt * 8))
    _p(d, neckT, backhand, 5)

    shoulder = neckT
    elbow = T((neck[0] + math.cos(pencilang) * 12, neck[1] + 6 + math.sin(pencilang) * 12))
    hand = T((neck[0] + math.cos(pencilang) * 22 + arm * 10,
              neck[1] + 6 + math.sin(pencilang) * 22))
    _p(d, shoulder, elbow, 5); _p(d, elbow, hand, 5)

    # The double-ended pencil, centered on the hand, pointing along the arm.
    pang = pencilang + 0.15 + arm * 0.6
    plen = 26
    tipA = (hand[0] + math.cos(pang) * plen, hand[1] + math.sin(pang) * plen)
    tipB = (hand[0] - math.cos(pang) * plen, hand[1] - math.sin(pang) * plen)
    d.line([tipB, tipA], fill=PENCIL, width=6)
    # BOTH ends sharpened (no eraser): a pale wood cone + a dark lead point on each tip.
    perp = (math.cos(pang + math.pi / 2), math.sin(pang + math.pi / 2))
    for tip, sgn in ((tipA, 1), (tipB, -1)):
        wbase = (tip[0] - math.cos(pang) * 10 * sgn, tip[1] - math.sin(pang) * 10 * sgn)
        d.polygon([tip,  # wood cone
                   (wbase[0] + perp[0] * 4, wbase[1] + perp[1] * 4),
                   (wbase[0] - perp[0] * 4, wbase[1] - perp[1] * 4)], fill=(225, 190, 120, 255))
        lbase = (tip[0] - math.cos(pang) * 4.5 * sgn, tip[1] - math.sin(pang) * 4.5 * sgn)
        d.polygon([tip,  # lead point
                   (lbase[0] + perp[0] * 2, lbase[1] + perp[1] * 2),
                   (lbase[0] - perp[0] * 2, lbase[1] - perp[1] * 2)], fill=LEAD)

    # HOLY SHARPENER working the top tip (sharpen clip only): a glowing device + shavings.
    if sharpen >= 0:
        sx, sy = tipA
        d.ellipse([sx-11, sy-11, sx+11, sy+11], outline=(150, 230, 255, 200), width=2)   # aura
        d.polygon([(sx-6, sy+7), (sx+6, sy+7), (sx+4, sy-4), (sx-4, sy-4)], fill=(210, 235, 250, 255))  # body
        d.polygon([(sx-4, sy-4), (sx+4, sy-4), (sx, sy-10)], fill=(150, 210, 240, 255))  # cap
        for k in range(4):
            a = sharpen * 0.9 + k * 1.57
            px_, py_ = sx + math.cos(a) * 13, sy + math.sin(a) * 13
            d.ellipse([px_-1.5, py_-1.5, px_+1.5, py_+1.5], fill=PENCIL)                  # shavings

    # HOLLOW ring head (empty circle — no fill, no face)
    hr = 9
    d.ellipse([headcT[0]-hr, headcT[1]-hr, headcT[0]+hr, headcT[1]+hr], outline=INK, width=3)

    # TOP HAT sitting on the ring
    brimw, hatw, hath = hr + 6, hr + 1, 15
    brimy = headcT[1] - hr + 1
    d.rectangle([headcT[0]-brimw, brimy, headcT[0]+brimw, brimy+3], fill=INK)      # brim
    d.rectangle([headcT[0]-hatw, brimy-hath, headcT[0]+hatw, brimy], fill=INK)     # crown
    return im


def clip_frames(name):
    if name == "idle":
        return [phil_frame(pencilang=-0.5, bob=b, arm=0.0)
                for b in (0, 1, 2, 1)]
    if name == "walk":
        return [phil_frame(legphase=ph, pencilang=-0.4, bob=(0 if i % 2 else 1))
                for i, ph in enumerate((0, math.pi/2, math.pi, 3*math.pi/2))]
    if name == "attack_side":
        # wind the pencil back, then stab forward
        return [phil_frame(pencilang=-1.1, arm=-0.2, lean=-0.1),
                phil_frame(pencilang=-0.7, arm=0.1, lean=0.0),
                phil_frame(pencilang=-0.1, arm=0.9, lean=0.25),
                phil_frame(pencilang=-0.2, arm=0.7, lean=0.15)]
    if name == "hurt":
        return [phil_frame(lean=-0.2, hurt=1.0, pencilang=-0.8),
                phil_frame(lean=-0.1, hurt=0.5, pencilang=-0.6)]
    if name == "death":
        return [phil_frame(fall=f, pencilang=-0.6 - f, hurt=1.0)
                for f in (0.15, 0.45, 0.75, 1.0)]
    if name == "sharpen":
        # Phil raises the double-pencil and works the Holy Sharpener over the top tip.
        return [phil_frame(sharpen=i, bob=(i % 2)) for i in range(6)]
    return [phil_frame()]


CLIPS = ["idle", "walk", "attack_side", "hurt", "death", "sharpen"]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--preview-dir", default=None)
    args = ap.parse_args()

    frames_dir = os.path.join(ROOT, "build", "phil_frames")
    if os.path.isdir(frames_dir):
        shutil.rmtree(frames_dir)
    os.makedirs(frames_dir)

    for clip in CLIPS:
        for i, im in enumerate(clip_frames(clip)):
            im.save(os.path.join(frames_dir, f"{clip}_{i:02d}.png"))
            if args.preview_dir:
                os.makedirs(args.preview_dir, exist_ok=True)
                im.resize((W*3, H*3), Image.NEAREST).save(
                    os.path.join(args.preview_dir, f"{clip}_{i:02d}.png"))
    print(f"[phil] wrote frames -> {frames_dir}")

    out = os.path.join(ROOT, "assets", "sprites", "bosses", "phil")
    rc = subprocess.call([sys.executable, os.path.join(HERE, "frames_to_atlas.py"),
                          frames_dir, "phil", "--outdir", out, "--no-trim"])
    print(f"[phil] atlas rc={rc} -> {out}")
    return rc


if __name__ == "__main__":
    sys.exit(main())
