using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages boss-drop materials that persist across runs.
/// Stored in PlayerPrefs as JSON via a serializable wrapper.
/// </summary>
public class MaterialSystem : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static MaterialSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ── Data ───────────────────────────────────────────────────────────────
    const string PREFS_KEY = "MaterialData";

    public Dictionary<string, int> Materials { get; private set; }
        = new Dictionary<string, int>();

    // Serialisation shim — Unity's JsonUtility cannot handle Dictionary directly.
    [System.Serializable]
    class MaterialSaveData
    {
        public List<string> keys   = new List<string>();
        public List<int>    values = new List<int>();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Add an amount of the named material.</summary>
    public void AddMaterial(string name, int amount)
    {
        if (string.IsNullOrEmpty(name) || amount <= 0) return;

        if (Materials.ContainsKey(name))
            Materials[name] += amount;
        else
            Materials[name] = amount;

        Debug.Log($"[MaterialSystem] +{amount} {name}. Total: {Materials[name]}.");
        Save();
    }

    /// <summary>Returns the amount of the named material owned, or 0.</summary>
    public int GetMaterial(string name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        return Materials.TryGetValue(name, out int v) ? v : 0;
    }

    /// <summary>Returns true if at least <paramref name="amount"/> of the material is owned.</summary>
    public bool HasEnough(string name, int amount)
        => GetMaterial(name) >= amount;

    /// <summary>
    /// Deducts <paramref name="amount"/> of the named material.
    /// Clamps to 0; logs a warning if insufficient.
    /// </summary>
    public void SpendMaterial(string name, int amount)
    {
        if (string.IsNullOrEmpty(name) || amount <= 0) return;

        int current = GetMaterial(name);
        if (current < amount)
        {
            Debug.LogWarning($"[MaterialSystem] Tried to spend {amount}x {name} but only have {current}.");
            amount = current;
        }

        Materials[name] = current - amount;
        Save();
    }

    /// <summary>Returns a formatted multi-line summary of all owned materials.</summary>
    public string GetAllMaterialsSummary()
    {
        if (Materials == null || Materials.Count == 0)
            return "(no materials)";

        var sb = new System.Text.StringBuilder();
        foreach (var kv in Materials)
        {
            if (kv.Value > 0)
                sb.AppendLine($"{kv.Key}: {kv.Value}");
        }
        return sb.Length > 0 ? sb.ToString().TrimEnd() : "(no materials)";
    }

    // ── Persistence ────────────────────────────────────────────────────────

    public void Save()
    {
        var data = new MaterialSaveData();
        foreach (var kv in Materials)
        {
            data.keys.Add(kv.Key);
            data.values.Add(kv.Value);
        }
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        Materials = new Dictionary<string, int>();
        string json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var data = JsonUtility.FromJson<MaterialSaveData>(json);
            if (data?.keys == null) return;
            for (int i = 0; i < data.keys.Count && i < data.values.Count; i++)
                Materials[data.keys[i]] = data.values[i];
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MaterialSystem] Failed to load material data: {ex.Message}");
        }
    }
}
