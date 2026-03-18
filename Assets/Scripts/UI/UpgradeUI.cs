using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance { get; private set; }

    GameObject overlay;
    List<UpgradeDefinition> currentUpgrades = new List<UpgradeDefinition>();

    void Awake() => Instance = this;

    public void Build(Canvas canvas)
    {
        // Dark semi-transparent overlay
        overlay = new GameObject("UpgradeOverlay");
        overlay.transform.SetParent(canvas.transform, false);

        var bg = overlay.AddComponent<Image>();
        bg.color  = new Color(0f, 0f, 0.04f, 0.88f);
        bg.sprite = SpriteFactory.Square(Color.white);
        var bgRect          = bg.rectTransform;
        bgRect.anchorMin    = Vector2.zero;
        bgRect.anchorMax    = Vector2.one;
        bgRect.offsetMin    = Vector2.zero;
        bgRect.offsetMax    = Vector2.zero;

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(overlay.transform, false);
        var title    = titleGO.AddComponent<Text>();
        title.text   = "— CHOOSE AN UPGRADE —";
        title.fontSize = 26;
        title.font   = GetFont();
        title.color  = new Color(0.9f, 0.9f, 1f);
        title.alignment = TextAnchor.MiddleCenter;
        var tRect = title.rectTransform;
        tRect.anchorMin = new Vector2(0f, 0.75f);
        tRect.anchorMax = new Vector2(1f, 0.88f);
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        overlay.SetActive(false);
    }

    public void Show(List<UpgradeDefinition> upgrades)
    {
        currentUpgrades = upgrades;
        overlay.SetActive(true);

        // Remove old cards
        for (int i = overlay.transform.childCount - 1; i >= 0; i--)
        {
            var c = overlay.transform.GetChild(i);
            if (c.name.StartsWith("Card")) Destroy(c.gameObject);
        }

        // Spawn 3 cards
        float cardW = 240f, cardH = 300f;
        float spacing = 30f;
        float totalW  = cardW * upgrades.Count + spacing * (upgrades.Count - 1);
        float startX  = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < upgrades.Count; i++)
        {
            CreateCard(upgrades[i], new Vector2(startX + i * (cardW + spacing), -30f));
        }
    }

    void CreateCard(UpgradeDefinition u, Vector2 pos)
    {
        var cardGO = new GameObject($"Card_{u.ID}");
        cardGO.transform.SetParent(overlay.transform, false);

        // Background
        var bg       = cardGO.AddComponent<Image>();
        bg.sprite    = SpriteFactory.Square(new Color(0.08f, 0.08f, 0.14f));
        bg.color     = new Color(0.08f, 0.08f, 0.14f);
        var bgRect   = bg.rectTransform;
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot     = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta        = new Vector2(240f, 290f);
        bgRect.anchoredPosition = pos;

        // Tier color strip at top
        var stripGO = new GameObject("TierStrip");
        stripGO.transform.SetParent(cardGO.transform, false);
        var strip    = stripGO.AddComponent<Image>();
        strip.color  = u.TierColor;
        strip.sprite = SpriteFactory.Square(Color.white);
        var stripRect = strip.rectTransform;
        stripRect.anchorMin = new Vector2(0f, 0.88f);
        stripRect.anchorMax = Vector2.one;
        stripRect.offsetMin = Vector2.zero;
        stripRect.offsetMax = Vector2.zero;

        // Tier label
        AddText(cardGO.transform, u.Tier.ToString().ToUpper(), 11, new Vector2(0f, 0.88f), new Vector2(1f, 1f), u.TierColor);

        // Name
        AddText(cardGO.transform, u.Name, 18, new Vector2(0f, 0.6f), new Vector2(1f, 0.88f), Color.white);

        // Description
        AddText(cardGO.transform, u.Description, 13, new Vector2(0f, 0.3f), new Vector2(1f, 0.62f), new Color(0.8f, 0.8f, 0.8f));

        // Pick button
        var btnGO = new GameObject("PickBtn");
        btnGO.transform.SetParent(cardGO.transform, false);
        var btnImg   = btnGO.AddComponent<Image>();
        btnImg.color = u.TierColor * 0.8f;
        btnImg.sprite = SpriteFactory.Square(Color.white);
        var btnRect  = btnImg.rectTransform;
        btnRect.anchorMin = new Vector2(0.1f, 0.04f);
        btnRect.anchorMax = new Vector2(0.9f, 0.24f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        var btn = btnGO.AddComponent<Button>();
        var captured = u;
        btn.onClick.AddListener(() => OnPick(captured));

        var btnText = AddText(btnGO.transform, "SELECT", 14, Vector2.zero, Vector2.one, Color.white);
        btnText.rectTransform.anchorMin = Vector2.zero;
        btnText.rectTransform.anchorMax = Vector2.one;
        btnText.rectTransform.offsetMin = Vector2.zero;
        btnText.rectTransform.offsetMax = Vector2.zero;
    }

    void OnPick(UpgradeDefinition u)
    {
        overlay.SetActive(false);
        UpgradeManager.Instance?.ApplyUpgrade(u);
    }

    Text AddText(Transform parent, string content, int size, Vector2 anchorMin, Vector2 anchorMax, Color col)
    {
        var go   = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t    = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.font      = GetFont();
        t.color     = col;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        var rect    = t.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(6f, 2f);
        rect.offsetMax = new Vector2(-6f, -2f);
        return t;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
