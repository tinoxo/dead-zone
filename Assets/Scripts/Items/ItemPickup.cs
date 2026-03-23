using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A physical item gem in the world. Shows a bobbing label when the
/// player is close. Auto-picks up on contact. Destroys its pair.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    // ── Data ──────────────────────────────────────────────────────────────
    public ItemDefinition Data { get; private set; }

    // The other item in this boss-drop pair — destroyed when this is picked up
    public ItemPickup Partner;

    // ── Visuals ───────────────────────────────────────────────────────────
    SpriteRenderer gemSr;
    GameObject     labelRoot;       // child that bobs
    CanvasGroup    labelGroup;      // for smooth fade
    Text           nameText;
    Text           descText;
    Text           rarityText;

    // ── State ─────────────────────────────────────────────────────────────
    float bobTimer;
    float pulseTimer;
    bool  pickedUp;

    const float LABEL_SHOW_DIST  = 3.5f;  // show label within this range
    const float PICKUP_DIST      = 0.55f; // auto-pickup radius
    const float BOB_SPEED        = 2.2f;
    const float BOB_AMPLITUDE    = 0.12f;
    const float LABEL_FADE_SPEED = 4f;

    // ── Init ──────────────────────────────────────────────────────────────
    public void Init(ItemDefinition def)
    {
        Data = def;
        BuildVisuals();
    }

    void BuildVisuals()
    {
        // ── Gem sprite ────────────────────────────────────────────────────
        gemSr = gameObject.AddComponent<SpriteRenderer>();
        gemSr.sprite       = SpriteFactory.Diamond(Data.GemColor);
        gemSr.sortingOrder = 6;
        transform.localScale = Vector3.one * 0.55f;

        // Trigger collider
        var col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.9f;

        // ── World-space label ─────────────────────────────────────────────
        labelRoot = new GameObject("ItemLabel");
        labelRoot.transform.SetParent(transform, false);
        labelRoot.transform.localPosition = Vector3.up * 1.6f;

        var canvas = labelRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 20;

        var rt = labelRoot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 70f);
        labelRoot.transform.localScale = Vector3.one * 0.013f;

        labelGroup       = labelRoot.AddComponent<CanvasGroup>();
        labelGroup.alpha = 0f;

        // Rarity badge (top)
        rarityText = MakeLabelText(labelRoot.transform,
            Data.Rarity.ToString().ToUpper(),
            fontSize: 13,
            anchor: TextAnchor.UpperCenter,
            col: RarityColor(Data.Rarity),
            aMin: new Vector2(0f, 0.72f), aMax: Vector2.one,
            bold: false);

        // Item name (bold, large)
        nameText = MakeLabelText(labelRoot.transform,
            Data.Name,
            fontSize: 20,
            anchor: TextAnchor.MiddleCenter,
            col: Color.white,
            aMin: new Vector2(0f, 0.40f), aMax: new Vector2(1f, 0.74f),
            bold: true);

        // Description (small, gray)
        descText = MakeLabelText(labelRoot.transform,
            Data.Description,
            fontSize: 12,
            anchor: TextAnchor.UpperCenter,
            col: new Color(0.78f, 0.78f, 0.78f),
            aMin: Vector2.zero, aMax: new Vector2(1f, 0.42f),
            bold: false);
    }

    // ── Update ────────────────────────────────────────────────────────────
    void Update()
    {
        if (pickedUp) return;

        float dt = Time.deltaTime;
        bobTimer   += dt * BOB_SPEED;
        pulseTimer += dt * 1.4f;

        // Bob the label up and down
        float bob = Mathf.Sin(bobTimer) * BOB_AMPLITUDE;
        labelRoot.transform.localPosition = new Vector3(0f, 1.6f + bob, 0f);

        // Pulse the gem scale slightly
        float pulse = 1f + Mathf.Sin(pulseTimer) * 0.06f;
        transform.localScale = Vector3.one * 0.55f * pulse;

        // Proximity fade for label
        float dist = PlayerDistance();
        float targetAlpha = dist < LABEL_SHOW_DIST ? 1f : 0f;
        labelGroup.alpha = Mathf.MoveTowards(labelGroup.alpha, targetAlpha, LABEL_FADE_SPEED * dt);

        // Auto-pickup on contact
        if (dist < PICKUP_DIST)
            PickUp();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;
        if (other.GetComponent<PlayerController>() != null)
            PickUp();
    }

    // ── Pickup logic ──────────────────────────────────────────────────────
    void PickUp()
    {
        if (pickedUp) return;
        pickedUp = true;

        // Apply effect
        ItemManager.Instance?.ApplyItem(Data);

        // Spawn pickup flash particles
        SpawnPickupFlash();

        // Destroy partner first
        if (Partner != null && !Partner.pickedUp)
            Destroy(Partner.gameObject);

        Destroy(gameObject);
    }

    void SpawnPickupFlash()
    {
        for (int i = 0; i < 16; i++)
        {
            var p = new GameObject("PickupP");
            var psr = p.AddComponent<SpriteRenderer>();
            psr.sprite       = SpriteFactory.Circle(Data.GemColor);
            psr.sortingOrder = 12;
            p.transform.position   = transform.position;
            p.transform.localScale = Vector3.one * 0.08f;
            var sp = p.AddComponent<SimpleParticle>();
            sp.Init(Random.insideUnitCircle.normalized * Random.Range(3f, 8f),
                    Random.Range(0.4f, 0.8f));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    float PlayerDistance()
    {
        var pc = PlayerController.Instance;
        if (pc == null) return 999f;
        return Vector2.Distance(transform.position, pc.transform.position);
    }

    Color RarityColor(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Legendary: return new Color(1f,   0.75f, 0.1f);
            case ItemRarity.Rare:      return new Color(0.4f, 0.5f,  1f);
            case ItemRarity.Uncommon:  return new Color(0.2f, 0.9f,  0.3f);
            default:                   return new Color(0.75f,0.75f, 0.75f);
        }
    }

    Text MakeLabelText(Transform parent, string content, int fontSize,
        TextAnchor anchor, Color col, Vector2 aMin, Vector2 aMax, bool bold)
    {
        var go = new GameObject("LT");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.alignment = anchor;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(6f, 2f);
        rt.offsetMax = new Vector2(-6f, -2f);
        return t;
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
