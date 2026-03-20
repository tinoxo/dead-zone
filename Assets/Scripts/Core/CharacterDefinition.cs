using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterDefinition
{
    public string Name;
    public string Title;          // Short flavor subtitle
    public string Flavor;         // Description line
    public string StartingBonus;  // Text shown on menu

    // Stat multipliers applied at game start
    public float HPMult      = 1f;
    public float SpeedMult   = 1f;
    public float DamageMult  = 1f;
    public float FireRateMult = 1f;
    public float BulletSizeMult = 1f;
    public float DashCDMult  = 1f;

    // Special starting flags
    public bool StartPiercing  = false;
    public bool StartSplitShot = false;
    public bool StartShield    = false;

    // Visual
    public Color PrimaryColor;
    public Color AccentColor;

    // Relative stat bar values (0–1) for the menu display
    public float StatHP;
    public float StatSPD;
    public float StatDMG;
    public float StatFIRE;

    // ── All Characters ────────────────────────────────────────────────────
    public static List<CharacterDefinition> All = new List<CharacterDefinition>
    {
        new CharacterDefinition
        {
            Name         = "AXIOM",
            Title        = "The Standard",
            Flavor       = "A balanced fighter. No frills — just skill.",
            StartingBonus = "No bonus — pure skill",
            HPMult       = 1f,
            SpeedMult    = 1f,
            DamageMult   = 1f,
            FireRateMult = 1f,
            DashCDMult   = 1f,
            PrimaryColor = new Color(0f,    1f,    1f),
            AccentColor  = new Color(0f,    0.6f,  0.8f),
            StatHP       = 0.5f,
            StatSPD      = 0.5f,
            StatDMG      = 0.5f,
            StatFIRE     = 0.5f,
        },
        new CharacterDefinition
        {
            Name         = "VELA",
            Title        = "The Ghost",
            Flavor       = "Moves like smoke. Hard to kill, hard to catch.",
            StartingBonus = "Dash cooldown -40%",
            HPMult       = 0.70f,
            SpeedMult    = 1.50f,
            DamageMult   = 0.85f,
            FireRateMult = 1.20f,
            DashCDMult   = 0.60f,
            PrimaryColor = new Color(0.7f,  0.2f,  1f),
            AccentColor  = new Color(0.5f,  0.1f,  0.8f),
            StatHP       = 0.25f,
            StatSPD      = 0.95f,
            StatDMG      = 0.35f,
            StatFIRE     = 0.65f,
        },
        new CharacterDefinition
        {
            Name         = "KRONOS",
            Title        = "The Juggernaut",
            Flavor       = "Slow. Relentless. Unstoppable.",
            StartingBonus = "+80% HP  |  +30% Damage",
            HPMult       = 1.80f,
            SpeedMult    = 0.72f,
            DamageMult   = 1.30f,
            FireRateMult = 0.80f,
            DashCDMult   = 1.30f,
            PrimaryColor = new Color(1f,    0.45f, 0.05f),
            AccentColor  = new Color(0.8f,  0.25f, 0f),
            StatHP       = 0.95f,
            StatSPD      = 0.15f,
            StatDMG      = 0.85f,
            StatFIRE     = 0.25f,
        },
        new CharacterDefinition
        {
            Name         = "ECHO",
            Title        = "The Rift",
            Flavor       = "Every shot echoes. Bullets fan out in all directions.",
            StartingBonus = "Starts with Split Shot",
            HPMult       = 1f,
            SpeedMult    = 1f,
            DamageMult   = 0.80f,
            FireRateMult = 1.25f,
            BulletSizeMult = 0.85f,
            StartSplitShot = true,
            PrimaryColor = new Color(0.1f,  0.95f, 0.4f),
            AccentColor  = new Color(0.05f, 0.6f,  0.25f),
            StatHP       = 0.5f,
            StatSPD      = 0.5f,
            StatDMG      = 0.3f,
            StatFIRE     = 0.85f,
        },
        new CharacterDefinition
        {
            Name         = "PHANTOM",
            Title        = "The Specter",
            Flavor       = "Bullets pass through everything. Nothing can stop the Phantom.",
            StartingBonus = "Starts with Piercing Shots",
            HPMult       = 0.75f,
            SpeedMult    = 1.15f,
            DamageMult   = 0.90f,
            FireRateMult = 1.10f,
            BulletSizeMult = 1.20f,
            StartPiercing = true,
            PrimaryColor = new Color(1f,    0.85f, 0.1f),
            AccentColor  = new Color(0.8f,  0.6f,  0f),
            StatHP       = 0.3f,
            StatSPD      = 0.65f,
            StatDMG      = 0.5f,
            StatFIRE     = 0.6f,
        },
    };
}
