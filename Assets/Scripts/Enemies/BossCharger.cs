using UnityEngine;
using System.Collections;

/// <summary>
/// Boss 2 – Charger: telegraphs then rockets toward the player.
/// Also spawns homing minions in phase 2.
/// </summary>
public class BossCharger : BossBase
{
    enum ChargerState { Idle, Telegraphing, Charging, Recovering }
    ChargerState chargerState = ChargerState.Idle;

    float idleTimer      = 1.5f;
    float telegraphTime  = 0.9f;
    float chargeSpeed    = 18f;
    float chargeDuration = 0.35f;
    Vector2 chargeDir;

    float shootTimer = 0f;
    float shootInterval = 0.5f;
    float bulletDmg  = 14f;
    float bulletSpd  = 6f;

    protected override void Awake()
    {
        base.Awake();
        BossName      = "RAMPAGE";
        MaxHealth     = 800f;
        ContactDamage = 30f;
        MoveSpeed     = 2.5f;
        health        = MaxHealth;
    }

    protected override void DoUpdate()
    {
        if (player == null) return;

        // Phase 2 at 50% HP — faster charges, shotgun bullets
        if (phase == 1 && health < MaxHealth * 0.5f)
        {
            phase         = 2;
            telegraphTime = 0.55f;
            chargeSpeed   = 24f;
            shootInterval = 0.28f;
            StartCoroutine(PhaseFlash());
        }

        switch (chargerState)
        {
            case ChargerState.Idle:
                // Slowly drift toward player
                Vector2 drift = (player.position - transform.position).normalized;
                rb.linearVelocity = drift * MoveSpeed;

                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    idleTimer    = phase == 1 ? 2f : 1.2f;
                    chargerState = ChargerState.Telegraphing;
                    StartCoroutine(TelegraphCharge());
                }

                // Spread shot toward player
                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0f)
                {
                    shootTimer = shootInterval;
                    ShootSpread(drift);
                }
                break;

            case ChargerState.Charging:
                rb.linearVelocity = chargeDir * chargeSpeed;
                break;

            case ChargerState.Telegraphing:
            case ChargerState.Recovering:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    IEnumerator TelegraphCharge()
    {
        chargerState = ChargerState.Telegraphing;
        chargeDir    = (player.position - transform.position).normalized;

        // Flash white to telegraph
        float elapsed = 0f;
        while (elapsed < telegraphTime)
        {
            if (sr) sr.color = elapsed % 0.15f < 0.075f ? Color.white : new Color(0.2f, 0.4f, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (sr) sr.color = new Color(0.2f, 0.4f, 1f);

        // CHARGE
        chargerState = ChargerState.Charging;
        yield return new WaitForSeconds(chargeDuration);

        // Recover
        chargerState = ChargerState.Recovering;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.4f);
        chargerState = ChargerState.Idle;
    }

    void ShootSpread(Vector2 dir)
    {
        int count = phase == 1 ? 3 : 5;
        float spread = 20f;
        for (int i = -(count / 2); i <= count / 2; i++)
        {
            float  a    = i * spread * Mathf.Deg2Rad;
            Vector2 d   = new Vector2(
                dir.x * Mathf.Cos(a) - dir.y * Mathf.Sin(a),
                dir.x * Mathf.Sin(a) + dir.y * Mathf.Cos(a));
            ShootBullet(d, bulletDmg, bulletSpd, new Color(0.2f, 0.5f, 1f));
        }
    }

    IEnumerator PhaseFlash()
    {
        for (int i = 0; i < 8; i++)
        {
            if (sr) sr.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            if (sr) sr.color = new Color(0.2f, 0.4f, 1f);
            yield return new WaitForSeconds(0.06f);
        }
    }
}
