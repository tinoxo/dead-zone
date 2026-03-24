using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBullet : MonoBehaviour
{
    public float Damage;
    public bool  Piercing;

    // ── Item effect flags (set from PlayerStats on Init) ──────────────────
    bool bouncing;
    bool explosive;
    bool chainLightning;
    bool poison;
    bool homing;
    bool frost;
    bool singularity;

    int   bounceCount;
    const int MAX_BOUNCES = 2;

    Vector2 dir;
    float   speed;
    float   life = 4f;

    public void Init(Vector2 direction, float dmg, float sizeMult, float spd, bool pierce)
    {
        dir      = direction.normalized;
        Damage   = dmg;
        speed    = spd;
        Piercing = pierce;
        life     = 4f;
        transform.localScale = Vector3.one * sizeMult * 0.18f;

        // Read item flags from PlayerStats
        var s = PlayerStats.Instance;
        if (s != null)
        {
            bouncing       = s.HasItem(ItemEffectType.BouncingRounds);
            explosive      = s.HasItem(ItemEffectType.Explosive);
            chainLightning = s.HasItem(ItemEffectType.ChainLightning);
            poison         = s.HasItem(ItemEffectType.PoisonTrail);
            homing         = s.HasItem(ItemEffectType.Homing);
            frost          = s.HasItem(ItemEffectType.FrostRound);
            singularity    = s.HasItem(ItemEffectType.Singularity);
            if (s.HasItem(ItemEffectType.GhostBlade)) Piercing = true;
        }
    }

    void Update()
    {
        // Homing — gently steer toward nearest enemy
        if (homing)
        {
            var target = FindNearestEnemy();
            if (target != null)
            {
                Vector2 toTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
                dir = Vector2.Lerp(dir, toTarget, Time.deltaTime * 4.5f).normalized;
            }
        }

        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerBullet>()    != null) return;
        if (other.GetComponent<PlayerController>() != null) return;

        // ── Wall bounce ───────────────────────────────────────────────────
        if (other.GetComponent<ArenaWall>() != null)
        {
            if (bouncing && bounceCount < MAX_BOUNCES)
            {
                bounceCount++;
                // Horizontal wall (top/bottom) → flip Y; vertical → flip X
                bool horiz = other.transform.localScale.x > other.transform.localScale.y * 2f;
                dir = horiz ? new Vector2(dir.x, -dir.y) : new Vector2(-dir.x, dir.y);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }

        // ── Enemy hit ─────────────────────────────────────────────────────
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(Damage);
            SpawnHitParticles();

            if (explosive)      TriggerExplosion(enemy);
            if (chainLightning) TriggerChainLightning(enemy);
            if (poison)         enemy.ApplyPoison(dps: Damage * 0.3f, duration: 3f);
            if (frost)          enemy.ApplySlow(factor: 0.45f, duration: 2f);
            if (singularity)    TriggerSingularity();

            if (!Piercing) Destroy(gameObject);
        }
    }

    // ── Explosive — AoE burst ─────────────────────────────────────────────
    void TriggerExplosion(EnemyBase hitEnemy)
    {
        float radius = 2.8f;
        float aoeDmg = Damage * 0.65f;

        // Visual ring
        SpawnExplosionParticles();

        foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
        {
            if (e == hitEnemy) continue;
            if (Vector2.Distance(transform.position, e.transform.position) < radius)
                e.TakeDamage(aoeDmg);
        }
    }

    void SpawnExplosionParticles()
    {
        for (int i = 0; i < 18; i++)
        {
            var p   = new GameObject("ExpP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(new Color(1f, 0.55f, 0.1f));
            psr.sortingOrder = 11;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(2f, 7f),
                    Random.Range(0.2f, 0.5f));
        }
    }

    // ── Chain Lightning — arc to nearest other enemy ──────────────────────
    void TriggerChainLightning(EnemyBase hitEnemy)
    {
        float chainRange = 5f;
        EnemyBase closest     = null;
        float     closestDist = chainRange;

        foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
        {
            if (e == hitEnemy) continue;
            float d = Vector2.Distance(hitEnemy.transform.position, e.transform.position);
            if (d < closestDist) { closestDist = d; closest = e; }
        }

        if (closest != null)
        {
            closest.TakeDamage(Damage * 0.75f);
            SpawnLightningParticles(hitEnemy.transform.position, closest.transform.position);
        }
    }

    void SpawnLightningParticles(Vector2 from, Vector2 to)
    {
        int steps = 8;
        for (int i = 0; i < steps; i++)
        {
            float t   = (float)i / steps;
            Vector2 p2 = Vector2.Lerp(from, to, t) + Random.insideUnitCircle * 0.3f;
            var p   = new GameObject("LightP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(new Color(0.6f, 0.7f, 1f));
            psr.sortingOrder = 11;
            p.transform.position   = p2;
            p.transform.localScale = Vector3.one * 0.07f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle * 0.5f, 0.15f);
        }
    }

    // ── Singularity — pull all nearby enemies toward impact point ─────────
    void TriggerSingularity()
    {
        float pullRadius = 6f;
        float pullForce  = 18f;

        foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < pullRadius)
            {
                var erb = e.GetComponent<Rigidbody2D>();
                if (erb)
                {
                    Vector2 pullDir = ((Vector2)transform.position - (Vector2)e.transform.position).normalized;
                    erb.linearVelocity += pullDir * pullForce * (1f - d / pullRadius);
                }
            }
        }

        // Purple flash at impact
        for (int i = 0; i < 14; i++)
        {
            var p   = new GameObject("SingP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(new Color(0.7f, 0.2f, 1f));
            psr.sortingOrder = 11;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * 0.10f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(1f, 4f), 0.35f);
        }
    }

    // ── Homing helper ─────────────────────────────────────────────────────
    Transform FindNearestEnemy()
    {
        Transform nearest  = null;
        float     bestDist = float.MaxValue;
        foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < bestDist) { bestDist = d; nearest = e.transform; }
        }
        return nearest;
    }

    // ── Hit particles ─────────────────────────────────────────────────────
    void SpawnHitParticles()
    {
        for (int i = 0; i < 5; i++)
        {
            var p   = new GameObject("HitP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(new Color(1f, 0.92f, 0.2f));
            psr.sortingOrder = 10;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * 0.09f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(1.5f, 5f),
                    Random.Range(0.15f, 0.3f));
        }
    }
}
