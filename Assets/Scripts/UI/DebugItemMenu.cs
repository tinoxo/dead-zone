using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Terraria-style slide-in debug panel on the right edge of the screen.
/// A small tab always sticks out — click it to slide the panel in/out.
/// Toggle items on/off one at a time so you can test effects without lag.
/// </summary>
public class DebugItemMenu : MonoBehaviour
{
    // ── Layout ────────────────────────────────────────────────────────────
    const float PANEL_W  = 310f;
    const float TAB_W    =  26f;
    const float HEADER_H =  30f;
    const float ROW_H    =  36f;

    // ── State ─────────────────────────────────────────────────────────────
    RectTransform containerRT;
    Text          arrowText;
    bool          isOpen;
    bool          animating;

    bool[]  itemOn;
    Image[] btnImages;
    Text[]  btnLabels;

    // ── Colours ───────────────────────────────────────────────────────────
    static readonly Color C_BG      = new Color(0.05f, 0.05f, 0.09f, 0.97f);
    static readonly Color C_TAB     = new Color(0.10f, 0.10f, 0.17f, 1.00f);
    static readonly Color C_HDR     = new Color(0.10f, 0.12f, 0.20f, 1.00f);
    static readonly Color C_ROW0    = new Color(0.08f, 0.08f, 0.12f, 1.00f);
    static readonly Color C_ROW1    = new Color(0.06f, 0.06f, 0.10f, 1.00f);
    static readonly Color C_ACCENT  = new Color(0.38f, 0.52f, 1.00f, 1.00f);
    static readonly Color C_ON      = new Color(0.14f, 0.72f, 0.26f, 1.00f);
    static readonly Color C_OFF     = new Color(0.22f, 0.22f, 0.28f, 1.00f);

    // ── Build ─────────────────────────────────────────────────────────────

    void Start()
    {
        int n  = ItemDefinition.All.Count;
        itemOn     = new bool[n];
        btnImages  = new Image[n];
        btnLabels  = new Text[n];
        Build();
    }

    void Build()
    {
        int   n       = ItemDefinition.All.Count;
        float totalH  = HEADER_H + n * ROW_H;

        // ── Container ────────────────────────────────────────────────────
        // Pivot at right edge so sliding anchoredPosition.x = PANEL_W
        // pushes the panel off-screen while leaving the TAB_W tab visible.
        var cGO = new GameObject("Container");
        containerRT = cGO.AddComponent<RectTransform>();
        containerRT.SetParent(transform, false);
        containerRT.anchorMin        = new Vector2(1f, 0.5f);
        containerRT.anchorMax        = new Vector2(1f, 0.5f);
        containerRT.pivot            = new Vector2(1f, 0.5f);
        containerRT.sizeDelta        = new Vector2(PANEL_W + TAB_W, totalH);
        containerRT.anchoredPosition = new Vector2(PANEL_W, 0f);   // start closed

        // ── Tab (leftmost strip, always visible when closed) ──────────────
        var tabGO = Child("Tab", containerRT);
        Anchor(tabGO, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(TAB_W, 0f));
        tabGO.AddComponent<Image>().color = C_TAB;

        // Blue accent line on the left edge of tab
        var acGO = Child("Accent", tabGO.transform);
        Anchor(acGO, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 4f), new Vector2(3f, -4f));
        acGO.AddComponent<Image>().color = C_ACCENT;

        // Arrow icon centred in tab
        arrowText = Txt(tabGO.transform, "◄", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);

        // Clickable
        var tabBtn = tabGO.AddComponent<Button>();
        tabBtn.targetGraphic = tabGO.GetComponent<Image>();
        var tc = tabBtn.colors;
        tc.highlightedColor = new Color(0.18f, 0.20f, 0.32f);
        tc.pressedColor     = new Color(0.25f, 0.28f, 0.45f);
        tabBtn.colors = tc;
        tabBtn.onClick.AddListener(TogglePanel);
        NoNav(tabBtn);

        // ── Panel (fills rest of container, right of tab) ─────────────────
        var pGO = Child("Panel", containerRT);
        Anchor(pGO, Vector2.zero, Vector2.one, new Vector2(TAB_W, 0f), Vector2.zero);
        pGO.AddComponent<Image>().color = C_BG;

        // Left border
        var bdr = Child("Border", pGO.transform);
        Anchor(bdr, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f));
        bdr.AddComponent<Image>().color = C_ACCENT;

        // Header
        var hdr = Child("Header", pGO.transform);
        Anchor(hdr, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -HEADER_H), Vector2.zero);
        hdr.AddComponent<Image>().color = C_HDR;
        Txt(hdr.transform, "◆  DEBUG ITEMS", 11, FontStyle.Bold,
            new Color(0.60f, 0.65f, 0.80f), TextAnchor.MiddleCenter);

        // Item rows
        for (int i = 0; i < n; i++)
        {
            int  idx = i;
            var  def = ItemDefinition.All[i];
            float y0 = -(HEADER_H + (i + 1) * ROW_H);
            float y1 = -(HEADER_H +  i      * ROW_H);

            var row = Child("Row" + i, pGO.transform);
            Anchor(row, new Vector2(0f, 1f), new Vector2(1f, 1f),
                        new Vector2(1f, y0), new Vector2(0f, y1));
            row.AddComponent<Image>().color = (i % 2 == 0) ? C_ROW0 : C_ROW1;

            // Rarity bar
            var bar = Child("Bar", row.transform);
            Anchor(bar, Vector2.zero, new Vector2(0f, 1f),
                        new Vector2(0f, 3f), new Vector2(4f, -3f));
            bar.AddComponent<Image>().color = RarityCol(def.Rarity);

            // Name
            var nm = Child("Name", row.transform);
            Anchor(nm, new Vector2(0f, 0.45f), new Vector2(0.72f, 1f),
                       new Vector2(9f, 0f), new Vector2(-2f, -2f));
            Txt(nm.transform, def.Name, 11, FontStyle.Bold,
                RarityCol(def.Rarity), TextAnchor.MiddleLeft);

            // Description
            var ds = Child("Desc", row.transform);
            Anchor(ds, new Vector2(0f, 0f), new Vector2(0.72f, 0.48f),
                       new Vector2(9f, 2f), new Vector2(-2f, 0f));
            Txt(ds.transform, def.Description, 8, FontStyle.Normal,
                new Color(0.50f, 0.50f, 0.56f), TextAnchor.MiddleLeft);

            // Toggle button
            var btn = Child("Btn", row.transform);
            Anchor(btn, new Vector2(0.73f, 0.14f), new Vector2(1f, 0.86f),
                        new Vector2(4f, 0f), new Vector2(-6f, 0f));
            btnImages[i]       = btn.AddComponent<Image>();
            btnImages[i].color = C_OFF;
            btnLabels[i]       = Txt(btn.transform, "OFF", 11, FontStyle.Bold,
                                     Color.white, TextAnchor.MiddleCenter);

            var b = btn.AddComponent<Button>();
            b.targetGraphic = btnImages[i];
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
        StartCoroutine(Slide(isOpen ? 0f : PANEL_W));
    }

    IEnumerator Slide(float targetX)
    {
        animating = true;
        float startX = containerRT.anchoredPosition.x;
        float dur    = 0.18f;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            float e = 1f - Mathf.Pow(1f - t / dur, 3f);
            containerRT.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, e), 0f);
            yield return null;
        }
        containerRT.anchoredPosition = new Vector2(targetX, 0f);
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

        btnImages[idx].color = itemOn[idx] ? C_ON  : C_OFF;
        btnLabels[idx].text  = itemOn[idx] ? "ON"  : "OFF";
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    GameObject Child(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    void Anchor(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        var rt      = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;  rt.anchorMax = aMax;
        rt.offsetMin = oMin;  rt.offsetMax = oMax;
    }

    Text Txt(Transform parent, string s, int size, FontStyle style, Color col, TextAnchor align)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = s;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt      = t.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4f, 0f); rt.offsetMax = Vector2.zero;
        return t;
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
            case ItemRarity.Rare:      return new Color(0.40f, 0.50f, 1.00f);
            case ItemRarity.Uncommon:  return new Color(0.20f, 0.90f, 0.30f);
            default:                   return new Color(0.65f, 0.65f, 0.65f);
        }
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
