using UnityEngine;
using System.Collections.Generic;

// ── Enums ──────────────────────────────────────────────────────────────────

public enum EnemyTheme
{
    Heavy,
    Swarm,
    Ranged,
    Void,
    Speed,
    Explosive,
    Electric
}

public enum PathRoomType
{
    Straight,
    Market,
    Chaos,
    Hybrid
}

// ── BossData ───────────────────────────────────────────────────────────────

[System.Serializable]
public class BossData
{
    public string       ID;
    public string       Name;
    public string       MaterialName;
    public Color        ThemeColor;
    public int          MaterialReward;
    public EnemyTheme   Theme;
    public PathRoomType RoomType;
    public string       Description;
    public string       AttackPatternDesc;

    public BossData(
        string id, string name, string materialName, Color themeColor,
        int materialReward, EnemyTheme theme, PathRoomType roomType,
        string description, string attackPatternDesc)
    {
        ID                = id;
        Name              = name;
        MaterialName      = materialName;
        ThemeColor        = themeColor;
        MaterialReward    = materialReward;
        Theme             = theme;
        RoomType          = roomType;
        Description       = description;
        AttackPatternDesc = attackPatternDesc;
    }
}

// ── BossRegistry ───────────────────────────────────────────────────────────

public static class BossRegistry
{
    public static readonly List<BossData> All = new List<BossData>
    {
        new BossData(
            id:                "warden",
            name:              "WARDEN",
            materialName:      "Iron Scrap",
            themeColor:        new Color(0.90f, 0.35f, 0.05f),   // red-orange
            materialReward:    3,
            theme:             EnemyTheme.Heavy,
            roomType:          PathRoomType.Straight,
            description:       "Armored sentinel that charges and slams",
            attackPatternDesc: "Telegraphed charge + slam shockwave"
        ),

        new BossData(
            id:                "vortex",
            name:              "VORTEX",
            materialName:      "Storm Core",
            themeColor:        new Color(0.0f, 0.85f, 1.0f),     // cyan
            materialReward:    4,
            theme:             EnemyTheme.Swarm,
            roomType:          PathRoomType.Chaos,
            description:       "Hurricane boss firing spiral bullet storms",
            attackPatternDesc: "Rotating spiral bursts + swarm minion summons"
        ),

        new BossData(
            id:                "siege",
            name:              "SIEGE",
            materialName:      "Siege Stone",
            themeColor:        new Color(1.0f, 0.55f, 0.0f),     // orange
            materialReward:    4,
            theme:             EnemyTheme.Ranged,
            roomType:          PathRoomType.Market,
            description:       "Artillery commander launching mortar barrages",
            attackPatternDesc: "Targeted mortar salvos + spread cannon fire"
        ),

        new BossData(
            id:                "hollow",
            name:              "HOLLOW",
            materialName:      "Void Shard",
            themeColor:        new Color(0.55f, 0.0f, 0.85f),    // purple
            materialReward:    5,
            theme:             EnemyTheme.Void,
            roomType:          PathRoomType.Hybrid,
            description:       "Void entity that teleports and fires dark waves",
            attackPatternDesc: "Blink teleport + radial dark-wave discharge"
        ),

        new BossData(
            id:                "drift",
            name:              "DRIFT",
            materialName:      "Kinetic Core",
            themeColor:        new Color(0.72f, 1.0f, 0.0f),     // yellow-green
            materialReward:    5,
            theme:             EnemyTheme.Speed,
            roomType:          PathRoomType.Chaos,
            description:       "Speed demon that charges and leaves trails",
            attackPatternDesc: "Hyper-dash chains + lingering hazard trails"
        ),

        new BossData(
            id:                "breach",
            name:              "BREACH",
            materialName:      "Blast Powder",
            themeColor:        new Color(1.0f, 0.30f, 0.05f),    // orange-red
            materialReward:    6,
            theme:             EnemyTheme.Explosive,
            roomType:          PathRoomType.Market,
            description:       "Demolition boss planting mines and chain explosions",
            attackPatternDesc: "Mine fields + triggered chain-explosion combos"
        ),

        new BossData(
            id:                "pulse",
            name:              "PULSE",
            materialName:      "Arc Crystal",
            themeColor:        new Color(0.20f, 0.55f, 1.0f),    // electric blue
            materialReward:    6,
            theme:             EnemyTheme.Electric,
            roomType:          PathRoomType.Hybrid,
            description:       "Electric conductor firing chain lightning arcs",
            attackPatternDesc: "Chain-lightning arcs + overcharge pulse blasts"
        ),

        new BossData(
            id:                "titan_l",
            name:              "TITAN-L",
            materialName:      "Dark Iron",
            themeColor:        new Color(0.50f, 0.04f, 0.04f),   // dark red
            materialReward:    7,
            theme:             EnemyTheme.Heavy,
            roomType:          PathRoomType.Straight,
            description:       "Colossal warrior with devastating melee slams",
            attackPatternDesc: "Ground-slam shockwaves + boulder throws"
        ),

        new BossData(
            id:                "omega",
            name:              "OMEGA",
            materialName:      "Omega Core",
            themeColor:        new Color(1.0f, 0.97f, 0.78f),    // white/gold
            materialReward:    10,
            theme:             EnemyTheme.Heavy,                  // represents all themes
            roomType:          PathRoomType.Straight,
            description:       "The final convergence — all patterns combined",
            attackPatternDesc: "Full rotation of all boss attack patterns at once"
        ),
    };

    public static BossData GetByID(string id)
    {
        foreach (var boss in All)
        {
            if (boss.ID == id)
                return boss;
        }
        Debug.LogWarning($"[BossRegistry] No boss found with ID: '{id}'");
        return null;
    }
}
