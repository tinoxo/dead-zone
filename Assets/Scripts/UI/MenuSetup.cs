using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// DEAD ZONE — Main Menu
/// Flow: Main Menu → Character Grid → Character Detail → Island Select → Game
/// </summary>
public class MenuSetup : MonoBehaviour
{
    // ── Layout constants ──────────────────────────────────────────────────
    const int   COLS     = 5;
    const int   ROWS     = 3;
    const float SLOT_W   = 148f;
    const float SLOT_H   = 178f;
    const float SLOT_GAP = 14f;

    // ── Island data ───────────────────────────────────────────────────────
    static readonly Color[] ISLAND_COLS = {
        new Color(0.15f, 0.65f, 0.2f),   // Wild Zone
        new Color(0.85f, 0.65f, 0.1f),   // Gilded Sands
        new Color(0.85f, 0.2f,  0.75f),  // Star Park
        new Color(0.1f,  0.45f, 0.85f),  // The Deep
    };
    static readonly string[] ISLAND_NAMES = {
        "THE WILD ZONE", "THE GILDED SANDS", "STAR PARK", "THE DEEP"
    };
    static readonly string[] ISLAND_DESC = {
        "A world of wild creatures — familiar yet wrong.",
        "Ancient sands. Buried gold. A god made of it.",
        "The park never closes. The park never sleeps.",
        "The ocean floor holds things best left unknown.",
    };

    // ── Palette ───────────────────────────────────────────────────────────
    static readonly Color C_BG     = new Color(0.04f, 0.04f, 0.09f);
    static readonly Color C_ACCENT = new Color(0f,    1f,    1f);
    static readonly Color C_SILVER = new Color(0.62f, 0.62f, 0.72f);
    static readonly Color C_GOLD   = new Color(1f,    0.82f, 0.1f);
    static readonly Color C_LOCKED = new Color(0.16f, 0.16f, 0.22f);
    static readonly Color C_DARK   = new Color(0.07f, 0.07f, 0.13f);

    // ── References ───────────────────────────────────────────────────────
    Canvas     canvas;
    Font       font;

    GameObject mainMenuPanel;
    GameObject charGridPanel;
    GameObject charDetailPanel;
    GameObject islandSelectPanel;
    GameObject loadingPanel;

    // Character detail
    Image   detailSprite;
    Text    detailName, detailTitle, detailFlavor;
    Image[] detailStatBars = new Image[4];
    Text[]  detailWeaponBtns;

    // Island select
    Image[]      islandLandImgs  = new Image[4];
    Image[]      islandGlowImgs  = new Image[4];
    RectTransform[] islandNodeRTs = new RectTransform[4];
    GameObject[] islandLabelGOs  = new GameObject[4];
    Image        charBoatSprite;
    Image        fadeOverlay;

    // Loading
    Image loadFill;
    Text  loadText;

    // State
    int selectedChar    = 0;
    int selectedWeapVar = 0;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!font) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        SetupCamera();
        BuildCanvas();
        BuildEventSystem();

        BuildMainMenu();
        BuildCharGrid();
        BuildCharDetail();
        BuildIslandSelect();
        BuildLoadingScreen();

        ShowMainMenu();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SETUP
    // ══════════════════════════════════════════════════════════════════════
    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }
        cam.orthographic    = true;
        cam.backgroundColor = C_BG;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    void BuildCanvas()
    {
        var go = new GameObject("UICanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
    }

    void BuildEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  MAIN MENU
    // ══════════════════════════════════════════════════════════════════════
    void BuildMainMenu()
    {
        mainMenuPanel = NewFullPanel("MainMenu");
        DrawGrid(mainMenuPanel.transform);

        var t1 = MakeText(mainMenuPanel.transform, "???", 88, new Vector2(0, 130));
        t1.color = C_ACCENT; t1.alignment = TextAnchor.MiddleCenter;
        SetRect(t1, new Vector2(800, 110), new Vector2(0, 130));

        var t2 = MakeText(mainMenuPanel.transform, "A WAVE-BASED BULLET HELL", 18, new Vector2(0, 58));
        t2.color = new Color(0.42f, 0.42f, 0.58f); t2.alignment = TextAnchor.MiddleCenter;
        SetRect(t2, new Vector2(600, 28), new Vector2(0, 58));

        MakeButton(mainMenuPanel.transform, "PLAY",
            new Vector2(0, -70), new Vector2(360, 66), C_ACCENT, 28, ShowCharGrid);

        MakeButton(mainMenuPanel.transform, "QUIT",
            new Vector2(0, -158), new Vector2(180, 42), new Color(0.32f, 0.32f, 0.42f), 16,
            () => Application.Quit());

        var ver = MakeText(mainMenuPanel.transform, "v0.2  ALPHA", 12, Vector2.zero);
        ver.color = new Color(0.28f, 0.28f, 0.38f); ver.alignment = TextAnchor.LowerRight;
        ver.rectTransform.anchorMin = new Vector2(1, 0);
        ver.rectTransform.anchorMax = new Vector2(1, 0);
        ver.rectTransform.pivot     = new Vector2(1, 0);
        ver.rectTransform.anchoredPosition = new Vector2(-14, 10);
        ver.rectTransform.sizeDelta = new Vector2(160, 20);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CHARACTER GRID  (5 × 3 = 15 slots)
    // ══════════════════════════════════════════════════════════════════════
    void BuildCharGrid()
    {
        charGridPanel = NewFullPanel("CharGrid");
        charGridPanel.SetActive(false);
        DrawGrid(charGridPanel.transform);

        // Header
        var hdr = MakeText(charGridPanel.transform, "SELECT CHARACTER", 30, Vector2.zero);
        hdr.color = new Color(0.52f, 0.52f, 0.72f); hdr.alignment = TextAnchor.MiddleCenter;
        hdr.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hdr.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hdr.rectTransform.pivot     = new Vector2(0.5f, 1f);
        hdr.rectTransform.anchoredPosition = new Vector2(0, -28);
        hdr.rectTransform.sizeDelta = new Vector2(700, 44);

        // Shared tooltip (shown on hover)
        var tooltip = BuildGridTooltip(charGridPanel.transform);

        // Build grid
        float totalW = COLS * SLOT_W + (COLS - 1) * SLOT_GAP;
        float totalH = ROWS * SLOT_H + (ROWS - 1) * SLOT_GAP;
        float x0 = -totalW / 2f + SLOT_W / 2f;
        float y0 =  totalH / 2f - SLOT_H / 2f - 35f;

        for (int i = 0; i < COLS * ROWS; i++)
        {
            int col = i % COLS, row = i / COLS;
            float x = x0 + col * (SLOT_W + SLOT_GAP);
            float y = y0 - row * (SLOT_H + SLOT_GAP);
            BuildCharSlot(charGridPanel.transform, i, new Vector2(x, y), tooltip);
        }

        MakeButton(charGridPanel.transform, "◄ BACK",
            new Vector2(-820, -460), new Vector2(160, 42), new Color(0.28f, 0.28f, 0.38f), 16,
            ShowMainMenu);
    }

    GameObject BuildGridTooltip(Transform parent)
    {
        var go = new GameObject("Tooltip");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(290, 70);

        go.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.11f, 0.97f);

        // Accent border
        var brd = new GameObject("Border");
        brd.transform.SetParent(go.transform, false);
        var bImg = brd.AddComponent<Image>();
        bImg.color = C_ACCENT;
        var bRT = bImg.rectTransform;
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = new Vector2(-1,-1); bRT.offsetMax = new Vector2(1,1);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(go.transform, false);
        var fImg = fill.AddComponent<Image>();
        fImg.color = new Color(0.04f, 0.05f, 0.11f, 0.97f);
        var fRT = fImg.rectTransform;
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = new Vector2(1,1); fRT.offsetMax = new Vector2(-1,-1);

        var txt = MakeText(go.transform, "", 13, Vector2.zero);
        txt.color = new Color(0.72f, 0.72f, 0.82f);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow   = VerticalWrapMode.Overflow;
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = new Vector2(8, 4);
        txt.rectTransform.offsetMax = new Vector2(-8, -4);
        txt.name = "TooltipText";

        go.SetActive(false);
        return go;
    }

    void BuildCharSlot(Transform parent, int idx, Vector2 pos, GameObject tooltip)
    {
        bool hasChar  = idx < CharacterDefinition.All.Count;
        var  def      = hasChar ? CharacterDefinition.All[idx] : null;
        bool unlocked = hasChar && def.IsUnlocked;
        bool beaten   = hasChar && GameData.HasBeatenGameWith(idx);
        int  cleared  = hasChar ? GameData.IslandsClearedCount(idx) : 0;

        Color frameCol = !unlocked ? C_LOCKED : beaten ? C_GOLD : C_SILVER;

        // Slot root
        var slot = new GameObject($"Slot_{idx}");
        slot.transform.SetParent(parent, false);
        var slotRT = slot.AddComponent<RectTransform>();
        slotRT.anchorMin = slotRT.anchorMax = new Vector2(0.5f, 0.5f);
        slotRT.pivot     = new Vector2(0.5f, 0.5f);
        slotRT.sizeDelta = new Vector2(SLOT_W, SLOT_H);
        slotRT.anchoredPosition = pos;

        // Frame border (outer image)
        var frameImg = slot.AddComponent<Image>();
        frameImg.color = frameCol;

        // Portrait background
        var portBG = new GameObject("Port");
        portBG.transform.SetParent(slot.transform, false);
        var portImg = portBG.AddComponent<Image>();
        portImg.color = C_DARK;
        var pRT = portImg.rectTransform;
        pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
        pRT.offsetMin = new Vector2(4, 26); pRT.offsetMax = new Vector2(-4, -4);

        if (unlocked && hasChar)
        {
            // Color tint background
            var tint = new GameObject("Tint");
            tint.transform.SetParent(portBG.transform, false);
            var tImg = tint.AddComponent<Image>();
            tImg.color = def.PrimaryColor * 0.25f;
            StretchToParent(tImg.rectTransform);

            // Character placeholder (triangle sprite)
            var spr = new GameObject("Spr");
            spr.transform.SetParent(portBG.transform, false);
            var sImg = spr.AddComponent<Image>();
            sImg.sprite = SpriteFactory.Triangle(def.PrimaryColor);
            sImg.color  = def.PrimaryColor;
            sImg.preserveAspect = true;
            var sRT = sImg.rectTransform;
            sRT.anchorMin = new Vector2(0.12f, 0.12f);
            sRT.anchorMax = new Vector2(0.88f, 0.88f);
            sRT.offsetMin = sRT.offsetMax = Vector2.zero;
        }
        else
        {
            // Locked: question mark
            var qm = MakeText(portBG.transform, "?", 54, Vector2.zero);
            qm.color = new Color(0.22f, 0.22f, 0.30f);
            qm.alignment = TextAnchor.MiddleCenter;
            qm.rectTransform.anchorMin = Vector2.zero;
            qm.rectTransform.anchorMax = Vector2.one;
            qm.rectTransform.offsetMin = qm.rectTransform.offsetMax = Vector2.zero;
        }

        // Island-cleared dots (top-right corner, one per island cleared)
        if (unlocked && hasChar && cleared > 0)
        {
            for (int d = 0; d < Mathf.Min(cleared, 4); d++)
            {
                var dot = new GameObject($"Dot{d}");
                dot.transform.SetParent(slot.transform, false);
                var dImg = dot.AddComponent<Image>();
                dImg.color = ISLAND_COLS[d];
                var dRT = dImg.rectTransform;
                dRT.anchorMin = dRT.anchorMax = new Vector2(1f, 1f);
                dRT.pivot     = new Vector2(1f, 1f);
                dRT.sizeDelta = new Vector2(11, 11);
                dRT.anchoredPosition = new Vector2(-4 - d * 14, -4);
            }
        }

        // Name label at bottom
        string displayName = (!hasChar || !unlocked) ? "???" : def.Name;
        var nameTxt = MakeText(slot.transform, displayName, 12, Vector2.zero);
        nameTxt.alignment = TextAnchor.MiddleCenter;
        nameTxt.color = (unlocked && hasChar) ? def.PrimaryColor : new Color(0.28f, 0.28f, 0.38f);
        var nRT = nameTxt.rectTransform;
        nRT.anchorMin = new Vector2(0, 0); nRT.anchorMax = new Vector2(1, 0);
        nRT.pivot     = new Vector2(0.5f, 0f);
        nRT.sizeDelta = new Vector2(0, 22);
        nRT.anchoredPosition = new Vector2(0, 3);

        // Button — only clickable if unlocked
        var btn = slot.AddComponent<Button>();
        btn.targetGraphic = frameImg;
        var bc = btn.colors;
        bc.normalColor      = Color.white;
        bc.highlightedColor = new Color(1.22f, 1.22f, 1.22f);
        bc.pressedColor     = new Color(0.78f, 0.78f, 0.78f);
        bc.disabledColor    = new Color(0.6f, 0.6f, 0.6f);
        btn.colors = bc;
        btn.interactable = unlocked && hasChar;

        int captIdx = idx;
        btn.onClick.AddListener(() => { selectedChar = captIdx; ShowCharDetail(); });

        // Hover tooltip
        string tipText = !hasChar ? "Coming soon..." :
                         !unlocked ? (def.IsSecret ? "???" : $"Unlock:\n{def.UnlockCondition}") :
                         $"{def.Name}  —  {def.Title}";

        var et = slot.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((_) => {
            tooltip.SetActive(true);
            var tt = tooltip.transform.Find("TooltipText")?.GetComponent<Text>();
            if (tt) tt.text = tipText;
            tooltip.GetComponent<RectTransform>().anchoredPosition = pos + new Vector2(0, SLOT_H * 0.5f + 12f);
        });
        et.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) => tooltip.SetActive(false));
        et.triggers.Add(exit);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CHARACTER DETAIL
    // ══════════════════════════════════════════════════════════════════════
    void BuildCharDetail()
    {
        charDetailPanel = NewFullPanel("CharDetail");
        charDetailPanel.SetActive(false);
        DrawGrid(charDetailPanel.transform);

        float cardY = 20f;

        // ── Left card: portrait ───────────────────────────────────────────
        var leftCard = MakeCard(charDetailPanel.transform, new Vector2(320, 480), new Vector2(-490, cardY));

        var portArea = MakeCard(leftCard.transform, new Vector2(278, 278), new Vector2(0, 72));
        portArea.color = new Color(0.04f, 0.04f, 0.10f);

        var sprGO = new GameObject("Spr");
        sprGO.transform.SetParent(portArea.transform, false);
        detailSprite = sprGO.AddComponent<Image>();
        detailSprite.preserveAspect = true;
        var sRT = detailSprite.rectTransform;
        sRT.anchorMin = new Vector2(0.1f, 0.1f);
        sRT.anchorMax = new Vector2(0.9f, 0.9f);
        sRT.offsetMin = sRT.offsetMax = Vector2.zero;

        detailName = MakeText(leftCard.transform, "TINO", 36, new Vector2(0, -92));
        detailName.alignment = TextAnchor.MiddleCenter;
        SetRect(detailName, new Vector2(290, 46), new Vector2(0, -92));

        detailTitle = MakeText(leftCard.transform, "", 15, new Vector2(0, -122));
        detailTitle.color = new Color(0.52f, 0.52f, 0.68f);
        detailTitle.alignment = TextAnchor.MiddleCenter;
        SetRect(detailTitle, new Vector2(290, 24), new Vector2(0, -122));

        detailFlavor = MakeText(leftCard.transform, "", 13, new Vector2(0, -156));
        detailFlavor.color = new Color(0.48f, 0.48f, 0.62f);
        detailFlavor.alignment = TextAnchor.MiddleCenter;
        detailFlavor.horizontalOverflow = HorizontalWrapMode.Wrap;
        detailFlavor.verticalOverflow   = VerticalWrapMode.Overflow;
        SetRect(detailFlavor, new Vector2(280, 44), new Vector2(0, -156));

        // ── Center card: stats ────────────────────────────────────────────
        var midCard = MakeCard(charDetailPanel.transform, new Vector2(340, 480), new Vector2(0, cardY));

        var sHdr = MakeText(midCard.transform, "BASE STATS", 17, new Vector2(0, 205));
        sHdr.color = C_ACCENT; sHdr.alignment = TextAnchor.MiddleCenter;
        SetRect(sHdr, new Vector2(300, 26), new Vector2(0, 205));

        string[] sNames = { "HP", "SPD", "DMG", "FIRE" };
        Color[]  sCols  = {
            new Color(0.2f, 0.88f, 0.28f), new Color(0.2f, 0.68f, 1f),
            new Color(1f,   0.38f, 0.18f), new Color(1f,   0.84f, 0.1f),
        };

        for (int i = 0; i < 4; i++)
        {
            float y = 145f - i * 58f;
            var lbl = MakeText(midCard.transform, sNames[i], 14, new Vector2(-124, y));
            lbl.color = new Color(0.52f, 0.52f, 0.68f); lbl.alignment = TextAnchor.MiddleLeft;
            SetRect(lbl, new Vector2(58, 22), new Vector2(-124, y));

            var barBG = MakeCard(midCard.transform, new Vector2(210, 16), new Vector2(48, y));
            barBG.color = new Color(0.1f, 0.1f, 0.2f);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(barBG.transform, false);
            var fill = fillGO.AddComponent<Image>();
            fill.color  = sCols[i];
            fill.sprite = SpriteFactory.Square(sCols[i]);
            var fRT = fill.rectTransform;
            fRT.anchorMin = new Vector2(0,0); fRT.anchorMax = new Vector2(0,1);
            fRT.pivot = new Vector2(0, 0.5f);
            fRT.offsetMin = fRT.offsetMax = Vector2.zero;
            detailStatBars[i] = fill;
        }

        // ── Right card: loadout ───────────────────────────────────────────
        var rightCard = MakeCard(charDetailPanel.transform, new Vector2(340, 480), new Vector2(490, cardY));

        var lHdr = MakeText(rightCard.transform, "LOADOUT", 17, new Vector2(0, 205));
        lHdr.color = C_ACCENT; lHdr.alignment = TextAnchor.MiddleCenter;
        SetRect(lHdr, new Vector2(300, 26), new Vector2(0, 205));

        var wLbl = MakeText(rightCard.transform, "WEAPON", 12, new Vector2(0, 162));
        wLbl.color = new Color(0.42f, 0.42f, 0.58f); wLbl.alignment = TextAnchor.MiddleCenter;
        SetRect(wLbl, new Vector2(290, 20), new Vector2(0, 162));

        string[] varLabels = { "BASE", "VARIATION  A", "VARIATION  B" };
        detailWeaponBtns = new Text[3];
        for (int v = 0; v < 3; v++)
        {
            int vv = v;
            float vy = 118f - v * 50f;
            detailWeaponBtns[v] = MakeLoadoutRow(rightCard.transform, varLabels[v],
                new Vector2(0, vy), () => SelectWeaponVar(vv));
        }

        var aLbl = MakeText(rightCard.transform, "ARMOR", 12, new Vector2(0, -68));
        aLbl.color = new Color(0.42f, 0.42f, 0.58f); aLbl.alignment = TextAnchor.MiddleCenter;
        SetRect(aLbl, new Vector2(290, 20), new Vector2(0, -68));

        var armRow = MakeCard(rightCard.transform, new Vector2(278, 40), new Vector2(0, -108));
        armRow.color = new Color(0.1f, 0.1f, 0.2f);
        var armTxt = MakeText(armRow.transform, "ARMOR  Lv.1", 14, Vector2.zero);
        armTxt.color = new Color(0.68f, 0.68f, 0.78f); armTxt.alignment = TextAnchor.MiddleCenter;
        StretchToParent(armTxt.rectTransform);

        var note = MakeText(rightCard.transform,
            "Weapons & armor are upgraded\nbetween runs at the Forge", 11, new Vector2(0, -165));
        note.color = new Color(0.36f, 0.36f, 0.5f); note.alignment = TextAnchor.MiddleCenter;
        note.horizontalOverflow = HorizontalWrapMode.Wrap;
        note.verticalOverflow   = VerticalWrapMode.Overflow;
        SetRect(note, new Vector2(280, 44), new Vector2(0, -165));

        // ── Buttons ───────────────────────────────────────────────────────
        MakeButton(charDetailPanel.transform, "SET SAIL  ►",
            new Vector2(220, -310), new Vector2(310, 64), C_ACCENT, 26, ShowIslandSelect);

        MakeButton(charDetailPanel.transform, "◄ BACK",
            new Vector2(-500, -310), new Vector2(160, 42), new Color(0.28f, 0.28f, 0.38f), 16,
            ShowCharGrid);
    }

    Text MakeLoadoutRow(Transform parent, string label, Vector2 pos, System.Action onClick)
    {
        var go = new GameObject("LRow");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.2f);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(278, 40);
        rt.anchoredPosition = pos;

        var txt = MakeText(go.transform, label, 14, Vector2.zero);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.68f, 0.68f, 0.78f);
        StretchToParent(txt.rectTransform);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var bc = btn.colors;
        bc.normalColor = Color.white; bc.highlightedColor = new Color(1.25f,1.25f,1.25f);
        bc.pressedColor = new Color(0.78f,0.78f,0.78f);
        btn.colors = bc;
        btn.onClick.AddListener(() => onClick?.Invoke());
        return txt;
    }

    void RefreshCharDetail()
    {
        var def = CharacterDefinition.All[selectedChar];
        detailName.text  = def.Name;   detailName.color = def.PrimaryColor;
        detailTitle.text = def.Title;  detailFlavor.text = def.Flavor;
        detailSprite.sprite = SpriteFactory.Triangle(def.PrimaryColor);
        detailSprite.color  = def.PrimaryColor;

        float[] vals = { def.StatHP, def.StatSPD, def.StatDMG, def.StatFIRE };
        for (int i = 0; i < 4; i++)
            detailStatBars[i].rectTransform.sizeDelta = new Vector2(210f * vals[i], 0f);

        RefreshWeaponButtons();
    }

    void SelectWeaponVar(int v) { selectedWeapVar = v; RefreshWeaponButtons(); }

    void RefreshWeaponButtons()
    {
        string[] names = { "BASE  Lv.1", "VAR A  ——  locked", "VAR B  ——  locked" };
        for (int v = 0; v < 3; v++)
        {
            if (detailWeaponBtns[v] == null) continue;
            bool sel   = v == selectedWeapVar;
            bool avail = v == 0;
            detailWeaponBtns[v].text  = names[v];
            detailWeaponBtns[v].color = !avail ? new Color(0.28f,0.28f,0.38f) :
                                         sel    ? C_ACCENT : new Color(0.68f,0.68f,0.78f);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ISLAND SELECT  (boat scene — the "cutscene")
    // ══════════════════════════════════════════════════════════════════════
    void BuildIslandSelect()
    {
        islandSelectPanel = NewFullPanel("IslandSelect");
        islandSelectPanel.SetActive(false);

        BuildOceanScene(islandSelectPanel.transform);
        BuildBoat(islandSelectPanel.transform);

        // 4 islands in a panoramic spread
        Vector2[] positions = {
            new Vector2(-700, 60), new Vector2(-230, 105),
            new Vector2( 230, 105), new Vector2( 700, 60),
        };
        for (int i = 0; i < 4; i++)
            BuildIslandNode(islandSelectPanel.transform, i, positions[i]);

        // Hint text
        var hint = MakeText(islandSelectPanel.transform, "CHOOSE YOUR DESTINATION", 20, Vector2.zero);
        hint.color = new Color(0.48f, 0.58f, 0.78f, 0.75f);
        hint.alignment = TextAnchor.MiddleCenter;
        hint.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hint.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hint.rectTransform.pivot     = new Vector2(0.5f, 1f);
        hint.rectTransform.anchoredPosition = new Vector2(0, -26);
        hint.rectTransform.sizeDelta = new Vector2(600, 30);

        MakeButton(islandSelectPanel.transform, "◄ BACK",
            new Vector2(-820, -460), new Vector2(160, 42), new Color(0.28f,0.28f,0.38f), 16,
            ShowCharDetail);

        // Black fade overlay (fades out on enter)
        var fGO = new GameObject("Fade");
        fGO.transform.SetParent(islandSelectPanel.transform, false);
        fadeOverlay = fGO.AddComponent<Image>();
        fadeOverlay.color = Color.black;
        StretchToParent(fadeOverlay.rectTransform);
    }

    void BuildOceanScene(Transform parent)
    {
        // Sky
        var sky = new GameObject("Sky");
        sky.transform.SetParent(parent, false);
        var sImg = sky.AddComponent<Image>();
        sImg.color = new Color(0.04f, 0.07f, 0.22f);
        var sRT = sImg.rectTransform;
        sRT.anchorMin = new Vector2(0, 0.32f); sRT.anchorMax = Vector2.one;
        sRT.offsetMin = sRT.offsetMax = Vector2.zero;

        // Ocean
        var ocean = new GameObject("Ocean");
        ocean.transform.SetParent(parent, false);
        var oImg = ocean.AddComponent<Image>();
        oImg.color = new Color(0.02f, 0.05f, 0.18f);
        var oRT = oImg.rectTransform;
        oRT.anchorMin = Vector2.zero; oRT.anchorMax = new Vector2(1, 0.32f);
        oRT.offsetMin = oRT.offsetMax = Vector2.zero;

        // Horizon line
        var hz = new GameObject("Horizon");
        hz.transform.SetParent(parent, false);
        var hImg = hz.AddComponent<Image>();
        hImg.color = new Color(0.18f, 0.38f, 0.68f, 0.45f);
        var hRT = hImg.rectTransform;
        hRT.anchorMin = new Vector2(0, 0.318f); hRT.anchorMax = new Vector2(1, 0.325f);
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;

        // Stars
        var rng = new System.Random(42); // fixed seed for consistent layout
        for (int s = 0; s < 50; s++)
        {
            var star = new GameObject($"Star{s}");
            star.transform.SetParent(parent, false);
            var stImg = star.AddComponent<Image>();
            stImg.color = new Color(1f, 1f, 1f, (float)(rng.NextDouble() * 0.5 + 0.15));
            var stRT = stImg.rectTransform;
            stRT.anchorMin = stRT.anchorMax = new Vector2(
                (float)rng.NextDouble(), (float)(rng.NextDouble() * 0.6 + 0.36));
            stRT.sizeDelta = new Vector2((float)(rng.NextDouble() * 2.5 + 1.5f),
                                         (float)(rng.NextDouble() * 2.5 + 1.5f));
            stRT.anchoredPosition = Vector2.zero;
        }
    }

    void BuildBoat(Transform parent)
    {
        // Hull
        var hull = new GameObject("Hull");
        hull.transform.SetParent(parent, false);
        var hImg = hull.AddComponent<Image>();
        hImg.color = new Color(0.12f, 0.08f, 0.06f);
        var hRT = hImg.rectTransform;
        hRT.anchorMin = new Vector2(0.3f, 0f); hRT.anchorMax = new Vector2(0.7f, 0.19f);
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;

        // Deck
        var deck = new GameObject("Deck");
        deck.transform.SetParent(hull.transform, false);
        var dImg = deck.AddComponent<Image>();
        dImg.color = new Color(0.19f, 0.13f, 0.09f);
        var dRT = dImg.rectTransform;
        dRT.anchorMin = new Vector2(0, 0.58f); dRT.anchorMax = Vector2.one;
        dRT.offsetMin = dRT.offsetMax = Vector2.zero;

        // Mast
        var mast = new GameObject("Mast");
        mast.transform.SetParent(parent, false);
        var mImg = mast.AddComponent<Image>();
        mImg.color = new Color(0.24f, 0.16f, 0.11f);
        var mRT = mImg.rectTransform;
        mRT.anchorMin = new Vector2(0.5f, 0.14f); mRT.anchorMax = new Vector2(0.5f, 0.64f);
        mRT.sizeDelta = new Vector2(7f, 0f);
        mRT.anchoredPosition = Vector2.zero;

        // Sail
        var sail = new GameObject("Sail");
        sail.transform.SetParent(parent, false);
        var sailImg = sail.AddComponent<Image>();
        sailImg.color  = new Color(0.76f, 0.73f, 0.65f, 0.88f);
        sailImg.sprite = SpriteFactory.Triangle(new Color(0.76f, 0.73f, 0.65f));
        var sailRT = sailImg.rectTransform;
        sailRT.anchorMin = sailRT.anchorMax = new Vector2(0.5f, 0.39f);
        sailRT.pivot     = new Vector2(0f, 0.5f);
        sailRT.sizeDelta = new Vector2(115f, 155f);
        sailRT.anchoredPosition = new Vector2(3.5f, 0f);

        // Character sprite on boat bow
        var charGO = new GameObject("CharOnBoat");
        charGO.transform.SetParent(parent, false);
        charBoatSprite = charGO.AddComponent<Image>();
        charBoatSprite.sprite = SpriteFactory.Triangle(Color.white);
        charBoatSprite.preserveAspect = true;
        var cRT = charBoatSprite.rectTransform;
        cRT.anchorMin = cRT.anchorMax = new Vector2(0.5f, 0.21f);
        cRT.sizeDelta = new Vector2(26f, 26f);
        cRT.anchoredPosition = new Vector2(-52f, 0f);
    }

    void BuildIslandNode(Transform parent, int idx, Vector2 pos)
    {
        Color  col  = ISLAND_COLS[idx];
        string name = ISLAND_NAMES[idx];
        string desc = ISLAND_DESC[idx];

        // Node container
        var node = new GameObject($"Island_{idx}");
        node.transform.SetParent(parent, false);
        var nodeRT = node.AddComponent<RectTransform>();
        nodeRT.anchorMin = nodeRT.anchorMax = new Vector2(0.5f, 0.5f);
        nodeRT.pivot     = new Vector2(0.5f, 0.5f);
        nodeRT.sizeDelta = new Vector2(145f, 120f);
        nodeRT.anchoredPosition = pos;
        islandNodeRTs[idx] = nodeRT;

        // Glow ring (behind, hidden at rest)
        var glow = new GameObject("Glow");
        glow.transform.SetParent(node.transform, false);
        var gImg = glow.AddComponent<Image>();
        gImg.color = new Color(col.r, col.g, col.b, 0f);
        var gRT = gImg.rectTransform;
        gRT.anchorMin = new Vector2(-0.18f, -0.18f);
        gRT.anchorMax = new Vector2(1.18f,  1.18f);
        gRT.offsetMin = gRT.offsetMax = Vector2.zero;
        islandGlowImgs[idx] = gImg;

        // Land base
        var land = new GameObject("Land");
        land.transform.SetParent(node.transform, false);
        var lImg = land.AddComponent<Image>();
        lImg.color = col * 0.65f;
        var lRT = lImg.rectTransform;
        lRT.anchorMin = new Vector2(0.08f, 0f);
        lRT.anchorMax = new Vector2(0.92f, 0.52f);
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;
        islandLandImgs[idx] = lImg;

        // Peak / trees
        var peak = new GameObject("Peak");
        peak.transform.SetParent(node.transform, false);
        var pImg = peak.AddComponent<Image>();
        pImg.sprite = SpriteFactory.Triangle(col);
        pImg.color  = col * 0.82f;
        var pRT = pImg.rectTransform;
        pRT.anchorMin = new Vector2(0.18f, 0.28f);
        pRT.anchorMax = new Vector2(0.82f, 0.92f);
        pRT.offsetMin = pRT.offsetMax = Vector2.zero;

        // Hover label (hidden at rest, appears above island on hover)
        var labelGO = new GameObject($"Label_{idx}");
        labelGO.transform.SetParent(parent, false);
        var labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = labelRT.anchorMax = new Vector2(0.5f, 0.5f);
        labelRT.pivot     = new Vector2(0.5f, 0f);
        labelRT.sizeDelta = new Vector2(255, 76);
        labelRT.anchoredPosition = pos + new Vector2(0, 75f);
        labelGO.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.13f, 0.94f);

        var nTxt = MakeText(labelGO.transform, name, 15, new Vector2(0, 14));
        nTxt.color = col; nTxt.alignment = TextAnchor.MiddleCenter; nTxt.fontStyle = FontStyle.Bold;
        SetRect(nTxt, new Vector2(235, 24), new Vector2(0, 14));

        var dTxt = MakeText(labelGO.transform, desc, 11, new Vector2(0, -12));
        dTxt.color = new Color(0.62f, 0.62f, 0.72f); dTxt.alignment = TextAnchor.MiddleCenter;
        dTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        dTxt.verticalOverflow   = VerticalWrapMode.Overflow;
        SetRect(dTxt, new Vector2(235, 34), new Vector2(0, -12));

        labelGO.SetActive(false);
        islandLabelGOs[idx] = labelGO;

        // Invisible overlay for clicks + hover
        var ovr = new GameObject("Overlay");
        ovr.transform.SetParent(node.transform, false);
        var oImg = ovr.AddComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0);
        StretchToParent(oImg.rectTransform);

        var btn = node.AddComponent<Button>();
        btn.targetGraphic = oImg;
        var bc = btn.colors;
        bc.normalColor      = new Color(0,0,0,0);
        bc.highlightedColor = new Color(1,1,1,0.04f);
        bc.pressedColor     = new Color(1,1,1,0.1f);
        btn.colors = bc;

        int captIdx = idx;
        btn.onClick.AddListener(() => StartIsland(captIdx));

        var et = node.AddComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((_) => {
            islandLabelGOs[captIdx].SetActive(true);
            islandLandImgs[captIdx].color = ISLAND_COLS[captIdx];
            islandGlowImgs[captIdx].color = new Color(
                ISLAND_COLS[captIdx].r, ISLAND_COLS[captIdx].g, ISLAND_COLS[captIdx].b, 0.28f);
            islandNodeRTs[captIdx].sizeDelta = new Vector2(160f, 133f);
        });
        et.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) => {
            islandLabelGOs[captIdx].SetActive(false);
            islandLandImgs[captIdx].color = ISLAND_COLS[captIdx] * 0.65f;
            islandGlowImgs[captIdx].color = new Color(0, 0, 0, 0);
            islandNodeRTs[captIdx].sizeDelta = new Vector2(145f, 120f);
        });
        et.triggers.Add(exit);
    }

    void StartIsland(int islandIdx)
    {
        GameData.SelectedCharacterIndex = selectedChar;
        GameData.SelectedIslandIndex    = islandIdx;
        StartCoroutine(LoadGame());
    }

    // ══════════════════════════════════════════════════════════════════════
    //  LOADING SCREEN
    // ══════════════════════════════════════════════════════════════════════
    void BuildLoadingScreen()
    {
        loadingPanel = NewFullPanel("Loading");
        loadingPanel.SetActive(false);
        loadingPanel.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f);

        var t1 = MakeText(loadingPanel.transform, "DEAD ZONE", 72, new Vector2(0, 90));
        t1.color = C_ACCENT; t1.alignment = TextAnchor.MiddleCenter;
        SetRect(t1, new Vector2(700, 90), new Vector2(0, 90));

        loadText = MakeText(loadingPanel.transform, "ENTERING THE DEAD ZONE...", 20, new Vector2(0, -10));
        loadText.color = new Color(0.48f, 0.48f, 0.62f); loadText.alignment = TextAnchor.MiddleCenter;
        SetRect(loadText, new Vector2(600, 30), new Vector2(0, -10));

        var barBG = MakeCard(loadingPanel.transform, new Vector2(500, 13), new Vector2(0, -65));
        barBG.color = new Color(0.1f, 0.1f, 0.18f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barBG.transform, false);
        loadFill = fillGO.AddComponent<Image>();
        loadFill.color = C_ACCENT;
        var fr = loadFill.rectTransform;
        fr.anchorMin = new Vector2(0,0); fr.anchorMax = new Vector2(0,1);
        fr.pivot = new Vector2(0, 0.5f);
        fr.offsetMin = fr.offsetMax = Vector2.zero;
        fr.sizeDelta = new Vector2(0, 0);
    }

    IEnumerator LoadGame()
    {
        loadingPanel.SetActive(true);
        islandSelectPanel.SetActive(false);
        float t = 0f;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / 1.2f);
            if (loadFill) loadFill.rectTransform.sizeDelta = new Vector2(500f * f, 0);
            if (loadText) loadText.text = t < 0.4f ? "ENTERING THE DEAD ZONE..."
                                        : t < 0.8f ? "CALIBRATING SYSTEMS..."
                                        :             "DEPLOYING...";
            yield return null;
        }
        SceneManager.LoadSceneAsync(1);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  NAVIGATION
    // ══════════════════════════════════════════════════════════════════════
    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        charGridPanel.SetActive(false);
        charDetailPanel.SetActive(false);
        islandSelectPanel.SetActive(false);
        loadingPanel.SetActive(false);
    }

    void ShowCharGrid()
    {
        mainMenuPanel.SetActive(false);
        charGridPanel.SetActive(true);
        charDetailPanel.SetActive(false);
        islandSelectPanel.SetActive(false);
    }

    void ShowCharDetail()
    {
        charGridPanel.SetActive(false);
        charDetailPanel.SetActive(true);
        islandSelectPanel.SetActive(false);
        RefreshCharDetail();
    }

    void ShowIslandSelect()
    {
        charDetailPanel.SetActive(false);
        islandSelectPanel.SetActive(true);

        if (charBoatSprite != null)
        {
            var def = CharacterDefinition.All[selectedChar];
            charBoatSprite.sprite = SpriteFactory.Triangle(def.PrimaryColor);
            charBoatSprite.color  = def.PrimaryColor;
        }

        if (fadeOverlay != null)
            StartCoroutine(FadeIn(fadeOverlay, 2f));
    }

    IEnumerator FadeIn(Image overlay, float dur)
    {
        overlay.color = Color.black;
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            overlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, t / dur));
            yield return null;
        }
        overlay.color = new Color(0,0,0,0);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  UI HELPERS
    // ══════════════════════════════════════════════════════════════════════
    GameObject NewFullPanel(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    Image MakeCard(Transform parent, Vector2 size, Vector2 pos)
    {
        var go = new GameObject("Card");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.07f, 0.07f, 0.13f);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return img;
    }

    void MakeButton(Transform parent, string label, Vector2 pos, Vector2 size,
                    Color col, int fontSize, System.Action onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = col;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size; rt.anchoredPosition = pos;

        var inner = new GameObject("Inner");
        inner.transform.SetParent(go.transform, false);
        var iImg = inner.AddComponent<Image>();
        iImg.color = C_BG;
        var iRT = iImg.rectTransform;
        iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
        iRT.offsetMin = new Vector2(2,2); iRT.offsetMax = new Vector2(-2,-2);

        var txt = MakeText(inner.transform, label, fontSize, Vector2.zero);
        txt.color = col; txt.alignment = TextAnchor.MiddleCenter;
        StretchToParent(txt.rectTransform);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var bc = btn.colors;
        bc.normalColor = Color.white; bc.highlightedColor = new Color(1.25f,1.25f,1.25f);
        bc.pressedColor = new Color(0.76f,0.76f,0.76f);
        btn.colors = bc;
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    Text MakeText(Transform parent, string content, int size, Vector2 pos)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = content; t.fontSize = size; t.color = Color.white;
        t.font = font; t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(300, 40);
        return t;
    }

    void SetRect(Text t, Vector2 size, Vector2 pos)
    {
        t.rectTransform.sizeDelta = size;
        t.rectTransform.anchoredPosition = pos;
    }

    void StretchToParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void DrawGrid(Transform parent)
    {
        Color gc = new Color(0.1f, 0.1f, 0.18f, 0.28f);
        for (int i = -12; i <= 12; i++)
        {
            MkLine(parent, gc, true, i * 96f);
            MkLine(parent, gc, false, i * 96f);
        }
    }

    void MkLine(Transform parent, Color col, bool vert, float offset)
    {
        var go = new GameObject(vert ? "GV" : "GH");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = col;
        var r = img.rectTransform;
        if (vert) {
            r.anchorMin = new Vector2(0.5f,0); r.anchorMax = new Vector2(0.5f,1);
            r.sizeDelta = new Vector2(1,0); r.anchoredPosition = new Vector2(offset,0);
        } else {
            r.anchorMin = new Vector2(0,0.5f); r.anchorMax = new Vector2(1,0.5f);
            r.sizeDelta = new Vector2(0,1); r.anchoredPosition = new Vector2(0,offset);
        }
    }
}
