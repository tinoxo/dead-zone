using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DoorRoomType { Combat, Boss, Shop, Item, Path }

/// <summary>
/// A door that appears after a wave is cleared or after a boss is defeated.
/// DoorRoomType.Path doors show boss info and let the player choose their next path.
/// </summary>
public class RoomDoor : MonoBehaviour
{
    public DoorRoomType RoomType;
    public BossData     PathBoss;   // filled for Path doors
    bool                goLeft;     // which side this path door leads to

    SpriteRenderer  glowCore;
    GameObject      labelRoot;
    CanvasGroup     labelGroup;

    bool  entered;
    float pulseTimer;
    float bobTimer;

    const float ENTER_DIST = 0.8f;
    const float LABEL_DIST = 6f;

    // ── Factories ─────────────────────────────────────────────────────────

    /// <summary>Standard wave/boss/shop/item door.</summary>
    public static RoomDoor Spawn(Vector2 position, DoorRoomType type)
    {
        var go   = new GameObject("RoomDoor_" + type);
        go.transform.position = position;
        var door  = go.AddComponent<RoomDoor>();
        door.RoomType = type;
        door.Build();
        return door;
    }

    /// <summary>Path-choice door — shows the target boss info in the label.</summary>
    public static RoomDoor SpawnPathDoor(Vector2 position, BossData boss, bool choosingLeft)
    {
        var go   = new GameObject("PathDoor_" + (choosingLeft ? "Left" : "Right"));
        go.transform.position = position;
        var door      = go.AddComponent<RoomDoor>();
        door.RoomType = DoorRoomType.Path;
        door.PathBoss = boss;
        door.goLeft   = choosingLeft;
        door.Build();
        return door;
    }

    // ── Build ─────────────────────────────────────────────────────────────

    void Build()
    {
        Color col = DoorColor();

        // Door frame pillars + beams
        MakeRect("PillarL",  col * 0.7f, new Vector2(-0.55f,  0f), new Vector2(0.18f, 2.0f));
        MakeRect("PillarR",  col * 0.7f, new Vector2( 0.55f,  0f), new Vector2(0.18f, 2.0f));
        MakeRect("BeamTop",  col,         new Vector2(0f,  1.0f),  new Vector2(1.28f, 0.18f));
        MakeRect("BeamBot",  col,         new Vector2(0f, -1.0f),  new Vector2(1.28f, 0.18f));

        // Glowing core
        var coreGO = new GameObject("Core");
        coreGO.transform.SetParent(transform, false);
        coreGO.transform.localScale    = new Vector3(0.9f, 1.6f, 1f);
        glowCore = coreGO.AddComponent<SpriteRenderer>();
        glowCore.sprite       = SpriteFactory.Square(col);
        glowCore.color        = new Color(col.r, col.g, col.b, 0.15f);
        glowCore.sortingOrder = 3;

        // Centre icon
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(transform, false);
        iconGO.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        iconGO.transform.localScale    = Vector3.one * 0.014f;
        var ic = iconGO.AddComponent<Canvas>();
        ic.renderMode   = RenderMode.WorldSpace;
        ic.sortingOrder = 15;
        iconGO.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 60f);
        var iT       = iconGO.AddComponent<Text>();
        iT.text      = DoorIcon();
        iT.fontSize  = 36;
        iT.font      = GetFont();
        iT.color     = col;
        iT.alignment = TextAnchor.MiddleCenter;
        var ir = iT.rectTransform;
        ir.anchorMin = Vector2.zero;
        ir.anchorMax = Vector2.one;
        ir.offsetMin = ir.offsetMax = Vector2.zero;

        // Floating label
        if (RoomType == DoorRoomType.Path && PathBoss != null)
            BuildPathLabel(col);
        else
            BuildStandardLabel(col);

        // Trigger collider
        var bc = gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size      = new Vector2(1.1f, 2.0f);

        StartCoroutine(OpenAnimation());
    }

    void BuildStandardLabel(Color col)
    {
        labelRoot = MakeLabelCanvas("DoorLabel", new Vector3(0f, 1.8f, 0f),
            new Vector2(200f, 70f), scale: 0.012f);

        MakeText(labelRoot.transform, DoorLabel(),
            18, FontStyle.Bold, col,
            new Vector2(0f, 0.5f), Vector2.one);

        MakeText(labelRoot.transform, "[ WALK THROUGH ]",
            11, FontStyle.Normal, new Color(0.6f, 0.6f, 0.6f),
            Vector2.zero, new Vector2(1f, 0.5f));
    }

    void BuildPathLabel(Color col)
    {
        // Taller canvas — boss name + material + description + hint
        labelRoot = MakeLabelCanvas("PathLabel", new Vector3(0f, 2.9f, 0f),
            new Vector2(290f, 170f), scale: 0.012f);

        // Boss name (large, bold, boss colour)
        MakeText(labelRoot.transform,
            PathBoss.Name.ToUpper(),
            26, FontStyle.Bold, col,
            new Vector2(0f, 0.74f), Vector2.one);

        // Material reward (gold)
        MakeText(labelRoot.transform,
            $"{PathBoss.MaterialReward}x  {PathBoss.MaterialName}",
            17, FontStyle.Normal, new Color(1f, 0.85f, 0.2f),
            new Vector2(0f, 0.52f), new Vector2(1f, 0.76f));

        // Description (gray)
        MakeText(labelRoot.transform,
            PathBoss.Description,
            13, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f),
            new Vector2(0f, 0.22f), new Vector2(1f, 0.54f));

        // Walk-through hint (dim)
        MakeText(labelRoot.transform,
            "[ WALK THROUGH ]",
            11, FontStyle.Normal, new Color(0.48f, 0.48f, 0.48f),
            Vector2.zero, new Vector2(1f, 0.24f));
    }

    // ── Update ─────────────────────────────────────────────────────────────

    void Update()
    {
        pulseTimer += Time.deltaTime * 1.6f;
        bobTimer   += Time.deltaTime * 2.0f;

        // Pulse glow
        if (glowCore)
        {
            float a = 0.12f + Mathf.Sin(pulseTimer) * 0.07f;
            var c = glowCore.color; c.a = a; glowCore.color = c;
        }

        // Bob label
        float labelY = (RoomType == DoorRoomType.Path) ? 2.9f : 1.8f;
        if (labelRoot)
            labelRoot.transform.localPosition = new Vector3(0f, labelY + Mathf.Sin(bobTimer) * 0.08f, 0f);

        // Fade label by proximity
        float dist = PlayerDist();
        if (labelGroup)
            labelGroup.alpha = Mathf.MoveTowards(labelGroup.alpha,
                dist < LABEL_DIST ? 1f : 0f, 5f * Time.deltaTime);

        if (!entered && dist < ENTER_DIST)
            Enter();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!entered && other.GetComponent<PlayerController>() != null)
            Enter();
    }

    void Enter()
    {
        if (entered) return;
        entered = true;
        StartCoroutine(EnterTransition());
    }

    IEnumerator EnterTransition()
    {
        // White screen flash
        var flashGO = new GameObject("DoorFlash");
        var canvas  = flashGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var img = flashGO.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        for (float t = 0; t < 0.25f; t += Time.deltaTime)
        {
            img.color = new Color(1f, 1f, 1f, t / 0.25f);
            yield return null;
        }

        if (RoomType == DoorRoomType.Path)
        {
            // Destroy the other path door so only one choice is taken
            foreach (var d in FindObjectsByType<RoomDoor>(FindObjectsSortMode.None))
                if (d != this && d.RoomType == DoorRoomType.Path)
                    Destroy(d.gameObject);

            GameManager.Instance?.PathChosen(goLeft);
        }
        else
        {
            WaveManager.Instance?.OnDoorEntered(RoomType);
        }

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            img.color = new Color(1f, 1f, 1f, 1f - t / 0.3f);
            yield return null;
        }

        Destroy(flashGO);
        Destroy(gameObject);
    }

    IEnumerator OpenAnimation()
    {
        transform.localScale = new Vector3(1f, 0f, 1f);
        float dur = 0.35f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float ease = 1f - Mathf.Pow(1f - t / dur, 3f);
            transform.localScale = new Vector3(1f, ease, 1f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    float PlayerDist()
    {
        var pc = PlayerController.Instance;
        return pc ? Vector2.Distance(transform.position, pc.transform.position) : 999f;
    }

    Color DoorColor()
    {
        if (RoomType == DoorRoomType.Path && PathBoss != null)
            return PathBoss.ThemeColor;
        return RoomType switch {
            DoorRoomType.Boss => new Color(1f,   0.25f, 0.25f),
            DoorRoomType.Shop => new Color(1f,   0.85f, 0.1f),
            DoorRoomType.Item => new Color(0.6f, 0.3f,  1f),
            _                 => new Color(0.2f, 0.7f,  1f),
        };
    }

    string DoorIcon() => RoomType switch {
        DoorRoomType.Shop => "$",
        DoorRoomType.Item => "?",
        DoorRoomType.Combat => ">",
        _ => "!",   // Boss and Path both get "!"
    };

    string DoorLabel() => RoomType switch {
        DoorRoomType.Boss => "BOSS FLOOR",
        DoorRoomType.Shop => "SHOP",
        DoorRoomType.Item => "ITEM ROOM",
        DoorRoomType.Path => PathBoss != null ? PathBoss.Name.ToUpper() : "PATH",
        _                 => "NEXT ROOM",
    };

    void MakeRect(string n, Color col, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale    = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square(col);
        sr.color  = col;
        sr.sortingOrder = 4;
    }

    GameObject MakeLabelCanvas(string name, Vector3 localPos, Vector2 size, float scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = Vector3.one * scale;
        var cv = go.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.WorldSpace;
        cv.sortingOrder = 20;
        go.GetComponent<RectTransform>().sizeDelta = size;
        labelGroup       = go.AddComponent<CanvasGroup>();
        labelGroup.alpha = 0f;
        return go;
    }

    Text MakeText(Transform parent, string content, int fontSize,
        FontStyle style, Color col, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(4f, 2f); rt.offsetMax = new Vector2(-4f, -2f);
        return t;
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
