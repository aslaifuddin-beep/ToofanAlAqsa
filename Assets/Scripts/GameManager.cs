using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, MissionComplete, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public int PlayerXP { get; private set; }
    public int PlayerLevel { get; private set; } = 1;
    public int TotalKills { get; private set; }
    public int MissionsCompleted { get; private set; }

    [Header("Level Progression")]
    public int xpPerKill = 50;
    public int xpPerMission = 500;
    public AnimationCurve xpCurve = AnimationCurve.Linear(1, 1000, 30, 50000);

    private int currentXpThreshold = 1000;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentXpThreshold = Mathf.RoundToInt(xpCurve.Evaluate(PlayerLevel));
    }

    public void AddXP(int amount)
    {
        PlayerXP += amount;
        while (PlayerXP >= currentXpThreshold)
        {
            PlayerXP -= currentXpThreshold;
            PlayerLevel++;
            currentXpThreshold = Mathf.RoundToInt(xpCurve.Evaluate(PlayerLevel));
            OnPlayerLevelUp();
        }
    }

    void OnPlayerLevelUp()
    {
        UpgradeSystem.Instance?.AddUpgradePoint();
    }

    public void AddKill()
    {
        TotalKills++;
        AddXP(xpPerKill);
    }

    public void CompleteMission()
    {
        MissionsCompleted++;
        AddXP(xpPerMission);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Time.timeScale = newState == GameState.Paused ? 0f : 1f;
        Cursor.lockState = newState == GameState.Playing ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = newState != GameState.Playing;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
