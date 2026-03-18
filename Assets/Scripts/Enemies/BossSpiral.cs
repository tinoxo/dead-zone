using UnityEngine;
using System.Collections;

/// <summary>
/// Boss 1 – Spiral: shoots bullets in an ever-rotating spiral pattern.
/// Phase 2 (below 50% HP): faster rotation + denser spiral.
/// </summary>
public class BossSpiral : BossBase
{
    float spiralAngle  = 0f;
    float spiralSpeed  = 200f;  // degrees per second
    float burstTimer   = 0f;
    float burstInterval= 0.08f;
    int   bulletsPerBurst = 3;
    float bulletDmg    = 10f;
    float bulletSpd    = 5.5f;

    float orbitRadius  = 3f;
    float orbitSpeed   = 1.2f;
    float orbitAngle   = 0f;

    protected override void Awake()
    {
        base.Awake();
        BossName      = "VORTEX";
        MaxHealth     = 600f;
        ContactDamage = 18f;
        MoveSpeed     = 1.5f;
        health        = MaxHealth;
    }

    protected override void DoUpdate()
    {
        if (player == null) return;

        // Orbit slowly around the center of the arena
        orbitAngle += orbitSpeed * Time.deltaTime;
        float ox = Mathf.Cos(orbitAngle) * orbitRadius;
        float oy = Mathf.Sin(orbitAngle) * orbitRadius;
        Vector2 target = new Vector2(ox, oy);
        rb.linearVelocity = (target - (Vector2)transform.position) * 3f;

        // Phase 2 at 50% HP
        if (phase == 1 && health < MaxHealth * 0.5f)
        {
            phase          = 2;
            spiralSpeed    = 340f;
            burstInterval  = 0.05f;
            bulletsPerBurst= 5;
            bulletSpd      = 7f;
            StartCoroutine(PhaseFlash());
        }

        // Spiral shooting
        spiralAngle  += spiralSpeed * Time.deltaTime;
        burstTimer   -= Time.deltaTime;
        if (burstTimer <= 0f)
        {
            burstTimer = burstInterval;
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                float a   = (spiralAngle + (360f / bulletsPerBurst) * i) * Mathf.Deg2Rad;
                Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                ShootBullet(d, bulletDmg, bulletSpd, new Color(1f, 0.3f, 0.05f));
            }
        }
    }

    IEnumerator PhaseFlash()
    {
        for (int i = 0; i < 6; i++)
        {
            if (sr) sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            if (sr) sr.color = new Color(1f, 0.2f, 0.0f);
            yield return new WaitForSeconds(0.08f);
        }
    }
}
