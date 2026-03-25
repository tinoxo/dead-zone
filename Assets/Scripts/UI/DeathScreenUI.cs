using UnityEngine;
using UnityEngine.UI;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; }

    GameObject overlay;
    Text statsText;
    Text titleText;
    Text subtitleText;
    Image bgImage;
    Image btnImage;

    void Awake() => Instance = this;

    public void Build(Canvas canvas)
    {
        overlay = new GameObject("DeathOverlay");
        overlay.transform.SetParent(canvas.transform, false);

        // Full-screen dark panel
        var bg    = overlay.AddComponent<Image>();
        bg.color  = new Color(0f, 0f, 0f, 0.9f);
        bg.sprite = SpriteFactory.Square(Color.white);
        bgImage   = bg;
        var bgR   = bg.rectTransform;
        bgR.anchorMin = Vector2.zero;
        bgR.anchorMax = Vector2.one;
        bgR.offsetMin = Vector2.zero;
        bgR.offsetMax = Vector2.zero;

        // Title
        titleText = AddText(overlay.transform, "YOU DIED", 52,
            new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.82f), new Color(0.9f, 0.1f, 0.1f));

        // Subtitle
        subtitleText = AddText(overlay.transform, "DEAD ZONE", 22,
            new Vector2(0.2f, 0.56f), new Vector2(0.8f, 0.65f), new Color(0.6f, 0.6f, 0.7f));

        // Stats
        var statsGO  = new GameObject("Stats");
        statsGO.transform.SetParent(overlay.transform, false);
        statsText    = statsGO.AddComponent<Text>();
        statsText.font      = GetFont();
        statsText.fontSize  = 20;
        statsText.color     = new Color(0.85f, 0.85f, 0.9f);
        statsText.alignment = TextAnchor.MiddleCenter;
        var sRect    = statsText.rectTransform;
        sRect.anchorMin = new Vector2(0.2f, 0.36f);
        sRect.anchorMax = new Vector2(0.8f, 0.56f);
        sRect.offsetMin = Vector2.zero;
        sRect.offsetMax = Vector2.zero;

        // Restart button
        var btnGO   = new GameObject("RestartBtn");
        btnGO.transform.SetParent(overlay.transform, false);
        var btnImg  = btnGO.AddComponent<Image>();
        btnImg.color  = new Color(0.7f, 0.1f, 0.1f);
        btnImg.sprite = SpriteFactory.Square(Color.white);
        var btnRect = btnImg.rectTransform;
        btnRect.anchorMin        = new Vector2(0.35f, 0.2f);
        btnRect.anchorMax        = new Vector2(0.65f, 0.32f);
        btnRect.offsetMin        = Vector2.zero;
        btnRect.offsetMax        = Vector2.zero;

        btnImage      = btnImg;
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => GameManager.Instance?.Restart());

        AddText(btnGO.transform, "TRY AGAIN", 20,
            Vector2.zero, Vector2.one, Color.white);

        overlay.SetActive(false);
    }

    public void Show(int wave, int score, int kills)
    {
        overlay.SetActive(true);
        // Death colours
        if (bgImage)      bgImage.color      = new Color(0f, 0f, 0f, 0.90f);
        if (titleText)    { titleText.text    = "YOU DIED";  titleText.color    = new Color(0.9f, 0.1f, 0.1f); }
        if (subtitleText) { subtitleText.text = "DEAD ZONE"; subtitleText.color = new Color(0.6f, 0.6f, 0.7f); }
        if (btnImage)     btnImage.color      = new Color(0.7f, 0.1f, 0.1f);
        if (statsText)
            statsText.text = $"WAVE REACHED:  {wave}\n\nSCORE:  {score:N0}\n\nENEMIES KILLED:  {kills}";
    }

    public void ShowWin(int wave, int score, int kills)
    {
        overlay.SetActive(true);
        // Win colours — gold/teal theme
        if (bgImage)      bgImage.color      = new Color(0.02f, 0.06f, 0.04f, 0.93f);
        if (titleText)    { titleText.text    = "YOU WIN!";      titleText.color    = new Color(1f, 0.88f, 0.1f); }
        if (subtitleText) { subtitleText.text = "OMEGA DEFEATED"; subtitleText.color = new Color(0.3f, 0.9f, 0.6f); }
        if (btnImage)     btnImage.color      = new Color(0.1f, 0.55f, 0.25f);
        if (statsText)
            statsText.text = $"WAVES CLEARED:  {wave}\n\nFINAL SCORE:  {score:N0}\n\nENEMIES KILLED:  {kills}";
    }

    Text AddText(Transform parent, string content, int size, Vector2 aMin, Vector2 aMax, Color col)
    {
        var go   = new GameObject("T");
        go.transform.SetParent(parent, false);
        var t    = go.AddComponent<Text>();
        t.text      = content;
        t.fontSize  = size;
        t.font      = GetFont();
        t.color     = col;
        t.alignment = TextAnchor.MiddleCenter;
        var rect = t.rectTransform;
        rect.anchorMin = aMin;
        rect.anchorMax = aMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return t;
    }

    Font GetFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
