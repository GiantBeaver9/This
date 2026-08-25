"""this.l — Master palette (single source of truth).

Mirrors ASSET_MANIFEST.md §0: the LOCKED 32-color base palette plus the
per-area 6-color accent ramps. Every generated sprite draws only from these
hues so characters read consistently across areas (only the environment
accent ramp swaps per theme).

Gore red is fixed at #B31E2B across all areas (VFX.md).
"""

from __future__ import annotations

# --- 32-color base palette (ASSET_MANIFEST.md §0, exact hex) -----------------
# Grouped as the bible groups them; order here is the canonical palette index.
BASE_PALETTE_GROUPS = {
    # Ink/mono ramp (6) — stick figures are ink on paper.
    "ink": ["#0D0B0E", "#2A2A33", "#4A4A57", "#7A7A88", "#B8B8C4", "#F4F2EC"],
    # Reds (4) — gore red is the one fixed gore hue (LOCKED).
    "red": ["#B31E2B", "#E8433F", "#F5794F", "#7A1420"],
    # Oranges/yellows (4).
    "orange": ["#F2A03D", "#FFD24A", "#C77A2A", "#FFF2B0"],
    # Greens (4).
    "green": ["#234D2C", "#3A7D44", "#6CBF5A", "#A8D98B"],
    # Blues/cyans (5) — sky & water.
    "blue": ["#1B3A5C", "#2E6FB0", "#4AA3D8", "#9FD6EF", "#CFEAF7"],
    # Purples/pinks (2).
    "purple": ["#6A3D8A", "#C86FA8"],
    # Browns/earth (4).
    "brown": ["#3F2A17", "#6B4423", "#9C6B3F", "#C99A6A"],
    # Warm skin/accent (3).
    "skin": ["#8A5A3A", "#D99A6C", "#E6B88A"],
}

# Flat, ordered list of the 32 base colors (canonical index order).
BASE_PALETTE = [c for group in BASE_PALETTE_GROUPS.values() for c in group]
assert len(BASE_PALETTE) == 32, f"base palette must be 32 colors, got {len(BASE_PALETTE)}"

# Named handles for the colors sprites reach for most often.
GORE_RED = "#B31E2B"          # LOCKED — the single gore hue everywhere.
INK_BLACK = "#0D0B0E"
INK_DARK = "#2A2A33"
INK_MID = "#4A4A57"
INK_LIGHT = "#7A7A88"
INK_PALE = "#B8B8C4"
PAPER = "#F4F2EC"             # near-white "paper" the stick figures are inked on.

# --- Per-area 6-color accent ramps -------------------------------------------
# Six extra hues layered on top of the base 32, chosen per area (ASSET_MANIFEST
# §0 / §7). The base 32 stay constant; only these swap at a theme boundary.
AREA_ACCENTS = {
    # Area 1 — Lincoln suburbs / Galleria mall (bright, sunny, then neon mall).
    "area1_suburb": ["#8FBF3A", "#C7E06B", "#D98C5A", "#F0C9A0", "#7EC8E3", "#E8E2D0"],
    "area1_mall":   ["#FF5FA2", "#5FE0C8", "#B06CFF", "#FFE04A", "#3A9BD8", "#22222E"],
    # Area 2 — Sacramento Victorian / airport tarmac.
    "area2_victorian": ["#9C5B7A", "#5B7C9C", "#C9A15A", "#7A9C6B", "#D8C8A8", "#4A3A55"],
    "area2_airport":   ["#5A6B7A", "#8FA3B3", "#C9C24A", "#D85A3A", "#B0B8C0", "#2E3540"],
    # Area 3 — hills / farm / Dixon deserted town (spaghetti-western dust).
    "area3_farm":  ["#C9A85A", "#8FA35A", "#B07A3A", "#6B8C4A", "#E0D0A0", "#5A4A2A"],
    "area3_dixon": ["#C99A5A", "#A88A5A", "#8A6B3A", "#D8B87A", "#B0A080", "#4A3A2A"],
    # Area 4 — Vallejo carnival / Marin redwoods / Golden Gate / SF.
    "area4_carnival": ["#FF3A6E", "#3AE0D8", "#FFC83A", "#8A3AFF", "#3AA0FF", "#22182E"],
    "area4_redwood":  ["#3A5A2E", "#6B8C4A", "#8A5A3A", "#4A6B5A", "#A8C090", "#2A3A28"],
    "area4_bridge":   ["#D8452B", "#8A2E1A", "#5A7A9C", "#9FB8C8", "#C0C8D0", "#2E3A45"],
    "area4_sf":       ["#8A9BB0", "#C9C4B0", "#D85A3A", "#5A6B8A", "#E0DCD0", "#2A3040"],
    # Finale — Salesforce Tower rooftop, dusk.
    "finale_rooftop": ["#E0784A", "#8A5A9C", "#5A6B9C", "#C9A0C0", "#3A2E45", "#1A1520"],
}


def hex_to_rgb(h: str) -> tuple[int, int, int]:
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16))


def hex_to_rgba(h: str, a: int = 255) -> tuple[int, int, int, int]:
    r, g, b = hex_to_rgb(h)
    return (r, g, b, a)


def rgb(h: str) -> tuple[int, int, int]:
    """Short alias used throughout the generators."""
    return hex_to_rgb(h)


def palette_for(area_key: str | None = None) -> list[str]:
    """Base 32 + the named area accent ramp (38 hues), or just the base 32."""
    if area_key is None:
        return list(BASE_PALETTE)
    return list(BASE_PALETTE) + AREA_ACCENTS[area_key]
