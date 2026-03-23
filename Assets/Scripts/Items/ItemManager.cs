using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the item pool and spawning pairs after boss kills.
/// Also applies item effects to the player.
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    // Items collected this run
    public List<ItemDefinition> CollectedItems { get; private set; } = new List<ItemDefinition>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Called after every boss kill ──────────────────────────────────────
    public void SpawnItemPair(Vector2 bossPosition)
    {
        // Pick 2 distinct items at random (weighted by rarity)
        var pool = ItemDefinition.All.ToList();
        Shuffle(pool);

        var pick1 = PickWeighted(pool);
        pool.Remove(pick1);
        var pick2 = PickWeighted(pool);

        // Spawn positions: spread either side of where boss died
        Vector2 offset = new Vector2(2.5f, 0f);
        var go1 = SpawnPickup(pick1, (Vector2)bossPosition - offset);
        var go2 = SpawnPickup(pick2, (Vector2)bossPosition + offset);

        // Link as partners so picking one destroys the other
        go1.Partner = go2;
        go2.Partner = go1;

        Debug.Log($"[ItemManager] Spawned items: {pick1.Name} & {pick2.Name}");
    }

    ItemPickup SpawnPickup(ItemDefinition def, Vector2 pos)
    {
        var go = new GameObject($"Item_{def.ID}");
        go.transform.position = pos;
        var pickup = go.AddComponent<ItemPickup>();
        pickup.Init(def);
        return pickup;
    }

    // ── Apply item effect to player ───────────────────────────────────────
    public void ApplyItem(ItemDefinition def)
    {
        CollectedItems.Add(def);
        Debug.Log($"[ItemManager] Picked up: {def.Name} ({def.Rarity})");

        var ps = PlayerStats.Instance;
        if (ps == null) return;

        // Register on PlayerStats so bullets can check active effects
        ps.AddItem(def);

        // Some effects modify stats directly
        switch (def.EffectType)
        {
            case ItemEffectType.GhostBlade:
                ps.Piercing = true;
                break;
            case ItemEffectType.DoubleShot:
                // handled in PlayerController fire logic
                break;
            case ItemEffectType.Vampiric:
                // handled in EnemyBase.Die()
                break;
        }

        HUDManager.Instance?.UpdateStats();
        HUDManager.Instance?.ShowItemPickupBanner(def);
    }

    // ── Weighted random by rarity ─────────────────────────────────────────
    ItemDefinition PickWeighted(List<ItemDefinition> pool)
    {
        // Weights: Common=60, Uncommon=25, Rare=12, Legendary=3
        var weighted = new List<(ItemDefinition item, int weight)>();
        foreach (var item in pool)
        {
            int w = item.Rarity switch {
                ItemRarity.Legendary => 3,
                ItemRarity.Rare      => 12,
                ItemRarity.Uncommon  => 25,
                _                    => 60,
            };
            weighted.Add((item, w));
        }

        int total = weighted.Sum(x => x.weight);
        int roll  = Random.Range(0, total);
        int cum   = 0;
        foreach (var (item, weight) in weighted)
        {
            cum += weight;
            if (roll < cum) return item;
        }
        return pool[0];
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
