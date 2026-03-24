using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float MaxHealth    = 40f;
    public float MoveSpeed    = 3f;
    public float ContactDamage= 10f;
    public int   ScoreValue   = 100;

    protected float         health;
    protected Rigidbody2D   rb;
    protected SpriteRenderer sr;
    protected Transform     player;
    protected bool          dead;

    protected virtual void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        sr     = GetComponent<SpriteRenderer>();
        health = MaxHealth;
    }

    protected virtual void Start()
    {
        player = PlayerController.Instance?.transform;
    }

    protected virtual void Update()
    {
        if (dead) return;
        var st = GameManager.Instance?.State;
        bool canAct = st == GameState.Playing || st == GameState.BossTime;
        if (!canAct) { rb.linearVelocity = Vector2.zero; return; }
        if (player == null) player = PlayerController.Instance?.transform;
        DoUpdate();
    }

    protected abstract void DoUpdate();

    public virtual void TakeDamage(float dmg)
    {
        if (dead) return;
        health -= dmg;
        StartCoroutine(HitFlash());
        if (health <= 0f) Die();
    }

    protected virtual void Die()
    {
        if (dead) return;
        dead = true;
        GameManager.Instance?.EnemyKilled(ScoreValue);
        SpawnDeathBurst();

        // Vampiric item — heal player on kill
        if (PlayerStats.Instance?.HasItem(ItemEffectType.Vampiric) == true)
            PlayerController.Instance?.HealPercent(0.03f);

        WaveManager.Instance?.OnEnemyDied(this);
        Destroy(gameObject);
    }

    // ── Status effects ────────────────────────────────────────────────────

    bool poisoned;

    public void ApplyPoison(float dps, float duration)
    {
        if (!poisoned) StartCoroutine(PoisonRoutine(dps, duration));
    }

    System.Collections.IEnumerator PoisonRoutine(float dps, float duration)
    {
        poisoned = true;
        Color orig = sr ? sr.color : Color.green;
        float elapsed = 0f;
        while (elapsed < duration && !dead)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            if (!dead)
            {
                TakeDamage(dps * 0.5f);
                // Green flash
                if (sr) { sr.color = new Color(0.3f, 1f, 0.3f); }
                yield return new WaitForSeconds(0.05f);
                if (sr && !dead) sr.color = orig;
            }
        }
        poisoned = false;
    }

    bool slowed;

    public void ApplySlow(float factor, float duration)
    {
        if (!slowed) StartCoroutine(SlowRoutine(factor, duration));
    }

    System.Collections.IEnumerator SlowRoutine(float factor, float duration)
    {
        slowed = true;
        float origSpeed = MoveSpeed;
        MoveSpeed *= factor;
        Color orig = sr ? sr.color : Color.white;
        if (sr) sr.color = new Color(0.5f, 0.75f, 1f);  // icy blue tint
        yield return new WaitForSeconds(duration);
        if (!dead)
        {
            MoveSpeed = origSpeed;
            if (sr) sr.color = orig;
        }
        slowed = false;
    }

    void SpawnDeathBurst()
    {
        Color c = sr ? sr.color : Color.white;
        for (int i = 0; i < 14; i++)
        {
            var p   = new GameObject("DP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(c);
            psr.sortingOrder = 9;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * 0.11f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(2f, 7f), Random.Range(0.3f, 0.65f));
        }
    }

    IEnumerator HitFlash()
    {
        if (!sr) yield break;
        Color orig = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (sr) sr.color = orig;
    }

    /// <summary>Scale up stats based on wave multiplier.</summary>
    public virtual void ScaleStats(float mult)
    {
        MaxHealth     *= mult;
        health         = MaxHealth;
        MoveSpeed     *= Mathf.Sqrt(mult);
        ContactDamage *= mult;
        ScoreValue     = Mathf.RoundToInt(ScoreValue * mult);
    }
}
