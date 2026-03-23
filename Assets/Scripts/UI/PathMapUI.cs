using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen path-selection overlay shown between bosses.
/// Includes a minimap strip showing the full boss lattice at the top.
/// </summary>
public class PathMapUI : MonoBehaviour
{
    public static PathMapUI Instance { get; private set; }
    void Awake() => Instance = this;

    // ── UI References ─────────────────────────────────────────────────────
    GameObject  overlay;
    Text        titleText;
    Text        materialText;

    // Left card
    GameObject  leftCard;
    Text        leftName, leftReward, leftRoom, leftDesc, leftSide;
    Image       leftBorder;

    // Right card
    GameObject  rightCard;
    Text        rightName, rightReward, rightRoom, rightDesc, rightSide;
    Image       rightBorder;

    // Minimap node images — indexed [depth 0-4][side 0=left,1=right]
    // depth 0 = Warden (both same), depth 4 = Omega (both same)
    Image[,] nodeImages = new Image[5, 2];

    // ── Node layout (anchored positions within minimap panel) ─────────────
    // x = depth column, y = 0.7 for left path, 0.3 for right, 0.5 for single
    static readonly Vector2[,] NodeAnchors = {
        { new Vector2(0.05f, 0.50f), new Vector2(0.05f, 0.50f) }, // depth 0 Warden
        { new Vector2(0.27f, 0.75f), new Vector2(0.27f, 0.25f) }, // depth 1
        { new Vector2(0.50f, 0.75f), new Vector2(0.50f, 0.25f) }, // depth 2
        { new Vector2(0.73f, 0.75f), new Vector2(0.73f, 0.25f) }, // depth 3
        { new Vector2(0.95f, 0.50f), new Vector2(0.95f, 0.50f) }, // depth 4 Omega
    };

    // ── Colors ────────────────────────────────────────────────────────────
    static readonly Color ColVisited   = new Color(0.30f, 1.00f, 0.30f); // bright green
    static readonly Color ColCurrent   = new Color(1.00f, 0.85f, 0.10f); // gold
    static readonly Color ColAvailable = new Color(0.40f, 0.70f, 1.00f); // blue
    static readonly Color ColFuture    = new Color(0.30f, 0.30f, 0.40f); // dim
    static readonly Color ColLine      = new Color(0.25f, 0.25f, 0.35f); // dim line
    static readonly Color ColLineLive  = new Color(0.40f, 0.40f, 0.55f); // brighter line

    // ── Called by SceneSetup ──────────────────────────────────────────────
    public void Build(Canvas canvas)
    {
        // Full-screen dark overlay
        overlay = MakePanel(canvas.transform, "PathMapOverlay",
            new Color(0.04f, 0.04f, 0.10f, 0.95f),
            Vector2.zero, Vector2.one);

        // Title
        titleText = MakeText(overlay.transform, "CHOOSE YOUR PATH",
            34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.1f, 0.90f), new Vector2(0.9f, 0.99f));

        // ── Minimap strip ─────────────────────────────────────────────────
        var mapBg = MakePanel(overlay.transform, "MinimapBg",
            new Color(0.06f, 0.06f, 0.12f, 0.9f),
            new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.91f));

        BuildMinimap(mapBg.transform);

        // Material earned notice (bottom)
        materialText = MakeText(overlay.transform, "",
            14, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.5f, 1f, 0.5f),
            new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.11f));

        // Left card
        leftCard = BuildCard(overlay.transform,
            new Vector2(0.04f, 0.13f), new Vector2(0.47f, 0.79f),
            ref leftBorder, ref leftName, ref leftReward,
            ref leftRoom, ref leftDesc, ref leftSide);
        var leftBtn = leftCard.AddComponent<Button>();
        leftBtn.transition = Selectable.Transition.ColorTint;
        leftBtn.onClick.AddListener(() => OnChoose(true));

        // Right card
        rightCard = BuildCard(overlay.transform,
            new Vector2(0.53f, 0.13f), new Vector2(0.96f, 0.79f),
            ref rightBorder, ref rightName, ref rightReward,
            ref rightRoom, ref rightDesc, ref rightSide);
        var rightBtn = rightCard.AddComponent<Button>();
        rightBtn.transition = Selectable.Transition.ColorTint;
        rightBtn.onClick.AddListener(() => OnChoose(false));

        overlay.SetActive(false);
    }

    void BuildMinimap(Transform mapParent)
    {
        // Draw connection lines first (so nodes render on top)
        // Straight connections (same side, adjacent depths)
        for (int d = 0; d < 4; d++)
        {
            DrawLine(mapParent, NodeAnchors[d, 0], NodeAnchors[d + 1, 0], ColLine); // left path
            DrawLine(mapParent, NodeAnchors[d, 1], NodeAnchors[d + 1, 1], ColLine); // right path
        }
        // Cross connections (depth 0→1 already included above since both point to same Warden)
        // Cross between L and R at each transition
        for (int d = 1; d < 4; d++)
        {
            DrawLine(mapParent, NodeAnchors[d, 0], NodeAnchors[d + 1, 1], ColLine); // L→R cross
            DrawLine(mapParent, NodeAnchors[d, 1], NodeAnchors[d + 1, 0], ColLine); // R→L cross
        }

        // Boss name labels above each column
        string[] leftNames  = { "WARDEN", "VORTEX",  "HOLLOW", "BREACH", "OMEGA" };
        string[] rightNames = { "",       "SIEGE",   "DRIFT",  "PULSE",  ""      };

        for (int d = 0; d < 5; d++)
        {
            // Left node
            var nodeL = MakeMapNode(mapParent, NodeAnchors[d, 0], d == 0 || d == 4);
            nodeImages[d, 0] = nodeL;

            // Label above left node
            MakeMapLabel(mapParent, leftNames[d], NodeAnchors[d, 0] + new Vector2(0f, 0.38f));

            // Right node (skip if same as left)
            if (d != 0 && d != 4)
            {
                var nodeR = MakeMapNode(mapParent, NodeAnchors[d, 1], false);
                nodeImages[d, 1] = nodeR;
                MakeMapLabel(mapParent, rightNames[d], NodeAnchors[d, 1] - new Vector2(0f, 0.40f));
            }
            else
            {
                nodeImages[d, 1] = nodeL; // same node
            }
        }
    }

    Image MakeMapNode(Transform parent, Vector2 anchor, bool large)
    {
        float sz = large ? 18f : 14f;
        var go  = new GameObject("Node");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = SpriteFactory.Circle(Color.white);
        img.color  = ColFuture;
        var rt = img.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(sz, sz);
        rt.anchoredPosition = Vector2.zero;
        return img;
    }

    void MakeMapLabel(Transform parent, string txt, Vector2 anchor)
    {
        if (string.IsNullOrEmpty(txt)) return;
        var go = new GameObject("NodeLabel");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = txt;
        t.fontSize  = 7;
        t.font      = GetFont();
        t.color     = new Color(0.6f, 0.6f, 0.7f);
        t.alignment = TextAnchor.MiddleCenter;
        var rt = t.rectTransform;
        rt.anchorMin = anchor - new Vector2(0.08f, 0.35f);
        rt.anchorMax = anchor + new Vector2(0.08f, 0.35f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void DrawLine(Transform parent, Vector2 fromAnchor, Vector2 toAnchor, Color col)
    {
        var go  = new GameObject("Line");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color  = col;

        // Place at midpoint, stretch to cover distance
        Vector2 mid = (fromAnchor + toAnchor) * 0.5f;
        var rt = img.rectTransform;
        rt.anchorMin = mid;
        rt.anchorMax = mid;
        rt.pivot     = new Vector2(0.5f, 0.5f);

        // We can't know pixel size at build time, so store as sizeDelta using
        // a rough estimate — 1 anchor unit ≈ 500px wide panel, 60px tall
        // We just draw a thin line using sizeDelta width + rotation
        // Use a fixed reference panel width of 900px for the line length calc
        float panelW = 900f;
        float panelH = 60f;
        Vector2 diff = new Vector2(
            (toAnchor.x - fromAnchor.x) * panelW,
            (toAnchor.y - fromAnchor.y) * panelH);
        float len   = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        rt.sizeDelta        = new Vector2(len, 1.5f);
        rt.localRotation    = Quaternion.Euler(0f, 0f, angle);
        rt.anchoredPosition = Vector2.zero;
    }

    // ── Public API ────────────────────────────────────────────────────────
    public void Show(BossData left, BossData right, string matName, int matAmt)
    {
        if (overlay == null) return;

        int  depth  = PathManager.Instance?.CurrentDepth ?? 0;
        bool onLeft = PathManager.Instance?.OnLeftSide   ?? true;
        var  history = PathManager.Instance?.PathHistory;

        // Update minimap node colors
        RefreshMinimap(depth, onLeft, history);

        // Depth label (shown inside title now)
        int nextDepth = depth + 1;
        titleText.text = $"CHOOSE YOUR PATH  —  DEPTH {nextDepth} / 4";

        if (left  != null) FillCard(left,  leftName,  leftReward,  leftRoom,  leftDesc,  leftSide,  leftBorder,  side: true,  onLeft);
        if (right != null) FillCard(right, rightName, rightReward, rightRoom, rightDesc, rightSide, rightBorder, side: false, onLeft);

        materialText.text = matAmt > 0 ? $"+{matAmt}x {matName} collected from boss!" : "";

        overlay.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (overlay != null) overlay.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Minimap coloring ──────────────────────────────────────────────────
    void RefreshMinimap(int currentDepth, bool onLeft, System.Collections.Generic.List<string> history)
    {
        for (int d = 0; d < 5; d++)
        {
            for (int s = 0; s < 2; s++)
            {
                if (nodeImages[d, s] == null) continue;

                Color c;
                if (d < currentDepth)
                {
                    // Already visited depth
                    c = ColVisited;
                }
                else if (d == currentDepth)
                {
                    // Current position — gold
                    c = ColCurrent;
                }
                else if (d == currentDepth + 1)
                {
                    // Next available choices — bright blue
                    c = ColAvailable;
                }
                else
                {
                    // Further future — dim
                    c = ColFuture;
                }

                nodeImages[d, s].color = c;
            }
        }
    }

    // ── Card helpers ──────────────────────────────────────────────────────
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
        roomT.text   = $"[ {data.RoomType.ToString().ToUpper()} PATH ]";
        descT.text   = data.Description;

        bool staying = (side == currentlyLeft);
        sideT.text  = staying ? "▶  STAY ON THIS PATH" : "↔  CROSS TO THIS PATH";
        sideT.color = staying ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.65f, 0.1f);

        if (border != null) { var c = data.ThemeColor; c.a = 1f; border.color = c; }
    }

    GameObject BuildCard(Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        ref Image border, ref Text nameT, ref Text rewardT,
        ref Text roomT, ref Text descT, ref Text sideT)
    {
        var card = MakePanel(parent, "Card", new Color(0.08f, 0.09f, 0.15f, 1f), anchorMin, anchorMax);

        var borderGO = MakePanel(card.transform, "Border", Color.white,
            new Vector2(0f, 0f), new Vector2(0.018f, 1f));
        border = borderGO.GetComponent<Image>();

        nameT   = MakeText(card.transform, "", 24, FontStyle.Bold,   TextAnchor.MiddleCenter, Color.white,                    new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.97f));
        rewardT = MakeText(card.transform, "", 14, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(1f,   0.9f,  0.3f),   new Vector2(0.05f, 0.67f), new Vector2(0.95f, 0.81f));
        roomT   = MakeText(card.transform, "", 13, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.5f, 0.8f,  1f),     new Vector2(0.05f, 0.57f), new Vector2(0.95f, 0.68f));
        descT   = MakeText(card.transform, "", 12, FontStyle.Normal, TextAnchor.UpperCenter,  new Color(0.78f,0.78f, 0.78f),  new Vector2(0.05f, 0.27f), new Vector2(0.95f, 0.57f));
        sideT   = MakeText(card.transform, "", 15, FontStyle.Bold,   TextAnchor.MiddleCenter, Color.white,                    new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.26f));

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
        var t  = go.AddComponent<Text>();
        t.text               = data;
        t.fontSize           = size;
        t.fontStyle          = style;
        t.alignment          = anchor;
        t.color              = col;
        t.font               = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt  = t.rectTransform;
        rt.anchorMin    = aMin;
        rt.anchorMax    = aMax;
        rt.offsetMin    = new Vector2(8f, 4f);
        rt.offsetMax    = new Vector2(-8f, -4f);
        return t;
    }
}
