using UnityEngine;

public class SimpleParticle : MonoBehaviour
{
    Vector2 velocity;
    float lifetime;
    float elapsed;
    SpriteRenderer sr;
    Color startColor;
    float startScale;

    public void Init(Vector2 vel, float life)
    {
        velocity  = vel;
        lifetime  = life;
        elapsed   = 0f;
        sr        = GetComponent<SpriteRenderer>();
        startColor = sr ? sr.color : Color.white;
        startScale = transform.localScale.x;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.88f;
        if (sr) sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
        transform.localScale = Vector3.one * startScale * (1f - t * 0.5f);
        if (elapsed >= lifetime) Destroy(gameObject);
    }
}
