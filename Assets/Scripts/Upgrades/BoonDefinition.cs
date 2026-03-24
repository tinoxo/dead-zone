using UnityEngine;
using System.Collections.Generic;

public enum BoonSet { Iron, Swift, Aegis }

[System.Serializable]
public class BoonEffect
{
    public string      Name;
    public string      Description;
    public UpgradeType Type;
    public float       Value;
}

/// <summary>
/// Three god-like boon sets, each with its own upgrade pool.
/// Iron = firepower, Swift = agility, Aegis = defense.
/// </summary>
public static class BoonDefinition
{
    public static readonly List<BoonEffect> IronPool = new List<BoonEffect>
    {
        new BoonEffect { Name="Iron Strike",    Description="+25% Damage",          Type=UpgradeType.Damage,       Value=0.25f },
        new BoonEffect { Name="Hollow Points",  Description="+40% Damage",          Type=UpgradeType.Damage,       Value=0.40f },
        new BoonEffect { Name="Heavy Rounds",   Description="+25% Bullet Size",     Type=UpgradeType.BulletSize,   Value=0.25f },
        new BoonEffect { Name="War Salvo",      Description="+25% Fire Rate",       Type=UpgradeType.FireRate,     Value=0.25f },
        new BoonEffect { Name="Piercing Shot",  Description="Bullets pierce enemies",Type=UpgradeType.Piercing,   Value=1f    },
        new BoonEffect { Name="Split Shot",     Description="Adds 2 side bullets",  Type=UpgradeType.SplitShot,   Value=1f    },
    };

    public static readonly List<BoonEffect> SwiftPool = new List<BoonEffect>
    {
        new BoonEffect { Name="Fleet Foot",   Description="+25% Move Speed",     Type=UpgradeType.MoveSpeed,    Value=0.25f },
        new BoonEffect { Name="Rapid Fire",   Description="+25% Fire Rate",      Type=UpgradeType.FireRate,     Value=0.25f },
        new BoonEffect { Name="Quickstep",    Description="-25% Dash Cooldown",  Type=UpgradeType.DashCooldown, Value=0.25f },
        new BoonEffect { Name="Bullet Rush",  Description="+30% Bullet Speed",   Type=UpgradeType.BulletSpeed,  Value=0.30f },
        new BoonEffect { Name="Ghost Step",   Description="+40% Move Speed",     Type=UpgradeType.MoveSpeed,    Value=0.40f },
        new BoonEffect { Name="Blurring",     Description="+20% Speed & -20% Dash CD", Type=UpgradeType.MoveSpeed, Value=0.20f },
    };

    public static readonly List<BoonEffect> AegisPool = new List<BoonEffect>
    {
        new BoonEffect { Name="Iron Skin",    Description="+25% Max HP & Heal",  Type=UpgradeType.MaxHealth,    Value=0.25f },
        new BoonEffect { Name="Steel Skin",   Description="+50% Max HP & Heal",  Type=UpgradeType.MaxHealth,    Value=0.50f },
        new BoonEffect { Name="Warp Dash",    Description="-35% Dash Cooldown",  Type=UpgradeType.DashCooldown, Value=0.35f },
        new BoonEffect { Name="Bulwark",      Description="Gain an energy shield",Type=UpgradeType.Shield,      Value=1f    },
        new BoonEffect { Name="Titan Body",   Description="+75% Max HP & Heal",  Type=UpgradeType.MaxHealth,    Value=0.75f },
        new BoonEffect { Name="Quick Mend",   Description="+30% Max HP & Heal",  Type=UpgradeType.MaxHealth,    Value=0.30f },
    };

    public static List<BoonEffect> GetPool(BoonSet set)
    {
        switch (set)
        {
            case BoonSet.Iron:  return IronPool;
            case BoonSet.Swift: return SwiftPool;
            default:            return AegisPool;
        }
    }

    public static Color SetColor(BoonSet set)
    {
        switch (set)
        {
            case BoonSet.Iron:  return new Color(1f,    0.32f, 0.08f);  // red-orange
            case BoonSet.Swift: return new Color(0.05f, 0.90f, 0.72f);  // teal-cyan
            default:            return new Color(0.35f, 0.55f, 1.00f);  // blue (Aegis)
        }
    }

    public static string SetLabel(BoonSet set)
    {
        switch (set)
        {
            case BoonSet.Iron:  return "IRON";
            case BoonSet.Swift: return "SWIFT";
            default:            return "AEGIS";
        }
    }

    public static string SetSubtitle(BoonSet set)
    {
        switch (set)
        {
            case BoonSet.Iron:  return "Power  &  Firepower";
            case BoonSet.Swift: return "Speed  &  Agility";
            default:            return "Defense  &  Survival";
        }
    }
}
