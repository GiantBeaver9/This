"""Actor sprite generation — players and enemies — as posed stick-figure clips.

Pose builders drive figure.draw_figure through each animation. Frame counts use
the LOCKED upper-bound rule (ASSET_MANIFEST.md §0): idle 4, walk 8, attack 5,
hurt 2, death 6, dash 3, jump 3, land 2. Names follow `actor_action_dir_frame`.
"""

from __future__ import annotations

import math

from .common import PLAYER_H, SWARMER_H, export_actor
from .figure import draw_figure
from .palette import (
    INK_BLACK, INK_MID, GORE_RED, PAPER,
    BASE_PALETTE_GROUPS,
)

# Neutral standing pose (figure facing screen-right, side view).
NEUTRAL = dict(
    l_hip=10, l_knee=4, r_hip=-10, r_knee=4,
    l_sh=18, l_el=8, r_sh=-18, r_el=-8,
    dx=0, dy=0, lean=0,
)


def _p(**over):
    p = dict(NEUTRAL)
    p.update(over)
    return p


def _frame_size(h):
    return round(h * 1.12), round(h * 1.36), round(h * 1.36) - round(h * 0.06)


# --- pose builders (each returns a list of pose dicts) -----------------------
def idle_poses(n=4):
    out = []
    for i in range(n):
        bob = -1 if i in (1, 2) else 0
        sway = 3 * math.sin(i / n * 2 * math.pi)
        out.append(_p(dy=bob, l_sh=18 + sway, r_sh=-18 - sway))
    return out


def walk_poses(n=8):
    out = []
    for i in range(n):
        t = i / n * 2 * math.pi
        swing = 26 * math.sin(t)
        bob = -1 if math.cos(t) > 0.5 else 0
        out.append(_p(
            l_hip=swing, l_knee=max(0, -18 * math.cos(t)),
            r_hip=-swing, r_knee=max(0, 18 * math.cos(t)),
            l_sh=18 - swing * 0.6, r_sh=-18 + swing * 0.6,
            dy=bob, lean=2,
        ))
    return out


def dash_poses(n=3):
    return [
        _p(lean=6, l_hip=30, r_hip=-24, l_sh=40, r_sh=-40, dx=-2),
        _p(lean=9, l_hip=36, r_hip=-30, l_sh=46, r_sh=-46, dx=2),
        _p(lean=7, l_hip=24, r_hip=-20, l_sh=36, r_sh=-36, dx=4),
    ]


def jump_poses(n=3):
    return [
        _p(dy=-2, l_hip=16, l_knee=20, r_hip=-16, r_knee=20, l_sh=30, r_sh=-30),  # rise/crouch-launch
        _p(dy=-6, l_hip=20, l_knee=28, r_hip=-14, r_knee=24, l_sh=-20, r_sh=20),  # peak, tucked
        _p(dy=-3, l_hip=10, l_knee=8, r_hip=-10, r_knee=8, l_sh=24, r_sh=-24),    # fall
    ]


def land_poses(n=2):
    return [
        _p(dy=2, l_hip=22, l_knee=34, r_hip=-22, r_knee=34, l_sh=34, r_sh=-34),
        _p(dy=0, l_hip=12, l_knee=10, r_hip=-12, r_knee=10),
    ]


def attack_side_poses(n=5, weapon=False):
    seq = [
        _p(r_sh=-70, r_el=-30, lean=-2, wpn_ang=-70),     # wind-up (arm back)
        _p(r_sh=-20, r_el=-10, lean=2, wpn_ang=-10),      # swing
        _p(r_sh=40, r_el=10, lean=5, wpn_ang=55),         # active (arm forward)
        _p(r_sh=30, r_el=8, lean=3, wpn_ang=45),          # follow-through
        _p(r_sh=-10, r_el=-6, lean=0, wpn_ang=-10),       # recover
    ]
    return seq[:n]


def attack_up_poses(n=4):
    return [
        _p(r_sh=-30, r_el=-20, wpn_ang=-30),
        _p(r_sh=-70, r_el=-30, wpn_ang=-80),
        _p(r_sh=-95, r_el=-20, wpn_ang=-110),
        _p(r_sh=-60, r_el=-15, wpn_ang=-70),
    ][:n]


def attack_down_poses(n=4):
    return [
        _p(r_sh=-80, r_el=-20, wpn_ang=-90),
        _p(r_sh=-20, r_el=-10, wpn_ang=-10),
        _p(r_sh=60, r_el=20, wpn_ang=95, lean=6),
        _p(r_sh=40, r_el=10, wpn_ang=70, lean=3),
    ][:n]


def air_side_poses(n=3):
    return [
        _p(dy=-4, l_hip=18, r_hip=-16, r_sh=-50, wpn_ang=-40),
        _p(dy=-4, l_hip=16, r_hip=-14, r_sh=30, lean=4, wpn_ang=50),
        _p(dy=-4, l_hip=14, r_hip=-12, r_sh=20, wpn_ang=35),
    ][:n]


def hurt_poses(n=2):
    return [
        _p(lean=-6, dx=-2, r_sh=-40, l_sh=40, r_el=-20, l_el=20),
        _p(lean=-4, dx=-1, r_sh=-30, l_sh=30),
    ][:n]


def death_poses(n=6):
    return [
        _p(lean=-8, dx=-2, r_sh=-50, l_sh=50),
        _p(lean=-16, dx=-3, r_sh=-70, l_sh=70, l_knee=20, r_knee=20),
        _p(lean=-30, dx=-4, dy=2, r_sh=-90, l_sh=90, l_hip=40, r_hip=-40),
        _p(lean=-55, dx=-6, dy=6, l_hip=70, r_hip=-70),
        _p(lean=-80, dx=-8, dy=12, l_hip=85, r_hip=-85),
        _p(lean=-90, dx=-10, dy=16, l_hip=90, r_hip=-90, l_sh=90, r_sh=-90),
    ][:n]


def _clip(style, poses, direction="side"):
    h = style["height"]
    fw, fh, gy = _frame_size(h)
    frames = [draw_figure(fw, fh, gy, style, p) for p in poses]
    return {"dir": direction, "frames": frames}


# --- actor style definitions -------------------------------------------------
BLUES = BASE_PALETTE_GROUPS["blue"]
GREENS = BASE_PALETTE_GROUPS["green"]
ORANGE = BASE_PALETTE_GROUPS["orange"]

PLAYER_STYLES = {
    # Tactical (you) — sniper; cool blue accent scarf, standard build.
    "player_tactical": dict(height=PLAYER_H, ink=INK_BLACK, accent=BLUES[2], bulk=1.0),
    # Shotgunner — bulky redhead.
    "player_shotgunner": dict(height=PLAYER_H, ink=INK_BLACK, accent=GORE_RED,
                              bulk=1.25, hair=GORE_RED),
    # Werewolf (Gabe) — brown/earth accent (human form placeholder).
    "player_werewolf": dict(height=PLAYER_H, ink=INK_BLACK, accent="#6B4423", bulk=1.1),
    # Underdog — short, hard-mode; bright orange accent.
    "player_underdog": dict(height=round(PLAYER_H * 0.85), ink=INK_BLACK,
                            accent=ORANGE[1], bulk=0.95),
}

ENEMY_STYLES = {
    # Regular Melee — plain ink figure, muted red band.
    "enemy_regular": dict(height=PLAYER_H, ink=INK_BLACK, accent="#7A1420", bulk=1.0),
    # Swarmer — half height, quick.
    "enemy_swarmer": dict(height=SWARMER_H, ink=INK_MID, accent="#7A1420", bulk=0.9),
    # Zombie — hunched, hollow-head-capable, sickly green accent.
    "enemy_zombie": dict(height=PLAYER_H, ink="#2A2A33", accent=GREENS[1],
                         bulk=1.0, hunch=1.0),
}


def _player_clips(style):
    return {
        "idle": _clip(style, idle_poses()),
        "walk": _clip(style, walk_poses()),
        "dash": _clip(style, dash_poses()),
        "jump": _clip(style, jump_poses()),
        "land": _clip(style, land_poses()),
        "attack_side": _clip(style, attack_side_poses()),
        "attack_up": _clip(style, attack_up_poses()),
        "attack_down": _clip(style, attack_down_poses()),
        "air_side": _clip(style, air_side_poses()),
        "hurt": _clip(style, hurt_poses()),
        "death": _clip(style, death_poses()),
    }


def _enemy_clips(style, hollow_death=False):
    clips = {
        "idle": _clip(style, idle_poses()),
        "walk": _clip(style, walk_poses()),
        "attack_side": _clip(style, attack_side_poses()),
        "hurt": _clip(style, hurt_poses()),
        "death": _clip(style, death_poses()),
    }
    if hollow_death:
        # Zombie hollow-head state: a second idle with the head knocked hollow.
        hs = dict(style)
        hs["head_hollow"] = True
        clips["idle_hollow"] = _clip(hs, idle_poses())
    return clips


def generate(out_dir: str) -> dict:
    """Emit all P0 player + enemy actors. Returns {actor: atlas_size}."""
    sizes = {}
    for name, style in PLAYER_STYLES.items():
        sizes[name] = export_actor(
            f"{out_dir}/characters", name, _player_clips(style),
            meta_extra={"tier": "P0", "kind": "player"},
        )
    for name, style in ENEMY_STYLES.items():
        hollow = name == "enemy_zombie"
        sizes[name] = export_actor(
            f"{out_dir}/enemies", name, _enemy_clips(style, hollow_death=hollow),
            meta_extra={"tier": "P0", "kind": "enemy"},
        )
    return sizes
