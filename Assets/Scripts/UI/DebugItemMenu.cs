using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Grammarly-style slide-in debug panel.
/// A floating pill badge sticks out from the right edge — click it to
/// slide the item panel in/out. Items are scrollable, toggled individually.
/// </summary>
public class DebugItemMenu : MonoBehaviour
{
    const float PANEL_W = 300f;
    const float PILL_W  =  48f;
    const float PILL_H  =  92f;
    const float HDR_H   =  44f;
    const float ROW_H   =  42f;

    RectTransform panelRT;
    Text          arrowText;
    bool          isOpen;
    bool          animating;

    bool[]  itemOn;
    Image[] btnImg;
    Text[]  btnLbl;

    static readonly Color C_BG     = new Color(0.06f, 0.07f, 0.12f, 0.98f);
    static readonly Color C_PILL   = new Color(0.15f, 0.19f, 0.36f, 1.00f);
    static readonly Color C_ACCENT = new Color(0.40f, 0.58f, 1.00f, 1.00f);
    static readonly Color C_HDR    = new Color(0.10f, 0.13f, 0.23f, 1.00f);
    static readonly Color C_EVEN   = new Color(0.09f, 0.10f, 0.15f, 1.00f);
    static readonly Color C_ODD    = new Color(0.07f, 0.08f, 0.12f, 1.00f);
    static readonly Color C_ON     = new Color(0.12f, 0.68f, 0.24f, 1.00f);
    static readonly Color C_OFF    = new Color(0.22f, 0.22f, 0.28f, 1.00f);

    // ── Init ──────────────────────────────────────────────────────────────

    void Start()
    {
        // Ensure we have a RectTransform (not guaranteed when added via script)
        var self = GetComponent<RectTransform>();
        if (self == null) self = gameObject.AddComponent<RectTransform>();

        // Fill the canvas so anchors reference the actual screen edges
        self.anchorMin = Vector2.zero;
        self.anchorMax = Vector2.one;
        self.offsetMin = self.offsetMax = Vector2.zero;

        int n  = ItemDefinition.All.Count;
        itemOn = new bool[n];
        btnImg = new Image[n];
        btnLbl = new Text[n];
        Build();
    }

    // ── Build ─────────────────────────────────────────────────────────────

    void Build()
    {
        BuildPill();
        BuildPanel();
    }

    /// <summary>Floating badge that sticks out from the right edge at all times.</summary>
    void BuildPill()
    {
        var go = new GameObject("DebugPill");
        go.transform.SetParent(transform, false);

        var rt           = go.AddComponent<RectTransform>();
        rt.anchorMin     = new Vector2(1f, 0.55f);
        rt.anchorMax     = new Vector2(1f, 0.55f);
        rt.pivot         = new Vector2(1f, 0.5f);
        rt.sizeDelta     = new Vector2(PILL_W, PILL_H);
        rt.anchoredPosition = Vector2.zero;   // flush with right edge

        // Pill background
        var img   = go.AddComponent<Image>();
        img.color = C_PILL;

        // Left accent stripe
        var acc = new GameObject("Stripe");
        acc.transform.SetParent(go.transform, false);
        var aRT           = acc.AddComponent<RectTransform>();
        aRT.anchorMin     = new Vector2(0f, 0f);
        aRT.anchorMax     = new Vector2(0f, 1f);
        aRT.pivot         = new Vector2(0f, 0.5f);
        aRT.sizeDelta     = new Vector2(4f, 0f);
        aRT.offsetMin     = new Vector2(0f, 6f);
        aRT.offsetMax     = new Vector2(4f, -6f);
        acc.AddComponent<Image>().color = C_ACCENT;

        // Arrow icon (top portion)
        var aGO = new GameObject("Arrow");
        aGO.transform.SetParent(go.transform, false);
        var aRt       = aGO.AddComponent<RectTransform>();
        aRt.anchorMin = new Vector2(0f, 0.42f);
        aRt.anchorMax = Vector2.one;
        aRt.offsetMin = aRt.offsetMax = Vector2.zero;
        arrowText       = aGO.AddComponent<Text>();
        arrowText.text  = "◄";
        arrowText.fontSize  = 16;
        arrowText.fontStyle = FontStyle.Bold;
        arrowText.alignment = TextAnchor.MiddleCenter;
        arrowText.color     = new Color(0.78f, 0.85f, 1f);
        arrowText.font      = GetFont();
        arrowText.horizontalOverflow = HorizontalWrapMode.Overflow;
        arrowText.verticalOverflow   = VerticalWrapMode.Overflow;

        // "ITEMS" label (bottom portion)
        var lGO = new GameObject("Label");
        lGO.transform.SetParent(go.transform, false);
        var lRt       = lGO.AddComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = new Vector2(1f, 0.44f);
        lRt.offsetMin = lRt.offsetMax = Vector2.zero;
        var lTxt       = lGO.AddComponent<Text>();
        lTxt.text      = "ITEMS";
        lTxt.fontSize  = 9;
        lTxt.fontStyle = FontStyle.Bold;
        lTxt.alignment = TextAnchor.MiddleCenter;
        lTxt.color     = new Color(0.50f, 0.58f, 0.80f);
        lTxt.font      = GetFont();
        lTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        lTxt.verticalOverflow   = VerticalWrapMode.Overflow;

        // Button covers entire pill
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var col = btn.colors;
        col.highlightedColor = new Color(0.24f, 0.29f, 0.52f);
        col.pressedColor     = new Color(0.30f, 0.36f, 0.65f);
        btn.colors = col;
        btn.onClick.AddListener(TogglePanel);
        NoNav(btn);
    }

    /// <summary>The slide-in panel anchored to the right edge, behind the pill.</summary>
    void BuildPanel()
    {
        int   n      = ItemDefinition.All.Count;
        float contentH = n * ROW_H;

        var go = new GameObject("DebugPanel");
        go.transform.SetParent(transform, false);

        panelRT              = go.AddComponent<RectTransform>();
        panelRT.anchorMin    = new Vector2(1f, 0f);
        panelRT.anchorMax    = new Vector2(1f, 1f);
        panelRT.pivot        = new Vector2(1f, 0.5f);
        panelRT.sizeDelta    = new Vector2(PANEL_W, 0f);
        panelRT.anchoredPosition = new Vector2(PANEL_W, 0f);  // start off-screen

        go.AddComponent<Image>().color = C_BG;

        // Left border line
        var bdr = new GameObject("Border");
        bdr.transform.SetParent(go.transform, false);
        var bRT       = bdr.AddComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0f, 0f);
        bRT.anchorMax = new Vector2(0f, 1f);
        bRT.pivot     = new Vector2(0f, 0.5f);
        bRT.sizeDelta = new Vector2(2f, 0f);
        bdr.AddComponent<Image>().color = C_ACCENT;

        // ── Header ────────────────────────────────────────────────────────
        var hdr = new GameObject("Header");
        hdr.transform.SetParent(go.transform, false);
        var hRT       = hdr.AddComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 1f);
        hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(0f, -HDR_H);
        hRT.offsetMax = Vector2.zero;
        hdr.AddComponent<Image>().color = C_HDR;

        var hTxt       = hdr.AddComponent<Text>();
        hTxt.text      = "◆  DEBUG ITEMS";
        hTxt.fontSize  = 12;
        hTxt.fontStyle = FontStyle.Bold;
        hTxt.alignment = TextAnchor.MiddleCenter;
        hTxt.color     = new Color(0.60f, 0.68f, 0.88f);
        hTxt.font      = GetFont();
        hTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        hTxt.verticalOverflow   = VerticalWrapMode.Overflow;

        // ── ScrollRect ────────────────────────────────────────────────────
        var scrGO = new GameObject("Scroll");
        scrGO.transform.SetParent(go.transform, false);
        var scrRT     = scrGO.AddComponent<RectTransform>();
        scrRT.anchorMin = new Vector2(0f, 0f);
        scrRT.anchorMax = new Vector2(1f, 1f);
        scrRT.offsetMin = new Vector2(0f, 0f);
        scrRT.offsetMax = new Vector2(0f, -HDR_H);
        var scr       = scrGO.AddComponent<ScrollRect>();
        scr.horizontal     = false;
        scr.vertical       = true;
        scr.scrollSensitivity = 25f;
        scrGO.AddComponent<Image>().color = Color.clear;  // needed for scroll input

        // Viewport
        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(scrGO.transform, false);
        var vpRT      = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<RectMask2D>();
        scr.viewport = vpRT;

        // Content container
        var ctGO = new GameObject("Content");
        ctGO.transform.SetParent(vpGO.transform, false);
        var ctRT      = ctGO.AddComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0f, 1f);
        ctRT.anchorMax = Vector2.one;
        ctRT.pivot     = new Vector2(0.5f, 1f);
        ctRT.offsetMin = new Vector2(0f, -contentH);
        ctRT.offsetMax = Vector2.zero;
        scr.content    = ctRT;

        // ── Item rows ─────────────────────────────────────────────────────
        for (int i = 0; i < n; i++)
        {
            int  idx  = i;
            var  def  = ItemDefinition.All[i];
            float yTop = -i * ROW_H;
            float yBot = -(i + 1) * ROW_H;

            var row = new GameObject("Row" + i);
            row.transform.SetParent(ctGO.transform, false);
            var rRT       = row.AddComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0f, 1f);
            rRT.anchorMax = new Vector2(1f, 1f);
            rRT.offsetMin = new Vector2(0f, yBot);
            rRT.offsetMax = new Vector2(0f, yTop);
            row.AddComponent<Image>().color = (i % 2 == 0) ? C_EVEN : C_ODD;

            // Rarity colour bar
            var bar = new GameObject("Bar");
            bar.transform.SetParent(row.transform, false);
            var barRT     = bar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(0f, 1f);
            barRT.offsetMin = new Vector2(0f, 4f);
            barRT.offsetMax = new Vector2(4f, -4f);
            bar.AddComponent<Image>().color = RarityCol(def.Rarity);

            // Item name
            AddText(row.transform, def.Name, 11, FontStyle.Bold,
                RarityCol(def.Rarity), TextAnchor.MiddleLeft,
                new Vector2(8f, 0.46f), new Vector2(0.70f, 1f),
                new Vector2(0f, 0f), new Vector2(-4f, -2f));

            // Description
            AddText(row.transform, def.Description, 8, FontStyle.Normal,
                new Color(0.48f, 0.50f, 0.56f), TextAnchor.MiddleLeft,
                new Vector2(8f, 0f), new Vector2(0.70f, 0.50f),
                new Vector2(0f, 2f), new Vector2(-4f, 0f));

            // ON / OFF toggle button
            var btn = new GameObject("Btn");
            btn.transform.SetParent(row.transform, false);
            var btnRT       = btn.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.71f, 0.16f);
            btnRT.anchorMax = new Vector2(1.00f, 0.84f);
            btnRT.offsetMin = new Vector2(4f, 0f);
            btnRT.offsetMax = new Vector2(-8f, 0f);
            btnImg[i]       = btn.AddComponent<Image>();
            btnImg[i].color = C_OFF;

            var bTxt       = btn.AddComponent<Text>();
            bTxt.text      = "OFF";
            bTxt.fontSize  = 11;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color     = Color.white;
            bTxt.font      = GetFont();
            bTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            bTxt.verticalOverflow   = VerticalWrapMode.Overflow;
            btnLbl[i]      = bTxt;

            var b = btn.AddComponent<Button>();
            b.targetGraphic = btnImg[i];
            b.onClick.AddListener(() => ToggleItem(idx));
            NoNav(b);
        }
    }

    // ── Logic ─────────────────────────────────────────────────────────────

    void TogglePanel()
    {
        if (animating) return;
        isOpen = !isOpen;
        arrowText.text = isOpen ? "►" : "◄";
        StartCoroutine(SlidePanel(isOpen ? 0f : PANEL_W));
    }

    IEnumerator SlidePanel(float targetX)
    {
        animating = true;
        float startX = panelRT.anchoredPosition.x;
        float dur    = 0.20f;
        for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
        {
            float ease = 1f - Mathf.Pow(1f - t / dur, 3f);
            panelRT.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, ease), 0f);
            yield return null;
        }
        panelRT.anchoredPosition = new Vector2(targetX, 0f);
        animating = false;
    }

    void ToggleItem(int idx)
    {
        itemOn[idx] = !itemOn[idx];
        var def = ItemDefinition.All[idx];
        var s   = PlayerStats.Instance;
        if (s == null) return;

        if (itemOn[idx])
        {
            s.AddItem(def);
            if (def.EffectType == ItemEffectType.GhostBlade) s.Piercing = true;
        }
        else
        {
            s.ActiveItems.Remove(def);
            if (def.EffectType == ItemEffectType.GhostBlade && !s.HasItem(ItemEffectType.GhostBlade))
                s.Piercing = false;
        }

        btnImg[idx].color = itemOn[idx] ? C_ON  : C_OFF;
        btnLbl[idx].text  = itemOn[idx] ? "ON"  : "OFF";
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void AddText(Transform parent,
        string content, int size, FontStyle style, Color col, TextAnchor align,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        // anchorMin/Max encode (x_anchor, y_anchor) but for offsetMin we treat x as pixels
        // Since we mix pixel offsets and normalised anchors, build carefully:
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        // anchorMin.x is pixel left-offset here (hack to pack params) — decode:
        rt.anchorMin = new Vector2(0f,       anchorMin.y);
        rt.anchorMax = new Vector2(anchorMax.x, anchorMax.y);
        rt.offsetMin = new Vector2(anchorMin.x, offsetMin.y);
        rt.offsetMax = new Vector2(offsetMax.x, offsetMax.y);
    }

    void NoNav(Button b)
    {
        var n = b.navigation; n.mode = Navigation.Mode.None; b.navigation = n;
    }

    Color RarityCol(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Legendary: return new Color(1.0f, 0.75f, 0.10f);
            case ItemRarity.Rare:      return new Color(0.40f, 0.52f, 1.00f);
            case ItemRarity.Uncommon:  return new Color(0.20f, 0.88f, 0.30f);
            default:                   return new Color(0.62f, 0.62f, 0.62f);
        }
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
