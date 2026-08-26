#!/usr/bin/env python3
"""skykey.py <png> [tolerance]
Make a painted-scene landmark usable as a prop: flood-fill from the 4 corners,
turning contiguous background pixels within `tolerance` of the corner color
transparent. Preserves internal shapes (bridge, hills, water stay if not
reachable from an edge as near-background color). Re-trims to bbox. In place.
"""
import sys, collections
from PIL import Image

def main():
    path = sys.argv[1]
    tol = int(sys.argv[2]) if len(sys.argv) > 2 else 42
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    seeds = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
    # reference background colors = corner colors
    refs = [px[x, y][:3] for (x, y) in seeds]
    def near(c):
        return any(abs(c[0]-r[0]) + abs(c[1]-r[1]) + abs(c[2]-r[2]) <= tol for r in refs)
    seen = [[False]*w for _ in range(h)]
    dq = collections.deque()
    for (x, y) in seeds:
        if not seen[y][x] and near(px[x, y][:3]):
            seen[y][x] = True; dq.append((x, y))
    cleared = 0
    while dq:
        x, y = dq.popleft()
        r, g, b, a = px[x, y]
        px[x, y] = (r, g, b, 0); cleared += 1
        for nx, ny in ((x+1,y),(x-1,y),(x,y+1),(x,y-1)):
            if 0 <= nx < w and 0 <= ny < h and not seen[ny][nx] and near(px[nx, ny][:3]):
                seen[ny][nx] = True; dq.append((nx, ny))
    bbox = im.getbbox()
    if bbox:
        im = im.crop(bbox)
    im.save(path)
    tp = sum(1 for a in im.getchannel("A").get_flattened_data() if a < 8)
    print(f"KEYED {path}: cleared {cleared}px, now {im.size} transparent={tp/(im.width*im.height):.0%}")

if __name__ == "__main__":
    main()
