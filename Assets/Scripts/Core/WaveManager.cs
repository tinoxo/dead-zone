using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    const float ARENA = 17f;   // spawn enemies within this radius from center

    List<EnemyBase> aliveEnemies = new List<EnemyBase>();
    bool waveDone;

    // ── Segment / path lattice state ───────────────────────────────────────
    public  EnemyTheme   CurrentTheme          { get; private set; } = EnemyTheme.Heavy;
    public  PathRoomType CurrentRoomType       { get; private set; } = PathRoomType.Straight;

    int  wavesThisSegment          = 6;
    int  wavesCompletedThisSegment = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Begin a new boss segment. Sets theme, resets wave counters, and starts the first wave.
    /// Called by PathManager after the player picks a path.
    /// </summary>
    public void StartSegment(EnemyTheme theme, PathRoomType roomType)
    {
        CurrentTheme               = theme;
        CurrentRoomType            = roomType;
        wavesCompletedThisSegment  = 0;
        wavesThisSegment           = Random.Range(5, 8);   // 5, 6, or 7 waves per segment

        Debug.Log($"[WaveManager] Starting segment — Theme: {theme}, Room: {roomType}, " +
                  $"Waves this segment: {wavesThisSegment}");

        StartWave(1);
    }

    /// <summary>Start a specific wave number (1-indexed within the segment).</summary>
    public void StartWave(int waveIndexInSegment)
    {
        aliveEnemies.Clear();
        waveDone = false;

        // Keep global wave counter in sync
        int globalWave = GameManager.Instance != null ? GameManager.Instance.Wave : 1;
        HUDManager.Instance?.SetWave(globalWave);

        StartCoroutine(SpawnThemeWave(globalWave));
    }

    /// <summary>Legacy entry point used by the old wave system — still supported.</summary>
    public void BeginWave(int wave)
    {
        aliveEnemies.Clear();
        waveDone = false;

        bool isBoss = wave % 10 == 0;
        if (isBoss)
            StartCoroutine(SpawnBossWave(wave));
        else
            StartCoroutine(SpawnNormalWave(wave));
    }

    public void OnEnemyDied(EnemyBase e)
    {
        aliveEnemies.Remove(e);

        // Award gold for the kill
        int goldDrop = Random.Range(1, 4);   // 1–3 gold
        GoldSystem.Instance?.EarnGold(goldDrop);

        if (!waveDone && aliveEnemies.Count == 0)
        {
            waveDone = true;

            bool wasBoss = GameManager.Instance != null &&
                           GameManager.Instance.Wave % 10 == 0;

            // Only advance segment counter when inside a path-lattice segment
            if (PathManager.Instance != null)
            {
                StartCoroutine(SegmentWaveClearDelay());
            }
            else
            {
                StartCoroutine(WaveClearDelay(wasBoss));
            }
        }
    }

    // ── Theme-aware wave spawning ──────────────────────────────────────────

    IEnumerator SpawnThemeWave(int wave)
    {
        float mult = StatMult(wave);

        switch (CurrentTheme)
        {
            case EnemyTheme.Swarm:
                yield return StartCoroutine(SpawnSwarmWave(mult));
                break;

            case EnemyTheme.Ranged:
                yield return StartCoroutine(SpawnRangedWave(mult));
                break;

            case EnemyTheme.Heavy:
                yield return StartCoroutine(SpawnHeavyWave(mult));
                break;

            case EnemyTheme.Void:
                yield return StartCoroutine(SpawnVoidWave(mult));
                break;

            case EnemyTheme.Speed:
                yield return StartCoroutine(SpawnSpeedWave(mult));
                break;

            case EnemyTheme.Explosive:
                yield return StartCoroutine(SpawnExplosiveWave(mult));
                break;

            case EnemyTheme.Electric:
                yield return StartCoroutine(SpawnElectricWave(mult));
                break;

            default:
                yield return StartCoroutine(SpawnNormalWave(wave));
                break;
        }
    }

    // Swarm — lots of fast chasers
    IEnumerator SpawnSwarmWave(float mult)
    {
        int chasers = Random.Range(8, 14);
        int ranged  = Random.Range(0, 3);
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult, Color.magenta); yield return new WaitForSeconds(0.15f); }
        for (int i = 0; i < ranged;  i++) { SpawnEnemy<RangedEnemy>(mult * 0.8f, new Color(1f, 0.5f, 0f)); yield return new WaitForSeconds(0.2f); }
    }

    // Ranged — heavy ranged presence
    IEnumerator SpawnRangedWave(float mult)
    {
        int ranged  = Random.Range(5, 9);
        int chasers = Random.Range(2, 5);
        for (int i = 0; i < ranged;  i++) { SpawnEnemy<RangedEnemy>(mult, new Color(1f, 0.5f, 0f)); yield return new WaitForSeconds(0.3f); }
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult * 0.7f, Color.magenta); yield return new WaitForSeconds(0.2f); }
    }

    // Heavy — tanky enemies with a few chasers
    IEnumerator SpawnHeavyWave(float mult)
    {
        int tanks   = Random.Range(3, 6);
        int chasers = Random.Range(2, 5);
        for (int i = 0; i < tanks;   i++) { SpawnEnemy<TankyEnemy>(mult, new Color(0.6f, 0f, 1f)); yield return new WaitForSeconds(0.5f); }
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult * 0.8f, Color.magenta); yield return new WaitForSeconds(0.25f); }
    }

    // Void — mix of all types (void entity theme)
    IEnumerator SpawnVoidWave(float mult)
    {
        int chasers = Random.Range(3, 6);
        int ranged  = Random.Range(3, 6);
        int tanks   = Random.Range(1, 3);
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult, new Color(0.55f, 0f, 0.85f)); yield return new WaitForSeconds(0.2f); }
        for (int i = 0; i < ranged;  i++) { SpawnEnemy<RangedEnemy>(mult, new Color(0.55f, 0f, 0.85f)); yield return new WaitForSeconds(0.3f); }
        for (int i = 0; i < tanks;   i++) { SpawnEnemy<TankyEnemy>(mult, new Color(0.55f, 0f, 0.85f));  yield return new WaitForSeconds(0.4f); }
    }

    // Speed — many fast chasers, few tanks
    IEnumerator SpawnSpeedWave(float mult)
    {
        int chasers = Random.Range(6, 12);
        int tanks   = Random.Range(0, 2);
        // Speed enemies move faster
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult * 1.3f, new Color(0.72f, 1f, 0f)); yield return new WaitForSeconds(0.15f); }
        for (int i = 0; i < tanks;   i++) { SpawnEnemy<TankyEnemy>(mult, new Color(0.72f, 1f, 0f)); yield return new WaitForSeconds(0.4f); }
    }

    // Explosive — moderate counts, heavier stats to simulate explosive threat
    IEnumerator SpawnExplosiveWave(float mult)
    {
        int chasers = Random.Range(4, 7);
        int ranged  = Random.Range(3, 6);
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult * 1.1f, new Color(1f, 0.3f, 0.05f)); yield return new WaitForSeconds(0.25f); }
        for (int i = 0; i < ranged;  i++) { SpawnEnemy<RangedEnemy>(mult * 1.1f, new Color(1f, 0.3f, 0.05f)); yield return new WaitForSeconds(0.3f); }
    }

    // Electric — mixed, slightly elevated ranged
    IEnumerator SpawnElectricWave(float mult)
    {
        int ranged  = Random.Range(4, 8);
        int chasers = Random.Range(3, 6);
        int tanks   = Random.Range(0, 3);
        for (int i = 0; i < ranged;  i++) { SpawnEnemy<RangedEnemy>(mult, new Color(0.2f, 0.55f, 1f)); yield return new WaitForSeconds(0.25f); }
        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult * 0.9f, new Color(0.2f, 0.55f, 1f)); yield return new WaitForSeconds(0.2f); }
        for (int i = 0; i < tanks;   i++) { SpawnEnemy<TankyEnemy>(mult, new Color(0.2f, 0.55f, 1f)); yield return new WaitForSeconds(0.4f); }
    }

    // ── Legacy wave composition ────────────────────────────────────────────

    IEnumerator SpawnNormalWave(int wave)
    {
        float mult = StatMult(wave);

        int chasers = Mathf.Min(2 + wave,       20);
        int ranged  = wave >= 5  ? Mathf.Min((wave - 4) * 2, 12) : 0;
        int tanks   = wave >= 10 ? Mathf.Min((wave - 9),      6) : 0;

        for (int i = 0; i < chasers; i++) { SpawnEnemy<ChaserEnemy>(mult, Color.magenta); yield return new WaitForSeconds(0.25f); }
        for (int i = 0; i < ranged;  i++) { SpawnEnemy<RangedEnemy>(mult, new Color(1f, 0.5f, 0f)); yield return new WaitForSeconds(0.3f); }
        for (int i = 0; i < tanks;   i++) { SpawnEnemy<TankyEnemy> (mult, new Color(0.6f, 0f, 1f)); yield return new WaitForSeconds(0.4f); }
    }

    IEnumerator SpawnBossWave(int wave)
    {
        yield return new WaitForSeconds(0.5f);

        float mult = StatMult(wave);
        bool isSecondBoss = (wave / 10) % 2 == 0;

        if (isSecondBoss)
            SpawnBoss<BossCharger>(mult, new Color(0.2f, 0.4f, 1f));
        else
            SpawnBoss<BossSpiral>(mult, new Color(1f, 0.2f, 0f));
    }

    // ── Spawn helpers ─────────────────────────────────────────────────────

    void SpawnEnemy<T>(float mult, Color color) where T : EnemyBase
    {
        Vector2 pos = RandomEdgePos();
        var go = new GameObject(typeof(T).Name);
        go.transform.position = pos;

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        var rb           = go.AddComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.constraints   = RigidbodyConstraints2D.FreezeRotation;

        var col      = go.AddComponent<CircleCollider2D>();
        col.radius   = 0.4f;

        T enemy = go.AddComponent<T>();
        enemy.ScaleStats(mult);

        sr.sprite = GetEnemySprite<T>(color);
        sr.color  = color;
        go.transform.localScale = GetEnemyScale<T>();

        aliveEnemies.Add(enemy);
    }

    void SpawnBoss<T>(float mult, Color color) where T : BossBase
    {
        var go = new GameObject(typeof(T).Name);
        go.transform.position = new Vector2(0f, 8f);

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        var rb           = go.AddComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.constraints   = RigidbodyConstraints2D.FreezeRotation;

        var col    = go.AddComponent<CircleCollider2D>();
        col.radius = 0.9f;

        T boss = go.AddComponent<T>();
        boss.ScaleStats(mult);

        sr.sprite = SpriteFactory.Diamond(color);
        sr.color  = color;
        go.transform.localScale = Vector3.one * 2.2f;

        aliveEnemies.Add(boss);
    }

    Sprite GetEnemySprite<T>(Color col) where T : EnemyBase
    {
        if (typeof(T) == typeof(ChaserEnemy))  return SpriteFactory.Circle(col);
        if (typeof(T) == typeof(RangedEnemy))  return SpriteFactory.Square(col);
        if (typeof(T) == typeof(TankyEnemy))   return SpriteFactory.Circle(col);
        return SpriteFactory.Circle(col);
    }

    Vector3 GetEnemyScale<T>() where T : EnemyBase
    {
        if (typeof(T) == typeof(TankyEnemy))  return Vector3.one * 1.5f;
        if (typeof(T) == typeof(RangedEnemy)) return Vector3.one * 0.9f;
        return Vector3.one;
    }

    Vector2 RandomEdgePos()
    {
        int side = Random.Range(0, 4);
        float r  = ARENA;
        return side switch
        {
            0 => new Vector2(Random.Range(-r, r),  r),
            1 => new Vector2(Random.Range(-r, r), -r),
            2 => new Vector2( r, Random.Range(-r, r)),
            _ => new Vector2(-r, Random.Range(-r, r)),
        };
    }

    float StatMult(int wave) => 1f + (wave - 1) * 0.18f;

    /// <summary>Exposes the alive-enemy list so GameManager can register path-lattice bosses.</summary>
    public List<EnemyBase> GetAliveList() => aliveEnemies;

    // ── Clear delay coroutines ─────────────────────────────────────────────

    // Track current door so we can clean it up if needed
    RoomDoor activeDoor;

    IEnumerator SegmentWaveClearDelay()
    {
        yield return new WaitForSeconds(0.8f);

        wavesCompletedThisSegment++;

        bool isFinalWave = wavesCompletedThisSegment >= wavesThisSegment;
        DoorRoomType doorType = isFinalWave ? DoorRoomType.Boss : DoorRoomType.Combat;

        // Spawn exit door — player walks through to continue
        SpawnExitDoor(doorType);
    }

    IEnumerator WaveClearDelay(bool boss)
    {
        yield return new WaitForSeconds(boss ? 1.2f : 0.8f);
        GameManager.Instance?.WaveCleared(boss);
    }

    void SpawnExitDoor(DoorRoomType type)
    {
        // Clean up any lingering door
        if (activeDoor != null) Destroy(activeDoor.gameObject);

        // Spawn door near the right wall
        Vector2 pos = new Vector2(15f, 0f);
        activeDoor = RoomDoor.Spawn(pos, type);
        Debug.Log($"[WaveManager] Spawned {type} door. Walk through to continue.");
    }

    /// <summary>Called by RoomDoor when the player walks through.</summary>
    public void OnDoorEntered(DoorRoomType type)
    {
        activeDoor = null;

        if (type == DoorRoomType.Boss)
        {
            Debug.Log($"[WaveManager] Segment complete ({wavesCompletedThisSegment}/{wavesThisSegment} waves). Spawning boss.");
            GameManager.Instance?.SpawnSegmentBoss();
        }
        else
        {
            Debug.Log($"[WaveManager] Wave cleared ({wavesCompletedThisSegment}/{wavesThisSegment}). Starting next wave.");
            GameManager.Instance?.NextWave();
        }
    }
}
