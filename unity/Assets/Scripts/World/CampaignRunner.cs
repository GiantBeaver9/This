using UnityEngine;

namespace ThisL
{
    /// <summary>
    /// Drives the whole LINEAR campaign end-to-end (STAGES.md §2/§4.1: Lincoln → SF →
    /// Phil, 13 stages). A drop-in for the old <see cref="EnemySpawner"/> — added by
    /// <see cref="GameFlow"/> at the end of the opening chain with no ctor args. It owns
    /// a single <see cref="StageDirector"/> (which runs ONE stage: its ENCOUNTERS.md
    /// spine + seeded filler + gates + act-end boss), listens for
    /// <see cref="StageDirector.OnStageComplete"/>, and advances to the next stage —
    /// laying the next lane out ahead of wherever the player finished — until Phil
    /// (Stage 13) goes down, which ends the run.
    ///
    /// The escalation curve is authored in <see cref="StageDatabase"/>: Stage 1 opens
    /// gentle (a light vignette + a couple of Regular-only waves, no shooters — Area 1
    /// is tier 0–1) and each subsequent stage layers in that area's new enemy types and
    /// tightens pacing (Area 2 Snapper/AA/Head-Thrower + shooters, Area 3 Sniper/Ninja/
    /// Arm-Ripper/Monkeys, Area 4 Gatling/Ground-Smasher/Heavy/Pickpocket) so it builds
    /// to chaos, then caps each act with its boss via the director's boss hook.
    /// </summary>
    public sealed class CampaignRunner : MonoBehaviour
    {
        /// <summary>The live campaign (for debug hooks like the O-key stage skip). Null off-campaign.</summary>
        public static CampaignRunner Instance { get; private set; }

        /// <summary>0-based index of the stage currently running.</summary>
        public int CurrentStage { get; private set; }

        private StageDirector _director;
        private bool _campaignComplete;

        private void Awake() => Instance = this;

        private void Start()
        {
            _director = gameObject.AddComponent<StageDirector>();
            _director.OnStageComplete += HandleStageComplete;
            StartStage(0);
        }

        private void OnDestroy()
        {
            if (_director != null) _director.OnStageComplete -= HandleStageComplete;
            if (Instance == this) Instance = null;
        }

        /// <summary>DEBUG (bound to O in PlayerController): clear the field and jump to the next
        /// stage, so the whole campaign can be walked through to test specials/bosses/art.</summary>
        public void SkipToNext()
        {
            if (_campaignComplete) return;
            var kill = new System.Collections.Generic.List<GameObject>();
            foreach (var a in Actor.All) if (a != null && a.Team == Team.Enemy) kill.Add(a.gameObject);
            foreach (var p in Object.FindObjectsByType<Projectile>(FindObjectsInactive.Exclude)) kill.Add(p.gameObject);
            foreach (var go in kill) if (go != null) Destroy(go);

            int next = CurrentStage + 1;
            if (next >= StageDatabase.StageCount) { WinRun(); return; }
            StartStage(next);
        }

        private void StartStage(int i)
        {
            CurrentStage = Mathf.Clamp(i, 0, StageDatabase.StageCount - 1);
            _director.StartStage(CurrentStage);
            Debug.Log($"[Campaign] Entering stage {CurrentStage + 1}/{StageDatabase.StageCount}.");
        }

        private void HandleStageComplete()
        {
            if (_campaignComplete) return;

            int next = CurrentStage + 1;
            if (next >= StageDatabase.StageCount)
            {
                WinRun();
                return;
            }
            // Chain straight into the next stage; StageDirector re-anchors the lane to the
            // player's current X, so the world reads as one continuous drive to SF.
            StartStage(next);
        }

        private void WinRun()
        {
            _campaignComplete = true;
            EnemySpawner.StageLabel = "RUN COMPLETE — PHIL DEFEATED";
            Music.PlayTitle();
            Debug.Log("[Campaign] Phil defeated — the run is complete. Returning to title.");
            // Hand back to the menu after a short beat (GameFlow tears the world down).
            Invoke(nameof(ReturnToTitle), 5f);
        }

        private void ReturnToTitle()
        {
            if (GameFlow.Instance != null) GameFlow.Instance.GoTitle();
        }
    }
}
