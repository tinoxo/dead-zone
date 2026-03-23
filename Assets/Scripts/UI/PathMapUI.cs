using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen path-selection overlay shown between bosses.
/// Works exactly like UpgradeUI — built into the main UICanvas, no nested canvas.
/// </summary>
public class PathMapUI : MonoBehaviour
{
    public static PathMapUI Instance { get; private set; }
    void Awake() => Instance = this;

    // ── UI References ─────────────────────────────────────────────────────
    GameObject  overlay;
    Text        titleText;
    Text        depthText;
    Text        materialText;

    // Left card
    GameObject  leftCard;
    Text        leftName, leftReward, leftRoom, leftDesc, leftSide;
    Image       leftBorder;

    // Right card
    GameObject  rightCard;
    Text        rightName, rightReward, rightRoom, rightDesc, rightSide;
    Image       rightBorder;

    BossData pendingLeft, pendingRight;

    // ── Called by SceneSetup ──────────────────────────────────────────────
    public void Build(Canvas canvas)
    {
        // Full-screen dark overlay
        overlay = MakePanel(canvas.transform, "PathMapOverlay",
            new Color(0.04f, 0.04f, 0.10f, 0.94f),
            Vector2.zero, Vector2.one);

        // Title
        titleText = MakeText(overlay.transform, "CHOOSE YOUR PATH",
            36, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.1f, 0.84f), new Vector2(0.9f, 0.97f));

        // Depth line
        depthText = MakeText(overlay.transform, "DEPTH 1 / 4",
            18, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.7f, 0.7f, 0.7f),
            new Vector2(0.3f, 0.77f), new Vector2(0.7f, 0.86f));

        // Material earned notice (bottom)
        materialText = MakeText(overlay.transform, "",
            15, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.5f, 1f, 0.5f),
            new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.13f));

        // Build left card
        leftCard = BuildCard(overlay.transform,
            new Vector2(0.04f, 0.15f), new Vector2(0.47f, 0.76f),
            ref leftBorder, ref leftName, ref leftReward,
            ref leftRoom, ref leftDesc, ref leftSide);

        // Click listener on left card
        var leftBtn = leftCard.AddComponent<Button>();
        leftBtn.transition = Selectable.Transition.ColorTint;
        leftBtn.onClick.AddListener(() => OnChoose(true));

        // Build right card
        rightCard = BuildCard(overlay.transform,
            new Vector2(0.53f, 0.15f), new Vector2(0.96f, 0.76f),
            ref rightBorder, ref rightName, ref rightReward,
            ref rightRoom, ref rightDesc, ref rightSide);

        var rightBtn = rightCard.AddComponent<Button>();
        rightBtn.transition = Selectable.Transition.ColorTint;
        rightBtn.onClick.AddListener(() => OnChoose(false));

        overlay.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────
    public void Show(BossData left, BossData right, string matName, int matAmt)
    {
        if (overlay == null) return;

        pendingLeft  = left;
        pendingRight = right;

        int depth = PathManager.Instance != null ? PathManager.Instance.CurrentDepth + 1 : 1;
        bool onLeft = PathManager.Instance?.OnLeftSide ?? true;

        depthText.text = $"DEPTH {depth} / 4";

        if (left  != null) FillCard(left,  leftName,  leftReward,  leftRoom,  leftDesc,  leftSide,  leftBorder,  side: true,  onLeft);
        if (right != null) FillCard(right, rightName, rightReward, rightRoom, rightDesc, rightSide, rightBorder, side: false, onLeft);

        materialText.text = matAmt > 0 ? $"BOSS DROP: +{matAmt}x {matName} collected!" : "";

        overlay.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Private ───────────────────────────────────────────────────────────
    void OnChoose(bool goLeft)
    {
        Hide();
        PathManager.Instance?.ChoosePath(goLeft);
    }

    void FillCard(BossData data,
        Text nameT, Text rewardT, Text roomT, Text descT, Text sideT,
        Image border, bool side, bool currentlyLeft)
    {
        nameT.text   = data.Name;
        rewardT.text = $"DROP: {data.MaterialReward}x {data.MaterialName}";
        roomT.text   = $"[{data.RoomType.ToString().ToUpper()} PATH]";
        descT.text   = data.Description;

        bool staying = (side == currentlyLeft);
        sideT.text  = staying ? "▶ STAY ON PATH" : "↔ CROSS TO THIS PATH";
        sideT.color = staying ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.65f, 0.1f);

        if (border != null)
        {
            var c = data.ThemeColor;
            c.a = 1f;
            border.color = c;
        }
    }

    GameObject BuildCard(Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        ref Image border, ref Text nameT, ref Text rewardT,
        ref Text roomT, ref Text descT, ref Text sideT)
    {
        // Background
        var card = MakePanel(parent, "Card", new Color(0.08f, 0.09f, 0.15f, 1f), anchorMin, anchorMax);

        // Coloured border strip (left edge)
        var borderGO = MakePanel(card.transform, "Border",
            Color.white,
            new Vector2(0f, 0f), new Vector2(0.018f, 1f));
        border = borderGO.GetComponent<Image>();

        // Texts
        nameT   = MakeText(card.transform, data: "",  size: 26, style: FontStyle.Bold,
            anchor: TextAnchor.MiddleCenter, col: Color.white,
            aMin: new Vector2(0.05f, 0.79f), aMax: new Vector2(0.95f, 0.97f));

        rewardT = MakeText(card.transform, data: "",  size: 15, style: FontStyle.Normal,
            anchor: TextAnchor.MiddleCenter, col: new Color(1f, 0.9f, 0.3f),
            aMin: new Vector2(0.05f, 0.67f), aMax: new Vector2(0.95f, 0.80f));

        roomT   = MakeText(card.transform, data: "",  size: 14, style: FontStyle.Italic,
            anchor: TextAnchor.MiddleCenter, col: new Color(0.5f, 0.8f, 1f),
            aMin: new Vector2(0.05f, 0.57f), aMax: new Vector2(0.95f, 0.68f));

        descT   = MakeText(card.transform, data: "",  size: 13, style: FontStyle.Normal,
            anchor: TextAnchor.UpperCenter, col: new Color(0.78f, 0.78f, 0.78f),
            aMin: new Vector2(0.05f, 0.27f), aMax: new Vector2(0.95f, 0.57f));

        sideT   = MakeText(card.transform, data: "",  size: 16, style: FontStyle.Bold,
            anchor: TextAnchor.MiddleCenter, col: Color.white,
            aMin: new Vector2(0.05f, 0.06f), aMax: new Vector2(0.95f, 0.26f));

        return card;
    }

    // ── UI Factories ──────────────────────────────────────────────────────
    static GameObject MakePanel(Transform parent, string name, Color col,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = col;
        var rt  = img.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    Text MakeText(Transform parent, string data, int size, FontStyle style,
        TextAnchor anchor, Color col, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t           = go.AddComponent<Text>();
        t.text          = data;
        t.fontSize      = size;
        t.fontStyle     = style;
        t.alignment     = anchor;
        t.color         = col;
        t.font          = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt          = t.rectTransform;
        rt.anchorMin    = aMin;
        rt.anchorMax    = aMax;
        rt.offsetMin    = new Vector2(8f, 4f);
        rt.offsetMax    = new Vector2(-8f, -4f);
        return t;
    }
}
