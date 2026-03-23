using UnityEngine;

/// <summary>Base class for bosses. Adds a world-space health bar above the boss.</summary>
public abstract class BossBase : EnemyBase
{
    public string BossName = "BOSS";

    // World-space health bar sprites
    GameObject barBgObj;
    GameObject barFillObj;
    SpriteRenderer barFillSr;

    protected int phase = 1;

    protected override void Awake()
    {
        base.Awake();
        ScoreValue = 1500;
    }

    protected override void Start()
    {
        base.Start();
        BuildHealthBar();
    }

    void BuildHealthBar()
    {
        // Background
        barBgObj = new GameObject("BossBarBg");
        barBgObj.transform.SetParent(transform);
        barBgObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        barBgObj.transform.localScale    = new Vector3(4f, 0.32f, 1f);
        var bgSr          = barBgObj.AddComponent<SpriteRenderer>();
        bgSr.sprite       = SpriteFactory.Square(new Color(0.1f, 0.1f, 0.1f, 0.85f));
        bgSr.sortingOrder = 20;

        // Fill
        barFillObj = new GameObject("BossBarFill");
        barFillObj.transform.SetParent(transform);
        barFillObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        barFillObj.transform.localScale    = new Vector3(4f, 0.28f, 1f);
        barFillSr          = barFillObj.AddComponent<SpriteRenderer>();
        barFillSr.sprite   = SpriteFactory.Square(new Color(0.85f, 0.15f, 0.15f));
        barFillSr.sortingOrder = 21;
    }

    protected override void Update()
    {
        base.Update();
        // Scale fill bar to show remaining health
        if (barFillObj != null)
        {
            float frac = MaxHealth > 0f ? health / MaxHealth : 0f;
            var s = barFillObj.transform.localScale;
            barFillObj.transform.localScale = new Vector3(4f * frac, s.y, 1f);
            // Shift pivot left so it shrinks from the right
            barFillObj.transform.localPosition = new Vector3(-4f * (1f - frac) * 0.5f, 1.6f, 0f);
        }
    }

    protected override void Die()
    {
        // Store position so item pair can spawn here
        if (GameManager.Instance != null)
            GameManager.Instance.LastBossPosition = transform.position;

        // Bigger death explosion for bosses
        Color c = sr ? sr.color : Color.red;
        for (int i = 0; i < 30; i++)
        {
            var p   = new GameObject("BossDP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(c);
            psr.sortingOrder = 9;
            p.transform.position   = transform.position + (Vector3)Random.insideUnitCircle * 0.8f;
            p.transform.localScale = Vector3.one * Random.Range(0.12f, 0.25f);
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(3f, 10f), Random.Range(0.4f, 0.9f));
        }
        CameraController.Instance?.Shake(0.6f, 0.45f);

        dead = true;
        GameManager.Instance?.EnemyKilled(ScoreValue);

        // Only notify WaveManager if this is a legacy wave boss (not a path-lattice boss).
        // Path bosses are tracked by BossDeathReporter, not by the alive-enemies list.
        if (GameManager.Instance?.State != GameState.BossTime)
            WaveManager.Instance?.OnEnemyDied(this);

        Destroy(gameObject);
    }

    // ── Helper: shoot a bullet in world direction ─────────────────────────
    protected void ShootBullet(Vector2 dir, float dmg = 12f, float spd = 6f, Color? col = null)
    {
        var go  = new GameObject("BossBullet");
        go.transform.position   = transform.position;
        go.transform.localScale = Vector3.one * 0.26f;

        var bsr          = go.AddComponent<SpriteRenderer>();
        bsr.sprite       = SpriteFactory.Circle(col ?? new Color(1f, 0.35f, 0.1f));
        bsr.sortingOrder = 7;

        var c      = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius    = 0.5f;

        var brb          = go.AddComponent<Rigidbody2D>();
        brb.gravityScale = 0f;
        brb.bodyType     = RigidbodyType2D.Kinematic;

        var eb = go.AddComponent<EnemyBullet>();
        eb.Init(dir.normalized, dmg, spd);
    }
}
