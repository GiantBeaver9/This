#!/usr/bin/env python3
"""fetch_prop.py <object_id> <dest_png>
Download a Pixellab map-object PNG, trim transparent margins to bbox, save to dest.
"""
import io, os, sys, urllib.request
from PIL import Image

TOK = "185a1f7e-d7cf-4a89-b331-f6bd4b140ada"


def main():
    oid, dest = sys.argv[1], sys.argv[2]
    url = f"https://api.pixellab.ai/mcp/map-objects/{oid}/download"
    req = urllib.request.Request(url, headers={"Authorization": f"Bearer {TOK}"})
    data = urllib.request.urlopen(req, timeout=60).read()
    im = Image.open(io.BytesIO(data)).convert("RGBA")
    bbox = im.getbbox()
    if bbox:
        im = im.crop(bbox)
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    im.save(dest)
    print(f"SAVED {dest}  {im.width}x{im.height}  ({os.path.getsize(dest)} bytes)")


if __name__ == "__main__":
    main()
