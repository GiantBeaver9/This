#!/usr/bin/env python3
"""
pixellab_zip_to_atlas.py — turn a Pixellab character .zip into the game's atlas.

The zip lays frames out as  <State>/animations/<clip>/<direction>/frame_NNN.png .
We take one direction (default east = right-facing for the side-scroller), rename
frames to <clip>_<NN>.png, and run frames_to_atlas.py to produce the actor's
<actor>_atlas.png + <actor>.json under assets/sprites/characters/<actor>/.

Usage:
    python tools/pixellab_zip_to_atlas.py CHAR.zip ACTOR_ID [--direction east]
"""
import argparse
import glob
import os
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("zip")
    ap.add_argument("actor")
    ap.add_argument("--direction", default="east")
    ap.add_argument("--outdir", default=None, help="output dir (default assets/sprites/characters/<actor>)")
    args = ap.parse_args()

    tmp = tempfile.mkdtemp()
    with zipfile.ZipFile(args.zip) as z:
        z.extractall(tmp)

    frames_dir = os.path.join("build", f"{args.actor}_frames")
    if os.path.isdir(frames_dir):
        shutil.rmtree(frames_dir)
    os.makedirs(frames_dir)

    pat = re.compile(
        r"animations[/\\](?P<clip>[^/\\]+)[/\\]" + re.escape(args.direction) +
        r"[/\\]frame_(?P<idx>\d+)\.png$", re.IGNORECASE)

    n = 0
    clips = set()
    for path in glob.glob(os.path.join(tmp, "**", "*.png"), recursive=True):
        m = pat.search(path.replace("\\", "/"))
        if not m:
            continue
        clip, idx = m["clip"], int(m["idx"])
        shutil.copy(path, os.path.join(frames_dir, f"{clip}_{idx:02d}.png"))
        clips.add(clip)
        n += 1

    if n == 0:
        print(f"[pixellab_zip_to_atlas] no '{args.direction}' frames found in {args.zip}", file=sys.stderr)
        return 1

    print(f"[pixellab_zip_to_atlas] extracted {n} frames / {len(clips)} clips ({', '.join(sorted(clips))})")
    cmd = [sys.executable, os.path.join(HERE, "frames_to_atlas.py"), frames_dir, args.actor]
    if args.outdir:
        cmd += ["--outdir", args.outdir]
    return subprocess.call(cmd)


if __name__ == "__main__":
    sys.exit(main())
