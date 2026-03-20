using UnityEngine;

/// <summary>
/// Manages the gold economy for the run and across runs.
///
/// RunGold   — earned this run; carries between waves within a run.
/// BankedGold — stored in PlayerPrefs across runs.
///              At run start a portion (BankReturnRate) is converted back to RunGold.
/// </summary>
public class GoldSystem : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static GoldSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPrefs();
    }

    // ── Fields ─────────────────────────────────────────────────────────────
    const string PREFS_KEY = "BankedGold";

    public int   RunGold       { get; private set; }
    public int   BankedGold    { get; private set; }
    public float BankReturnRate = 0.125f;   // 1/8 of banked gold returned at run start

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Call at the start of a new run.
    /// Converts a portion of BankedGold back into RunGold, then clears BankedGold.
    /// </summary>
    public void Initialize()
    {
        LoadPrefs();

        int returned = Mathf.FloorToInt(BankedGold * BankReturnRate);
        RunGold    = returned;
        BankedGold = 0;
        SavePrefs();

        Debug.Log($"[GoldSystem] Run started. Banked gold returned: {returned}. RunGold = {RunGold}.");
        NotifyHUD();
    }

    /// <summary>Add gold earned during gameplay (enemy drops, pickups, etc.).</summary>
    public void EarnGold(int amount)
    {
        if (amount <= 0) return;
        RunGold += amount;
        NotifyHUD();
    }

    /// <summary>Subtract gold for a purchase. Clamps at 0.</summary>
    public void SpendGold(int amount)
    {
        if (amount <= 0) return;
        RunGold = Mathf.Max(0, RunGold - amount);
        NotifyHUD();
    }

    /// <summary>Returns true if the player can afford the given cost.</summary>
    public bool CanAfford(int amount) => RunGold >= amount;

    /// <summary>
    /// Bank all current RunGold after a boss.
    /// RunGold resets to 0; BankedGold accumulates for the next run's start bonus.
    /// </summary>
    public void BankAfterBoss()
    {
        BankedGold += RunGold;
        Debug.Log($"[GoldSystem] Banked {RunGold} gold. Total banked: {BankedGold}.");
        RunGold = 0;
        SavePrefs();
        NotifyHUD();
    }

    /// <summary>Returns the amount currently stored across runs.</summary>
    public int GetBankedGold() => BankedGold;

    // ── Persistence ────────────────────────────────────────────────────────

    public void SavePrefs()
    {
        PlayerPrefs.SetInt(PREFS_KEY, BankedGold);
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        BankedGold = PlayerPrefs.GetInt(PREFS_KEY, 0);
    }

    // ── Internal ───────────────────────────────────────────────────────────

    void NotifyHUD()
    {
        HUDManager.Instance?.SetGold(RunGold);
    }
}
