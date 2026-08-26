using UnityEditor;
using UnityEngine;

namespace ThisL.EditorTools
{
    /// <summary>
    /// Headless play-mode smoke test (run via -executeMethod ThisL.EditorTools.SmokeTest.Run).
    /// Enters play mode with domain reload disabled (so these statics survive), lets the
    /// game boot and run ~150 frames, drives a scripted input burst, then reports whether
    /// the bootstrap ran, actors spawned, and no exceptions fired — exiting 0 on success.
    /// </summary>
    public static class SmokeTest
    {
        private static int _frames;
        private static bool _started;
        private static bool _sawBoot;
        private static int _errors;
        private static int _maxEnemies;
        private static bool _sawPlayer;
        private static float _foeHpBefore = -1f;
        private static float _foeHpAfter = -1f;
        private static bool _foeDied;
        private static bool _combatRan;
        private static bool _weaponsFired;
        private static bool _sawP2;

        public static void Run()
        {
            Application.logMessageReceived += OnLog;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        private static void OnLog(string condition, string stack, LogType type)
        {
            if (condition != null && condition.Contains("GameBootstrap")) _sawBoot = true;
            if (type == LogType.Exception || type == LogType.Error)
            {
                // Ignore editor-internal noise unrelated to the game (Search DB indexing on startup).
                if (stack != null && stack.Contains("UnityEditor.Search")) return;
                if (condition != null && condition.Contains("audio listeners")) return;
                _errors++;
                Debug.Log($"SMOKE_GAME_ERROR: {condition}");
            }
        }

        private static void Tick()
        {
            if (EditorApplication.isPlaying)
            {
                _started = true;
                _frames++;

                // Drive the flow past Title -> pick Tactical -> Playing so gameplay actually runs.
                if (_frames == 20 && GameFlow.Instance != null && GameFlow.Instance.Current != GameFlow.State.Playing)
                    GameFlow.Instance.StartRun(CharacterDef.Tactical());

                // Blow through vignettes (they wait on input in batchmode) so the opening
                // chain proceeds and its code paths are exercised.
                if (VignettePlayer.Instance != null && VignettePlayer.Instance.IsPlaying)
                    VignettePlayer.Instance.Skip();

                // Force-spawn a real crowd so the back-off AI is exercised (batchmode
                // barely advances Time, so the timer ramp won't fire on its own).
                if (_frames == 30 && PlayerController.Instance != null)
                {
                    var p = PlayerController.Instance;
                    for (int i = 0; i < 12; i++) EnemySpawner.SpawnRegular(p);
                    for (int i = 0; i < 3; i++) EnemySpawner.SpawnGunner(p);
                    EnemySpawner.PlacePod(p.WorldX + 5f, 4f);
                }

                // Fire every character special once to exercise the payloads + SpecialFx (glow/ring).
                if (_frames == 50 && PlayerController.Instance != null)
                {
                    var p = PlayerController.Instance;
                    CharacterDef.Tactical().Special.Fire(p, 3);
                    CharacterDef.Shotgunner().Special.Fire(p, 3);
                    CharacterDef.Werewolf().Special.Fire(p, 3);
                    CharacterDef.Underdog().Special.Fire(p, 3);
                }

                // Force-start the gate-based level spawner to exercise its Update/gate logic
                // (the flow only adds it after the tutorial, which needs input to complete).
                if (_frames == 45 && Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude).Length == 0)
                    new GameObject("SmokeSpawner").AddComponent<EnemySpawner>();

                // COMBAT CHECK: put a real spawned enemy in fist range, punch it via the
                // exact pipeline ResolveSwing uses, and confirm its HP drops and it dies.
                if (_frames == 60 && !_combatRan && PlayerController.Instance != null)
                {
                    var p = PlayerController.Instance;
                    Actor foe = null;
                    foreach (var a in Actor.All)
                        if (a != null && a.Alive && a.Team == Team.Enemy) { foe = a; break; }
                    if (foe != null)
                    {
                        foe.WorldX = p.WorldX + p.Facing * 1.0f;
                        foe.Z = p.Z;
                        _foeHpBefore = foe.Hp;
                        for (int i = 0; i < 6; i++)      // ~6 punches
                            Combat.MeleeHitDirectional(p, new Vector2(p.Facing, 0f), 2.2f, 1.3f, 10, 1f);
                        _foeHpAfter = foe.Hp;
                        _foeDied = !foe.Alive;
                        _combatRan = true;
                    }
                }

                // Uppercut LAUNCH: pop an enemy so the arc/gravity/land-knockdown cycle runs.
                if (_frames == 66 && PlayerController.Instance != null)
                    foreach (var a in Actor.All)
                        if (a is EnemyController le && le.Alive) { le.Launch(14f, 3f); break; }

                // Exercise every area's backdrop so a malformed/missing real prop sprite
                // (assets/backdrops/areaN_props/*.png) surfaces as a logged exception.
                if (_frames == 70)
                    for (int a = 1; a <= 4; a++) Backdrop.SetArea(a);

                // Equip a sword so the weapon-skin overlay (idle/swing) path runs (any NRE → error).
                if (_frames == 75 && PlayerController.Instance != null)
                    PlayerController.Instance.CurrentWeapon = Weapon.Create(WeaponKind.Sword);

                // Fire EVERY ranged weapon once so all FireImpl paths run at least once (pistol/
                // revolver/gatling zombify, staff status bolt, grenade blast, boomerang, ball&chain
                // launch, shotgun). Projectiles then connect with the live crowd over later frames,
                // exercising zombify/staff-status/walking-bomb/reflect on-hit. Any NRE → errors++.
                if (_frames == 80 && !_weaponsFired && PlayerController.Instance != null)
                {
                    _weaponsFired = true;
                    var p = PlayerController.Instance;
                    foreach (WeaponKind k in System.Enum.GetValues(typeof(WeaponKind)))
                    {
                        if (k == WeaponKind.Fists) continue;
                        var w = Weapon.Create(k);
                        p.CurrentWeapon = w;
                        w.FireCooldown = 0f;
                        w.TryFire(p);   // no-op for pure-melee kinds; fires the ranged ones
                    }
                    p.CurrentWeapon = Weapon.Fists();
                }

                // Drop in P2 so the co-op paths run (2-player HUD, shared lives, and the explosive
                // teammate friendly-fire when the next grenade detonates near both). Crash coverage.
                if (_frames == 85 && GameFlow.Instance != null && PlayerController.All.Count < 2)
                    GameFlow.Instance.TryJoinPlayer2(0);
                if (PlayerController.All.Count >= 2) _sawP2 = true;

                // With P2 present, lob a grenade right onto the pair to exercise teammate friendly-fire.
                if (_frames == 95 && _sawP2 && PlayerController.Instance != null)
                {
                    var p = PlayerController.Instance;
                    var g = Weapon.Create(WeaponKind.Grenade);
                    p.CurrentWeapon = g; g.FireCooldown = 0f; g.TryFire(p);
                    p.CurrentWeapon = Weapon.Fists();
                }

                if (PlayerController.Instance != null) _sawPlayer = true;
                int enemies = 0;
                foreach (var a in Actor.All)
                    if (a != null && a.Alive && a.Team == Team.Enemy) enemies++;
                if (enemies > _maxEnemies) _maxEnemies = enemies;

                if (_frames > 400) Finish();
            }
            else if (_started)
            {
                Finish();
            }
        }

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            bool combatOk = _combatRan && _foeHpAfter < _foeHpBefore && _foeDied;
            bool ok = _sawBoot && _sawPlayer && _errors == 0 && combatOk;
            Debug.Log($"SMOKE_RESULT boot={_sawBoot} player={_sawPlayer} p2={_sawP2} maxEnemies={_maxEnemies} errors={_errors} frames={_frames} " +
                      $"combatRan={_combatRan} foeHp={_foeHpBefore}->{_foeHpAfter} foeDied={_foeDied} ok={ok}");
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(ok ? 0 : 2);
        }

        // ================= Campaign chain smoke (RunCampaign) =====================
        // Verifies every boss id instantiates without throwing, and that the 13-stage
        // campaign walks end-to-end via CampaignRunner.SkipToNext. Run with
        //   -executeMethod ThisL.EditorTools.SmokeTest.RunCampaign
        private static readonly string[] BossIds =
        {
            "sandwich_bros", "burly", "colossus", "helicopter", "monkey_boss",
            "big_armripper", "tank", "boomergunner", "gatlinggunguy", "phil",
        };

        private static int _cFrames, _cBossesOk, _cMaxStage;
        private static bool _cStarted, _cSweptBosses;

        public static void RunCampaign()
        {
            Application.logMessageReceived += OnLog;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            EditorApplication.update += TickCampaign;
            EditorApplication.EnterPlaymode();
        }

        private static void TickCampaign()
        {
            if (EditorApplication.isPlaying)
            {
                _cStarted = true;
                _cFrames++;

                if (_cFrames == 20 && GameFlow.Instance != null && GameFlow.Instance.Current != GameFlow.State.Playing)
                    GameFlow.Instance.StartRun(CharacterDef.Tactical());
                if (VignettePlayer.Instance != null && VignettePlayer.Instance.IsPlaying)
                    VignettePlayer.Instance.Skip();

                // Boss sweep: spawn every boss and LET THEM LIVE a while so their Update/attack
                // patterns actually run (projectile spawns, phase logic) — the crash-prone part.
                // The first SkipToNext (frame 70+) clears them as Team.Enemy.
                if (_cFrames >= 40 && !_cSweptBosses && PlayerController.Instance != null)
                {
                    _cSweptBosses = true;
                    for (int i = 0; i < BossIds.Length; i++)
                    {
                        var b = Bosses.Spawn(BossIds[i], 90f + i * 7f, Mathf.Clamp(1.5f + i * 0.4f, 0f, 5f));
                        if (b != null) { _cBossesOk++; b.TakeDamage(20f, null); } // poke → phase/execute paths
                        else Debug.Log($"SMOKE_CAMPAIGN_BOSS_NULL: {BossIds[i]}");
                    }
                }

                // Ensure a campaign is running, then walk the whole 13-stage chain.
                if (_cFrames == 55 && CampaignRunner.Instance == null)
                    new GameObject("SmokeCampaign").AddComponent<CampaignRunner>();
                if (_cFrames > 70 && _cFrames % 10 == 0 && CampaignRunner.Instance != null)
                    CampaignRunner.Instance.SkipToNext();
                if (CampaignRunner.Instance != null)
                    _cMaxStage = Mathf.Max(_cMaxStage, CampaignRunner.Instance.CurrentStage);

                if (_cFrames > 320) FinishCampaign();
            }
            else if (_cStarted) FinishCampaign();
        }

        private static void FinishCampaign()
        {
            EditorApplication.update -= TickCampaign;
            int lastStage = StageDatabase.StageCount - 1;
            bool ok = _errors == 0 && _cBossesOk == BossIds.Length && _cMaxStage >= lastStage;
            Debug.Log($"SMOKE_CAMPAIGN_RESULT bosses={_cBossesOk}/{BossIds.Length} maxStage={_cMaxStage}/{lastStage} " +
                      $"errors={_errors} frames={_cFrames} ok={ok}");
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(ok ? 0 : 2);
        }
    }
}
