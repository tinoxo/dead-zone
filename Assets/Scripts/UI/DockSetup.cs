using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// DEAD ZONE — Dock Hub Scene
/// A small walkable city dock before the ship run.
/// Attach to ONE empty GameObject in DockScene.
/// </summary>
public class DockSetup : MonoBehaviour
{
    // ── Layout ────────────────────────────────────────────────────────────
    const float SHIP_X      =  8.5f;
    const float SHIP_Y      =  0.0f;
    const float DOCK_Y      =  0.0f;
    const float DOCK_H      =  3.2f;
    const float BOARD_DIST  =  2.2f;

    // ── Colors ────────────────────────────────────────────────────────────
    static readonly Color C_WATER      = new Color(0.04f, 0.10f, 0.20f);
    static readonly Color C_WATER_SHIMMER = new Color(0.06f, 0.14f, 0.26f);
    static readonly Color C_DOCK       = new Color(0.32f, 0.21f, 0.11f);
    static readonly Color C_PLANK      = new Color(0.40f, 0.27f, 0.14f);
    static readonly Color C_BUILDING   = new Color(0.10f, 0.12f, 0.18f);
    static readonly Color C_BUILDING2  = new Color(0.13f, 0.15f, 0.22f);
    static readonly Color C_WINDOW_LIT = new Color(0.90f, 0.78f, 0.38f);
    static readonly Color C_WINDOW_OFF = new Color(0.07f, 0.09f, 0.14f);
    static readonly Color C_SHIP_HULL  = new Color(0.22f, 0.13f, 0.07f);
    static readonly Color C_SHIP_DARK  = new Color(0.15f, 0.08f, 0.04f);
    static readonly Color C_MAST       = new Color(0.30f, 0.19f, 0.09f);
    static readonly Color C_SAIL       = new Color(0.82f, 0.76f, 0.62f);
    static readonly Color C_ROPE       = new Color(0.55f, 0.42f, 0.22f);

    // ── Runtime refs ──────────────────────────────────────────────────────
    GameObject  playerGO;
    Transform   shipTransform;
    Canvas      uiCanvas;
    GameObject  promptGO;
    bool        transitioning;

    // ══════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Camera.main.backgroundColor = C_WATER;
        Camera.main.orthographic    = true;
        Camera.main.orthographicSize = 6.5f;

        BuildEnvironment();
        BuildShip();
        playerGO = BuildPlayer();
        BuildCamera();
        BuildUI();
    }

    void Update()
    {
        if (transitioning || playerGO == null || shipTransform == null) return;

        float dist = Vector2.Distance(playerGO.transform.position,
                                      (Vector2)shipTransform.position);
        bool near = dist < BOARD_DIST;
        if (promptGO) promptGO.SetActive(near);

        if (near && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(BoardShip());
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ENVIRONMENT
    // ══════════════════════════════════════════════════════════════════════
    void BuildEnvironment()
    {
        var env = new GameObject("Environment");

        // Water fill
        MakeRect("Water", env.transform, new Vector2(0, 0),
                 new Vector2(36f, 22f), C_WATER, -10);

        // Water shimmer lines (above & below dock)
        for (int i = -6; i <= 6; i++)
        {
            float ox = i * 2.8f;
            MakeRect($"Shimmer_Top{i}", env.transform,
                     new Vector2(ox, DOCK_Y + DOCK_H * 0.5f + 1.1f),
                     new Vector2(2.4f, 0.12f), C_WATER_SHIMMER, -9);
            MakeRect($"Shimmer_Bot{i}", env.transform,
                     new Vector2(ox + 1.4f, DOCK_Y - DOCK_H * 0.5f - 0.8f),
                     new Vector2(2.4f, 0.12f), C_WATER_SHIMMER, -9);
        }

        // Dock base (wooden pier)
        float dockL = -12f, dockR = SHIP_X + 1.5f;
        float dockCX = (dockL + dockR) * 0.5f;
        float dockW  = dockR - dockL;
        MakeRect("Dock_Base", env.transform,
                 new Vector2(dockCX, DOCK_Y), new Vector2(dockW, DOCK_H), C_DOCK, -8);

        // Plank lines
        int planks = Mathf.RoundToInt(dockW / 1.4f);
        for (int i = 0; i < planks; i++)
        {
            float px = dockL + (i + 0.5f) * (dockW / planks);
            MakeRect($"Plank{i}", env.transform,
                     new Vector2(px, DOCK_Y),
                     new Vector2(0.07f, DOCK_H * 0.9f), C_PLANK, -7);
        }

        // Dock edge borders
        MakeRect("DockEdgeTop", env.transform,
                 new Vector2(dockCX, DOCK_Y + DOCK_H * 0.5f),
                 new Vector2(dockW, 0.18f), C_MAST, -6);
        MakeRect("DockEdgeBot", env.transform,
                 new Vector2(dockCX, DOCK_Y - DOCK_H * 0.5f),
                 new Vector2(dockW, 0.18f), C_MAST, -6);

        // Dock mooring posts
        float[] postXs = { -10f, -6f, -2f, 2f, 6f };
        foreach (float px in postXs)
        {
            MakeRect($"Post_T{px}", env.transform,
                     new Vector2(px, DOCK_Y + DOCK_H * 0.5f - 0.15f),
                     new Vector2(0.28f, 0.28f), C_SHIP_DARK, -5);
            MakeRect($"Post_B{px}", env.transform,
                     new Vector2(px, DOCK_Y - DOCK_H * 0.5f + 0.15f),
                     new Vector2(0.28f, 0.28f), C_SHIP_DARK, -5);
        }

        // City buildings (upper portion)
        BuildBuildings(env.transform, DOCK_Y + DOCK_H * 0.5f + 0.2f);

        // Boundary walls (invisible, keep player on dock)
        BuildWall(env.transform, new Vector2(dockCX, DOCK_Y + DOCK_H * 0.5f + 0.5f),
                  new Vector2(dockW, 0.4f)); // top
        BuildWall(env.transform, new Vector2(dockCX, DOCK_Y - DOCK_H * 0.5f - 0.5f),
                  new Vector2(dockW, 0.4f)); // bottom
        BuildWall(env.transform, new Vector2(dockL - 0.5f, DOCK_Y),
                  new Vector2(0.4f, DOCK_H + 2f)); // left
    }

    void BuildBuildings(Transform parent, float baseY)
    {
        // (centerX, width, height)
        (float x, float w, float h, int wRows, int wCols)[] data =
        {
            (-10f, 4.2f, 6.0f, 3, 2),
            ( -5f, 3.0f, 4.5f, 2, 2),
            (  0f, 5.0f, 7.5f, 4, 2),
            (  5f, 3.5f, 5.5f, 3, 2),
            ( 10f, 4.0f, 6.5f, 3, 2),
        };

        var rng = new System.Random(42); // seeded so same every load
        foreach (var (bx, bw, bh, wRows, wCols) in data)
        {
            Color bldCol = rng.Next(2) == 0 ? C_BUILDING : C_BUILDING2;
            MakeRect($"Bld{bx}", parent,
                     new Vector2(bx, baseY + bh * 0.5f),
                     new Vector2(bw, bh), bldCol, -9);

            // Roof accent
            MakeRect($"Bld{bx}_Roof", parent,
                     new Vector2(bx, baseY + bh),
                     new Vector2(bw + 0.1f, 0.2f),
                     new Color(0.06f, 0.08f, 0.13f), -8);

            // Windows
            for (int wy = 0; wy < wRows; wy++)
            for (int wx = 0; wx < wCols; wx++)
            {
                float winX = bx + (wx - (wCols - 1) * 0.5f) * (bw / (wCols + 0.5f));
                float winY = baseY + 0.5f + wy * (bh / (wRows + 0.5f));
                bool lit   = rng.Next(3) != 0;
                MakeRect($"Win{bx}_{wx}_{wy}", parent,
                         new Vector2(winX, winY),
                         new Vector2(0.38f, 0.50f),
                         lit ? C_WINDOW_LIT : C_WINDOW_OFF, -7);
            }
        }
    }

    void BuildWall(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Wall");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  SHIP
    // ══════════════════════════════════════════════════════════════════════
    void BuildShip()
    {
        var shipGO = new GameObject("Ship");
        shipTransform = shipGO.transform;
        shipGO.transform.position = new Vector2(SHIP_X, SHIP_Y);

        // Hull
        MakeRect("Hull",     shipGO.transform, new Vector2( 0.0f,  0.0f), new Vector2(3.8f, 2.4f), C_SHIP_HULL, -4);
        MakeRect("HullKeel", shipGO.transform, new Vector2( 0.0f, -0.9f), new Vector2(3.0f, 0.9f), C_SHIP_DARK, -3);
        MakeRect("Bow",      shipGO.transform, new Vector2(-2.2f, -0.3f), new Vector2(0.8f, 1.6f), C_SHIP_DARK, -3);

        // Deck railing
        MakeRect("RailTop", shipGO.transform, new Vector2(0f, 1.1f), new Vector2(3.8f, 0.16f), C_MAST, -3);

        // Mast
        MakeRect("Mast",       shipGO.transform, new Vector2( 0.3f, 2.2f), new Vector2(0.14f, 4.0f), C_MAST, -3);
        MakeRect("Boom",       shipGO.transform, new Vector2( 0.3f, 1.6f), new Vector2(2.6f, 0.10f), C_MAST, -3);
        MakeRect("TopYard",    shipGO.transform, new Vector2( 0.3f, 4.0f), new Vector2(1.8f, 0.10f), C_MAST, -3);

        // Sails
        MakeRect("Sail_Main", shipGO.transform, new Vector2( 0.3f, 2.8f), new Vector2(2.2f, 2.0f), C_SAIL, -4);
        MakeRect("Sail_Top",  shipGO.transform, new Vector2( 0.3f, 4.3f), new Vector2(1.2f, 0.9f), C_SAIL, -4);

        // Ropes (diagonal lines approximated)
        MakeRect("Rope1", shipGO.transform, new Vector2( 1.0f, 2.5f), new Vector2(0.08f, 1.8f), C_ROPE, -3);
        MakeRect("Rope2", shipGO.transform, new Vector2(-0.4f, 2.5f), new Vector2(0.08f, 1.8f), C_ROPE, -3);

        // Gangplank from ship to dock
        MakeRect("Gangplank", shipGO.transform,
                 new Vector2(-2.8f, -0.4f), new Vector2(2.0f, 0.32f), C_PLANK, -3);

        // Flag at top
        MakeRect("Flag", shipGO.transform,
                 new Vector2( 0.7f, 4.35f), new Vector2(0.6f, 0.35f),
                 new Color(0.7f, 0.1f, 0.1f), -3);

        // Boarding trigger collider
        var trigger = new GameObject("BoardTrigger");
        trigger.transform.SetParent(shipGO.transform, false);
        trigger.transform.localPosition = new Vector2(-2.0f, 0f);
        var tc = trigger.AddComponent<CircleCollider2D>();
        tc.radius    = 1.6f;
        tc.isTrigger = true;

        // World-space "BOARD SHIP" label above ship
        BuildShipWorldLabel(shipGO.transform);
    }

    void BuildShipWorldLabel(Transform parent)
    {
        var cGO = new GameObject("ShipLabel");
        cGO.transform.SetParent(parent, false);
        cGO.transform.localPosition = new Vector3(-0.5f, 3.6f, 0f);

        var canv = cGO.AddComponent<Canvas>();
        canv.renderMode   = RenderMode.WorldSpace;
        canv.sortingOrder = 20;

        var rt = cGO.GetComponent<RectTransform>();
        rt.sizeDelta   = new Vector2(500f, 80f);
        rt.localScale  = new Vector3(0.010f, 0.010f, 1f);

        var txt  = new GameObject("T").AddComponent<Text>();
        txt.transform.SetParent(cGO.transform, false);
        var tRT  = txt.rectTransform;
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        txt.text      = "[ E ]  BOARD SHIP";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize  = 32;
        txt.fontStyle = FontStyle.Bold;
        txt.color     = new Color(1f, 0.88f, 0.40f);
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PLAYER
    // ══════════════════════════════════════════════════════════════════════
    GameObject BuildPlayer()
    {
        var go = new GameObject("Player");
        go.tag   = "Player";
        go.transform.position = new Vector2(-9f, DOCK_Y);

        // Circle sprite in character color
        Color col = GameData.SelectedCharacter?.PrimaryColor ?? Color.cyan;
        var   sr  = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        sr.sprite       = MakeCircleSprite(col, 64);
        go.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        // Physics
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale  = 0;
        rb.freezeRotation = true;
        rb.linearDamping  = 10f;

        var cc = go.AddComponent<CircleCollider2D>();
        cc.radius = 0.5f;

        go.AddComponent<DockPlayerWalk>();
        return go;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CAMERA
    // ══════════════════════════════════════════════════════════════════════
    void BuildCamera()
    {
        var cam = Camera.main;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.orthographicSize   = 6.5f;

        var follow = cam.gameObject.AddComponent<DockCameraFollow>();
        follow.Target    = playerGO.transform;
        follow.BoundsMin = new Vector2(-7f, -1f);
        follow.BoundsMax = new Vector2( 7f,  1f);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  UI
    // ══════════════════════════════════════════════════════════════════════
    void BuildUI()
    {
        var cGO = new GameObject("UICanvas");
        uiCanvas = cGO.AddComponent<Canvas>();
        uiCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 50;
        var sc = cGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Interaction prompt pill (bottom center, hidden by default)
        promptGO = new GameObject("Prompt");
        promptGO.transform.SetParent(cGO.transform, false);
        var pRT = promptGO.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0f);
        pRT.anchorMax = new Vector2(0.5f, 0f);
        pRT.pivot     = new Vector2(0.5f, 0f);
        pRT.anchoredPosition = new Vector2(0f, 30f);
        pRT.sizeDelta        = new Vector2(340f, 54f);

        var bg = promptGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.80f);

        var pTxt = new GameObject("Txt").AddComponent<Text>();
        pTxt.transform.SetParent(promptGO.transform, false);
        var ptRT = pTxt.rectTransform;
        ptRT.anchorMin = Vector2.zero; ptRT.anchorMax = Vector2.one;
        ptRT.offsetMin = ptRT.offsetMax = Vector2.zero;
        pTxt.text      = "[ E ]  BOARD SHIP";
        pTxt.alignment = TextAnchor.MiddleCenter;
        pTxt.fontSize  = 24;
        pTxt.fontStyle = FontStyle.Bold;
        pTxt.color     = new Color(1f, 0.88f, 0.40f);
        pTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        promptGO.SetActive(false);

        // Character name (top-left)
        var nmGO = new GameObject("CharName");
        nmGO.transform.SetParent(cGO.transform, false);
        var nmRT = nmGO.AddComponent<RectTransform>();
        nmRT.anchorMin = new Vector2(0f, 1f);
        nmRT.anchorMax = new Vector2(0f, 1f);
        nmRT.pivot     = new Vector2(0f, 1f);
        nmRT.anchoredPosition = new Vector2(18f, -18f);
        nmRT.sizeDelta        = new Vector2(300f, 36f);
        var nmTxt = nmGO.AddComponent<Text>();
        nmTxt.text      = GameData.SelectedCharacter?.Name?.ToUpper() ?? "PLAYER";
        nmTxt.fontSize  = 17;
        nmTxt.fontStyle = FontStyle.Bold;
        nmTxt.color     = GameData.SelectedCharacter?.PrimaryColor ?? Color.white;
        nmTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Location label (top-center)
        var locGO = new GameObject("Location");
        locGO.transform.SetParent(cGO.transform, false);
        var locRT = locGO.AddComponent<RectTransform>();
        locRT.anchorMin = new Vector2(0.5f, 1f);
        locRT.anchorMax = new Vector2(0.5f, 1f);
        locRT.pivot     = new Vector2(0.5f, 1f);
        locRT.anchoredPosition = new Vector2(0f, -18f);
        locRT.sizeDelta        = new Vector2(500f, 32f);
        var locTxt = locGO.AddComponent<Text>();
        locTxt.text      = "THE DOCKS";
        locTxt.alignment = TextAnchor.UpperCenter;
        locTxt.fontSize  = 15;
        locTxt.color     = new Color(0.42f, 0.52f, 0.68f);
        locTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  BOARD SHIP
    // ══════════════════════════════════════════════════════════════════════
    IEnumerator BoardShip()
    {
        transitioning = true;
        if (promptGO) promptGO.SetActive(false);

        // Fade overlay
        var fadeGO = new GameObject("Fade");
        fadeGO.transform.SetParent(uiCanvas.transform, false);
        var fRT = fadeGO.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        var fImg = fadeGO.AddComponent<Image>();
        fImg.color = new Color(0f, 0f, 0f, 0f);

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 1.8f;
            fImg.color = new Color(0f, 0f, 0f, Mathf.Clamp01(timer));
            yield return null;
        }

        GameData.IsShipRun = true;
        SceneManager.LoadScene("SampleScene");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════════
    GameObject MakeRect(string name, Transform parent, Vector2 pos, Vector2 size,
                        Color col, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortOrder;
        sr.sprite = MakeSolidSprite(col);
        sr.color  = col;
        return go;
    }

    Sprite MakeSolidSprite(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    Sprite MakeCircleSprite(Color c, int res)
    {
        var t = new Texture2D(res, res);
        float r = res * 0.5f;
        for (int py = 0; py < res; py++)
        for (int px = 0; px < res; px++)
        {
            float dx = px - r, dy = py - r;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float alpha = Mathf.Clamp01((r - 2f - dist));
            t.SetPixel(px, py, new Color(c.r, c.g, c.b, alpha));
        }
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}

// ══════════════════════════════════════════════════════════════════════════
/// <summary>Simple WASD/arrow walk for the dock area.</summary>
public class DockPlayerWalk : MonoBehaviour
{
    Rigidbody2D rb;
    const float SPEED = 5.5f;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        rb.linearVelocity = new Vector2(x, y).normalized * SPEED;
    }
}

// ══════════════════════════════════════════════════════════════════════════
/// <summary>Smooth camera follow with clamped bounds for the dock.</summary>
public class DockCameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector2   BoundsMin;
    public Vector2   BoundsMax;
    const float SMOOTH = 6f;

    void LateUpdate()
    {
        if (Target == null) return;
        float cx = Mathf.Clamp(Target.position.x, BoundsMin.x, BoundsMax.x);
        float cy = Mathf.Clamp(Target.position.y, BoundsMin.y, BoundsMax.y);
        transform.position = Vector3.Lerp(transform.position,
                                          new Vector3(cx, cy, -10f),
                                          SMOOTH * Time.deltaTime);
    }
}
