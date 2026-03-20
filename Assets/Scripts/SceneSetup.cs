using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// ════════════════════════════════════════════════════════
///  DEAD ZONE — Scene Bootstrap
///  Attach this to ONE empty GameObject in SampleScene.
///  Press Play. Everything is built automatically.
/// ════════════════════════════════════════════════════════
/// </summary>
public class SceneSetup : MonoBehaviour
{
    void Awake()
    {
        // Dark background
        Camera.main.backgroundColor = new Color(0.04f, 0.04f, 0.09f);

        BuildManagers();
        BuildArena();
        var player = BuildPlayer();
        BuildCamera(player.transform);
        BuildUI();
    }

    void Start()
    {
        // Apply selected character modifiers before game starts
        ApplyCharacter();
        GameManager.Instance.StartGame();
    }

    void ApplyCharacter()
    {
        var c = GameData.SelectedCharacter;
        var s = PlayerStats.Instance;
        if (s == null || c == null) return;

        s.MaxHealthMult     = c.HPMult;
        s.MoveMult          = c.SpeedMult;
        s.DamageMult        = c.DamageMult;
        s.FireRateMult      = c.FireRateMult;
        s.BulletSizeMult    = c.BulletSizeMult;
        s.DashCooldownMult  = c.DashCDMult;
        s.Piercing          = c.StartPiercing;
        s.SplitShot         = c.StartSplitShot;
        s.SplitCount        = c.StartSplitShot ? 1 : 0;
        s.HasShield         = c.StartShield;

        // Tint player to match character color
        var sr = PlayerController.Instance?.GetComponent<SpriteRenderer>();
        if (sr) sr.color = c.PrimaryColor;
    }

    // ── Managers ──────────────────────────────────────────────────────────
    void BuildManagers()
    {
        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        gm.AddComponent<WaveManager>();
        gm.AddComponent<UpgradeManager>();
        gm.AddComponent<PathManager>();
        gm.AddComponent<GoldSystem>();
        gm.AddComponent<MaterialSystem>();
    }

    // ── Arena walls ───────────────────────────────────────────────────────
    void BuildArena()
    {
        const float H = 20f, T = 1f;
        MakeWall("WallTop",    new Vector2(0,  H),  new Vector2(H * 2 + T * 2, T));
        MakeWall("WallBot",    new Vector2(0, -H),  new Vector2(H * 2 + T * 2, T));
        MakeWall("WallLeft",   new Vector2(-H, 0),  new Vector2(T, H * 2));
        MakeWall("WallRight",  new Vector2( H, 0),  new Vector2(T, H * 2));

        // Visual floor grid (subtle)
        DrawFloorGrid();
    }

    void MakeWall(string name, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        // Subtle wall visual
        var sr   = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square(new Color(0.18f, 0.18f, 0.28f));
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        sr.sortingOrder = 0;
    }

    void DrawFloorGrid()
    {
        // Draw faint grid lines as thin sprite objects
        Color gridCol = new Color(0.1f, 0.1f, 0.18f, 0.6f);
        for (int x = -18; x <= 18; x += 4)
        {
            var line = new GameObject("GridV");
            line.transform.position   = new Vector3(x, 0, 0);
            line.transform.localScale = new Vector3(0.04f, 38f, 1f);
            var sr          = line.AddComponent<SpriteRenderer>();
            sr.sprite       = SpriteFactory.Square(gridCol);
            sr.sortingOrder = -1;
        }
        for (int y = -18; y <= 18; y += 4)
        {
            var line = new GameObject("GridH");
            line.transform.position   = new Vector3(0, y, 0);
            line.transform.localScale = new Vector3(38f, 0.04f, 1f);
            var sr          = line.AddComponent<SpriteRenderer>();
            sr.sprite       = SpriteFactory.Square(gridCol);
            sr.sortingOrder = -1;
        }
    }

    // ── Player ────────────────────────────────────────────────────────────
    GameObject BuildPlayer()
    {
        var go = new GameObject("Player");
        go.transform.position = Vector3.zero;

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = SpriteFactory.Triangle(new Color(0f, 1f, 1f));
        sr.color        = new Color(0f, 1f, 1f);
        sr.sortingOrder = 5;
        go.transform.localScale = Vector3.one * 0.9f;

        var rb             = go.AddComponent<Rigidbody2D>();
        rb.gravityScale    = 0f;
        rb.constraints     = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col     = go.AddComponent<CircleCollider2D>();
        col.radius  = 0.35f;

        go.AddComponent<PlayerStats>();
        go.AddComponent<PlayerController>();

        return go;
    }

    // ── Camera ────────────────────────────────────────────────────────────
    void BuildCamera(Transform target)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
        }

        cam.orthographic     = true;
        cam.orthographicSize = 11f;
        cam.backgroundColor  = new Color(0.04f, 0.04f, 0.09f);
        cam.transform.position = new Vector3(0f, 0f, -10f);

        var cc    = cam.gameObject.AddComponent<CameraController>();
        cc.Target = target;
    }

    // ── UI Canvas ─────────────────────────────────────────────────────────
    void BuildUI()
    {
        var canvasGO = new GameObject("UICanvas");

        var canvas             = canvasGO.AddComponent<Canvas>();
        canvas.renderMode      = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder    = 100;

        var scaler             = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode     = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — required for ALL UI button clicks to work
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // HUD
        var hudGO = new GameObject("HUDManager");
        hudGO.transform.SetParent(canvasGO.transform, false);
        var hud = hudGO.AddComponent<HUDManager>();
        hud.Build(canvas);

        // Upgrade picker
        var upGO = new GameObject("UpgradeUI");
        upGO.transform.SetParent(canvasGO.transform, false);
        var upUI = upGO.AddComponent<UpgradeUI>();
        upUI.Build(canvas);

        // Death screen
        var deathGO = new GameObject("DeathScreenUI");
        deathGO.transform.SetParent(canvasGO.transform, false);
        var deathUI = deathGO.AddComponent<DeathScreenUI>();
        deathUI.Build(canvas);

        // Path map (shown between bosses)
        var pathGO = new GameObject("PathMapUI");
        pathGO.transform.SetParent(canvasGO.transform, false);
        pathGO.AddComponent<PathMapUI>();
    }
}
