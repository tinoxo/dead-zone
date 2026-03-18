using UnityEngine;

/// <summary>Fast enemy that chases the player directly.</summary>
public class ChaserEnemy : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth     = 40f;
        MoveSpeed     = 3.5f;
        ContactDamage = 12f;
        ScoreValue    = 100;
        health        = MaxHealth;
    }

    protected override void DoUpdate()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * MoveSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
