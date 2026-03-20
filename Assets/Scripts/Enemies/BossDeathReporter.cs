using UnityEngine;

/// <summary>
/// Lightweight component attached to path-lattice boss GameObjects.
/// Waits for the BossBase to report death (via OnDestroy) then
/// notifies GameManager so materials can be awarded.
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossDeathReporter : MonoBehaviour
{
    public string BossID;

    void OnDestroy()
    {
        // Only fire if the game is still running (not a scene unload)
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State == GameState.Dead) return;

        BossData data = BossRegistry.GetByID(BossID);
        if (data != null)
            GameManager.Instance.OnBossDefeated(data);
    }
}
