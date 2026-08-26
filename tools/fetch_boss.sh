#!/usr/bin/env bash
# fetch_boss.sh <character_id> <boss_id>
# Downloads a completed Pixellab character zip and atlases it into assets/sprites/bosses/<boss_id>/
set -e
TOK=185a1f7e-d7cf-4a89-b331-f6bd4b140ada
CID="$1"; BID="$2"
mkdir -p build/pixellab
curl -sL --fail -H "Authorization: Bearer $TOK" \
  "https://api.pixellab.ai/mcp/characters/$CID/download" -o "build/pixellab/$BID.zip"
python tools/pixellab_zip_to_atlas.py "build/pixellab/$BID.zip" "$BID" --outdir "assets/sprites/bosses/$BID"
echo "ATLASED boss $BID"
