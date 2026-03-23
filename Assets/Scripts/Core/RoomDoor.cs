using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DoorRoomType { Combat, Boss, Shop, Item }

/// <summary>
/// A door that appears after a wave is cleared.
/// Player walks into it to transition to the next room.
/// </summary>
public class RoomDoor : MonoBehaviour
{
    public DoorRoomType RoomType;

    SpriteRenderer  bodyLeft, bodyRight, topBeam, bottomBeam;
    SpriteRenderer  glowCore;
    GameObject      labelRoot;
    CanvasGroup     labelGroup;
    Text            roomTypeText;
    Text            hintText;

    bool entered;
    float pulseTimer;
    float bobTimer;

    const float ENTER_DIST = 0.8f;
    const float LABEL_DIST = 5f;

    // ── Spawn helpers called by WaveManager ────────────────────────────────
    public static RoomDoor Spawn(Vector2 position, DoorRoomType type)
    {
        var go   = new GameObject("RoomDoor");
        go.transform.position = position;
        var door = go.AddComponent<RoomDoor>();
        door.RoomType = type;
        door.Build();
        return door;
    }

    void Build()
    {
        Color col  = DoorColor();
        Color dark = col * 0.3f;
        dark.a = 1f;

        // ── Door frame (two vertical pillars + top & bottom beams) ─────────
        bodyLeft   = MakeRect("PillarL",  col * 0.7f, new Vector2(-0.55f, 0f),  new Vector2(0.18f, 2.0f));
        bodyRight  = MakeRect("PillarR",  col * 0.7f, new Vector2( 0.55f, 0f),  new Vector2(0.18f, 2.0f));
        topBeam    = MakeRect("BeamTop",  col,         new Vector2(0f,  1.0f),  new Vector2(1.28f, 0.18f));
        bottomBeam = MakeRect("BeamBot",  col,         new Vector2(0f, -1.0f), new Vector2(1.28f, 0.18f));

        // ── Glowing core (door interior) ────────────────────────────────────
        var coreGO  = new GameObject("Core");
        coreGO.transform.SetParent(transform, false);
        coreGO.transform.localPosition = Vector3.zero;
        coreGO.transform.localScale    = new Vector3(0.9f, 1.6f, 1f);
        glowCore = coreGO.AddComponent<SpriteRenderer>();
        glowCore.sprite       = SpriteFactory.Square(col);
        glowCore.color        = new Color(col.r, col.g, col.b, 0.15f);
        glowCore.sortingOrder = 3;

        // ── Room type icon text ─────────────────────────────────────────────
        var iconGO = new GameObject("DoorIcon");
        iconGO.transform.SetParent(transform, false);
        iconGO.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        iconGO.transform.localScale    = Vector3.one * 0.014f;

        var iconCanvas = iconGO.AddComponent<Canvas>();
        iconCanvas.renderMode  = RenderMode.WorldSpace;
        iconCanvas.sortingOrder = 15;
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(100f, 60f);

        var iconText      = iconGO.AddComponent<Text>();
        iconText.text     = DoorIcon();
        iconText.fontSize = 36;
        iconText.font     = GetFont();
        iconText.color    = col;
        iconText.alignment = TextAnchor.MiddleCenter;
        var ir = iconText.rectTransform;
        ir.anchorMin = Vector2.zero;
        ir.anchorMax = Vector2.one;
        ir.offsetMin = Vector2.zero;
        ir.offsetMax = Vector2.zero;

        // ── Floating label (shows when close) ──────────────────────────────
        labelRoot = new GameObject("DoorLabel");
        labelRoot.transform.SetParent(transform, false);
        labelRoot.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        labelRoot.transform.localScale    = Vector3.one * 0.012f;

        var lCanvas = labelRoot.AddComponent<Canvas>();
        lCanvas.renderMode  = RenderMode.WorldSpace;
        lCanvas.sortingOrder = 20;
        var lRect = labelRoot.GetComponent<RectTransform>();
        lRect.sizeDelta = new Vector2(200f, 70f);

        labelGroup       = labelRoot.AddComponent<CanvasGroup>();
        labelGroup.alpha = 0f;

        roomTypeText = MakeLabelText(labelRoot.transform, DoorLabel(),
            18, FontStyle.Bold, col,
            new Vector2(0f, 0.5f), Vector2.one);

        hintText = MakeLabelText(labelRoot.transform, "[ WALK THROUGH ]",
            11, FontStyle.Normal, new Color(0.65f, 0.65f, 0.65f),
            Vector2.zero, new Vector2(1f, 0.5f));

        // ── Trigger collider ────────────────────────────────────────────────
        var col2 = gameObject.AddComponent<BoxCollider2D>();
        col2.isTrigger = true;
        col2.size      = new Vector2(1.1f, 2.0f);

        // Entrance animation
        StartCoroutine(OpenAnimation());
    }

    // ── Update ─────────────────────────────────────────────────────────────
    void Update()
    {
        pulseTimer += Time.deltaTime * 1.6f;
        bobTimer   += Time.deltaTime * 2.0f;

        // Pulse glow core alpha
        float pulse = 0.12f + Mathf.Sin(pulseTimer) * 0.07f;
        if (glowCore) glowCore.color = new Color(glowCore.color.r, glowCore.color.g, glowCore.color.b, pulse);

        // Bob the label
        float bob = Mathf.Sin(bobTimer) * 0.08f;
        if (labelRoot) labelRoot.transform.localPosition = new Vector3(0f, 1.8f + bob, 0f);

        // Fade label by proximity
        float dist        = PlayerDist();
        float targetAlpha = dist < LABEL_DIST ? 1f : 0f;
        if (labelGroup) labelGroup.alpha = Mathf.MoveTowards(labelGroup.alpha, targetAlpha, 5f * Time.deltaTime);

        // Enter on contact
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
        // Screen flash
        var flashGO = new GameObject("DoorFlash");
        var canvas  = flashGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var img   = flashGO.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f);
        var rt    = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Fade to white
        for (float t = 0; t < 0.25f; t += Time.deltaTime)
        {
            img.color = new Color(1f, 1f, 1f, t / 0.25f);
            yield return null;
        }

        // Trigger next room
        WaveManager.Instance?.OnDoorEntered(RoomType);

        // Fade back in
        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            img.color = new Color(1f, 1f, 1f, 1f - t / 0.3f);
            yield return null;
        }

        Destroy(flashGO);
        Destroy(gameObject);
    }

    // ── Door open animation (scales in from 0) ─────────────────────────────
    IEnumerator OpenAnimation()
    {
        transform.localScale = new Vector3(1f, 0f, 1f);
        float dur = 0.35f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float ease = 1f - Mathf.Pow(1f - t / dur, 3f); // ease out cubic
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

    Color DoorColor() => RoomType switch {
        DoorRoomType.Boss   => new Color(1f,   0.25f, 0.25f),
        DoorRoomType.Shop   => new Color(1f,   0.85f, 0.1f),
        DoorRoomType.Item   => new Color(0.6f, 0.3f,  1f),
        _                   => new Color(0.2f, 0.7f,  1f),   // Combat
    };

    string DoorIcon() => RoomType switch {
        DoorRoomType.Boss   => "!",
        DoorRoomType.Shop   => "$",
        DoorRoomType.Item   => "?",
        _                   => ">",
    };

    string DoorLabel() => RoomType switch {
        DoorRoomType.Boss   => "BOSS FLOOR",
        DoorRoomType.Shop   => "SHOP",
        DoorRoomType.Item   => "ITEM ROOM",
        _                   => "NEXT ROOM",
    };

    SpriteRenderer MakeRect(string name, Color col, Vector2 localPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = new Vector3(size.x, size.y, 1f);
        var sr   = go.AddComponent<SpriteRenderer>();
        sr.sprite       = SpriteFactory.Square(col);
        sr.color        = col;
        sr.sortingOrder = 4;
        return sr;
    }

    Text MakeLabelText(Transform parent, string content, int size,
        FontStyle style, Color col, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject("LT");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = col;
        t.font      = GetFont();
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        var rt = t.rectTransform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(4f, 2f);
        rt.offsetMax = new Vector2(-4f, -2f);
        return t;
    }

    Font GetFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
