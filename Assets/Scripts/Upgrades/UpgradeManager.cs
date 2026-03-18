using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // IDs of permanently unlocked upgrades (survive death, saved to PlayerPrefs)
    HashSet<string> permanentUnlocks = new HashSet<string>();

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        LoadUnlocks();
    }

    // ── Called by GameManager ─────────────────────────────────────────────
    public void ShowUpgradeSelection(bool bossKill)
    {
        var pool   = BuildPool(bossKill);
        var chosen = PickThree(pool);
        UpgradeUI.Instance?.Show(chosen);
    }

    public void ApplyUpgrade(UpgradeDefinition u)
    {
        permanentUnlocks.Add(u.ID);
        PlayerStats.Instance?.Apply(u);
        GameManager.Instance?.UpgradePicked();
    }

    public void SaveUnlocks()
    {
        PlayerPrefs.SetString("DZ_Unlocks", string.Join(",", permanentUnlocks));
        PlayerPrefs.Save();
    }

    // ── Persistence ───────────────────────────────────────────────────────
    void LoadUnlocks()
    {
        string saved = PlayerPrefs.GetString("DZ_Unlocks", "");
        if (!string.IsNullOrEmpty(saved))
            foreach (var id in saved.Split(','))
                permanentUnlocks.Add(id);
    }

    // ── Pool building ─────────────────────────────────────────────────────
    List<UpgradeDefinition> BuildPool(bool bossKill)
    {
        var all = UpgradeDefinition.All;

        if (bossKill)
        {
            // Boss: Uncommon / Rare / Legendary only
            return all.Where(u =>
                u.Tier == UpgradeTier.Uncommon ||
                u.Tier == UpgradeTier.Rare     ||
                u.Tier == UpgradeTier.Legendary).ToList();
        }
        else
        {
            // Normal wave: Common / Uncommon
            return all.Where(u =>
                u.Tier == UpgradeTier.Common ||
                u.Tier == UpgradeTier.Uncommon).ToList();
        }
    }

    List<UpgradeDefinition> PickThree(List<UpgradeDefinition> pool)
    {
        // Shuffle and take 3
        var result = new List<UpgradeDefinition>();
        var copy   = new List<UpgradeDefinition>(pool);

        // Bias toward unlocked upgrades appearing again (25% chance each to be included first)
        var unlocked = copy.Where(u => permanentUnlocks.Contains(u.ID)).ToList();
        foreach (var u in unlocked)
            if (Random.value < 0.25f && result.Count < 3) { result.Add(u); copy.Remove(u); }

        // Fill rest randomly
        while (result.Count < 3 && copy.Count > 0)
        {
            int idx = Random.Range(0, copy.Count);
            result.Add(copy[idx]);
            copy.RemoveAt(idx);
        }

        return result;
    }
}
