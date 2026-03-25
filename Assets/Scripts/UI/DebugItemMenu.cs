using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Debug dropdown at the top of the screen.
/// Click the tab to expand/collapse a table of every item with ON/OFF toggles.
/// Lets you test item effects one at a time without running them all at once.
/// </summary>
public class DebugItemMenu : MonoBehaviour
{
    // ── Layout constants ──────────────────────────────────────────────────
    const float TAB_W     = 110f;
    const float TAB_H     = 28f;
    const float PANEL_W   = 480f;
    const float ROW_H     = 34f;
    const float TOGGLE_W  = 54f;
    const float PAD       = 8f;

    // ── State ─────────────────────────────────────────────────────────────
    bool panelOpen;

    GameObject     panel;
    Text           tabText;
    List<Button>   toggleButtons = new List<Button>();
    List<Text>     toggleLabels  = new List<Text>();

    // Track which items are currently active (parallel to ItemDefinition.All)
    bool[] active;

    // ── Colours ───────────────────────────────────────────────────────────
    static readonly Color COL_ON      = new Color(0.15f, 0.75f, 0.25f);
    static readonly Color COL_OFF     = new Color(0.28f, 0.28f, 0.32f);
    static readonly Color COL_BG      = new Color(0.06f, 0.06f, 0.10f, 0.94f);
    static readonly Color COL_TAB     = new Color(0.12f, 0.12f, 0.18f, 0.95f);
    static readonly Color COL_HDR     = new Color(0.55f, 0.55f, 0.65f);
    static readonly Color COL_BORDER  = new Color(0.35f, 0.35f, 0.50f, 0.6f);

    void Start()
    {
        active = new bool[ItemDefinition.All.Count];
        BuildUI();
    }

    // ── Build ─────────────────────────────────────────────────────────────

    void BuildUI()
    {
        // ── Tab button (always visible, top-center) ───────────────────────
        var tabGO = MakeRect("DebugTab", transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -TAB_H), new Vector2(TAB_W, TAB_H));
        AddImage(tabGO, COL_TAB);
        AddOutline(tabGO, COL_BORDER);

        tabText       = AddText(tabGO, "ITEMS  ▼", 13, FontStyle.Bold, Color.white);
        var tabBtn    = tabGO.AddComponent<Button>();
        tabBtn.targetGraphic = tabGO.GetComponent<Image>();
        tabBtn.onClick.AddListener(TogglePanel);
        SetNavNone(tabBtn);

        // ── Dropdown panel ────────────────────────────────────────────────
        int rows     = ItemDefinition.All.Count;
        float hdrH   = 28f;
        float panelH = hdrH + rows * ROW_H + PAD;

        panel = MakeRect("DebugPanel", transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -TAB_H - panelH), new Vector2(PANEL_W, panelH));
        AddImage(panel, COL_BG);
        AddOutline(panel, COL_BORDER);
        panel.SetActive(false);

        // Header row
        var hdrGO = MakeRect("Hdr", panel.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -hdrH), new Vector2(0f, hdrH));
        AddImage(hdrGO, new Color(0.10f, 0.10f, 0.16f));

        MakeColumnText(hdrGO, "ITEM",        COL_HDR, 0.01f, 0.40f, 12, FontStyle.Bold);
        MakeColumnText(hdrGO, "DESCRIPTION", COL_HDR, 0.40f, 0.78f, 12, FontStyle.Bold);
        MakeColumnText(hdrGO, "ACTIVE",      COL_HDR, 0.78f, 1.00f, 12, FontStyle.Bold);

        // Item rows
        for (int i = 0; i < rows; i++)
        {
            int idx = i;   // capture for closure
            var def = ItemDefinition.All[i];

            float yAnchorTop = 1f - (hdrH + i * ROW_H) / panelH;
            float yAnchorBot = yAnchorTop - ROW_H / panelH;

            var rowGO = MakeRect("Row" + i, panel.transform,
                new Vector2(0f, yAnchorBot), new Vector2(1f, yAnchorTop),
                Vector2.zero, Vector2.zero);

            // Zebra stripe
            Color stripe = (i % 2 == 0)
                ? new Color(0.09f, 0.09f, 0.14f, 0.5f)
                : new Color(0.06f, 0.06f, 0.10f, 0.3f);
            AddImage(rowGO, stripe);

            // Item name (with rarity color dot)
            MakeColumnText(rowGO, def.Name, RarityColor(def.Rarity), 0.01f, 0.40f, 12, FontStyle.Bold);

            // Description
            MakeColumnText(rowGO, def.Description, new Color(0.65f, 0.65f, 0.65f),
                0.40f, 0.78f, 10, FontStyle.Normal);

            // Toggle button
            var btnGO = MakeRect("Btn" + i, rowGO.transform,
                new Vector2(0.80f, 0.12f), new Vector2(1f, 0.88f),
                new Vector2(PAD, 0f), new Vector2(-PAD, 0f));
            AddImage(btnGO, COL_OFF);

            var lbl  = AddText(btnGO, "OFF", 12, FontStyle.Bold, Color.white);
            toggleLabels.Add(lbl);

            var btn  = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnGO.GetComponent<Image>();
            btn.onClick.AddListener(() => ToggleItem(idx));
            SetNavNone(btn);
            toggleButtons.Add(btn);
        }
    }

    // ── Toggle logic ──────────────────────────────────────────────────────

    void TogglePanel()
    {
        panelOpen = !panelOpen;
        panel.SetActive(panelOpen);
        tabText.text = panelOpen ? "ITEMS  ▲" : "ITEMS  ▼";
    }

    void ToggleItem(int idx)
    {
        active[idx] = !active[idx];
        var def = ItemDefinition.All[idx];
        var s   = PlayerStats.Instance;
        if (s == null) return;

        if (active[idx])
        {
            s.AddItem(def);
            if (def.EffectType == ItemEffectType.GhostBlade) s.Piercing = true;
        }
        else
        {
            s.ActiveItems.Remove(def);
            // If no more GhostBlade items, reset Piercing (unless character starts with it)
            if (def.EffectType == ItemEffectType.GhostBlade &&
                !s.HasItem(ItemEffectType.GhostBlade))
                s.Piercing = false;
        }

        // Update button colour + label
        var btnImg = toggleButtons[idx].GetComponent<Image>();
        if (btnImg) btnImg.color = active[idx] ? COL_ON : COL_OFF;
        toggleLabels[idx].text  = active[idx] ? "ON"  : "OFF";
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a RectTransform child with anchor-based positioning.
    /// anchorMin/Max set stretch; offsetMin/Max add pixel offsets.
    /// </summary>
    GameObject MakeRect(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt        = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        return go;
    }

    Image AddImage(GameObject go, Color col)
    {
        var img   = go.AddComponent<Image>();
        img.color = col;
        return img;
    }

    void AddOutline(GameObject go, Color col)
    {
        var out2 = go.AddComponent<Outline>();
        out2.effectColor    = col;
        out2.effectDistance = new Vector2(1f, -1f);
    }

    Text AddText(GameObject go, string content, int size, FontStyle style, Color col)
    {
        var t  = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt       = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return t;
    }

    void MakeColumnText(GameObject row, string content, Color col,
        float xMin, float xMax, int size, FontStyle style)
    {
        var go = new GameObject("CT");
        go.transform.SetParent(row.transform, false);
        var rt        = go.AddComponent<RectTransform>();
        rt.anchorMin  = new Vector2(xMin, 0f);
        rt.anchorMax  = new Vector2(xMax, 1f);
        rt.offsetMin  = new Vector2(6f,  2f);
        rt.offsetMax  = new Vector2(-4f, -2f);
        var t  = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleLeft;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
    }

    void SetNavNone(Button btn)
    {
        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
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

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
