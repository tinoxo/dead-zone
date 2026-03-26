using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Grammarly-style debug sidebar.
///
/// The panel is anchored to the right edge and starts off-screen.
/// The pill tab is a CHILD of the panel, docked to the panel's left edge —
/// so it always protrudes from wherever the panel currently is.
/// When panel is hidden the pill sits at the right edge of the screen.
/// When panel slides in the pill comes with it.
/// </summary>
public class DebugItemMenu : MonoBehaviour
{
    const float PANEL_W = 280f;
    const float PILL_W  =  44f;
    const float PILL_H  =  88f;
    const float HDR_H   =  40f;
    const float ROW_H   =  48f;

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
        // Ensure this GO has a RectTransform that fills the whole canvas
        var self = GetComponent<RectTransform>();
        if (self == null) self = gameObject.AddComponent<RectTransform>();
        self.anchorMin = Vector2.zero;
        self.anchorMax = Vector2.one;
        self.offsetMin = self.offsetMax = Vector2.zero;

        int n  = ItemDefinition.All.Count;
        itemOn = new bool[n];
        btnImg = new Image[n];
        btnLbl = new Text[n];

        BuildPanel();   // panel first so pill (child of panel) renders on top
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Panel
    // ─────────────────────────────────────────────────────────────────────────

    void BuildPanel()
    {
        int   n       = ItemDefinition.All.Count;
        float contentH = n * ROW_H;

        // ── Panel root ────────────────────────────────────────────────────────
        var panelGO = new GameObject("DebugPanel");
        panelGO.transform.SetParent(transform, false);

        panelRT           = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 0f);
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot     = new Vector2(1f, 0.5f);
        panelRT.sizeDelta = new Vector2(PANEL_W, 0f);
        // Start fully off-screen to the right
        panelRT.anchoredPosition = new Vector2(PANEL_W, 0f);

        var panelBG = panelGO.AddComponent<Image>();
        panelBG.color = C_BG;

        // Left border accent line
        var bdr   = MakeRT("Border", panelGO.transform);
        bdr.anchorMin = new Vector2(0f, 0f);
        bdr.anchorMax = new Vector2(0f, 1f);
        bdr.pivot     = new Vector2(0f, 0.5f);
        bdr.sizeDelta = new Vector2(2f, 0f);
        bdr.gameObject.AddComponent<Image>().color = C_ACCENT;

        // ── Header ────────────────────────────────────────────────────────────
        var hdrRT     = MakeRT("Header", panelGO.transform);
        hdrRT.anchorMin = new Vector2(0f, 1f);
        hdrRT.anchorMax = Vector2.one;
        hdrRT.offsetMin = new Vector2(0f, -HDR_H);
        hdrRT.offsetMax = Vector2.zero;
        hdrRT.gameObject.AddComponent<Image>().color = C_HDR;

        var hTxt = AddText(hdrRT.gameObject, "◆  DEBUG ITEMS", 12, FontStyle.Bold,
                           new Color(0.58f, 0.68f, 0.90f), TextAnchor.MiddleCenter);
        hTxt.rectTransform.anchorMin = Vector2.zero;
        hTxt.rectTransform.anchorMax = Vector2.one;
        hTxt.rectTransform.offsetMin = hTxt.rectTransform.offsetMax = Vector2.zero;

        // ── Scroll area ───────────────────────────────────────────────────────
        var scrGO = new GameObject("Scroll");
        scrGO.transform.SetParent(panelGO.transform, false);
        var scrRT     = scrGO.AddComponent<RectTransform>();
        scrRT.anchorMin = new Vector2(0f, 0f);
        scrRT.anchorMax = new Vector2(1f, 1f);
        scrRT.offsetMin = Vector2.zero;
        scrRT.offsetMax = new Vector2(0f, -HDR_H);
        scrGO.AddComponent<Image>().color = Color.clear;  // required for scroll input

        var scr = scrGO.AddComponent<ScrollRect>();
        scr.horizontal        = false;
        scr.vertical          = true;
        scr.scrollSensitivity = 30f;
        scr.movementType      = ScrollRect.MovementType.Clamped;

        // Viewport
        var vpGO  = new GameObject("Viewport");
        vpGO.transform.SetParent(scrGO.transform, false);
        var vpRT  = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<RectMask2D>();
        scr.viewport = vpRT;

        // Content
        var ctGO  = new GameObject("Content");
        ctGO.transform.SetParent(vpGO.transform, false);
        var ctRT  = ctGO.AddComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0f, 1f);
        ctRT.anchorMax = Vector2.one;
        ctRT.pivot     = new Vector2(0.5f, 1f);
        ctRT.sizeDelta = new Vector2(0f, contentH);
        scr.content    = ctRT;

        // ── Item rows ─────────────────────────────────────────────────────────
        for (int i = 0; i < n; i++)
        {
            int  idx  = i;
            var  def  = ItemDefinition.All[i];

            var rowGO = new GameObject("Row" + i);
            rowGO.transform.SetParent(ctGO.transform, false);
            var rRT       = rowGO.AddComponent<RectTransform>();
            rRT.anchorMin = new Vector2(0f, 1f);
            rRT.anchorMax = new Vector2(1f, 1f);
            rRT.pivot     = new Vector2(0.5f, 1f);
            rRT.sizeDelta = new Vector2(0f, ROW_H);
            rRT.anchoredPosition = new Vector2(0f, -i * ROW_H);
            rowGO.AddComponent<Image>().color = (i % 2 == 0) ? C_EVEN : C_ODD;

            // Rarity colour bar (left edge)
            var barRT     = MakeRT("Bar", rowGO.transform);
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(0f, 1f);
            barRT.pivot     = new Vector2(0f, 0.5f);
            barRT.sizeDelta = new Vector2(4f, 0f);
            barRT.offsetMin = new Vector2(0f,  4f);
            barRT.offsetMax = new Vector2(4f, -4f);
            barRT.gameObject.AddComponent<Image>().color = RarityCol(def.Rarity);

            // Item name
            var nameTxt = AddText(rowGO, def.Name, 11, FontStyle.Bold,
                                  RarityCol(def.Rarity), TextAnchor.LowerLeft);
            var nRT = nameTxt.rectTransform;
            nRT.anchorMin = new Vector2(0f, 0.48f);
            nRT.anchorMax = new Vector2(0.68f, 1f);
            nRT.offsetMin = new Vector2(10f, 0f);
            nRT.offsetMax = new Vector2(-4f, -3f);

            // Description
            var descTxt = AddText(rowGO, def.Description, 8, FontStyle.Normal,
                                  new Color(0.46f, 0.50f, 0.58f), TextAnchor.UpperLeft);
            var dRT = descTxt.rectTransform;
            dRT.anchorMin = new Vector2(0f, 0f);
            dRT.anchorMax = new Vector2(0.68f, 0.52f);
            dRT.offsetMin = new Vector2(10f, 2f);
            dRT.offsetMax = new Vector2(-4f, 0f);

            // ON / OFF button
            var btnGO = new GameObject("Btn");
            btnGO.transform.SetParent(rowGO.transform, false);
            var btnRT       = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.69f, 0.18f);
            btnRT.anchorMax = new Vector2(1.00f, 0.82f);
            btnRT.offsetMin = new Vector2(4f,  0f);
            btnRT.offsetMax = new Vector2(-8f, 0f);
            btnImg[i]       = btnGO.AddComponent<Image>();
            btnImg[i].color = C_OFF;

            var bTxt = AddText(btnGO, "OFF", 11, FontStyle.Bold,
                               Color.white, TextAnchor.MiddleCenter);
            bTxt.rectTransform.anchorMin = Vector2.zero;
            bTxt.rectTransform.anchorMax = Vector2.one;
            bTxt.rectTransform.offsetMin = bTxt.rectTransform.offsetMax = Vector2.zero;
            btnLbl[i] = bTxt;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg[i];
            btn.onClick.AddListener(() => ToggleItem(idx));
            NoNav(btn);
        }

        // ── Pill tab — child of panel, docked at panel's left edge ───────────
        // This makes the pill move with the panel automatically.
        // When panel is off-screen the pill protrudes at the right edge.
        // When panel is open the pill protrudes from the panel's left edge.
        BuildPill(panelGO.transform);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Pill tab
    // ─────────────────────────────────────────────────────────────────────────

    void BuildPill(Transform parent)
    {
        var pillGO = new GameObject("DebugPill");
        pillGO.transform.SetParent(parent, false);

        // Anchor to the LEFT edge of the panel, pivot at right of pill
        var rt         = pillGO.AddComponent<RectTransform>();
        rt.anchorMin   = new Vector2(0f, 0.5f);
        rt.anchorMax   = new Vector2(0f, 0.5f);
        rt.pivot       = new Vector2(1f, 0.5f);   // right edge of pill = panel left edge
        rt.sizeDelta   = new Vector2(PILL_W, PILL_H);
        rt.anchoredPosition = Vector2.zero;

        // Background
        var bg    = pillGO.AddComponent<Image>();
        bg.color  = C_PILL;

        // Right accent stripe (where pill meets panel)
        var strGO = new GameObject("Stripe");
        strGO.transform.SetParent(pillGO.transform, false);
        var sRT       = strGO.AddComponent<RectTransform>();
        sRT.anchorMin = new Vector2(1f, 0f);
        sRT.anchorMax = new Vector2(1f, 1f);
        sRT.pivot     = new Vector2(1f, 0.5f);
        sRT.sizeDelta = new Vector2(3f, 0f);
        sRT.offsetMin = new Vector2(-3f, 6f);
        sRT.offsetMax = new Vector2(0f, -6f);
        strGO.AddComponent<Image>().color = C_ACCENT;

        // Arrow (upper half)
        var arrGO = new GameObject("Arrow");
        arrGO.transform.SetParent(pillGO.transform, false);
        var aRT       = arrGO.AddComponent<RectTransform>();
        aRT.anchorMin = new Vector2(0f, 0.42f);
        aRT.anchorMax = Vector2.one;
        aRT.offsetMin = aRT.offsetMax = Vector2.zero;
        arrowText       = AddText(arrGO, "◄", 18, FontStyle.Bold,
                                  new Color(0.80f, 0.88f, 1f), TextAnchor.MiddleCenter);
        arrowText.rectTransform.anchorMin = Vector2.zero;
        arrowText.rectTransform.anchorMax = Vector2.one;
        arrowText.rectTransform.offsetMin = arrowText.rectTransform.offsetMax = Vector2.zero;

        // Label (lower half)
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(pillGO.transform, false);
        var lRT       = lblGO.AddComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero;
        lRT.anchorMax = new Vector2(1f, 0.44f);
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;
        var lTxt = AddText(lblGO, "ITEMS", 8, FontStyle.Bold,
                           new Color(0.48f, 0.56f, 0.78f), TextAnchor.MiddleCenter);
        lTxt.rectTransform.anchorMin = Vector2.zero;
        lTxt.rectTransform.anchorMax = Vector2.one;
        lTxt.rectTransform.offsetMin = lTxt.rectTransform.offsetMax = Vector2.zero;

        // Button covers whole pill
        var btn = pillGO.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cols = btn.colors;
        cols.highlightedColor = new Color(0.22f, 0.28f, 0.50f);
        cols.pressedColor     = new Color(0.28f, 0.35f, 0.64f);
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
        isOpen = !isOpen;
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

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    RectTransform MakeRT(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    Text AddText(GameObject host, string content, int size, FontStyle style,
                 Color col, TextAnchor align)
    {
        var t = host.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        return t;
    }

    void NoNav(Button b)
    {
        var n = b.navigation;
        n.mode    = Navigation.Mode.None;
        b.navigation = n;
    }

    Color RarityCol(ItemRarity r)
    {
        switch (r)
        {
            case ItemRarity.Legendary: return new Color(1.0f, 0.75f, 0.10f);
            case ItemRarity.Rare:      return new Color(0.40f, 0.52f, 1.00f);
            case ItemRarity.Uncommon:  return new Color(0.20f, 0.88f, 0.30f);
            default:                   return new Color(0.62f, 0.62f, 0.65f);
        }
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
