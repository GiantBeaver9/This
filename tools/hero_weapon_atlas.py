#!/usr/bin/env python3
"""hero_weapon_atlas.py <zip> <state_folder> <actor_id> [--direction east]
Extract ONE state's east-facing animation frames from a Pixellab character-state
zip (which bundles every state in the group as <State>/animations/<clip>/<dir>/)
and atlas them into assets/sprites/characters/<actor_id>/. Isolates a single
state so a weapon-hold variant doesn't merge clips with the base Idle state.
"""
import argparse, glob, os, re, shutil, subprocess, sys, tempfile, zipfile

HERE = os.path.dirname(os.path.abspath(__file__))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("zip")
    ap.add_argument("state")          # e.g. SwordCorpse
    ap.add_argument("actor")          # e.g. player_tactical_sword
    ap.add_argument("--direction", default="east")
    ap.add_argument("--outdir", default=None)
    args = ap.parse_args()

    tmp = tempfile.mkdtemp()
    with zipfile.ZipFile(args.zip) as z:
        z.extractall(tmp)

    frames_dir = os.path.join("build", f"{args.actor}_frames")
    if os.path.isdir(frames_dir):
        shutil.rmtree(frames_dir)
    os.makedirs(frames_dir)

    pat = re.compile(
        re.escape(args.state) + r"[/\\]animations[/\\](?P<clip>[^/\\]+)[/\\]" +
        re.escape(args.direction) + r"[/\\]frame_(?P<idx>\d+)\.png$", re.IGNORECASE)

    n = 0; clips = set()
    for path in glob.glob(os.path.join(tmp, "**", "*.png"), recursive=True):
        m = pat.search(path.replace("\\", "/"))
        if not m:
            continue
        clip, idx = m["clip"], int(m["idx"])
        shutil.copy(path, os.path.join(frames_dir, f"{clip}_{idx:02d}.png"))
        clips.add(clip); n += 1

    if n == 0:
        print(f"[hero_weapon_atlas] no '{args.state}/.../{args.direction}' frames in {args.zip}", file=sys.stderr)
        return 1
    print(f"[hero_weapon_atlas] {n} frames / {len(clips)} clips ({', '.join(sorted(clips))})")
    cmd = [sys.executable, os.path.join(HERE, "frames_to_atlas.py"), frames_dir, args.actor]
    if args.outdir:
        cmd += ["--outdir", args.outdir]
    return subprocess.call(cmd)


if __name__ == "__main__":
    sys.exit(main())
