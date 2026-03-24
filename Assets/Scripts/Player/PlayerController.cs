using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    Rigidbody2D  rb;
    PlayerStats  stats;
    Camera       cam;
    SpriteRenderer sr;

    float health;
    float shootTimer;
    float dashTimer;
    bool  dashing;
    bool  invincible;
    bool  shieldActive;
    float shieldRechargeTimer;

    public float HealthFrac => stats ? health / stats.MaxHealth : 1f;
    public float DashFrac   => stats ? Mathf.Clamp01(1f - dashTimer / stats.DashCooldown) : 1f;

    void Awake()
    {
        Instance = this;
        rb    = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        sr    = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        cam    = Camera.main;
        health = stats.MaxHealth;
        HUDManager.Instance?.UpdateHealth(HealthFrac);
    }

    bool IsActive() {
        var s = GameManager.Instance?.State;
        return s == GameState.Playing || s == GameState.BossTime;
    }

    void Update()
    {
        if (!IsActive()) return;
        HandleShooting();
        HandleDash();

        if (stats.HasShield && !shieldActive)
        {
            shieldRechargeTimer -= Time.deltaTime;
            if (shieldRechargeTimer <= 0f) shieldActive = true;
        }
    }

    void FixedUpdate()
    {
        if (!IsActive()) return;
        if (dashing) return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        rb.linearVelocity = input * stats.MoveSpeed;

        if (cam != null)
        {
            Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 lookDir = (mouse - transform.position).normalized;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // ── Shooting ──────────────────────────────────────────────────────────
    void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0) && shootTimer <= 0f)
        {
            shootTimer = 1f / stats.FireRate;
            FireBullets();
        }
    }

    void FireBullets()
    {
        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir   = (mouse - transform.position).normalized;
        SpawnBullet(dir);

        // Double Shot — second bullet slightly offset
        if (stats.HasItem(ItemEffectType.DoubleShot))
            SpawnBullet(Rotate(dir, 14f));

        if (stats.SplitShot)
        {
            float spread = 22f;
            for (int i = 1; i <= stats.SplitCount; i++)
            {
                SpawnBullet(Rotate(dir,  spread * i));
                SpawnBullet(Rotate(dir, -spread * i));
            }
        }
    }

    void SpawnBullet(Vector2 dir)
    {
        var go  = new GameObject("PlayerBullet");
        go.transform.position = transform.position;

        var bsr          = go.AddComponent<SpriteRenderer>();
        bsr.sprite       = SpriteFactory.Circle(new Color(1f, 0.95f, 0.15f));
        bsr.sortingOrder = 8;

        var col      = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.5f;

        var brb           = go.AddComponent<Rigidbody2D>();
        brb.gravityScale  = 0f;
        brb.bodyType      = RigidbodyType2D.Kinematic;

        var b = go.AddComponent<PlayerBullet>();
        b.Init(dir, stats.Damage, stats.BulletSize, stats.BulletSpeed, stats.Piercing);
    }

    // ── Dash ──────────────────────────────────────────────────────────────
    void HandleDash()
    {
        if (dashTimer > 0f) { dashTimer -= Time.deltaTime; HUDManager.Instance?.UpdateDash(DashFrac); }
        if (Input.GetKeyDown(KeyCode.Space) && dashTimer <= 0f && !dashing)
            StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        dashing    = true;
        invincible = true;
        dashTimer  = stats.DashCooldown;

        Vector2 dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dashDir == Vector2.zero)
        {
            Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
            dashDir = (mouse - transform.position).normalized;
        }

        StartCoroutine(GhostTrail());

        float elapsed = 0f, dur = 0.12f;
        while (elapsed < dur)
        {
            rb.linearVelocity = dashDir * stats.MoveSpeed * 7f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        dashing = false;
        yield return new WaitForSeconds(0.07f);
        invincible = false;
        HUDManager.Instance?.UpdateDash(DashFrac);
    }

    IEnumerator GhostTrail()
    {
        for (int i = 0; i < 6; i++)
        {
            var ghost    = new GameObject("Ghost");
            ghost.transform.position   = transform.position;
            ghost.transform.rotation   = transform.rotation;
            ghost.transform.localScale = transform.localScale;

            var gsr          = ghost.AddComponent<SpriteRenderer>();
            gsr.sprite       = sr?.sprite;
            gsr.color        = new Color(0f, 0.85f, 1f, 0.5f);
            gsr.sortingOrder = 4;
            ghost.AddComponent<DashGhost>();

            yield return new WaitForSeconds(0.018f);
        }
    }

    // ── Damage & Healing ──────────────────────────────────────────────────
    public void TakeDamage(float dmg)
    {
        if (invincible || GameManager.Instance?.State == GameState.Dead) return;

        if (shieldActive && stats.HasShield)
        {
            shieldActive         = false;
            shieldRechargeTimer  = 10f;
            StartCoroutine(FlashColor(new Color(0f, 0.8f, 1f)));
            CameraController.Instance?.Shake(0.1f, 0.15f);
            return;
        }

        health = Mathf.Max(0f, health - dmg);
        HUDManager.Instance?.UpdateHealth(HealthFrac);
        CameraController.Instance?.Shake(0.2f, 0.22f);
        StartCoroutine(DamageFlash());

        if (health <= 0f) GameManager.Instance.PlayerDied();
    }

    public void HealPercent(float pct)
    {
        health = Mathf.Min(health + stats.MaxHealth * pct, stats.MaxHealth);
        HUDManager.Instance?.UpdateHealth(HealthFrac);
    }

    IEnumerator DamageFlash()
    {
        invincible = true;
        for (int i = 0; i < 5; i++)
        {
            if (sr) sr.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            if (sr) sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
        }
        invincible = false;
    }

    IEnumerator FlashColor(Color c)
    {
        if (sr) sr.color = c;
        yield return new WaitForSeconds(0.25f);
        if (sr) sr.color = Color.white;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        var e = col.gameObject.GetComponent<EnemyBase>();
        if (e != null) TakeDamage(e.ContactDamage);
    }

    // ── Utility ───────────────────────────────────────────────────────────
    static Vector2 Rotate(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad));
    }
}
