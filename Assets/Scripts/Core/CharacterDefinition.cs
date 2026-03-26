using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterDefinition
{
    public string Name;
    public string Title;
    public string Flavor;
    public string UnlockCondition;   // shown on hover when locked
    public bool   IsSecret;          // shows ??? even on hover
    public bool   StarterCharacter;  // never needs unlocking

    // Stat multipliers applied at game start
    public float HPMult       = 1f;
    public float SpeedMult    = 1f;
    public float DamageMult   = 1f;
    public float FireRateMult = 1f;
    public float BulletSizeMult = 1f;
    public float DashCDMult   = 1f;

    // Special starting flags
    public bool StartPiercing  = false;
    public bool StartSplitShot = false;

    // Visual
    public Color PrimaryColor;
    public Color AccentColor;

    // Stat bar display values (0–1)
    public float StatHP;
    public float StatSPD;
    public float StatDMG;
    public float StatFIRE;

    // Is this character currently usable?
    public bool IsUnlocked => StarterCharacter || GameData.IsCharacterUnlocked(AllIndex);
    public int  AllIndex   => All.IndexOf(this);

    // ── All Characters ────────────────────────────────────────────────────
    public static List<CharacterDefinition> All = new List<CharacterDefinition>
    {
        // ── UNIVERSAL STARTERS (4) ────────────────────────────────────────
        new CharacterDefinition
        {
            Name = "TINO", Title = "The Maverick",
            Flavor = "Adaptable. Reliable. Ready for anything.",
            StarterCharacter = true,
            HPMult=1f, SpeedMult=1f, DamageMult=1f, FireRateMult=1f, DashCDMult=1f,
            PrimaryColor = new Color(0f,    1f,    1f),
            AccentColor  = new Color(0f,    0.6f,  0.8f),
            StatHP=0.5f, StatSPD=0.5f, StatDMG=0.5f, StatFIRE=0.5f,
        },
        new CharacterDefinition
        {
            Name = "JESSICA", Title = "The Blur",
            Flavor = "Never stops moving. Never gets hit.",
            StarterCharacter = true,
            HPMult=0.75f, SpeedMult=1.35f, DamageMult=0.9f, FireRateMult=1.2f, DashCDMult=0.7f,
            PrimaryColor = new Color(1f,    0.2f,  0.6f),
            AccentColor  = new Color(0.8f,  0.1f,  0.4f),
            StatHP=0.35f, StatSPD=0.85f, StatDMG=0.4f, StatFIRE=0.7f,
        },
        new CharacterDefinition
        {
            Name = "SONDRO", Title = "The Wall",
            Flavor = "Slow and unstoppable. Built different.",
            StarterCharacter = true,
            HPMult=1.7f, SpeedMult=0.75f, DamageMult=1.15f, FireRateMult=0.8f, DashCDMult=1.3f,
            PrimaryColor = new Color(0.6f,  0.2f,  1f),
            AccentColor  = new Color(0.4f,  0.1f,  0.8f),
            StatHP=0.9f, StatSPD=0.2f, StatDMG=0.75f, StatFIRE=0.3f,
        },
        new CharacterDefinition
        {
            Name = "MARCO", Title = "The Predator",
            Flavor = "Every bullet hits harder. All of them.",
            StarterCharacter = true,
            HPMult=1f, SpeedMult=1f, DamageMult=1.4f, FireRateMult=0.85f, DashCDMult=1f,
            PrimaryColor = new Color(1f,    0.5f,  0.1f),
            AccentColor  = new Color(0.8f,  0.3f,  0f),
            StatHP=0.5f, StatSPD=0.5f, StatDMG=0.9f, StatFIRE=0.4f,
        },

        // ── MILESTONE UNLOCKS (2) ─────────────────────────────────────────
        new CharacterDefinition
        {
            Name = "RICO", Title = "Good Boy",
            Flavor = "Fastest thing alive. Also a dog.",
            UnlockCondition = "Reach 1,000 total kills across all runs",
            HPMult=0.7f, SpeedMult=1.6f, DamageMult=0.85f, FireRateMult=1.3f, DashCDMult=0.6f,
            PrimaryColor = new Color(1f,    0.8f,  0.2f),
            AccentColor  = new Color(0.8f,  0.6f,  0f),
            StatHP=0.25f, StatSPD=0.95f, StatDMG=0.3f, StatFIRE=0.8f,
        },
        new CharacterDefinition
        {
            Name = "SNOOPY", Title = "Old Reliable",
            Flavor = "Taken every hit. Still standing.",
            UnlockCondition = "Die 100 times across all runs",
            HPMult=1.45f, SpeedMult=0.85f, DamageMult=1f, FireRateMult=0.9f, DashCDMult=1.1f,
            PrimaryColor = new Color(0.9f,  0.9f,  0.95f),
            AccentColor  = new Color(0.6f,  0.6f,  0.7f),
            StatHP=0.8f, StatSPD=0.4f, StatDMG=0.5f, StatFIRE=0.45f,
        },

        // ── SPECIAL UNLOCKS (5) ───────────────────────────────────────────
        new CharacterDefinition
        {
            Name = "LATIOS", Title = "The Eon",
            Flavor = "A legendary once thought to be only a myth.",
            UnlockCondition = "???",
            IsSecret = true,
            HPMult=0.55f, SpeedMult=1.5f, DamageMult=1.1f, FireRateMult=1.5f, DashCDMult=0.5f,
            PrimaryColor = new Color(0.4f,  0.7f,  1f),
            AccentColor  = new Color(0.2f,  0.5f,  0.9f),
            StatHP=0.15f, StatSPD=0.95f, StatDMG=0.7f, StatFIRE=0.95f,
        },
        new CharacterDefinition
        {
            Name = "BARLEY", Title = "The Brewer",
            Flavor = "Every bottle does something different. Usually explosive.",
            UnlockCondition = "Complete Star Park island",
            HPMult=0.9f, SpeedMult=0.9f, DamageMult=1.05f, FireRateMult=1.1f, DashCDMult=1f,
            PrimaryColor = new Color(0.2f,  0.7f,  0.3f),
            AccentColor  = new Color(0.1f,  0.5f,  0.2f),
            StatHP=0.45f, StatSPD=0.45f, StatDMG=0.55f, StatFIRE=0.65f,
        },
        new CharacterDefinition
        {
            Name = "THE GILDED", Title = "Walking Fortune",
            Flavor = "Worth more than most islands combined.",
            UnlockCondition = "Bank 10,000 total gold across all runs",
            HPMult=1.5f, SpeedMult=0.75f, DamageMult=1f, FireRateMult=0.8f, DashCDMult=1.2f,
            PrimaryColor = new Color(1f,    0.85f, 0f),
            AccentColor  = new Color(0.8f,  0.65f, 0f),
            StatHP=0.85f, StatSPD=0.25f, StatDMG=0.55f, StatFIRE=0.35f,
        },
        new CharacterDefinition
        {
            Name = "SANGU", Title = "The Sand Penguin",
            Flavor = "A sand monster. Shaped like a penguin. Don't question it.",
            UnlockCondition = "Clear the Gilded Sands on any character",
            HPMult=1.1f, SpeedMult=1.05f, DamageMult=0.9f, FireRateMult=1f, DashCDMult=0.95f,
            PrimaryColor = new Color(0.9f,  0.75f, 0.4f),
            AccentColor  = new Color(0.7f,  0.55f, 0.2f),
            StatHP=0.6f, StatSPD=0.6f, StatDMG=0.4f, StatFIRE=0.5f,
        },
        new CharacterDefinition
        {
            Name = "CHEESE", Title = "The Puffer",
            Flavor = "Named after a real pufferfish. Still dangerous.",
            UnlockCondition = "Clear The Deep on any character",
            HPMult=0.85f, SpeedMult=0.9f, DamageMult=0.85f, FireRateMult=1.1f, DashCDMult=1f,
            PrimaryColor = new Color(1f,    0.7f,  0.1f),
            AccentColor  = new Color(0.8f,  0.5f,  0f),
            StatHP=0.35f, StatSPD=0.5f, StatDMG=0.35f, StatFIRE=0.65f,
        },

        // ── TBD SLOTS (4) — coming in future updates ──────────────────────
        new CharacterDefinition { Name="???", IsSecret=true, UnlockCondition="Coming Soon",
            PrimaryColor=new Color(0.3f,0.3f,0.4f), AccentColor=new Color(0.2f,0.2f,0.3f) },
        new CharacterDefinition { Name="???", IsSecret=true, UnlockCondition="Coming Soon",
            PrimaryColor=new Color(0.3f,0.3f,0.4f), AccentColor=new Color(0.2f,0.2f,0.3f) },
        new CharacterDefinition { Name="???", IsSecret=true, UnlockCondition="Coming Soon",
            PrimaryColor=new Color(0.3f,0.3f,0.4f), AccentColor=new Color(0.2f,0.2f,0.3f) },
        new CharacterDefinition { Name="???", IsSecret=true, UnlockCondition="Coming Soon",
            PrimaryColor=new Color(0.3f,0.3f,0.4f), AccentColor=new Color(0.2f,0.2f,0.3f) },
    };
}
