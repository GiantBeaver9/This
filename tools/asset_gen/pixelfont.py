"""this.l — bundled bitmap pixel font (ASSET_MANIFEST.md §0 [LOCKED]).

Single "chunky-arcade" all-caps display face, 5x7 ink-on-cell glyphs (~8px cap
height at the 640x360 internal res), uniform heavy strokes. Emits `ui_font.png`
(a glyph atlas, same import settings as sprites) + `ui_font.json` metrics.

Glyph set (LOCKED): A-Z, 0-9, and  $ . : cent % ! ? PLAY(triangle) MIDDOT / + -  plus SPACE.
The ':' is required for the Endless mm:ss timer (TUNING.md §8.3).
"""

from __future__ import annotations

from PIL import Image

from .common import blank, next_pow2, save_png, write_json
from .palette import rgb, INK_BLACK

GW, GH = 5, 7          # glyph pixel box
CELL_W, CELL_H = 6, 8  # atlas cell (glyph + 1px spacing / descender)

# Each glyph = 7 rows of 5 chars; '#' is an ink pixel.
GLYPHS: dict[str, list[str]] = {
    "A": [".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    "B": ["####.", "#...#", "#...#", "####.", "#...#", "#...#", "####."],
    "C": [".####", "#....", "#....", "#....", "#....", "#....", ".####"],
    "D": ["####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####."],
    "E": ["#####", "#....", "#....", "####.", "#....", "#....", "#####"],
    "F": ["#####", "#....", "#....", "####.", "#....", "#....", "#...."],
    "G": [".####", "#....", "#....", "#..##", "#...#", "#...#", ".####"],
    "H": ["#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    "I": ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####"],
    "J": ["..###", "...#.", "...#.", "...#.", "#..#.", "#..#.", ".##.."],
    "K": ["#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#"],
    "L": ["#....", "#....", "#....", "#....", "#....", "#....", "#####"],
    "M": ["#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#"],
    "N": ["#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#"],
    "O": [".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
    "P": ["####.", "#...#", "#...#", "####.", "#....", "#....", "#...."],
    "Q": [".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#"],
    "R": ["####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#"],
    "S": [".####", "#....", "#....", ".###.", "....#", "....#", "####."],
    "T": ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."],
    "U": ["#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
    "V": ["#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.."],
    "W": ["#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#"],
    "X": ["#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#"],
    "Y": ["#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.."],
    "Z": ["#####", "....#", "...#.", "..#..", ".#...", "#....", "#####"],
    "0": [".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###."],
    "1": ["..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "2": [".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####"],
    "3": ["#####", "...#.", "..#..", "...#.", "....#", "#...#", ".###."],
    "4": ["...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#."],
    "5": ["#####", "#....", "####.", "....#", "....#", "#...#", ".###."],
    "6": [".###.", "#....", "#....", "####.", "#...#", "#...#", ".###."],
    "7": ["#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..."],
    "8": [".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###."],
    "9": [".###.", "#...#", "#...#", ".####", "....#", "....#", ".###."],
    "$": ["..#..", ".####", "#.#..", ".###.", "..#.#", "####.", "..#.."],
    ".": [".....", ".....", ".....", ".....", ".....", ".##..", ".##.."],
    ":": [".....", ".##..", ".##..", ".....", ".##..", ".##..", "....."],
    "¢": ["..#..", ".###.", "#.#..", "#.#..", "#.#..", ".###.", "..#.."],  # cent
    "%": ["##..#", "##.#.", "...#.", "..#..", ".#...", ".#.##", "#..##"],
    "!": ["..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#.."],
    "?": [".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.."],
    "▶": [".#...", ".##..", ".###.", ".####", ".###.", ".##..", ".#..."],  # play triangle
    "·": [".....", ".....", ".....", ".##..", ".##..", ".....", "....."],  # middot
    "/": ["....#", "....#", "...#.", "..#..", ".#...", "#....", "#...."],
    "+": [".....", "..#..", "..#..", "#####", "..#..", "..#..", "....."],
    "-": [".....", ".....", ".....", "#####", ".....", ".....", "....."],
    " ": [".....", ".....", ".....", ".....", ".....", ".....", "....."],
}

# Atlas order: the LOCKED glyph set, space last.
GLYPH_ORDER = (
    list("ABCDEFGHIJKLMNOPQRSTUVWXYZ")
    + list("0123456789")
    + ["$", ".", ":", "¢", "%", "!", "?", "▶", "·", "/", "+", "-", " "]
)


def _draw_glyph(img: Image.Image, ox: int, oy: int, rows: list[str], color) -> None:
    px = img.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == "#":
                px[ox + x, oy + y] = color


def generate(out_dir: str) -> tuple[int, int]:
    color = rgb(INK_BLACK) + (255,)
    cols = 16
    rows_n = (len(GLYPH_ORDER) + cols - 1) // cols
    page_w = next_pow2(cols * CELL_W)
    page_h = next_pow2(rows_n * CELL_H)
    atlas = blank(page_w, page_h)

    metrics = {}
    for i, g in enumerate(GLYPH_ORDER):
        cx = (i % cols) * CELL_W
        cy = (i // cols) * CELL_H
        _draw_glyph(atlas, cx, cy, GLYPHS[g], color)
        metrics[g] = {"rect": [cx, cy, GW, GH], "advance": CELL_W}

    save_png(atlas, f"{out_dir}/ui_font.png")
    write_json(
        {
            "face": "chunky-arcade",
            "cap_height": GH,
            "cell": [CELL_W, CELL_H],
            "line_height": CELL_H,
            "atlas": "ui_font.png",
            "atlas_size": [page_w, page_h],
            "columns": cols,
            "glyphs": metrics,
            "import": {"type": "Sprite2D", "filter": "Point", "compression": "None"},
            "placeholder": True,
        },
        f"{out_dir}/ui_font.json",
    )
    return page_w, page_h


def render_text(text: str, color=None, scale: int = 1, tracking: int = 0) -> Image.Image:
    """Render a string with this font — used by UI generators (combo popups,
    name cards, HUD numerals). Returns a tight RGBA image."""
    color = (rgb(INK_BLACK) + (255,)) if color is None else color
    text = text.upper()
    glyphs = [g if g in GLYPHS else " " for g in text]
    w = sum(CELL_W + tracking for _ in glyphs)
    img = blank(max(1, w * scale), GH * scale)
    x = 0
    tmp = blank(max(1, w), GH)
    for g in glyphs:
        _draw_glyph(tmp, x, 0, GLYPHS[g], color)
        x += CELL_W + tracking
    if scale != 1:
        tmp = tmp.resize((tmp.width * scale, tmp.height * scale), Image.NEAREST)
    return tmp
