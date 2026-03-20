using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// ════════════════════════════════════════════════════════
///  DEAD ZONE — Main Menu Bootstrap
///  Attach this to ONE empty GameObject in the MainMenu scene.
///  Builds the full menu + character select programmatically.
/// ════════════════════════════════════════════════════════
/// </summary>
public class MenuSetup : MonoBehaviour
{
    // UI References
    Canvas canvas;
    GameObject mainMenuPanel;
    GameObject charSelectPanel;
    GameObject loadingPanel;

    // Character select state
    int charIndex = 0;
    Image   charSprite;
    Text    charName;
    Text    charTitle;
    Text    charFlavor;
    Text    charBonus;
    Image[] statBars = new Image[4];
    Text[]  statLabels = new Text[4];

    // Loading bar
    Image loadFill;
    Text  loadText;

    Font font;

    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!font) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        SetupCamera();
        BuildCanvas();
        BuildEventSystem();
        BuildMainMenu();
        BuildCharacterSelect();
        BuildLoadingScreen();

        ShowMainMenu();
    }

    // ── Camera ────────────────────────────────────────────────────────────
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
        cam.backgroundColor = new Color(0.04f, 0.04f, 0.09f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    // ── Canvas ────────────────────────────────────────────────────────────
    void BuildCanvas()
    {
        var go = new GameObject("UICanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    void BuildEventSystem()
    {
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  MAIN MENU PANEL
    // ══════════════════════════════════════════════════════════════════════
    void BuildMainMenu()
    {
        mainMenuPanel = new GameObject("MainMenuPanel");
        mainMenuPanel.transform.SetParent(canvas.transform, false);
        StretchFull(mainMenuPanel);

        // Subtle grid background
        DrawGrid(mainMenuPanel.transform);

        // ── Title ─────────────────────────────────────────────────────────
        var titleBg = MakePanel(mainMenuPanel.transform, new Color(0f, 0f, 0f, 0f),
            new Vector2(600, 140), new Vector2(0, 160));
        titleBg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        titleBg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        titleBg.rectTransform.pivot     = new Vector2(0.5f, 0.5f);

        var title = MakeText(titleBg.transform, "DEAD ZONE", 82, Vector2.zero);
        title.alignment = TextAnchor.MiddleCenter;
        title.color     = new Color(0f, 1f, 1f);
        SetRect(title, new Vector2(600, 100), new Vector2(0, 20));

        var sub = MakeText(titleBg.transform, "A WAVE-BASED BULLET HELL", 18, Vector2.zero);
        sub.alignment = TextAnchor.MiddleCenter;
        sub.color     = new Color(0.5f, 0.5f, 0.65f);
        SetRect(sub, new Vector2(500, 30), new Vector2(0, -48));

        // ── Play Button ───────────────────────────────────────────────────
        MakeButton(mainMenuPanel.transform, "ENTER THE ZONE",
            new Vector2(0, -120), new Vector2(340, 62),
            new Color(0f, 1f, 1f), new Color(0.04f, 0.04f, 0.09f),
            28, OnPlayClicked);

        // ── Version tag ───────────────────────────────────────────────────
        var ver = MakeText(mainMenuPanel.transform, "v0.1  ALPHA", 12, Vector2.zero);
        ver.color = new Color(0.35f, 0.35f, 0.45f);
        ver.rectTransform.anchorMin = new Vector2(1, 0);
        ver.rectTransform.anchorMax = new Vector2(1, 0);
        ver.rectTransform.pivot     = new Vector2(1, 0);
        ver.rectTransform.anchoredPosition = new Vector2(-14, 10);
        ver.rectTransform.sizeDelta = new Vector2(160, 20);
        ver.alignment = TextAnchor.LowerRight;
    }

    void OnPlayClicked()
    {
        ShowCharacterSelect();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CHARACTER SELECT PANEL
    // ══════════════════════════════════════════════════════════════════════
    void BuildCharacterSelect()
    {
        charSelectPanel = new GameObject("CharSelectPanel");
        charSelectPanel.transform.SetParent(canvas.transform, false);
        StretchFull(charSelectPanel);
        charSelectPanel.SetActive(false);

        DrawGrid(charSelectPanel.transform);

        // ── Header ────────────────────────────────────────────────────────
        var header = MakeText(charSelectPanel.transform, "SELECT YOUR CHARACTER", 26, Vector2.zero);
        header.alignment = TextAnchor.MiddleCenter;
        header.color     = new Color(0.6f, 0.6f, 0.8f);
        header.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        header.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        header.rectTransform.pivot     = new Vector2(0.5f, 1f);
        header.rectTransform.anchoredPosition = new Vector2(0, -30);
        header.rectTransform.sizeDelta = new Vector2(700, 40);

        // ── Character card (center) ────────────────────────────────────────
        // Card background
        var cardBg = MakePanel(charSelectPanel.transform,
            new Color(0.07f, 0.07f, 0.14f, 0.95f),
            new Vector2(480, 520), new Vector2(0, 10));
        cardBg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        cardBg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cardBg.rectTransform.pivot     = new Vector2(0.5f, 0.5f);

        // Thin border
        var border = MakePanel(cardBg.transform,
            new Color(0f, 1f, 1f, 0.25f), new Vector2(478, 518), Vector2.zero);
        border.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        border.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        border.rectTransform.pivot     = new Vector2(0.5f, 0.5f);

        // Character sprite display
        var spriteHolder = MakePanel(cardBg.transform,
            new Color(0.04f, 0.04f, 0.1f, 0.9f), new Vector2(200, 200), new Vector2(0, 120));
        spriteHolder.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        spriteHolder.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        spriteHolder.rectTransform.pivot     = new Vector2(0.5f, 0.5f);

        var sprGO = new GameObject("CharSprite");
        sprGO.transform.SetParent(spriteHolder.transform, false);
        charSprite = sprGO.AddComponent<Image>();
        charSprite.preserveAspect = true;
        var sprRect = charSprite.rectTransform;
        sprRect.anchorMin = Vector2.zero;
        sprRect.anchorMax = Vector2.one;
        sprRect.offsetMin = new Vector2(20, 20);
        sprRect.offsetMax = new Vector2(-20, -20);

        // Character name
        charName = MakeText(cardBg.transform, "AXIOM", 42, new Vector2(0, 10));
        charName.alignment = TextAnchor.MiddleCenter;
        charName.color     = Color.white;
        SetRect(charName, new Vector2(440, 52), new Vector2(0, 10));

        // Title
        charTitle = MakeText(cardBg.transform, "The Standard", 18, Vector2.zero);
        charTitle.alignment = TextAnchor.MiddleCenter;
        charTitle.color     = new Color(0.5f, 0.5f, 0.7f);
        SetRect(charTitle, new Vector2(440, 26), new Vector2(0, -26));

        // Flavor
        charFlavor = MakeText(cardBg.transform, "", 14, Vector2.zero);
        charFlavor.alignment = TextAnchor.MiddleCenter;
        charFlavor.color     = new Color(0.65f, 0.65f, 0.75f);
        SetRect(charFlavor, new Vector2(420, 36), new Vector2(0, -64));

        // ── Stat bars ─────────────────────────────────────────────────────
        string[] statNames = { "HP", "SPD", "DMG", "FIRE" };
        Color[]  statCols  =
        {
            new Color(0.2f, 0.9f, 0.3f),
            new Color(0.2f, 0.7f, 1.0f),
            new Color(1.0f, 0.4f, 0.2f),
            new Color(1.0f, 0.85f, 0.1f),
        };

        for (int i = 0; i < 4; i++)
        {
            float yOff = -108f - i * 28f;

            // Label
            var lbl = MakeText(cardBg.transform, statNames[i], 13, Vector2.zero);
            lbl.color = new Color(0.55f, 0.55f, 0.65f);
            lbl.alignment = TextAnchor.MiddleLeft;
            SetRect(lbl, new Vector2(50, 22), new Vector2(-178, yOff));
            statLabels[i] = lbl;

            // Bar background
            var bg = MakePanel(cardBg.transform, new Color(0.12f, 0.12f, 0.2f),
                new Vector2(300, 16), new Vector2(30, yOff));
            bg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            bg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            bg.rectTransform.pivot     = new Vector2(0.5f, 0.5f);

            // Bar fill
            var fillGO = new GameObject("StatFill");
            fillGO.transform.SetParent(bg.transform, false);
            var fill = fillGO.AddComponent<Image>();
            fill.color  = statCols[i];
            fill.sprite = SpriteFactory.Square(statCols[i]);
            var fr = fill.rectTransform;
            fr.anchorMin = new Vector2(0, 0);
            fr.anchorMax = new Vector2(0, 1);
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = new Vector2(0, 0);
            fr.pivot     = new Vector2(0, 0.5f);
            fr.anchoredPosition = Vector2.zero;
            statBars[i] = fill;
        }

        // Bonus text
        charBonus = MakeText(cardBg.transform, "", 13, Vector2.zero);
        charBonus.alignment = TextAnchor.MiddleCenter;
        charBonus.color     = new Color(1f, 0.85f, 0.1f);
        SetRect(charBonus, new Vector2(420, 22), new Vector2(0, -222));

        // ── Nav arrows ────────────────────────────────────────────────────
        MakeArrow(charSelectPanel.transform, "◄", new Vector2(-320, 10), OnPrevChar);
        MakeArrow(charSelectPanel.transform, "►", new Vector2( 320, 10), OnNextChar);

        // ── Counter (1 / 5) ───────────────────────────────────────────────
        var counter = MakeText(charSelectPanel.transform, "", 16, Vector2.zero);
        counter.alignment = TextAnchor.MiddleCenter;
        counter.color     = new Color(0.4f, 0.4f, 0.55f);
        counter.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        counter.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        counter.rectTransform.pivot     = new Vector2(0.5f, 0.5f);
        counter.rectTransform.anchoredPosition = new Vector2(0, -270);
        counter.rectTransform.sizeDelta = new Vector2(200, 24);
        // Update counter in RefreshCard (we store ref below)
        charCounter = counter;

        // ── BEGIN button ──────────────────────────────────────────────────
        MakeButton(charSelectPanel.transform, "BEGIN",
            new Vector2(0, -310), new Vector2(260, 56),
            new Color(0f, 1f, 1f), new Color(0.04f, 0.04f, 0.09f),
            26, OnBeginClicked);

        // ── Back button ───────────────────────────────────────────────────
        MakeButton(charSelectPanel.transform, "BACK",
            new Vector2(-380, -310), new Vector2(140, 42),
            new Color(0.3f, 0.3f, 0.4f), new Color(0.7f, 0.7f, 0.8f),
            16, ShowMainMenu);

        // Init display
        RefreshCard();
    }

    Text charCounter;

    void OnPrevChar()
    {
        charIndex = (charIndex - 1 + CharacterDefinition.All.Count) % CharacterDefinition.All.Count;
        RefreshCard();
    }

    void OnNextChar()
    {
        charIndex = (charIndex + 1) % CharacterDefinition.All.Count;
        RefreshCard();
    }

    void RefreshCard()
    {
        var c = CharacterDefinition.All[charIndex];

        // Sprite
        charSprite.sprite = SpriteFactory.Triangle(c.PrimaryColor);
        charSprite.color  = c.PrimaryColor;

        // Text
        charName.text   = c.Name;
        charName.color  = c.PrimaryColor;
        charTitle.text  = c.Title;
        charFlavor.text = c.Flavor;
        charBonus.text  = "BONUS: " + c.StartingBonus;

        // Stat bars
        float[] vals = { c.StatHP, c.StatSPD, c.StatDMG, c.StatFIRE };
        for (int i = 0; i < 4; i++)
        {
            if (statBars[i] != null)
            {
                // Animate bar width from 0 → val
                statBars[i].rectTransform.sizeDelta = new Vector2(300f * vals[i], 16f);
            }
        }

        // Border color matches character
        if (charCounter != null)
            charCounter.text = $"{charIndex + 1}  /  {CharacterDefinition.All.Count}";
    }

    void OnBeginClicked()
    {
        GameData.SelectedCharacterIndex = charIndex;
        StartCoroutine(LoadGameScene());
    }

    // ══════════════════════════════════════════════════════════════════════
    //  LOADING SCREEN
    // ══════════════════════════════════════════════════════════════════════
    void BuildLoadingScreen()
    {
        loadingPanel = new GameObject("LoadingPanel");
        loadingPanel.transform.SetParent(canvas.transform, false);
        StretchFull(loadingPanel);
        loadingPanel.SetActive(false);

        // Full black overlay
        var bg = loadingPanel.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.05f, 1f);

        // Game title
        var title = MakeText(loadingPanel.transform, "DEAD ZONE", 72, new Vector2(0, 80));
        title.alignment = TextAnchor.MiddleCenter;
        title.color     = new Color(0f, 1f, 1f);
        SetRect(title, new Vector2(700, 90), new Vector2(0, 80));

        // Loading label
        loadText = MakeText(loadingPanel.transform, "ENTERING THE DEAD ZONE...", 20, Vector2.zero);
        loadText.alignment = TextAnchor.MiddleCenter;
        loadText.color     = new Color(0.5f, 0.5f, 0.65f);
        SetRect(loadText, new Vector2(600, 30), new Vector2(0, -30));

        // Loading bar background
        var barBg = MakePanel(loadingPanel.transform,
            new Color(0.1f, 0.1f, 0.18f), new Vector2(500, 14), new Vector2(0, -80));
        barBg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        barBg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        barBg.rectTransform.pivot     = new Vector2(0.5f, 0.5f);

        // Loading bar fill
        var fillGO = new GameObject("LoadFill");
        fillGO.transform.SetParent(barBg.transform, false);
        loadFill = fillGO.AddComponent<Image>();
        loadFill.sprite = SpriteFactory.Square(new Color(0f, 1f, 1f));
        var fr = loadFill.rectTransform;
        fr.anchorMin        = new Vector2(0, 0);
        fr.anchorMax        = new Vector2(0, 1);
        fr.offsetMin        = Vector2.zero;
        fr.offsetMax        = Vector2.zero;
        fr.pivot            = new Vector2(0, 0.5f);
        fr.sizeDelta        = new Vector2(0, 14);
        fr.anchoredPosition = Vector2.zero;

        // Character name shown during load
        var charLine = MakeText(loadingPanel.transform, "", 16, Vector2.zero);
        charLine.alignment = TextAnchor.MiddleCenter;
        charLine.color     = new Color(0.6f, 0.6f, 0.8f);
        SetRect(charLine, new Vector2(500, 24), new Vector2(0, -115));
        loadingCharText = charLine;
    }

    Text loadingCharText;

    IEnumerator LoadGameScene()
    {
        // Show loading screen
        loadingPanel.SetActive(true);
        charSelectPanel.SetActive(false);

        var c = CharacterDefinition.All[charIndex];
        if (loadingCharText) loadingCharText.text = $"Playing as  {c.Name}  —  {c.Title}";

        // Fake load bar fill over 1.2 seconds before actually loading
        float t = 0f;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / 1.2f);
            if (loadFill)
                loadFill.rectTransform.sizeDelta = new Vector2(500f * frac, 14f);
            if (loadText)
                loadText.text = t < 0.4f ? "ENTERING THE DEAD ZONE..."
                              : t < 0.8f ? "CALIBRATING SYSTEMS..."
                              :             "DEPLOYING...";
            yield return null;
        }

        // Actually load
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        while (!op.isDone)
        {
            if (loadFill)
                loadFill.rectTransform.sizeDelta = new Vector2(500f * (0.9f + op.progress * 0.1f), 14f);
            yield return null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PANEL NAVIGATION
    // ══════════════════════════════════════════════════════════════════════
    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        charSelectPanel.SetActive(false);
        loadingPanel.SetActive(false);
    }

    void ShowCharacterSelect()
    {
        mainMenuPanel.SetActive(false);
        charSelectPanel.SetActive(true);
        loadingPanel.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  UI BUILDER HELPERS
    // ══════════════════════════════════════════════════════════════════════
    void DrawGrid(Transform parent)
    {
        Color gridCol = new Color(0.1f, 0.1f, 0.18f, 0.35f);
        for (int i = -10; i <= 10; i++)
        {
            CreateGridLine(parent, gridCol, true,  i * 96f);
            CreateGridLine(parent, gridCol, false, i * 96f);
        }
    }

    void CreateGridLine(Transform parent, Color col, bool vertical, float offset)
    {
        var go = new GameObject(vertical ? "GV" : "GH");
        go.transform.SetParent(parent, false);
        var img   = go.AddComponent<Image>();
        img.color = col;
        img.sprite = SpriteFactory.Square(col);
        var r = img.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        if (vertical)
        {
            r.anchorMin = new Vector2(0.5f, 0f);
            r.anchorMax = new Vector2(0.5f, 1f);
            r.sizeDelta  = new Vector2(1f, 0f);
            r.anchoredPosition = new Vector2(offset, 0f);
        }
        else
        {
            r.anchorMin = new Vector2(0f, 0.5f);
            r.anchorMax = new Vector2(1f, 0.5f);
            r.sizeDelta  = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(0f, offset);
        }
    }

    void MakeArrow(Transform parent, string label, Vector2 pos, System.Action onClick)
    {
        var go  = new GameObject("Arrow");
        go.transform.SetParent(parent, false);

        var img  = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f); // transparent bg

        var rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(80, 80);

        var t = MakeText(go.transform, label, 52, Vector2.zero);
        t.color     = new Color(0f, 1f, 1f, 0.8f);
        t.alignment = TextAnchor.MiddleCenter;
        SetRect(t, new Vector2(80, 80), Vector2.zero);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cols = btn.colors;
        cols.normalColor      = new Color(1, 1, 1, 1);
        cols.highlightedColor = new Color(1, 1, 1, 1);
        cols.pressedColor     = new Color(0.7f, 0.7f, 0.7f, 1);
        btn.colors = cols;

        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    void MakeButton(Transform parent, string label, Vector2 pos, Vector2 size,
                    Color borderCol, Color textCol, int fontSize, System.Action onClick)
    {
        var go  = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.Square(borderCol);
        img.color  = borderCol;

        var rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        // Inner dark fill
        var inner = new GameObject("Inner");
        inner.transform.SetParent(go.transform, false);
        var innerImg = inner.AddComponent<Image>();
        innerImg.color = new Color(0.04f, 0.04f, 0.09f);
        innerImg.sprite = SpriteFactory.Square(Color.white);
        var ir = innerImg.rectTransform;
        ir.anchorMin = Vector2.zero;
        ir.anchorMax = Vector2.one;
        ir.offsetMin = new Vector2(2, 2);
        ir.offsetMax = new Vector2(-2, -2);

        var t = MakeText(inner.transform, label, fontSize, Vector2.zero);
        t.color     = borderCol;
        t.alignment = TextAnchor.MiddleCenter;
        SetRect(t, size + new Vector2(-4, -4), Vector2.zero);
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cols = btn.colors;
        cols.normalColor      = Color.white;
        cols.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
        cols.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cols;

        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    Image MakePanel(Transform parent, Color col, Vector2 size, Vector2 pos)
    {
        var go  = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color  = col;
        img.sprite = SpriteFactory.Square(Color.white);
        var rt = img.rectTransform;
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        return img;
    }

    Text MakeText(Transform parent, string content, int size, Vector2 pos)
    {
        var go = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var t  = go.AddComponent<Text>();
        t.text            = content;
        t.fontSize        = size;
        t.color           = Color.white;
        t.font            = font;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(300, 40);
        return t;
    }

    void SetRect(Text t, Vector2 size, Vector2 pos)
    {
        t.rectTransform.sizeDelta        = size;
        t.rectTransform.anchoredPosition = pos;
    }

    void StretchFull(GameObject go)
    {
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
