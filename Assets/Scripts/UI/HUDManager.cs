using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    // Health bar
    Image healthFill;

    // Dash bar
    Image dashFill;

    // Text elements
    Text waveText;
    Text scoreText;
    Text statsText;

    void Awake() => Instance = this;

    // Called by SceneSetup to build the HUD inside the given canvas
    public void Build(Canvas canvas)
    {
        var root = canvas.gameObject;

        // ── Health Bar ─────────────────────────────────────────────────────
        var hbBg = MakeImage(root.transform, new Color(0.1f, 0.1f, 0.1f, 0.85f),
            new Vector2(220, 22), new Vector2(-10, 14), TextAnchor.LowerLeft);
        hbBg.rectTransform.anchorMin = new Vector2(0, 0);
        hbBg.rectTransform.anchorMax = new Vector2(0, 0);
        hbBg.rectTransform.pivot     = new Vector2(0, 0);
        hbBg.rectTransform.anchoredPosition = new Vector2(14, 14);

        var hbFillGO = new GameObject("HealthFill");
        hbFillGO.transform.SetParent(hbBg.transform, false);
        healthFill = hbFillGO.AddComponent<Image>();
        healthFill.sprite = SpriteFactory.Square(new Color(0.15f, 0.85f, 0.2f));
        var hbFillRect = healthFill.GetComponent<RectTransform>();
        hbFillRect.anchorMin = Vector2.zero;
        hbFillRect.anchorMax = Vector2.one;
        hbFillRect.offsetMin = Vector2.zero;
        hbFillRect.offsetMax = Vector2.zero;

        // HP label
        var hpLabel = MakeText(hbBg.transform, "HP", 11, new Vector2(0, 0));
        hpLabel.rectTransform.anchorMin = Vector2.zero;
        hpLabel.rectTransform.anchorMax = Vector2.one;
        hpLabel.rectTransform.offsetMin = Vector2.zero;
        hpLabel.rectTransform.offsetMax = Vector2.zero;
        hpLabel.alignment = TextAnchor.MiddleCenter;
        hpLabel.color = Color.white;

        // ── Dash Cooldown Bar ──────────────────────────────────────────────
        var dashBg = MakeImage(root.transform, new Color(0.1f, 0.1f, 0.1f, 0.85f),
            new Vector2(220, 10), Vector2.zero, TextAnchor.LowerLeft);
        dashBg.rectTransform.anchorMin = new Vector2(0, 0);
        dashBg.rectTransform.anchorMax = new Vector2(0, 0);
        dashBg.rectTransform.pivot     = new Vector2(0, 0);
        dashBg.rectTransform.anchoredPosition = new Vector2(14, 40);

        var dashFillGO = new GameObject("DashFill");
        dashFillGO.transform.SetParent(dashBg.transform, false);
        dashFill = dashFillGO.AddComponent<Image>();
        dashFill.sprite = SpriteFactory.Square(new Color(0.1f, 0.6f, 1f));
        var dashRect = dashFill.GetComponent<RectTransform>();
        dashRect.anchorMin = Vector2.zero;
        dashRect.anchorMax = Vector2.one;
        dashRect.offsetMin = Vector2.zero;
        dashRect.offsetMax = Vector2.zero;

        var dashLabel = MakeText(dashBg.transform, "DASH", 9, Vector2.zero);
        dashLabel.rectTransform.anchorMin = Vector2.zero;
        dashLabel.rectTransform.anchorMax = Vector2.one;
        dashLabel.rectTransform.offsetMin = Vector2.zero;
        dashLabel.rectTransform.offsetMax = Vector2.zero;
        dashLabel.alignment = TextAnchor.MiddleCenter;
        dashLabel.color = Color.white;

        // ── Stats line ─────────────────────────────────────────────────────
        statsText = MakeText(root.transform, "", 10, new Vector2(14, 56));
        statsText.rectTransform.anchorMin = new Vector2(0, 0);
        statsText.rectTransform.anchorMax = new Vector2(0, 0);
        statsText.rectTransform.pivot     = new Vector2(0, 0);
        statsText.rectTransform.sizeDelta = new Vector2(600, 20);
        statsText.color = new Color(0.7f, 0.7f, 0.7f);

        // ── Wave ───────────────────────────────────────────────────────────
        waveText = MakeText(root.transform, "WAVE 1", 20, new Vector2(-14, -14));
        waveText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        waveText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        waveText.rectTransform.pivot     = new Vector2(0.5f, 1f);
        waveText.rectTransform.sizeDelta = new Vector2(300, 30);
        waveText.alignment = TextAnchor.UpperCenter;
        waveText.color = new Color(0.9f, 0.9f, 1f);

        // ── Score ──────────────────────────────────────────────────────────
        scoreText = MakeText(root.transform, "SCORE: 0", 16, new Vector2(-14, -14));
        scoreText.rectTransform.anchorMin = new Vector2(1f, 1f);
        scoreText.rectTransform.anchorMax = new Vector2(1f, 1f);
        scoreText.rectTransform.pivot     = new Vector2(1f, 1f);
        scoreText.rectTransform.sizeDelta = new Vector2(220, 25);
        scoreText.alignment = TextAnchor.UpperRight;
        scoreText.color = new Color(1f, 0.85f, 0.2f);
    }

    // ── Update methods ────────────────────────────────────────────────────
    public void UpdateHealth(float frac)
    {
        if (!healthFill) return;
        healthFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(frac), 1f);
        healthFill.color = Color.Lerp(new Color(0.9f, 0.15f, 0.1f), new Color(0.15f, 0.85f, 0.2f), frac);
    }

    public void UpdateDash(float frac)
    {
        if (!dashFill) return;
        dashFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(frac), 1f);
    }

    public void SetWave(int wave)
    {
        if (waveText) waveText.text = wave % 10 == 0 ? $"⚠ BOSS WAVE {wave} ⚠" : $"WAVE  {wave}";
    }

    public void SetScore(int score)
    {
        if (scoreText) scoreText.text = $"SCORE: {score:N0}";
    }

    public void UpdateStats()
    {
        if (!statsText || !PlayerStats.Instance) return;
        var s = PlayerStats.Instance;
        statsText.text =
            $"DMG {s.DamageMult:F1}x  |  " +
            $"FIRE {s.FireRateMult:F1}x  |  " +
            $"SIZE {s.BulletSizeMult:F1}x  |  " +
            $"SPD {s.MoveMult:F1}x" +
            (s.Piercing  ? "  |  PIERCE" : "") +
            (s.SplitShot ? $"  |  SPLIT x{s.SplitCount * 2}" : "") +
            (s.HasShield ? "  |  SHIELD" : "");
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    Image MakeImage(Transform parent, Color col, Vector2 size, Vector2 pos, TextAnchor anchor)
    {
        var go   = new GameObject("UIPanel");
        go.transform.SetParent(parent, false);
        var img  = go.AddComponent<Image>();
        img.color = col;
        img.sprite = SpriteFactory.Square(Color.white);
        var rect = img.rectTransform;
        rect.sizeDelta        = size;
        rect.anchoredPosition = pos;
        return img;
    }

    Text MakeText(Transform parent, string content, int size, Vector2 pos)
    {
        var go   = new GameObject("UIText");
        go.transform.SetParent(parent, false);
        var t    = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.color     = Color.white;
        t.font      = GetFont();
        t.supportRichText = true;
        var rect = t.rectTransform;
        rect.anchoredPosition = pos;
        rect.sizeDelta        = new Vector2(200, 24);
        return t;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
