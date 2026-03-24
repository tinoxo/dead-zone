using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Upgrading,
    Dead,
    BossTime,
    PathChoice
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State      { get; private set; } = GameState.Playing;
    public int Wave             { get; private set; }
    public int Score            { get; private set; }
    public int TotalKills       { get; private set; }
    public Vector2 LastBossPosition { get; set; }

    // Stored while waiting for boon pick in segment mode, then used to spawn the door
    DoorRoomType pendingDoorType;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Game lifecycle ─────────────────────────────────────────────────────

    /// <summary>Called by SceneSetup after everything is wired up.</summary>
    public void StartGame()
    {
        Wave       = 0;
        Score      = 0;
        TotalKills = 0;

        // Initialize path system and economy
        PathManager.Instance?.Initialize();
        GoldSystem.Instance?.Initialize();

        NextWave();
    }

    public void NextWave()
    {
        Wave++;
        State = GameState.Playing;
        HUDManager.Instance?.SetWave(Wave);

        // In path-lattice mode use the segment-aware spawner (respects CurrentTheme).
        // Legacy mode (no PathManager) uses the old wave scaler.
        if (PathManager.Instance != null)
            WaveManager.Instance?.StartWave(0);
        else
            WaveManager.Instance?.BeginWave(Wave);
    }

    public void WaveCleared(bool isBoss)
    {
        State = GameState.Upgrading;
        UpgradeManager.Instance.ShowUpgradeSelection(isBoss);
    }

    /// <summary>
    /// Called by WaveManager in segment mode when all enemies are dead.
    /// Shows the boon selection; the door spawns only AFTER a boon is picked.
    /// </summary>
    public void WaveCleared_Segment(DoorRoomType doorType)
    {
        pendingDoorType = doorType;
        // Player stays mobile — BoonManager spawns physical orbs to walk into.
        // Door appears only after a boon is picked (BoonManager calls ShowExitDoor).
        BoonManager.Instance?.SpawnChoices(doorType);
    }

    public void UpgradePicked()
    {
        // Segment mode: re-enable player movement then spawn the door
        if (PathManager.Instance != null)
        {
            State = GameState.Playing;
            WaveManager.Instance?.ShowExitDoor(pendingDoorType);
        }
        else
        {
            NextWave();
        }
    }

    public void EnemyKilled(int pts)
    {
        Score += pts;
        TotalKills++;
        HUDManager.Instance?.SetScore(Score);
    }

    public void PlayerDied()
    {
        if (State == GameState.Dead) return;
        State = GameState.Dead;
        UpgradeManager.Instance?.SaveUnlocks();
        DeathScreenUI.Instance?.Show(Wave, Score, TotalKills);
    }

    public void Restart() => SceneManager.LoadScene(0);

    // ── Path lattice — boss spawning ───────────────────────────────────────

    /// <summary>
    /// Spawn the segment boss defined by the path lattice.
    /// Called by WaveManager when all segment waves are cleared.
    /// </summary>
    public void SpawnSegmentBoss()
    {
        State = GameState.BossTime;

        BossData bossData = PathManager.Instance?.CurrentBoss;
        if (bossData == null)
        {
            Debug.LogWarning("[GameManager] SpawnSegmentBoss: no current boss data from PathManager.");
            return;
        }

        Debug.Log($"[GameManager] Spawning segment boss: {bossData.Name}");

        float mult = 1f + (Wave - 1) * 0.18f;

        switch (bossData.ID)
        {
            // Spiral-pattern bosses
            case "vortex":
            case "hollow":
            case "pulse":
                SpawnPathBoss<BossSpiral>(mult, bossData.ThemeColor, bossData.Name, isOmega: false);
                break;

            // Omega — final boss: bigger BossSpiral variant
            case "omega":
                SpawnPathBoss<BossSpiral>(mult * 1.5f, bossData.ThemeColor, bossData.Name, isOmega: true);
                break;

            // Charger-pattern bosses (default for heavy/speed/explosive types)
            default:
                SpawnPathBoss<BossCharger>(mult, bossData.ThemeColor, bossData.Name, isOmega: false);
                break;
        }
    }

    void SpawnPathBoss<T>(float mult, Color color, string bossName, bool isOmega) where T : BossBase
    {
        var go = new GameObject(bossName);
        go.transform.position = Vector2.zero;   // centre of arena

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        var rb           = go.AddComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.constraints   = RigidbodyConstraints2D.FreezeRotation;

        var col    = go.AddComponent<CircleCollider2D>();
        col.radius = isOmega ? 1.4f : 0.9f;

        T boss = go.AddComponent<T>();
        boss.BossName = bossName;
        boss.ScaleStats(mult);

        // Hook the death callback so materials are awarded
        // We use a BossDeathReporter component as a lightweight bridge
        var reporter = go.AddComponent<BossDeathReporter>();
        reporter.BossID = PathManager.Instance?.CurrentBoss?.ID ?? "";

        sr.sprite = SpriteFactory.Diamond(color);
        sr.color  = color;
        go.transform.localScale = isOmega ? Vector3.one * 3.5f : Vector3.one * 2.2f;

        // Do NOT add path bosses to WaveManager's alive list —
        // their death is handled entirely by BossDeathReporter → OnBossDefeated.
    }

    /// <summary>
    /// Called by BossDeathReporter when a path-lattice boss dies.
    /// Awards materials and hands off to PathManager.
    /// </summary>
    public void OnBossDefeated(BossData bossData)
    {
        if (bossData == null) return;

        // Award materials
        MaterialSystem.Instance?.AddMaterial(bossData.MaterialName, bossData.MaterialReward);

        // Update score
        Score += 5000 * bossData.MaterialReward;
        HUDManager.Instance?.SetScore(Score);

        // Bank gold after boss
        GoldSystem.Instance?.BankAfterBoss();

        // Spawn item pair where boss died (stored in PathManager)
        Vector2 bossPos = LastBossPosition;
        ItemManager.Instance?.SpawnItemPair(bossPos);

        // Hand off to path manager (it will show PathMapUI or end the run)
        PathManager.Instance?.OnBossDefeated();
    }

    /// <summary>
    /// Called by a Path door when the player walks through it.
    /// Advances the wave counter, sets state to Playing, and starts the chosen segment.
    /// </summary>
    public void PathChosen(bool goLeft)
    {
        Wave++;
        State = GameState.Playing;
        HUDManager.Instance?.SetWave(Wave);
        PathManager.Instance?.ChoosePath(goLeft);
    }

    /// <summary>Called when Omega (final boss) is defeated — run complete.</summary>
    public void PlayerWon()
    {
        if (State == GameState.Dead) return;
        State = GameState.Dead;
        Debug.Log("[GameManager] Player WON — run complete!");
        // For now reuse the death screen to show the victory score; future: dedicated win screen
        DeathScreenUI.Instance?.Show(Wave, Score, TotalKills);
    }
}
