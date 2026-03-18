using UnityEngine;

public class DashGhost : MonoBehaviour
{
    SpriteRenderer sr;
    float lifetime = 0.22f;
    float elapsed;
    Color startColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr) startColor = sr.color;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;
        if (sr) sr.color = new Color(startColor.r, startColor.g, startColor.b, (1f - t) * 0.45f);
        if (elapsed >= lifetime) Destroy(gameObject);
    }
}
