using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    Image healthFill;
    Image dashFill;
    Text waveText;
    Text scoreText;

    // Individual stat value texts (colored)
    Text statDmg, statFire, statSize, statSpd, statDash;
    // Badge texts for special unlocks
    Text badgePierce, badgeSplit, badgeShield;

    // For flash coroutine
    Dictionary<Text, Coroutine> flashRoutines = new Dictionary<Text, Coroutine>();

    void Awake() => Instance = this;

    public void Build(Canvas canvas)
    {
        var root = canvas.gameObject;

        // ── Stats Panel (bottom left, above bars) ──────────────────────────
        // Background panel
        var statsBg = MakeImage(root.transform, new Color(0.05f, 0.05f, 0.1f, 0.9f),
            new Vector2(340, 44), new Vector2(14, 56));
        statsBg.rectTransform.anchorMin = new Vector2(0, 0);
        statsBg.rectTransform.anchorMax = new Vector2(0, 0);
        statsBg.rectTransform.pivot     = new Vector2(0, 0);

        // 5 stat columns inside the panel
        float colW = 62f;
        float startX = 6f;
        float labelY = 28f;
        float valueY = 8f;

        statDmg  = MakeStatColumn(statsBg.transform, "DMG",  startX + colW * 0, labelY, valueY);
        statFire = MakeStatColumn(statsBg.transform, "FIRE", startX + colW * 1, labelY, valueY);
        statSize = MakeStatColumn(statsBg.transform, "SIZE", startX + colW * 2, labelY, valueY);
        statSpd  = MakeStatColumn(statsBg.transform, "SPD",  startX + colW * 3, labelY, valueY);
        statDash = MakeStatColumn(statsBg.transform, "DASH", startX + colW * 4, labelY, valueY);

        // Special badge row — small tags that appear when unlocked
        badgePierce = MakeBadge(statsBg.transform, "PIERCE", new Vector2(210, 10));
        badgeSplit  = MakeBadge(statsBg.transform, "SPLIT",  new Vector2(262, 10));
        badgeShield = MakeBadge(statsBg.transform, "SHIELD", new Vector2(316, 10));

        // ── Health Bar ─────────────────────────────────────────────────────
        var hbBg = MakeImage(root.transform, new Color(0.1f, 0.1f, 0.1f, 0.9f),
            new Vector2(340, 22), new Vector2(14, 30));
        hbBg.rectTransform.anchorMin = new Vector2(0, 0);
        hbBg.rectTransform.anchorMax = new Vector2(0, 0);
        hbBg.rectTransform.pivot     = new Vector2(0, 0);

        var hbFillGO = new GameObject("HealthFill");
        hbFillGO.transform.SetParent(hbBg.transform, false);
        healthFill = hbFillGO.AddComponent<Image>();
        healthFill.sprite = SpriteFactory.Square(new Color(0.15f, 0.85f, 0.2f));
        var hbFillRect = healthFill.rectTransform;
        hbFillRect.anchorMin = Vector2.zero;
        hbFillRect.anchorMax = Vector2.one;
        hbFillRect.offsetMin = Vector2.zero;
        hbFillRect.offsetMax = Vector2.zero;

        var hpLabel = MakeText(hbBg.transform, "HP", 11, Vector2.zero);
        hpLabel.rectTransform.anchorMin = Vector2.zero;
        hpLabel.rectTransform.anchorMax = Vector2.one;
        hpLabel.rectTransform.offsetMin = Vector2.zero;
        hpLabel.rectTransform.offsetMax = Vector2.zero;
        hpLabel.alignment = TextAnchor.MiddleCenter;
        hpLabel.color = Color.white;

        // ── Dash Bar ───────────────────────────────────────────────────────
        var dashBg = MakeImage(root.transform, new Color(0.1f, 0.1f, 0.1f, 0.9f),
            new Vector2(340, 12), new Vector2(14, 14));
        dashBg.rectTransform.anchorMin = new Vector2(0, 0);
        dashBg.rectTransform.anchorMax = new Vector2(0, 0);
        dashBg.rectTransform.pivot     = new Vector2(0, 0);

        var dashFillGO = new GameObject("DashFill");
        dashFillGO.transform.SetParent(dashBg.transform, false);
        dashFill = dashFillGO.AddComponent<Image>();
        dashFill.sprite = SpriteFactory.Square(new Color(0.1f, 0.6f, 1f));
        var dashRect = dashFill.rectTransform;
        dashRect.anchorMin = Vector2.zero;
        dashRect.anchorMax = Vector2.one;
        dashRect.offsetMin = Vector2.zero;
        dashRect.offsetMax = Vector2.zero;

        var dashLabel = MakeText(dashBg.transform, "DASH", 8, Vector2.zero);
        dashLabel.rectTransform.anchorMin = Vector2.zero;
        dashLabel.rectTransform.anchorMax = Vector2.one;
        dashLabel.rectTransform.offsetMin = Vector2.zero;
        dashLabel.rectTransform.offsetMax = Vector2.zero;
        dashLabel.alignment = TextAnchor.MiddleCenter;
        dashLabel.color = Color.white;

        // ── Wave (top center) ──────────────────────────────────────────────
        waveText = MakeText(root.transform, "WAVE 1", 22, new Vector2(0, -14));
        waveText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        waveText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        waveText.rectTransform.pivot     = new Vector2(0.5f, 1f);
        waveText.rectTransform.sizeDelta = new Vector2(400, 34);
        waveText.alignment = TextAnchor.UpperCenter;
        waveText.color = new Color(0.9f, 0.9f, 1f);

        // ── Score (top right) ──────────────────────────────────────────────
        scoreText = MakeText(root.transform, "SCORE: 0", 16, new Vector2(-14, -14));
        scoreText.rectTransform.anchorMin = new Vector2(1f, 1f);
        scoreText.rectTransform.anchorMax = new Vector2(1f, 1f);
        scoreText.rectTransform.pivot     = new Vector2(1f, 1f);
        scoreText.rectTransform.sizeDelta = new Vector2(220, 26);
        scoreText.alignment = TextAnchor.UpperRight;
        scoreText.color = new Color(1f, 0.85f, 0.2f);

        // Init stats display
        UpdateStats();
    }

    // ── Public Update Methods ─────────────────────────────────────────────

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
        dashFill.color = frac >= 1f
            ? new Color(0.1f, 0.6f, 1f)
            : new Color(0.1f, 0.3f, 0.6f);
    }

    public void SetWave(int wave)
    {
        if (!waveText) return;
        waveText.text = wave % 10 == 0
            ? $"!! BOSS WAVE {wave} !!"
            : $"WAVE  {wave}";
        waveText.color = wave % 10 == 0
            ? new Color(1f, 0.3f, 0.3f)
            : new Color(0.9f, 0.9f, 1f);
    }

    public void SetScore(int score)
    {
        if (scoreText) scoreText.text = $"SCORE: {score:N0}";
    }

    public void UpdateStats()
    {
        if (!PlayerStats.Instance) return;
        var s = PlayerStats.Instance;

        SetStat(statDmg,  s.DamageMult);
        SetStat(statFire, s.FireRateMult);
        SetStat(statSize, s.BulletSizeMult);
        SetStat(statSpd,  s.MoveMult);
        SetStat(statDash, 2f - s.DashCooldownMult); // inverted: lower CD = higher shown value

        // Badges
        if (badgePierce) badgePierce.gameObject.SetActive(s.Piercing);
        if (badgeSplit)  badgeSplit.gameObject.SetActive(s.SplitShot);
        if (badgeShield) badgeShield.gameObject.SetActive(s.HasShield);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    // Creates a label + value pair and returns the value Text for updating
    Text MakeStatColumn(Transform parent, string label, float x, float labelY, float valueY)
    {
        var labelT = MakeText(parent, label, 9, new Vector2(x, labelY));
        labelT.rectTransform.sizeDelta = new Vector2(58, 16);
        labelT.alignment = TextAnchor.MiddleCenter;
        labelT.color = new Color(0.55f, 0.55f, 0.65f);

        var valueT = MakeText(parent, "1.0x", 13, new Vector2(x, valueY));
        valueT.rectTransform.sizeDelta = new Vector2(58, 18);
        valueT.alignment = TextAnchor.MiddleCenter;
        valueT.color = Color.white;
        return valueT;
    }

    void SetStat(Text t, float mult)
    {
        if (!t) return;
        string prev = t.text;
        t.text = $"{mult:F1}x";
        t.color = StatColor(mult);

        // Flash if changed
        if (prev != t.text)
            FlashStat(t);
    }

    // White → green → blue → gold as multiplier climbs
    Color StatColor(float mult)
    {
        if (mult >= 4f)  return new Color(1.0f, 0.75f, 0.1f);  // gold
        if (mult >= 3f)  return new Color(0.4f, 0.6f,  1.0f);  // blue
        if (mult >= 2f)  return new Color(0.2f, 0.95f, 0.4f);  // green
        if (mult >= 1.5f)return new Color(0.7f, 1.0f,  0.4f);  // yellow-green
        return new Color(0.85f, 0.85f, 0.85f);                  // white/gray
    }

    void FlashStat(Text t)
    {
        if (flashRoutines.ContainsKey(t) && flashRoutines[t] != null)
            StopCoroutine(flashRoutines[t]);
        flashRoutines[t] = StartCoroutine(FlashRoutine(t));
    }

    IEnumerator FlashRoutine(Text t)
    {
        Color target = t.color;
        t.color = Color.white;
        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.unscaledDeltaTime;
            t.color = Color.Lerp(Color.white, target, elapsed / 0.35f);
            yield return null;
        }
        t.color = target;
    }

    Text MakeBadge(Transform parent, string label, Vector2 pos)
    {
        var t = MakeText(parent, label, 8, pos);
        t.rectTransform.sizeDelta = new Vector2(44, 14);
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(1f, 0.75f, 0.1f);
        t.gameObject.SetActive(false);
        return t;
    }

    Image MakeImage(Transform parent, Color col, Vector2 size, Vector2 pos)
    {
        var go   = new GameObject("UIPanel");
        go.transform.SetParent(parent, false);
        var img  = go.AddComponent<Image>();
        img.color  = col;
        img.sprite = SpriteFactory.Square(Color.white);
        var rect = img.rectTransform;
        rect.sizeDelta        = size;
        rect.anchoredPosition = pos;
        return img;
    }

    Text MakeText(Transform parent, string content, int size, Vector2 pos)
    {
        var go = new GameObject("UIText");
        go.transform.SetParent(parent, false);
        var t  = go.AddComponent<Text>();
        t.text            = content;
        t.fontSize        = size;
        t.color           = Color.white;
        t.font            = GetFont();
        t.supportRichText = true;
        var rect = t.rectTransform;
        rect.anchoredPosition = pos;
        rect.sizeDelta        = new Vector2(200, 24);
        return t;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
