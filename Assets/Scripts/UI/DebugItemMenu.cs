using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Grammarly-style debug sidebar.
/// Pill tab is a child of the panel — slides with it.
/// Closed: pill protrudes at screen right edge.
/// Open  : pill protrudes from panel left edge.
/// </summary>
public class DebugItemMenu : MonoBehaviour
{
    const float PANEL_W = 280f;
    const float PILL_W  =  44f;
    const float PILL_H  =  88f;
    const float HDR_H   =  40f;
    const float ROW_H   =  50f;

    RectTransform panelRT;
    Text          arrowText;
    bool          isOpen;
    bool          animating;

    bool[]  itemOn;
    Image[] btnImg;
    Text[]  btnLbl;

    static readonly Color C_BG     = new Color(0.06f, 0.07f, 0.13f, 0.97f);
    static readonly Color C_PILL   = new Color(0.14f, 0.18f, 0.34f, 1.00f);
    static readonly Color C_ACCENT = new Color(0.38f, 0.56f, 1.00f, 1.00f);
    static readonly Color C_HDR    = new Color(0.10f, 0.12f, 0.22f, 1.00f);
    static readonly Color C_EVEN   = new Color(0.09f, 0.10f, 0.16f, 1.00f);
    static readonly Color C_ODD    = new Color(0.07f, 0.08f, 0.12f, 1.00f);
    static readonly Color C_ON     = new Color(0.12f, 0.68f, 0.24f, 1.00f);
    static readonly Color C_OFF    = new Color(0.20f, 0.21f, 0.27f, 1.00f);

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        var self = GetComponent<RectTransform>();
        if (self == null) self = gameObject.AddComponent<RectTransform>();
        self.anchorMin = Vector2.zero;
        self.anchorMax = Vector2.one;
        self.offsetMin = self.offsetMax = Vector2.zero;

        int n  = ItemDefinition.All.Count;
        itemOn = new bool[n];
        btnImg = new Image[n];
        btnLbl = new Text[n];

        BuildPanel();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Panel
    // ─────────────────────────────────────────────────────────────────────────

    void BuildPanel()
    {
        int   n        = ItemDefinition.All.Count;
        float contentH = n * ROW_H;

        // Root panel GO
        var panelGO      = UIObj("DebugPanel", transform);
        panelRT          = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 0f);
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot     = new Vector2(1f, 0.5f);
        panelRT.sizeDelta = new Vector2(PANEL_W, 0f);
        panelRT.anchoredPosition = new Vector2(PANEL_W, 0f);   // off-screen right
        panelGO.AddComponent<Image>().color = C_BG;

        // Left accent border
        var bdrGO = UIObj("Border", panelGO.transform);
        SetAnchors(bdrGO, 0f, 0f, 0f, 1f);
        bdrGO.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 0f);
        bdrGO.AddComponent<Image>().color = C_ACCENT;

        // ── Header ────────────────────────────────────────────────────────────
        var hdrGO = UIObj("Header", panelGO.transform);
        var hRT   = hdrGO.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 1f);
        hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(0f, -HDR_H);
        hRT.offsetMax = Vector2.zero;
        hdrGO.AddComponent<Image>().color = C_HDR;

        // Header text — separate child GO (Image + Text cannot share a GO)
        var hTxtGO = UIObj("HdrTxt", hdrGO.transform);
        SetAnchors(hTxtGO, 0f, 0f, 1f, 1f);
        MakeText(hTxtGO, "◆  DEBUG ITEMS", 12, FontStyle.Bold,
                 new Color(0.58f, 0.68f, 0.90f), TextAnchor.MiddleCenter);

        // ── Scroll area ───────────────────────────────────────────────────────
        var scrGO = UIObj("Scroll", panelGO.transform);
        var scrRT = scrGO.GetComponent<RectTransform>();
        scrRT.anchorMin = Vector2.zero;
        scrRT.anchorMax = Vector2.one;
        scrRT.offsetMin = Vector2.zero;
        scrRT.offsetMax = new Vector2(0f, -HDR_H);
        scrGO.AddComponent<Image>().color = Color.clear;   // needed for scroll input

        var scr = scrGO.AddComponent<ScrollRect>();
        scr.horizontal        = false;
        scr.vertical          = true;
        scr.scrollSensitivity = 30f;
        scr.movementType      = ScrollRect.MovementType.Clamped;

        // Viewport
        var vpGO = UIObj("Viewport", scrGO.transform);
        SetAnchors(vpGO, 0f, 0f, 1f, 1f);
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<RectMask2D>();
        scr.viewport = vpGO.GetComponent<RectTransform>();

        // Content
        var ctGO = UIObj("Content", vpGO.transform);
        var ctRT = ctGO.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0f, 1f);
        ctRT.anchorMax = Vector2.one;
        ctRT.pivot     = new Vector2(0.5f, 1f);
        ctRT.sizeDelta = new Vector2(0f, contentH);
        scr.content    = ctRT;

        // ── Item rows ─────────────────────────────────────────────────────────
        for (int i = 0; i < n; i++)
        {
            int idx = i;
            var def = ItemDefinition.All[i];

            var rowGO = UIObj("Row" + i, ctGO.transform);
            var rRT   = rowGO.GetComponent<RectTransform>();
            rRT.anchorMin       = new Vector2(0f, 1f);
            rRT.anchorMax       = new Vector2(1f, 1f);
            rRT.pivot           = new Vector2(0.5f, 1f);
            rRT.sizeDelta       = new Vector2(0f, ROW_H);
            rRT.anchoredPosition = new Vector2(0f, -i * ROW_H);
            rowGO.AddComponent<Image>().color = (i % 2 == 0) ? C_EVEN : C_ODD;

            // Rarity bar (left edge strip)
            var barGO = UIObj("Bar", rowGO.transform);
            var bRT   = barGO.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0f, 0f);
            bRT.anchorMax = new Vector2(0f, 1f);
            bRT.pivot     = new Vector2(0f, 0.5f);
            bRT.offsetMin = new Vector2(2f, 4f);
            bRT.offsetMax = new Vector2(6f, -4f);
            barGO.AddComponent<Image>().color = RarityCol(def.Rarity);

            // Name label
            var nameGO = UIObj("Name", rowGO.transform);
            var nRT    = nameGO.GetComponent<RectTransform>();
            nRT.anchorMin = new Vector2(0f, 0.48f);
            nRT.anchorMax = new Vector2(0.68f, 1f);
            nRT.offsetMin = new Vector2(10f, 0f);
            nRT.offsetMax = new Vector2(-4f, -3f);
            MakeText(nameGO, def.Name, 11, FontStyle.Bold,
                     RarityCol(def.Rarity), TextAnchor.LowerLeft);

            // Description label
            var descGO = UIObj("Desc", rowGO.transform);
            var dRT    = descGO.GetComponent<RectTransform>();
            dRT.anchorMin = new Vector2(0f, 0f);
            dRT.anchorMax = new Vector2(0.68f, 0.52f);
            dRT.offsetMin = new Vector2(10f, 2f);
            dRT.offsetMax = new Vector2(-4f, 0f);
            MakeText(descGO, def.Description, 8, FontStyle.Normal,
                     new Color(0.46f, 0.50f, 0.58f), TextAnchor.UpperLeft);

            // Toggle button background
            var btnBgGO = UIObj("BtnBg", rowGO.transform);
            var btnBgRT = btnBgGO.GetComponent<RectTransform>();
            btnBgRT.anchorMin = new Vector2(0.69f, 0.18f);
            btnBgRT.anchorMax = new Vector2(1.00f, 0.82f);
            btnBgRT.offsetMin = new Vector2(4f,  0f);
            btnBgRT.offsetMax = new Vector2(-8f, 0f);
            btnImg[i]         = btnBgGO.AddComponent<Image>();
            btnImg[i].color   = C_OFF;

            // Button label — child of button bg
            var btnTxtGO = UIObj("BtnTxt", btnBgGO.transform);
            SetAnchors(btnTxtGO, 0f, 0f, 1f, 1f);
            btnLbl[i] = MakeText(btnTxtGO, "OFF", 11, FontStyle.Bold,
                                 Color.white, TextAnchor.MiddleCenter);

            // Button component on bg GO (targetGraphic = Image already on it)
            var btn = btnBgGO.AddComponent<Button>();
            btn.targetGraphic = btnImg[i];
            btn.onClick.AddListener(() => ToggleItem(idx));
            NoNav(btn);
        }

        // ── Pill — child of panel so it travels with it ───────────────────────
        BuildPill(panelGO.transform);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Pill
    // ─────────────────────────────────────────────────────────────────────────

    void BuildPill(Transform panelTR)
    {
        var pillGO = UIObj("DebugPill", panelTR);
        var rt     = pillGO.GetComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0f, 0.5f);
        rt.anchorMax       = new Vector2(0f, 0.5f);
        rt.pivot           = new Vector2(1f, 0.5f);   // right edge = panel left edge
        rt.sizeDelta       = new Vector2(PILL_W, PILL_H);
        rt.anchoredPosition = Vector2.zero;

        var bg    = pillGO.AddComponent<Image>();
        bg.color  = C_PILL;

        // Accent stripe on the right side of the pill (where it meets the panel)
        var strGO = UIObj("Stripe", pillGO.transform);
        var sRT   = strGO.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(1f, 0f);
        sRT.anchorMax = new Vector2(1f, 1f);
        sRT.pivot     = new Vector2(1f, 0.5f);
        sRT.offsetMin = new Vector2(-3f, 6f);
        sRT.offsetMax = new Vector2(0f, -6f);
        strGO.AddComponent<Image>().color = C_ACCENT;

        // Arrow text — upper half (separate GO, no Image conflict)
        var arrGO = UIObj("Arrow", pillGO.transform);
        var aRT   = arrGO.GetComponent<RectTransform>();
        aRT.anchorMin = new Vector2(0f, 0.42f);
        aRT.anchorMax = Vector2.one;
        aRT.offsetMin = aRT.offsetMax = Vector2.zero;
        arrowText = MakeText(arrGO, "◄", 18, FontStyle.Bold,
                             new Color(0.82f, 0.90f, 1f), TextAnchor.MiddleCenter);

        // "ITEMS" label — lower half
        var lblGO = UIObj("Label", pillGO.transform);
        var lRT   = lblGO.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero;
        lRT.anchorMax = new Vector2(1f, 0.44f);
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;
        MakeText(lblGO, "ITEMS", 8, FontStyle.Bold,
                 new Color(0.48f, 0.58f, 0.80f), TextAnchor.MiddleCenter);

        // Button (targetGraphic = pill bg Image)
        var btn = pillGO.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cols = btn.colors;
        cols.highlightedColor = new Color(0.22f, 0.28f, 0.50f);
        cols.pressedColor     = new Color(0.28f, 0.36f, 0.64f);
        btn.colors = cols;
        btn.onClick.AddListener(TogglePanel);
        NoNav(btn);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Logic
    // ─────────────────────────────────────────────────────────────────────────

    void TogglePanel()
    {
        if (animating) return;
        isOpen         = !isOpen;
        arrowText.text = isOpen ? "►" : "◄";
        StartCoroutine(SlidePanel(isOpen ? 0f : PANEL_W));
    }

    IEnumerator SlidePanel(float targetX)
    {
        animating = true;
        float startX = panelRT.anchoredPosition.x;
        float dur    = 0.22f;
        for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
        {
            float e = 1f - Mathf.Pow(1f - t / dur, 3f);
            panelRT.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, e), 0f);
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

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Create a new GameObject with a RectTransform under the given parent.</summary>
    static GameObject UIObj(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    /// <summary>Set all four anchors + zero offsets so the child fills the parent.</summary>
    static void SetAnchors(GameObject go, float minX, float minY, float maxX, float maxY)
    {
        var rt     = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Add a Text component to <paramref name="go"/>.
    /// The GO must NOT already have an Image/Graphic on it.
    /// </summary>
    static Text MakeText(GameObject go, string content, int size, FontStyle style,
                         Color col, TextAnchor align)
    {
        var t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color     = col;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;

        // Try to grab a built-in font; leave default if none found
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (f != null) t.font = f;

        return t;
    }

    static void NoNav(Button b)
    {
        var n = b.navigation;
        n.mode       = Navigation.Mode.None;
        b.navigation = n;
    }

    static Color RarityCol(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Legendary: return new Color(1.0f, 0.75f, 0.10f);
            case ItemRarity.Rare:      return new Color(0.40f, 0.52f, 1.00f);
            case ItemRarity.Uncommon:  return new Color(0.20f, 0.88f, 0.30f);
            default:                   return new Color(0.62f, 0.62f, 0.65f);
        }
    }
}
