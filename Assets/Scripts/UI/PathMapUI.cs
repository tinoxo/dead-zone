using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Full-screen path-selection overlay shown between bosses.
/// Built entirely in code — no prefabs required.
/// </summary>
public class PathMapUI : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static PathMapUI Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureEventSystem();
        BuildUI();
        Hide();
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("PathMapEventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }
    }

    // ── UI references ──────────────────────────────────────────────────────
    Canvas      canvas;
    GameObject  root;           // the semi-transparent overlay
    Text        titleText;
    Text        depthText;
    Text        materialEarnedText;

    // Left panel
    GameObject  leftPanel;
    Image       leftBorder;
    Text        leftNameText;
    Text        leftRewardText;
    Text        leftRoomText;
    Text        leftDescText;
    Text        leftSideLabel;
    Button      leftButton;

    // Right panel
    GameObject  rightPanel;
    Image       rightBorder;
    Text        rightNameText;
    Text        rightRewardText;
    Text        rightRoomText;
    Text        rightDescText;
    Text        rightSideLabel;
    Button      rightButton;

    // ── Build UI in code ───────────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var canvasGo = new GameObject("PathMapCanvas");
        canvasGo.transform.SetParent(transform);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Full-screen semi-transparent overlay
        root = MakePanel(canvasGo, "PathMapRoot",
            new Color(0.05f, 0.05f, 0.08f, 0.92f),
            Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero);

        // Title
        titleText = MakeText(root, "TitleText",
            "CHOOSE YOUR PATH",
            36, FontStyle.Bold, TextAnchor.MiddleCenter,
            Color.white,
            new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.95f));

        // Depth indicator
        depthText = MakeText(root, "DepthText",
            "DEPTH 1 / 4",
            20, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.75f, 0.75f, 0.75f),
            new Vector2(0.3f, 0.76f), new Vector2(0.7f, 0.83f));

        // LEFT choice panel
        leftPanel  = MakePanel(root, "LeftPanel",
            new Color(0.10f, 0.10f, 0.14f, 1f),
            new Vector2(0.04f, 0.18f), new Vector2(0.47f, 0.74f),
            Vector2.zero, Vector2.zero);

        leftBorder  = leftPanel.GetComponent<Image>();

        // Add a coloured outline image layered on top of the panel
        leftBorder  = MakeOutline(leftPanel, "LeftBorder", Color.white);

        leftNameText  = MakeText(leftPanel, "LeftName",  "BOSS",  28, FontStyle.Bold,  TextAnchor.MiddleCenter, Color.white,        new Vector2(0.05f,0.78f), new Vector2(0.95f,0.96f));
        leftRewardText= MakeText(leftPanel, "LeftReward","4x Material", 18, FontStyle.Normal,TextAnchor.MiddleCenter, new Color(1f,0.9f,0.3f), new Vector2(0.05f,0.64f), new Vector2(0.95f,0.78f));
        leftRoomText  = MakeText(leftPanel, "LeftRoom",  "[CHAOS]",18, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.7f,0.85f,1f),  new Vector2(0.05f,0.52f), new Vector2(0.95f,0.65f));
        leftDescText  = MakeText(leftPanel, "LeftDesc",  "...",   15, FontStyle.Normal, TextAnchor.UpperCenter,  new Color(0.85f,0.85f,0.85f),new Vector2(0.07f,0.28f), new Vector2(0.93f,0.52f));
        leftSideLabel = MakeText(leftPanel, "LeftSide",  "STAY",  22, FontStyle.Bold,   TextAnchor.MiddleCenter, new Color(0.4f,1f,0.4f),    new Vector2(0.05f,0.08f), new Vector2(0.95f,0.26f));

        leftButton = leftPanel.AddComponent<Button>();
        leftButton.onClick.AddListener(() => OnPanelClicked(true));

        // RIGHT choice panel
        rightPanel = MakePanel(root, "RightPanel",
            new Color(0.10f, 0.10f, 0.14f, 1f),
            new Vector2(0.53f, 0.18f), new Vector2(0.96f, 0.74f),
            Vector2.zero, Vector2.zero);

        rightBorder   = MakeOutline(rightPanel, "RightBorder", Color.white);

        rightNameText  = MakeText(rightPanel,"RightName",  "BOSS",  28, FontStyle.Bold,  TextAnchor.MiddleCenter, Color.white,        new Vector2(0.05f,0.78f), new Vector2(0.95f,0.96f));
        rightRewardText= MakeText(rightPanel,"RightReward","4x Material",18,FontStyle.Normal,TextAnchor.MiddleCenter,new Color(1f,0.9f,0.3f),new Vector2(0.05f,0.64f),new Vector2(0.95f,0.78f));
        rightRoomText  = MakeText(rightPanel,"RightRoom",  "[MARKET]",18, FontStyle.Italic,TextAnchor.MiddleCenter, new Color(0.7f,0.85f,1f), new Vector2(0.05f,0.52f), new Vector2(0.95f,0.65f));
        rightDescText  = MakeText(rightPanel,"RightDesc",  "...",   15, FontStyle.Normal, TextAnchor.UpperCenter,  new Color(0.85f,0.85f,0.85f),new Vector2(0.07f,0.28f),new Vector2(0.93f,0.52f));
        rightSideLabel = MakeText(rightPanel,"RightSide",  "CROSS", 22, FontStyle.Bold,   TextAnchor.MiddleCenter, new Color(1f,0.6f,0.2f),   new Vector2(0.05f,0.08f), new Vector2(0.95f,0.26f));

        rightButton = rightPanel.AddComponent<Button>();
        rightButton.onClick.AddListener(() => OnPanelClicked(false));

        // Material earned notice (bottom strip)
        materialEarnedText = MakeText(root, "MaterialEarned",
            "",
            16, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.6f, 1f, 0.6f),
            new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.14f));
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Show the path selection screen.</summary>
    public void Show(BossData leftChoice, BossData rightChoice,
                     string materialEarned, int materialAmount)
    {
        if (root == null) return;

        int nextDepth = PathManager.Instance != null
            ? PathManager.Instance.CurrentDepth + 1
            : 1;
        bool currentlyLeft = PathManager.Instance?.OnLeftSide ?? true;

        depthText.text = $"DEPTH {nextDepth} / 4";

        // Populate left panel
        if (leftChoice != null)
            PopulatePanel(leftChoice,
                leftNameText, leftRewardText, leftRoomText,
                leftDescText, leftSideLabel, leftBorder,
                isLeft: true, currentlyLeft: currentlyLeft);

        // Populate right panel
        if (rightChoice != null)
            PopulatePanel(rightChoice,
                rightNameText, rightRewardText, rightRoomText,
                rightDescText, rightSideLabel, rightBorder,
                isLeft: false, currentlyLeft: currentlyLeft);

        // Material earned notice
        materialEarnedText.text = materialAmount > 0
            ? $"BOSS DROP: {materialAmount}x {materialEarned} collected"
            : "";

        root.SetActive(true);
        Time.timeScale = 0f;   // pause game while choosing
    }

    /// <summary>Hide the path selection screen and resume play.</summary>
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Private helpers ────────────────────────────────────────────────────

    void OnPanelClicked(bool goLeft)
    {
        Hide();
        PathManager.Instance?.ChoosePath(goLeft);
    }

    void PopulatePanel(BossData data,
        Text nameT, Text rewardT, Text roomT,
        Text descT, Text sideT, Image border,
        bool isLeft, bool currentlyLeft)
    {
        nameT.text   = data.Name;
        rewardT.text = $"{data.MaterialReward}x {data.MaterialName}";
        roomT.text   = $"[{data.RoomType.ToString().ToUpper()}]";
        descT.text   = data.Description + "\n\n" + data.AttackPatternDesc;

        bool staying = (isLeft == currentlyLeft);
        sideT.text  = staying ? "STAY" : "CROSS";
        sideT.color = staying
            ? new Color(0.4f, 1f, 0.4f)
            : new Color(1f, 0.6f, 0.2f);

        if (border != null)
        {
            Color c = data.ThemeColor;
            c.a = 0.85f;
            border.color = c;
        }
    }

    // ── UI factory helpers ─────────────────────────────────────────────────

    static GameObject MakePanel(GameObject parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var img   = go.AddComponent<Image>();
        img.color = color;

        var rt          = go.GetComponent<RectTransform>();
        rt.anchorMin    = anchorMin;
        rt.anchorMax    = anchorMax;
        rt.offsetMin    = offsetMin;
        rt.offsetMax    = offsetMax;

        return go;
    }

    static Image MakeOutline(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var img   = go.AddComponent<Image>();
        img.color = color;
        img.type  = Image.Type.Simple;

        var rt       = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-4f, -4f);
        rt.offsetMax = new Vector2( 4f,  4f);

        // Send outline behind child text elements
        go.transform.SetAsFirstSibling();

        return img;
    }

    static Text MakeText(GameObject parent, string name, string content,
        int fontSize, FontStyle fontStyle, TextAnchor anchor, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var t          = go.AddComponent<Text>();
        t.text         = content;
        t.fontSize     = fontSize;
        t.fontStyle    = fontStyle;
        t.alignment    = anchor;
        t.color        = color;
        t.font         = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.resizeTextForBestFit = false;

        var rt       = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return t;
    }
}
