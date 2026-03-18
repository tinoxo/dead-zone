using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    public float Damage = 10f;
    Vector2 dir;
    float   speed;
    float   life = 6f;

    public void Init(Vector2 direction, float dmg, float spd)
    {
        dir    = direction.normalized;
        Damage = dmg;
        speed  = spd;
        life   = 6f;
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<EnemyBullet>() != null) return;
        if (other.GetComponent<EnemyBase>()   != null) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.TakeDamage(Damage);
            Destroy(gameObject);
        }
    }
}
