using UnityEngine;

/// <summary>Slow, massive health, deals heavy contact damage.</summary>
public class TankyEnemy : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth     = 220f;
        MoveSpeed     = 1.3f;
        ContactDamage = 28f;
        ScoreValue    = 250;
        health        = MaxHealth;
    }

    protected override void DoUpdate()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * MoveSpeed;
    }
}
