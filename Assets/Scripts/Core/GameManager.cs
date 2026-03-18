using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Upgrading, Dead }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State  { get; private set; } = GameState.Playing;
    public int Wave         { get; private set; }
    public int Score        { get; private set; }
    public int TotalKills   { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called by SceneSetup after everything is wired up
    public void StartGame()
    {
        Wave      = 0;
        Score     = 0;
        TotalKills = 0;
        NextWave();
    }

    public void NextWave()
    {
        Wave++;
        State = GameState.Playing;
        WaveManager.Instance.BeginWave(Wave);
        HUDManager.Instance?.SetWave(Wave);
    }

    public void WaveCleared(bool isBoss)
    {
        State = GameState.Upgrading;
        UpgradeManager.Instance.ShowUpgradeSelection(isBoss);
    }

    public void UpgradePicked()
    {
        NextWave();
    }

    public void EnemyKilled(int pts)
    {
        Score += pts;
        TotalKills++;
        HUDManager.Instance?.SetScore(Score);
    }

    public void PlayerDied()
    {
        if (State == GameState.Dead) return;
        State = GameState.Dead;
        UpgradeManager.Instance?.SaveUnlocks();
        DeathScreenUI.Instance?.Show(Wave, Score, TotalKills);
    }

    public void Restart() => SceneManager.LoadScene(0);
}
