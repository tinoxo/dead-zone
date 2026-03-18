using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBullet : MonoBehaviour
{
    public float Damage;
    public bool  Piercing;

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
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit other bullets or player
        if (other.GetComponent<PlayerBullet>() != null) return;
        if (other.GetComponent<PlayerController>() != null) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(Damage);
            SpawnHitParticles();
            if (!Piercing) Destroy(gameObject);
        }
    }

    void SpawnHitParticles()
    {
        for (int i = 0; i < 5; i++)
        {
            var p = new GameObject("HitP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(new Color(1f, 0.92f, 0.2f));
            psr.sortingOrder = 10;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * 0.09f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(1.5f, 5f), Random.Range(0.15f, 0.3f));
        }
    }
}
