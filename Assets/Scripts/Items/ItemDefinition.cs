using UnityEngine;
using System.Collections.Generic;

public enum ItemEffectType
{
    BouncingRounds,     // bullets ricochet off walls
    PoisonTrail,        // bullets leave poison zones on impact
    ChainLightning,     // bullets arc to nearby enemy on hit
    GhostBlade,         // bullets pierce all enemies
    Homing,             // bullets slightly steer toward nearest enemy
    Explosive,          // bullets explode in small radius on impact
    Vampiric,           // heal small amount on kill
    Singularity,        // bullets pull nearby enemies on impact  (legendary)
    DoubleShot,         // fire 2 bullets per shot
    FrostRound,         // bullets slow enemies briefly
}

public enum ItemRarity { Common, Uncommon, Rare, Legendary }

[System.Serializable]
public class ItemDefinition
{
    public string        ID;
    public string        Name;
    public string        Description;
    public ItemEffectType EffectType;
    public ItemRarity    Rarity;
    public float         Value;       // rarity-scaled strength
    public Color         GemColor;    // color of the world pickup gem

    // ── Full item pool ────────────────────────────────────────────────────
    public static readonly List<ItemDefinition> All = new List<ItemDefinition>
    {
        new ItemDefinition {
            ID="bounce1", Name="Bouncing Rounds",
            Description="Your bullets ricochet off arena walls up to 2 times.",
            EffectType=ItemEffectType.BouncingRounds, Rarity=ItemRarity.Common,
            Value=2f, GemColor=new Color(0.2f, 0.9f, 1f) },

        new ItemDefinition {
            ID="poison1", Name="Poison Trail",
            Description="Bullets leave a small toxic zone on impact.",
            EffectType=ItemEffectType.PoisonTrail, Rarity=ItemRarity.Uncommon,
            Value=5f, GemColor=new Color(0.3f, 1f, 0.2f) },

        new ItemDefinition {
            ID="chain1", Name="Chain Lightning",
            Description="Bullets arc to one nearby enemy on hit.",
            EffectType=ItemEffectType.ChainLightning, Rarity=ItemRarity.Rare,
            Value=0.6f, GemColor=new Color(0.6f, 0.6f, 1f) },

        new ItemDefinition {
            ID="ghost1", Name="Ghost Blade",
            Description="Bullets phase through all enemies they hit.",
            EffectType=ItemEffectType.GhostBlade, Rarity=ItemRarity.Uncommon,
            Value=1f, GemColor=new Color(0.8f, 0.8f, 1f) },

        new ItemDefinition {
            ID="homing1", Name="Homing Instinct",
            Description="Bullets gently curve toward the nearest enemy.",
            EffectType=ItemEffectType.Homing, Rarity=ItemRarity.Common,
            Value=0.15f, GemColor=new Color(1f, 0.7f, 0.2f) },

        new ItemDefinition {
            ID="explode1", Name="Explosive Rounds",
            Description="Bullets detonate in a small burst on impact.",
            EffectType=ItemEffectType.Explosive, Rarity=ItemRarity.Rare,
            Value=0.8f, GemColor=new Color(1f, 0.45f, 0.1f) },

        new ItemDefinition {
            ID="vamp1", Name="Vampiric Edge",
            Description="Killing an enemy restores a small amount of health.",
            EffectType=ItemEffectType.Vampiric, Rarity=ItemRarity.Uncommon,
            Value=3f, GemColor=new Color(0.9f, 0.1f, 0.3f) },

        new ItemDefinition {
            ID="frost1", Name="Frost Round",
            Description="Bullets slow enemies by 40% for 1.5 seconds.",
            EffectType=ItemEffectType.FrostRound, Rarity=ItemRarity.Common,
            Value=0.4f, GemColor=new Color(0.5f, 0.9f, 1f) },

        new ItemDefinition {
            ID="double1", Name="Double Shot",
            Description="Each trigger pull fires 2 bullets simultaneously.",
            EffectType=ItemEffectType.DoubleShot, Rarity=ItemRarity.Uncommon,
            Value=2f, GemColor=new Color(1f, 1f, 0.3f) },

        new ItemDefinition {
            ID="singular1", Name="Singularity",
            Description="Bullets collapse into a micro black hole on impact, pulling all nearby enemies.",
            EffectType=ItemEffectType.Singularity, Rarity=ItemRarity.Legendary,
            Value=6f, GemColor=new Color(0.7f, 0.2f, 1f) },
    };
}
