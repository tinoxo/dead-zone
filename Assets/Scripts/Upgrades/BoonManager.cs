using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns 3 physical boon orbs (Iron, Swift, Aegis) after each wave clear.
/// The door to the next room only appears after the player picks one.
/// </summary>
public class BoonManager : MonoBehaviour
{
    public static BoonManager Instance { get; private set; }

    DoorRoomType     pendingDoorType;
    List<BoonPickup> activeOrbs = new List<BoonPickup>();

    // Triangle formation at the bottom of the arena
    static readonly Vector2[] Positions =
    {
        new Vector2(-7f, -4.5f),
        new Vector2( 0f, -6.5f),
        new Vector2( 7f, -4.5f),
    };

    static readonly BoonSet[] Sets = { BoonSet.Iron, BoonSet.Swift, BoonSet.Aegis };

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    /// <summary>Called by GameManager when a segment wave clears.</summary>
    public void SpawnChoices(DoorRoomType doorType)
    {
        pendingDoorType = doorType;
        activeOrbs.Clear();

        for (int i = 0; i < 3; i++)
        {
            var orb = BoonPickup.Spawn(Positions[i], Sets[i]);
            activeOrbs.Add(orb);
        }

        Debug.Log($"[BoonManager] Spawned Iron / Swift / Aegis boons. Door type after pick: {doorType}");
    }

    /// <summary>Called by BoonPickup when the player walks into it.</summary>
    public void OnBoonPicked(BoonPickup picked)
    {
        ApplyEffect(picked.Effect);

        // Destroy sibling orbs
        foreach (var o in activeOrbs)
            if (o != null && o != picked) Destroy(o.gameObject);
        activeOrbs.Clear();

        // Spawn the door now that a boon has been chosen
        WaveManager.Instance?.ShowExitDoor(pendingDoorType);

        Debug.Log($"[BoonManager] Applied boon: {picked.Effect.Name} ({picked.Set})");
    }

    void ApplyEffect(BoonEffect e)
    {
        var s = PlayerStats.Instance;
        if (s == null) return;

        switch (e.Type)
        {
            case UpgradeType.Damage:       s.DamageMult      += e.Value; break;
            case UpgradeType.FireRate:     s.FireRateMult    += e.Value; break;
            case UpgradeType.BulletSize:   s.BulletSizeMult  += e.Value; break;
            case UpgradeType.BulletSpeed:  s.BulletSpeedMult += e.Value; break;
            case UpgradeType.MoveSpeed:    s.MoveMult        += e.Value; break;
            case UpgradeType.MaxHealth:
                s.MaxHealthMult += e.Value;
                PlayerController.Instance?.HealPercent(e.Value);
                break;
            case UpgradeType.DashCooldown:
                s.DashCooldownMult = Mathf.Max(0.25f, s.DashCooldownMult - e.Value);
                break;
            case UpgradeType.Piercing:  s.Piercing  = true;              break;
            case UpgradeType.SplitShot: s.SplitShot = true; s.SplitCount++; break;
            case UpgradeType.Shield:    s.HasShield  = true;              break;
        }

        HUDManager.Instance?.UpdateStats();
    }
}
