using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent game state — selected run options + cross-run unlock tracking.
/// </summary>
public static class GameData
{
    // ── Run-selection (set before loading game scene) ──────────────────────
    public static int  SelectedCharacterIndex    = 0;
    public static int  SelectedIslandIndex       = 0;
    public static bool IsShipRun                 = false;  // true while in the ship battle
    public static bool ShowIslandSelectOnLoad    = false;  // mainmenu reads this to skip to island select

    public static CharacterDefinition SelectedCharacter =>
        CharacterDefinition.All[SelectedCharacterIndex];

    // ── Character unlock tracking ──────────────────────────────────────────
    static HashSet<int> _unlockedChars;

    static HashSet<int> UnlockedChars
    {
        get { if (_unlockedChars == null) LoadUnlocks(); return _unlockedChars; }
    }

    public static bool IsCharacterUnlocked(int index)
    {
        if (index < 0 || index >= CharacterDefinition.All.Count) return false;
        if (CharacterDefinition.All[index].StarterCharacter) return true;
        return UnlockedChars.Contains(index);
    }

    public static void UnlockCharacter(int index)
    {
        UnlockedChars.Add(index);
        SaveUnlocks();
    }

    // ── Island clear tracking per character ────────────────────────────────
    /// <summary>Has charIndex cleared islandIndex at least once?</summary>
    public static bool HasClearedIslandWith(int charIndex, int islandIndex)
    {
        string val = PlayerPrefs.GetString($"iclr_{charIndex}", "");
        if (string.IsNullOrEmpty(val)) return false;
        foreach (var s in val.Split(','))
            if (s == islandIndex.ToString()) return true;
        return false;
    }

    public static void MarkIslandClearedWith(int charIndex, int islandIndex)
    {
        string key = $"iclr_{charIndex}";
        string val = PlayerPrefs.GetString(key, "");
        var set = new HashSet<string>(val.Split(','));
        set.Add(islandIndex.ToString());
        set.Remove("");
        PlayerPrefs.SetString(key, string.Join(",", set));
        PlayerPrefs.Save();
    }

    /// <summary>How many distinct islands has charIndex cleared?</summary>
    public static int IslandsClearedCount(int charIndex)
    {
        string val = PlayerPrefs.GetString($"iclr_{charIndex}", "");
        if (string.IsNullOrEmpty(val)) return 0;
        int count = 0;
        foreach (var s in val.Split(','))
            if (!string.IsNullOrEmpty(s)) count++;
        return count;
    }

    // ── Full-game clear tracking ───────────────────────────────────────────
    public static bool HasBeatenGameWith(int charIndex)
        => PlayerPrefs.GetInt($"beaten_{charIndex}", 0) == 1;

    public static void MarkGameBeatenWith(int charIndex)
    {
        PlayerPrefs.SetInt($"beaten_{charIndex}", 1);
        PlayerPrefs.Save();
    }

    // ── Milestone counters ────────────────────────────────────────────────
    public static int TotalKills
    {
        get => PlayerPrefs.GetInt("totalKills", 0);
        set { PlayerPrefs.SetInt("totalKills", value); PlayerPrefs.Save(); }
    }

    public static int TotalDeaths
    {
        get => PlayerPrefs.GetInt("totalDeaths", 0);
        set { PlayerPrefs.SetInt("totalDeaths", value); PlayerPrefs.Save(); }
    }

    public static long TotalGoldBanked
    {
        get => long.Parse(PlayerPrefs.GetString("totalGold", "0"));
        set { PlayerPrefs.SetString("totalGold", value.ToString()); PlayerPrefs.Save(); }
    }

    // ── Save / Load ───────────────────────────────────────────────────────
    static void LoadUnlocks()
    {
        _unlockedChars = new HashSet<int>();
        string val = PlayerPrefs.GetString("unlockedChars", "");
        if (string.IsNullOrEmpty(val)) return;
        foreach (var s in val.Split(','))
            if (int.TryParse(s, out int i)) _unlockedChars.Add(i);
    }

    static void SaveUnlocks()
    {
        PlayerPrefs.SetString("unlockedChars", string.Join(",", _unlockedChars));
        PlayerPrefs.Save();
    }

    /// <summary>Wipe all saved data. Debug only.</summary>
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        _unlockedChars = null;
    }
}
