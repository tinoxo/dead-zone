using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    const float ARENA = 17f;   // spawn enemies within this radius from center

    List<EnemyBase> aliveEnemies = new List<EnemyBase>();
    bool waveDone;

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    // ── Public API ────────────────────────────────────────────────────────
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
        if (!waveDone && aliveEnemies.Count == 0)
        {
            waveDone = true;
            bool wasBoss = GameManager.Instance.Wave % 10 == 0;
            StartCoroutine(WaveClearDelay(wasBoss));
        }
    }

    // ── Wave composition ──────────────────────────────────────────────────
    IEnumerator SpawnNormalWave(int wave)
    {
        float mult = StatMult(wave);

        // How many of each type
        int chasers = Mathf.Min(2 + wave,       20);
        int ranged  = wave >= 5  ? Mathf.Min((wave - 4) * 2, 12) : 0;
        int tanks   = wave >= 10 ? Mathf.Min((wave - 9),      6) : 0;

        // Spawn with slight delays
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

        // Assign sprite & color after component is added (Awake runs on AddComponent)
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
        // Spawn on random edge of arena
        int side = Random.Range(0, 4);
        float r = ARENA;
        return side switch
        {
            0 => new Vector2(Random.Range(-r, r),  r),
            1 => new Vector2(Random.Range(-r, r), -r),
            2 => new Vector2( r, Random.Range(-r, r)),
            _ => new Vector2(-r, Random.Range(-r, r)),
        };
    }

    float StatMult(int wave) => 1f + (wave - 1) * 0.18f;

    IEnumerator WaveClearDelay(bool boss)
    {
        yield return new WaitForSeconds(boss ? 1.2f : 0.8f);
        GameManager.Instance?.WaveCleared(boss);
    }
}
