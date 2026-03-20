using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the boss path lattice. Singleton MonoBehaviour.
///
/// Lattice layout (boss IDs):
///   Depth 0 : "warden"  (fixed start)
///   Depth 1 : Left="vortex"   Right="siege"
///   Depth 2 : Left="hollow"   Right="drift"
///   Depth 3 : Left="breach"   Right="pulse"
///   Depth 4 : "omega"   (always — final boss)
/// </summary>
public class PathManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static PathManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Lattice definition ─────────────────────────────────────────────────
    // [depth][side]  side 0 = left, side 1 = right
    // Depth 0 and 4 are single-node depths (left == right == same boss).
    static readonly string[,] Lattice =
    {
        // depth 0 — fixed start
        { "warden",  "warden"  },
        // depth 1
        { "vortex",  "siege"   },
        // depth 2
        { "hollow",  "drift"   },
        // depth 3
        { "breach",  "pulse"   },
        // depth 4 — final boss
        { "omega",   "omega"   },
    };

    // ── State ──────────────────────────────────────────────────────────────
    public int          CurrentDepth { get; private set; }
    public bool         OnLeftSide   { get; private set; }
    public List<string> PathHistory  { get; private set; } = new List<string>();

    // ── Derived properties ─────────────────────────────────────────────────
    public BossData CurrentBoss =>
        BossRegistry.GetByID(Lattice[CurrentDepth, OnLeftSide ? 0 : 1]);

    public BossData NextBossLeft =>
        CurrentDepth < 4 ? BossRegistry.GetByID(Lattice[CurrentDepth + 1, 0]) : null;

    public BossData NextBossRight =>
        CurrentDepth < 4 ? BossRegistry.GetByID(Lattice[CurrentDepth + 1, 1]) : null;

    public PathRoomType CurrentRoomType =>
        CurrentBoss != null ? CurrentBoss.RoomType : PathRoomType.Straight;

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Reset to depth 0 (Warden), left side.</summary>
    public void Initialize()
    {
        CurrentDepth = 0;
        OnLeftSide   = true;
        PathHistory.Clear();
        Debug.Log("[PathManager] Initialized — starting at WARDEN (depth 0).");
    }

    /// <summary>Returns the BossData at the given depth and side.</summary>
    public BossData GetBossAt(int depth, bool left)
    {
        if (depth < 0 || depth >= Lattice.GetLength(0))
        {
            Debug.LogWarning($"[PathManager] GetBossAt: depth {depth} out of range.");
            return null;
        }
        return BossRegistry.GetByID(Lattice[depth, left ? 0 : 1]);
    }

    /// <summary>
    /// Advance one depth in the lattice toward the chosen side.
    /// Triggers WaveManager to begin themed waves for the new room.
    /// </summary>
    public void ChoosePath(bool goLeft)
    {
        if (CurrentDepth >= 4)
        {
            Debug.LogWarning("[PathManager] ChoosePath called but already at final depth.");
            return;
        }

        CurrentDepth++;
        OnLeftSide = goLeft;

        BossData chosen = CurrentBoss;
        if (chosen != null)
        {
            PathHistory.Add(chosen.ID);
            Debug.Log($"[PathManager] Path chosen: {chosen.Name} (depth {CurrentDepth}, {(goLeft ? "LEFT" : "RIGHT")})");

            // Start themed waves for this segment
            WaveManager.Instance?.StartSegment(chosen.Theme, chosen.RoomType);
        }
    }

    /// <summary>True when the player is about to fight the final boss (Omega).</summary>
    public bool IsFinalBoss() => CurrentDepth == 4;

    /// <summary>
    /// Called by a boss when it is defeated.
    /// Awards boss materials then either shows the path map or triggers end sequence.
    /// </summary>
    public void OnBossDefeated()
    {
        BossData defeated = CurrentBoss;
        if (defeated == null) return;

        // Award materials
        MaterialSystem.Instance?.AddMaterial(defeated.MaterialName, defeated.MaterialReward);
        Debug.Log($"[PathManager] Boss defeated: {defeated.Name}. " +
                  $"Awarded {defeated.MaterialReward}x {defeated.MaterialName}.");

        if (IsFinalBoss())
        {
            Debug.Log("[PathManager] OMEGA defeated — run complete!");
            GameManager.Instance?.PlayerWon();
        }
        else
        {
            // Show path selection UI
            BossData left  = NextBossLeft;
            BossData right = NextBossRight;
            string matName = defeated.MaterialName;
            int    matAmt  = defeated.MaterialReward;
            PathMapUI.Instance?.Show(left, right, matName, matAmt);
        }
    }
}
