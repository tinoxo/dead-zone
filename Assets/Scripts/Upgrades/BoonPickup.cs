using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Physical boon orb in the world. Player walks into it to receive the upgrade.
/// Looks like a glowing circle (distinct from item diamond gems).
/// </summary>
public class BoonPickup : MonoBehaviour
{
    public BoonSet    Set;
    public BoonEffect Effect;

    SpriteRenderer orbSr;
    SpriteRenderer ringSr;
    GameObject     labelRoot;
    CanvasGroup    labelGroup;

    bool  pickedUp;
    float bobTimer;
    float pulseTimer;

    const float LABEL_DIST  = 5.5f;
    const float PICKUP_DIST = 1.0f;

    public static BoonPickup Spawn(Vector2 pos, BoonSet set)
    {
        var pool   = BoonDefinition.GetPool(set);
        var effect = pool[Random.Range(0, pool.Count)];

        var go = new GameObject("Boon_" + set);
        go.transform.position = pos;
        var bp    = go.AddComponent<BoonPickup>();
        bp.Set    = set;
        bp.Effect = effect;
        bp.Build();
        return bp;
    }

    void Build()
    {
        Color col = BoonDefinition.SetColor(Set);

        // Orb (circle, larger than item gems which are diamonds at 1.1)
        orbSr              = gameObject.AddComponent<SpriteRenderer>();
        orbSr.sprite       = SpriteFactory.Circle(col);
        orbSr.color        = col;
        orbSr.sortingOrder = 6;
        transform.localScale = Vector3.one * 1.3f;

        // Outer glow ring
        var ringGO = new GameObject("Ring");
        ringGO.transform.SetParent(transform, false);
        ringGO.transform.localScale = Vector3.one * 1.6f;
        ringSr              = ringGO.AddComponent<SpriteRenderer>();
        ringSr.sprite       = SpriteFactory.Circle(col);
        ringSr.color        = new Color(col.r, col.g, col.b, 0.20f);
        ringSr.sortingOrder = 5;

        // Trigger collider
        var cc = gameObject.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius    = 0.85f;

        BuildLabel(col);
    }

    void BuildLabel(Color col)
    {
        labelRoot = new GameObject("BoonLabel");
        labelRoot.transform.SetParent(transform, false);
        labelRoot.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        labelRoot.transform.localScale    = Vector3.one * 0.012f;

        var cv = labelRoot.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.WorldSpace;
        cv.sortingOrder = 20;
        labelRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(290f, 160f);

        labelGroup       = labelRoot.AddComponent<CanvasGroup>();
        labelGroup.alpha = 0f;

        Color dimCol = new Color(col.r * 0.65f, col.g * 0.65f, col.b * 0.65f);

        // Set name — large, bold, set color
        MakeText(labelRoot.transform, BoonDefinition.SetLabel(Set),
            30, FontStyle.Bold, col,
            new Vector2(0f, 0.74f), Vector2.one);

        // Set subtitle — small, dim
        MakeText(labelRoot.transform, BoonDefinition.SetSubtitle(Set),
            12, FontStyle.Normal, dimCol,
            new Vector2(0f, 0.58f), new Vector2(1f, 0.76f));

        // Upgrade name — white bold
        MakeText(labelRoot.transform, Effect.Name,
            21, FontStyle.Bold, Color.white,
            new Vector2(0f, 0.34f), new Vector2(1f, 0.60f));

        // Upgrade description — gray
        MakeText(labelRoot.transform, Effect.Description,
            14, FontStyle.Normal, new Color(0.70f, 0.70f, 0.70f),
            new Vector2(0f, 0.14f), new Vector2(1f, 0.36f));

        // Hint
        MakeText(labelRoot.transform, "[ WALK INTO ]",
            11, FontStyle.Normal, new Color(0.42f, 0.42f, 0.42f),
            Vector2.zero, new Vector2(1f, 0.16f));
    }

    void Update()
    {
        if (pickedUp) return;

        bobTimer   += Time.deltaTime * 2.0f;
        pulseTimer += Time.deltaTime * 1.6f;

        // Bob label
        if (labelRoot)
            labelRoot.transform.localPosition =
                new Vector3(0f, 2.2f + Mathf.Sin(bobTimer) * 0.1f, 0f);

        // Pulse ring
        if (ringSr)
        {
            float a = 0.14f + Mathf.Sin(pulseTimer) * 0.10f;
            var c = ringSr.color; c.a = a; ringSr.color = c;
        }

        // Fade label by proximity
        float dist = PlayerDist();
        if (labelGroup)
            labelGroup.alpha = Mathf.MoveTowards(labelGroup.alpha,
                dist < LABEL_DIST ? 1f : 0f, 5f * Time.deltaTime);

        if (dist < PICKUP_DIST) PickUp();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!pickedUp && other.GetComponent<PlayerController>() != null)
            PickUp();
    }

    void PickUp()
    {
        if (pickedUp) return;
        pickedUp = true;
        BoonManager.Instance?.OnBoonPicked(this);
        SpawnPickupFlash();
        Destroy(gameObject);
    }

    void SpawnPickupFlash()
    {
        Color col = BoonDefinition.SetColor(Set);
        for (int i = 0; i < 22; i++)
        {
            var p   = new GameObject("BP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(col);
            psr.sortingOrder = 12;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * 0.14f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(3f, 10f),
                    Random.Range(0.4f, 1.0f));
        }
    }

    float PlayerDist()
    {
        var pc = PlayerController.Instance;
        return pc ? Vector2.Distance(transform.position, pc.transform.position) : 999f;
    }

    Text MakeText(Transform parent, string content, int fontSize,
        FontStyle style, Color col, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(4f, 2f); rt.offsetMax = new Vector2(-4f, -2f);
        return t;
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
