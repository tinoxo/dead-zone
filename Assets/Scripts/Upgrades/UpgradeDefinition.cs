using UnityEngine;
using System.Collections.Generic;

public enum UpgradeType
{
    Damage, FireRate, BulletSize, BulletSpeed, MoveSpeed,
    MaxHealth, DashCooldown, Piercing, SplitShot, Shield
}

public enum UpgradeTier { Common, Uncommon, Rare, Legendary }

[System.Serializable]
public class UpgradeDefinition
{
    public string   ID;
    public string   Name;
    public string   Description;
    public UpgradeType Type;
    public UpgradeTier Tier;
    public float    Value;

    public Color TierColor => Tier switch
    {
        UpgradeTier.Common    => new Color(0.75f, 0.75f, 0.75f),
        UpgradeTier.Uncommon  => new Color(0.2f,  0.9f,  0.2f),
        UpgradeTier.Rare      => new Color(0.3f,  0.5f,  1.0f),
        UpgradeTier.Legendary => new Color(1.0f,  0.65f, 0.0f),
        _                     => Color.white
    };

    // ── All upgrades in the pool ──────────────────────────────────────────
    public static readonly List<UpgradeDefinition> All = new List<UpgradeDefinition>
    {
        // Damage
        new UpgradeDefinition { ID="dmg1",    Name="Sharp Rounds",     Description="+25% Damage",          Type=UpgradeType.Damage,      Tier=UpgradeTier.Common,    Value=0.25f },
        new UpgradeDefinition { ID="dmg2",    Name="Hollow Points",    Description="+50% Damage",          Type=UpgradeType.Damage,      Tier=UpgradeTier.Uncommon,  Value=0.50f },
        new UpgradeDefinition { ID="dmg3",    Name="Explosive Rounds", Description="+100% Damage",         Type=UpgradeType.Damage,      Tier=UpgradeTier.Rare,      Value=1.00f },
        new UpgradeDefinition { ID="dmg4",    Name="God Bullets",      Description="+200% Damage",         Type=UpgradeType.Damage,      Tier=UpgradeTier.Legendary, Value=2.00f },
        // Fire Rate
        new UpgradeDefinition { ID="fr1",     Name="Rapid Fire",       Description="+30% Fire Rate",       Type=UpgradeType.FireRate,    Tier=UpgradeTier.Common,    Value=0.30f },
        new UpgradeDefinition { ID="fr2",     Name="Auto Fire",        Description="+60% Fire Rate",       Type=UpgradeType.FireRate,    Tier=UpgradeTier.Uncommon,  Value=0.60f },
        new UpgradeDefinition { ID="fr3",     Name="Bullet Storm",     Description="+120% Fire Rate",      Type=UpgradeType.FireRate,    Tier=UpgradeTier.Rare,      Value=1.20f },
        new UpgradeDefinition { ID="fr4",     Name="Minigun",          Description="+200% Fire Rate",      Type=UpgradeType.FireRate,    Tier=UpgradeTier.Legendary, Value=2.00f },
        // Bullet Size
        new UpgradeDefinition { ID="bs1",     Name="Big Bullets",      Description="+25% Bullet Size",     Type=UpgradeType.BulletSize,  Tier=UpgradeTier.Common,    Value=0.25f },
        new UpgradeDefinition { ID="bs2",     Name="Mega Bullets",     Description="+60% Bullet Size",     Type=UpgradeType.BulletSize,  Tier=UpgradeTier.Uncommon,  Value=0.60f },
        new UpgradeDefinition { ID="bs3",     Name="Cannonballs",      Description="+120% Bullet Size",    Type=UpgradeType.BulletSize,  Tier=UpgradeTier.Rare,      Value=1.20f },
        // Bullet Speed
        new UpgradeDefinition { ID="bspd1",   Name="Swift Shots",      Description="+30% Bullet Speed",    Type=UpgradeType.BulletSpeed, Tier=UpgradeTier.Common,    Value=0.30f },
        new UpgradeDefinition { ID="bspd2",   Name="Hypersonic",       Description="+70% Bullet Speed",    Type=UpgradeType.BulletSpeed, Tier=UpgradeTier.Uncommon,  Value=0.70f },
        // Move Speed
        new UpgradeDefinition { ID="spd1",    Name="Swift Boots",      Description="+20% Move Speed",      Type=UpgradeType.MoveSpeed,   Tier=UpgradeTier.Common,    Value=0.20f },
        new UpgradeDefinition { ID="spd2",    Name="Turbo Boots",      Description="+40% Move Speed",      Type=UpgradeType.MoveSpeed,   Tier=UpgradeTier.Uncommon,  Value=0.40f },
        new UpgradeDefinition { ID="spd3",    Name="Speed Demon",      Description="+80% Move Speed",      Type=UpgradeType.MoveSpeed,   Tier=UpgradeTier.Rare,      Value=0.80f },
        // Health
        new UpgradeDefinition { ID="hp1",     Name="Iron Skin",        Description="+25% Max HP + Heal",   Type=UpgradeType.MaxHealth,   Tier=UpgradeTier.Common,    Value=0.25f },
        new UpgradeDefinition { ID="hp2",     Name="Steel Skin",       Description="+50% Max HP + Heal",   Type=UpgradeType.MaxHealth,   Tier=UpgradeTier.Uncommon,  Value=0.50f },
        new UpgradeDefinition { ID="hp3",     Name="Titanium Skin",    Description="+100% Max HP + Heal",  Type=UpgradeType.MaxHealth,   Tier=UpgradeTier.Rare,      Value=1.00f },
        // Dash
        new UpgradeDefinition { ID="dash1",   Name="Quick Dash",       Description="-20% Dash Cooldown",   Type=UpgradeType.DashCooldown,Tier=UpgradeTier.Common,    Value=0.20f },
        new UpgradeDefinition { ID="dash2",   Name="Warp Dash",        Description="-40% Dash Cooldown",   Type=UpgradeType.DashCooldown,Tier=UpgradeTier.Uncommon,  Value=0.40f },
        // Special
        new UpgradeDefinition { ID="pierce1", Name="Piercing Shot",    Description="Bullets pierce enemies",Type=UpgradeType.Piercing,   Tier=UpgradeTier.Rare,      Value=1f    },
        new UpgradeDefinition { ID="split1",  Name="Split Shot",       Description="Adds 2 side bullets",  Type=UpgradeType.SplitShot,  Tier=UpgradeTier.Rare,      Value=1f    },
        new UpgradeDefinition { ID="split2",  Name="Spread Shot",      Description="Adds 2 more bullets",  Type=UpgradeType.SplitShot,  Tier=UpgradeTier.Legendary, Value=1f    },
        new UpgradeDefinition { ID="shield1", Name="Energy Shield",    Description="Absorbs one hit",       Type=UpgradeType.Shield,     Tier=UpgradeTier.Legendary, Value=1f    },
    };
}
