using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    // ── Base values ───────────────────────────────────────────────────────
    public float BaseMoveSpeed    = 6f;
    public float BaseDamage       = 12f;
    public float BaseFireRate     = 4f;    // shots per second
    public float BaseBulletSize   = 1f;    // scale multiplier
    public float BaseBulletSpeed  = 14f;
    public float BaseMaxHealth    = 100f;
    public float BaseDashCooldown = 1.5f;

    // ── Multipliers (modified by upgrades) ────────────────────────────────
    [HideInInspector] public float MoveMult        = 1f;
    [HideInInspector] public float DamageMult      = 1f;
    [HideInInspector] public float FireRateMult    = 1f;
    [HideInInspector] public float BulletSizeMult  = 1f;
    [HideInInspector] public float BulletSpeedMult = 1f;
    [HideInInspector] public float MaxHealthMult   = 1f;
    [HideInInspector] public float DashCooldownMult= 1f;

    // ── Special flags ─────────────────────────────────────────────────────
    [HideInInspector] public bool  Piercing   = false;
    [HideInInspector] public bool  SplitShot  = false;
    [HideInInspector] public int   SplitCount = 0;
    [HideInInspector] public bool  HasShield  = false;

    // ── Active items (collected this run) ─────────────────────────────────
    public List<ItemDefinition> ActiveItems { get; private set; } = new List<ItemDefinition>();

    public bool HasItem(ItemEffectType t)  => ActiveItems.Any(i => i.EffectType == t);
    public int  ItemCount(ItemEffectType t)=> ActiveItems.Count(i => i.EffectType == t);
    public float ItemValue(ItemEffectType t)
    {
        var items = ActiveItems.Where(i => i.EffectType == t).ToList();
        return items.Count > 0 ? items.Sum(i => i.Value) : 0f;
    }

    public void AddItem(ItemDefinition def) => ActiveItems.Add(def);

    // ── Computed properties ───────────────────────────────────────────────
    public float MoveSpeed    => BaseMoveSpeed    * MoveMult;
    public float Damage       => BaseDamage       * DamageMult;
    public float FireRate     => BaseFireRate      * FireRateMult;
    public float BulletSize   => BaseBulletSize   * BulletSizeMult;
    public float BulletSpeed  => BaseBulletSpeed  * BulletSpeedMult;
    public float MaxHealth    => BaseMaxHealth     * MaxHealthMult;
    public float DashCooldown => BaseDashCooldown  * DashCooldownMult;

    void Awake() => Instance = this;

    public void Apply(UpgradeDefinition u)
    {
        switch (u.Type)
        {
            case UpgradeType.Damage:       DamageMult      += u.Value; break;
            case UpgradeType.FireRate:     FireRateMult    += u.Value; break;
            case UpgradeType.BulletSize:   BulletSizeMult  += u.Value; break;
            case UpgradeType.BulletSpeed:  BulletSpeedMult += u.Value; break;
            case UpgradeType.MoveSpeed:    MoveMult        += u.Value; break;
            case UpgradeType.MaxHealth:
                MaxHealthMult += u.Value;
                PlayerController.Instance?.HealPercent(u.Value);
                break;
            case UpgradeType.DashCooldown:
                DashCooldownMult = Mathf.Max(0.25f, DashCooldownMult - u.Value);
                break;
            case UpgradeType.Piercing:  Piercing  = true; break;
            case UpgradeType.SplitShot: SplitShot = true; SplitCount++; break;
            case UpgradeType.Shield:    HasShield  = true; break;
        }

        HUDManager.Instance?.UpdateStats();
    }
}
