"""Procedural placeholder audio — SFX one-shots + music/ambient beds.

These are throwaway synth placeholders so the prototype has sound against the
real shot-list (ASSET_CHECKLIST.md §B/§C, AUDIO.md). VO + SFX are ultimately
creator-produced; music is creator-composed (CC0 fallback). Output is WAV
(Unity-native); music targets OGG later. SFX = mono 44.1 kHz (AUDIO.md spec);
music/ambient placeholders = mono 22.05 kHz to keep the repo light.
"""

from __future__ import annotations

import os
import struct
import wave

import numpy as np

SR = 44100
MSR = 22050  # music/ambient placeholder rate


def _write_wav(path, samples, sr):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    s = np.clip(samples, -1.0, 1.0)
    pcm = (s * 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(pcm.tobytes())


def _env(n, a=0.005, d=0.1, sr=SR, curve=3.0):
    """AD envelope (attack then exponential decay) over n samples."""
    t = np.arange(n) / sr
    atk = np.clip(t / max(a, 1e-4), 0, 1)
    dec = np.exp(-(np.maximum(t - a, 0)) / max(d, 1e-4) * curve)
    return atk * dec


def _tone(freq, dur, sr=SR, a=0.005, d=0.12, wave_="sine"):
    n = int(dur * sr)
    t = np.arange(n) / sr
    if wave_ == "square":
        y = np.sign(np.sin(2 * np.pi * freq * t))
    elif wave_ == "saw":
        y = 2 * (t * freq - np.floor(0.5 + t * freq))
    else:
        y = np.sin(2 * np.pi * freq * t)
    return y * _env(n, a, d, sr)


def _sweep(f0, f1, dur, sr=SR, a=0.005, d=0.15, wave_="sine"):
    n = int(dur * sr)
    t = np.arange(n) / sr
    f = np.linspace(f0, f1, n)
    ph = 2 * np.pi * np.cumsum(f) / sr
    y = np.sign(np.sin(ph)) if wave_ == "square" else np.sin(ph)
    return y * _env(n, a, d, sr)


def _noise(dur, sr=SR, a=0.002, d=0.1, rng=None, low=False):
    n = int(dur * sr)
    rng = rng or np.random.default_rng(0)
    y = rng.uniform(-1, 1, n)
    if low:  # cheap low-pass for thuds/rumbles
        for _ in range(6):
            y = np.convolve(y, np.ones(8) / 8, mode="same")
    return y * _env(n, a, d, sr)


def _mix(*parts):
    n = max(len(p) for p in parts)
    out = np.zeros(n)
    for p in parts:
        out[:len(p)] += p
    m = np.max(np.abs(out)) or 1.0
    return out / m * 0.9


# --- SFX recipes ------------------------------------------------------------
def _recipe(tag, seed):
    rng = np.random.default_rng(seed)
    p = 2 ** (rng.uniform(-2, 2) / 12)  # +/-2 semitone pitch randomization
    if tag == "punch":
        return _mix(_noise(0.09, low=True, rng=rng), _tone(150 * p, 0.08, d=0.05))
    if tag == "swipe":
        return _sweep(1800 * p, 400 * p, 0.12, d=0.08)
    if tag == "heavy":
        return _mix(_noise(0.18, low=True, rng=rng), _tone(90 * p, 0.2, d=0.14))
    if tag == "boom":
        return _mix(_noise(0.5, low=True, rng=rng, d=0.4), _tone(60 * p, 0.5, d=0.35))
    if tag == "blast":
        return _mix(_noise(0.25, rng=rng, d=0.18), _sweep(500 * p, 80 * p, 0.25, d=0.18))
    if tag == "shot":
        return _mix(_noise(0.06, rng=rng, d=0.04), _tone(220 * p, 0.05, d=0.03))
    if tag == "chime":
        return _mix(_tone(880 * p, 0.3, d=0.25), _tone(1320 * p, 0.3, d=0.22))
    if tag == "coin":
        return _mix(_tone(1200 * p, 0.08, d=0.06), _tone(1800 * p, 0.12, a=0.05, d=0.08))
    if tag == "moan":
        return _sweep(180 * p, 120 * p, 0.5, a=0.05, d=0.4, wave_="saw")
    if tag == "chitter":
        return _mix(*[_tone(rng.uniform(800, 2000), 0.03, d=0.02) for _ in range(4)])
    if tag == "zap":
        return _sweep(300 * p, 2400 * p, 0.15, d=0.1, wave_="square")
    if tag == "thud":
        return _noise(0.15, low=True, rng=rng, d=0.1)
    if tag == "click":
        return _tone(2000 * p, 0.03, d=0.02, wave_="square")
    if tag == "alarm":
        return _mix(_tone(700 * p, 0.4, d=0.4, wave_="square"),
                    _tone(700 * p, 0.4, d=0.4, wave_="square") * (np.sin(np.arange(int(0.4 * SR)) / SR * 2 * np.pi * 8) > 0))
    if tag == "splash":
        return _noise(0.35, rng=rng, d=0.28)
    if tag == "bell":
        return _mix(_tone(660 * p, 0.6, d=0.5), _tone(990 * p, 0.6, d=0.4))
    if tag == "whoosh":
        return _sweep(200, 1600, 0.3, d=0.2) + _sweep(1600, 200, 0.3, d=0.2)
    if tag == "howl":
        return _sweep(300 * p, 700 * p, 0.7, a=0.1, d=0.6, wave_="saw")
    if tag == "cast":
        return _mix(_sweep(400 * p, 1600 * p, 0.4, d=0.3), _tone(800 * p, 0.4, d=0.3))
    if tag == "engine":
        return _noise(0.9, low=True, rng=rng, d=0.8) + _tone(110, 0.9, d=0.8, wave_="saw") * 0.3
    if tag == "creak":
        return _sweep(120, 90, 0.8, a=0.2, d=0.7, wave_="saw")
    if tag == "moo":
        return _sweep(240 * p, 160 * p, 0.6, a=0.08, d=0.5, wave_="saw")
    if tag == "pop":
        return _mix(_noise(0.04, rng=rng, d=0.03), _tone(500 * p, 0.05, d=0.03))
    if tag == "tick":
        return _tone(1400 * p, 0.02, d=0.015, wave_="square")
    return _tone(440 * p, 0.1)


# SFX shot-list (ASSET_CHECKLIST.md §B, grouped) -> synth recipe tag.
SFX = {
    "player_melee": {"punch_1": "punch", "punch_2": "punch", "sweep": "swipe",
                     "finisher_heavy": "heavy", "air_hit": "punch",
                     "dash_whoosh": "whoosh", "jump": "swipe", "land": "thud"},
    "player_states": {"hurt_grunt": "moan", "death": "heavy", "weapon_pickup": "coin",
                      "heal_pickup_chime": "chime", "weapon_break_puff": "pop",
                      "shield_rush_scrape": "whoosh"},
    "impacts": {"hit_spark": "punch", "finisher_crunch": "heavy", "enemy_stagger": "thud",
                "knockdown_thud": "thud", "block_soak": "thud", "screen_shake_boom": "boom"},
    "weapon_fire": {"sword_swing": "swipe", "sword_break": "pop", "shotgun_blast": "blast",
                    "shotgun_cock": "click", "boomerang_throw": "whoosh",
                    "boomerang_return": "whoosh", "pistol": "shot", "revolver": "shot",
                    "grenade_throw": "whoosh", "grenade_explode": "boom",
                    "ballchain_launch": "whoosh", "ballchain_impact": "boom",
                    "whip_crack": "zap", "whip_headrip_pop": "pop", "staff_ice": "cast",
                    "staff_fire": "cast", "staff_lightning": "zap", "gatling_barrage": "shot",
                    "boomeranggun_spin": "whoosh", "boomeranggun_shotdown_break": "pop",
                    "rocket_launch": "whoosh", "rocket_blast": "boom", "club_whack": "heavy",
                    "bat_reflect_ping": "tick"},
    "enemy_signature": {"zombie_moan": "moan", "zombie_grab": "thud", "swarmer_chitter": "chitter",
                        "head_throw": "whoosh", "fire_blink_boom": "boom",
                        "snapper_snap_to_sword": "click", "arm_rip": "heavy",
                        "gatling_contort": "zap", "ninja_smoke_teleport": "whoosh",
                        "sniper_scope_in": "tick", "sniper_shot": "shot",
                        "groundsmash_overhead": "heavy", "groundsmash_shockwave": "boom",
                        "tamer_whistle": "chime", "monkey_merc_chatter": "chitter",
                        "aa_rock_throw": "whoosh", "ninja_shuriken_throw": "swipe",
                        "boomergunner_gun_throw": "whoosh", "pod_spawn_burst": "blast",
                        "tank_mg_stream": "shot"},
    "meter_specials": {"meter_tick_up": "tick", "armed_ready_chime": "chime",
                       "sniper_timeslow_enter": "whoosh", "time_resume_whoosh": "whoosh",
                       "werewolf_transform_howl": "howl", "werewolf_auto_slash": "swipe",
                       "giant_shotgun_boom": "boom", "underdog_vaporize_whomp": "boom"},
    "phil_finale": {"pencil_draw_scribble": "chitter", "sharpen_scrape": "whoosh",
                    "pencil_laser_fire": "zap"},
    "ui": {"menu_move": "tick", "confirm": "coin", "cancel": "click", "coin_pickup": "coin",
           "full_dime_highlight": "chime", "combo_popup_pips": "tick",
           "barrage_incoming_alarm": "alarm"},
    "economy": {"pickpocket_steal": "swipe", "coins_doubled_jingle": "coin",
                "checkpoint_chime": "bell"},
    "hazards": {"car_bus_passby": "engine", "car_horn": "alarm", "plane_jet_blast": "engine",
                "taxiing_plane_whine": "engine", "cow_moo": "moo", "trolley_bell": "bell",
                "trolley_rumble": "engine", "tower_sway_creak": "creak",
                "rollercoaster_pass": "engine", "causeway_water_splash": "splash"},
}


def generate_sfx(out_dir):
    count = 0
    idx = {}
    for group, items in SFX.items():
        for name, tag in items.items():
            seed = abs(hash((group, name))) % (2**31)
            _write_wav(f"{out_dir}/{group}/{name}.wav", _recipe(tag, seed), SR)
            idx.setdefault(group, []).append(name)
            count += 1
    return count, idx


# --- Music / ambient placeholders -------------------------------------------
_MAJOR = [0, 2, 4, 5, 7, 9, 11]


def _loop(root_hz, dur, seed, sr=MSR, kind="melody"):
    rng = np.random.default_rng(seed)
    n = int(dur * sr)
    out = np.zeros(n)
    if kind == "ambient":
        out += _noise(dur, sr=sr, a=0.5, d=dur, rng=rng, low=True) * 0.5
        out += np.sin(2 * np.pi * root_hz * np.arange(n) / sr) * 0.1
    else:
        beat = dur / 8
        for b in range(8):
            deg = rng.choice(_MAJOR)
            f = root_hz * 2 ** (deg / 12)
            seg = _tone(f, beat, sr=sr, a=0.01, d=beat * 0.9,
                        wave_="square" if kind == "boss" else "saw")
            s = int(b * beat * sr)
            out[s:s + len(seg)] += seg[:max(0, n - s)] * 0.6
        # bass pulse
        for b in range(4):
            seg = _tone(root_hz / 2, dur / 4, sr=sr, a=0.01, d=0.2, wave_="sine")
            s = int(b * dur / 4 * sr)
            out[s:s + len(seg)] += seg[:max(0, n - s)] * 0.5
    m = np.max(np.abs(out)) or 1.0
    return out / m * 0.85


STAGE_LOOPS = [
    "a1_surfrock_opener", "a1_synthpunk_mall", "a2_ragtime_garagerock",
    "a2_industrial_electronic", "a3_spaghetti_western", "a3_hoedown_bluegrass",
    "a3_western_dread", "a4_circusrock", "a4_psychrock", "a4_orchestralrock",
    "a4_electropunk", "finale_rooftop_approach",
]
BOSS_CUES = ["burly", "colossus", "helicopter", "monkeyboss", "big_armripper",
             "tank", "gatlinggunguy", "boomergunner", "phil_realized"]
STINGERS = ["a1", "a2", "a3", "a4", "finale"]
AMBIENT = ["lincoln_birds_traffic", "galleria_murmur", "sacramento_oldtown",
           "airport_tarmac_jet", "causeway_marsh_wind", "farm_barnyard",
           "dixon_town_wind", "vallejo_carnival", "marin_redwood",
           "goldengate_bridge_wind", "sf_city_crowd", "finale_rooftop_wind"]


def generate_music(out_dir):
    roots = [220, 246, 262, 196, 174, 233, 165, 294, 208, 262, 233, 155]
    for i, name in enumerate(STAGE_LOOPS):
        _write_wav(f"{out_dir}/stage_loops/{name}.wav", _loop(roots[i % len(roots)], 3.0, i), MSR)
    _write_wav(f"{out_dir}/other/title_theme.wav", _loop(262, 3.5, 100), MSR)
    _write_wav(f"{out_dir}/other/endless_layered.wav", _loop(196, 3.5, 101), MSR)
    for i, name in enumerate(BOSS_CUES):
        _write_wav(f"{out_dir}/boss_cues/{name}.wav", _loop(147, 2.5, 200 + i, kind="boss"), MSR)
    for i, name in enumerate(STINGERS):
        _write_wav(f"{out_dir}/stingers/{name}_stinger.wav", _loop(330, 1.2, 300 + i), MSR)
    for i, name in enumerate(AMBIENT):
        _write_wav(f"{out_dir}/ambient/{name}.wav", _loop(110, 3.0, 400 + i, kind="ambient"), MSR)
    return {"stage_loops": len(STAGE_LOOPS), "boss_cues": len(BOSS_CUES),
            "stingers": len(STINGERS), "ambient": len(AMBIENT), "other": 2}
