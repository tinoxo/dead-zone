using UnityEngine;

/// <summary>Keeps a preferred distance and shoots bullets at the player.</summary>
public class RangedEnemy : EnemyBase
{
    float preferredDist = 6.5f;
    float shootInterval = 2f;
    float shootTimer;
    float bulletDmg     = 8f;
    float bulletSpd     = 7f;

    protected override void Awake()
    {
        base.Awake();
        MaxHealth     = 28f;
        MoveSpeed     = 2.2f;
        ContactDamage = 8f;
        ScoreValue    = 150;
        health        = MaxHealth;
        shootTimer    = Random.Range(0.5f, shootInterval);
    }

    protected override void DoUpdate()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 toPlayer = (player.position - transform.position).normalized;

        // Orbit / keep distance
        if (dist < preferredDist - 1f)
            rb.linearVelocity = -toPlayer * MoveSpeed;
        else if (dist > preferredDist + 1f)
            rb.linearVelocity = toPlayer * MoveSpeed;
        else
            rb.linearVelocity = new Vector2(-toPlayer.y, toPlayer.x) * MoveSpeed * 0.4f; // strafe

        // Face player
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Shoot
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = shootInterval;
            FireBullet(toPlayer);
        }
    }

    void FireBullet(Vector2 dir)
    {
        var go  = new GameObject("EnemyBullet");
        go.transform.position = transform.position;

        var bsr          = go.AddComponent<SpriteRenderer>();
        bsr.sprite       = SpriteFactory.Circle(new Color(1f, 0.25f, 0.25f));
        bsr.sortingOrder = 7;
        go.transform.localScale = Vector3.one * 0.2f;

        var col      = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.5f;

        var brb          = go.AddComponent<Rigidbody2D>();
        brb.gravityScale = 0f;
        brb.bodyType     = RigidbodyType2D.Kinematic;

        var eb = go.AddComponent<EnemyBullet>();
        eb.Init(dir, bulletDmg, bulletSpd);
    }

    public override void ScaleStats(float mult)
    {
        base.ScaleStats(mult);
        bulletDmg      *= mult;
        bulletSpd      *= Mathf.Sqrt(mult);
        shootInterval   = Mathf.Max(0.5f, shootInterval / Mathf.Sqrt(mult));
    }
}
