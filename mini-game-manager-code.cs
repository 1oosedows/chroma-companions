// MiniGameManager.cs - Manages mini-games and their integration with the main game
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    private static MiniGameManager _instance;
    public static MiniGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("MiniGameManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Mini-Game References")]
    [SerializeField] private List<MiniGameInfo> availableMiniGames = new List<MiniGameInfo>();
    
    [Header("UI References")]
    [SerializeField] private GameObject miniGameSelectionPanel;
    [SerializeField] private Transform miniGameButtonContainer;
    [SerializeField] private GameObject miniGameButtonPrefab;
    
    // Current active mini-game
    private MiniGameBase currentMiniGame;
    private GameObject currentMiniGameObject;
    
    // Events
    public Action<MiniGameInfo> OnMiniGameSelected;
    public Action<MiniGameInfo, int, int> OnMiniGameCompleted; // game info, currency earned, exp earned
    public Action OnReturnToMainGame;
    
    // Tracking
    private Dictionary<string, int> miniGameHighScores = new Dictionary<string, int>();
    private Dictionary<string, DateTime> miniGameLastPlayed = new Dictionary<string, DateTime>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Load saved data
        LoadMiniGameData();
        
        // Setup UI
        SetupMiniGameButtons();
        
        // Initially hide the selection panel
        if (miniGameSelectionPanel != null)
            miniGameSelectionPanel.SetActive(false);
    }
    
    private void LoadMiniGameData()
    {
        // In a real game, this would load from UserData or PlayerPrefs
        // For now, just initialize empty dictionaries
        miniGameHighScores = new Dictionary<string, int>();
        miniGameLastPlayed = new Dictionary<string, DateTime>();
        
        foreach (MiniGameInfo gameInfo in availableMiniGames)
        {
            // Check if we have saved high scores
            string highScoreKey = $"MiniGame_{gameInfo.gameId}_HighScore";
            if (PlayerPrefs.HasKey(highScoreKey))
            {
                miniGameHighScores[gameInfo.gameId] = PlayerPrefs.GetInt(highScoreKey);
            }
            else
            {
                miniGameHighScores[gameInfo.gameId] = 0;
            }
            
            // Check for last played timestamp
            string lastPlayedKey = $"MiniGame_{gameInfo.gameId}_LastPlayed";
            if (PlayerPrefs.HasKey(lastPlayedKey))
            {
                long ticks = Convert.ToInt64(PlayerPrefs.GetString(lastPlayedKey));
                miniGameLastPlayed[gameInfo.gameId] = new DateTime(ticks);
            }
            else
            {
                miniGameLastPlayed[gameInfo.gameId] = DateTime.MinValue;
            }
        }
    }
    
    private void SetupMiniGameButtons()
    {
        if (miniGameButtonContainer == null || miniGameButtonPrefab == null)
            return;
            
        // Clear existing buttons
        foreach (Transform child in miniGameButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each mini-game
        foreach (MiniGameInfo gameInfo in availableMiniGames)
        {
            GameObject buttonObj = Instantiate(miniGameButtonPrefab, miniGameButtonContainer);
            MiniGameButton button = buttonObj.GetComponent<MiniGameButton>();
            
            if (button != null)
            {
                // Set up button
                button.Setup(gameInfo, GetHighScore(gameInfo.gameId), GetLastPlayedText(gameInfo.gameId));
                
                // Add click handler
                button.OnButtonClicked += () => StartMiniGame(gameInfo);
            }
        }
    }
    
    public void ShowMiniGameSelection()
    {
        if (miniGameSelectionPanel != null)
            miniGameSelectionPanel.SetActive(true);
    }
    
    public void HideMiniGameSelection()
    {
        if (miniGameSelectionPanel != null)
            miniGameSelectionPanel.SetActive(false);
    }
    
    public void StartMiniGame(MiniGameInfo gameInfo)
    {
        // Hide selection panel
        HideMiniGameSelection();
        
        // Pause main game
        GameManager.Instance.PauseGame();
        
        // Instantiate the mini-game
        GameObject miniGameObj = Instantiate(gameInfo.gamePrefab);
        currentMiniGameObject = miniGameObj;
        
        // Get the mini-game component
        MiniGameBase miniGame = miniGameObj.GetComponent<MiniGameBase>();
        if (miniGame == null)
        {
            Debug.LogError($"Mini-game prefab {gameInfo.gameName} does not have a MiniGameBase component!");
            Destroy(miniGameObj);
            return;
        }
        
        currentMiniGame = miniGame;
        
        // Subscribe to events
        miniGame.OnGameCompleted += (currency, exp) => HandleMiniGameCompleted(gameInfo, currency, exp);
        
        // Start the mini-game
        miniGame.StartGame();
        
        // Update last played time
        miniGameLastPlayed[gameInfo.gameId] = DateTime.Now;
        SaveMiniGameData(gameInfo.gameId);
        
        // Notify listeners
        OnMiniGameSelected?.Invoke(gameInfo);
    }
    
    private void HandleMiniGameCompleted(MiniGameInfo gameInfo, int currencyEarned, int expEarned)
    {
        // Check if this is a new high score
        int score = currentMiniGame.currentScore;
        if (score > GetHighScore(gameInfo.gameId))
        {
            miniGameHighScores[gameInfo.gameId] = score;
            SaveMiniGameData(gameInfo.gameId);
        }
        
        // Notify listeners
        OnMiniGameCompleted?.Invoke(gameInfo, currencyEarned, expEarned);
        
        // Clean up and return to main game after a delay
        StartCoroutine(ReturnToMainGameDelayed(2f));
    }
    
    private IEnumerator ReturnToMainGameDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Clean up mini-game
        if (currentMiniGameObject != null)
        {
            Destroy(currentMiniGameObject);
            currentMiniGameObject = null;
            currentMiniGame = null;
        }
        
        // Resume main game
        GameManager.Instance.ResumeGame();
        
        // Notify listeners
        OnReturnToMainGame?.Invoke();
    }
    
    public void QuitCurrentMiniGame()
    {
        if (currentMiniGame != null)
        {
            currentMiniGame.EndGame();
        }
        else
        {
            // No active mini-game, just return to main game
            ReturnToMainGame();
        }
    }
    
    public void ReturnToMainGame()
    {
        // Clean up mini-game
        if (currentMiniGameObject != null)
        {
            Destroy(currentMiniGameObject);
            currentMiniGameObject = null;
            currentMiniGame = null;
        }
        
        // Resume main game
        GameManager.Instance.ResumeGame();
        
        // Notify listeners
        OnReturnToMainGame?.Invoke();
    }
    
    private int GetHighScore(string gameId)
    {
        if (miniGameHighScores.ContainsKey(gameId))
        {
            return miniGameHighScores[gameId];
        }
        
        return 0;
    }
    
    private string GetLastPlayedText(string gameId)
    {
        if (miniGameLastPlayed.ContainsKey(gameId) && miniGameLastPlayed[gameId] > DateTime.MinValue)
        {
            DateTime lastPlayed = miniGameLastPlayed[gameId];
            TimeSpan timeSince = DateTime.Now - lastPlayed;
            
            if (timeSince.TotalHours < 1)
            {
                return $"{(int)timeSince.TotalMinutes} minutes ago";
            }
            else if (timeSince.TotalDays < 1)
            {
                return $"{(int)timeSince.TotalHours} hours ago";
            }
            else if (timeSince.TotalDays < 30)
            {
                return $"{(int)timeSince.TotalDays} days ago";
            }
            else
            {
                return lastPlayed.ToString("MMM d, yyyy");
            }
        }
        
        return "Never played";
    }
    
    private void SaveMiniGameData(string gameId)
    {
        // Save high score
        if (miniGameHighScores.ContainsKey(gameId))
        {
            PlayerPrefs.SetInt($"MiniGame_{gameId}_HighScore", miniGameHighScores[gameId]);
        }
        
        // Save last played time
        if (miniGameLastPlayed.ContainsKey(gameId))
        {
            PlayerPrefs.SetString($"MiniGame_{gameId}_LastPlayed", miniGameLastPlayed[gameId].Ticks.ToString());
        }
        
        PlayerPrefs.Save();
    }
    
    // Get daily bonus eligibility
    public bool IsDailyBonusAvailable(string gameId)
    {
        if (miniGameLastPlayed.ContainsKey(gameId) && miniGameLastPlayed[gameId] > DateTime.MinValue)
        {
            DateTime lastPlayed = miniGameLastPlayed[gameId];
            DateTime today = DateTime.Today;
            DateTime lastPlayedDay = lastPlayed.Date;
            
            // Check if last played was before today
            return lastPlayedDay < today;
        }
        
        // Never played before, so bonus is available
        return true;
    }
}

[Serializable]
public class MiniGameInfo
{
    public string gameId;
    public string gameName;
    public string description;
    public Sprite gameIcon;
    public GameObject gamePrefab;
    public int minPlayerLevel = 1;
    public bool isDailyBonusEnabled = false;
    public int dailyBonusCurrency = 10;
}

// Helper component for mini-game selection buttons
public class MiniGameButton : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text nameText;
    [SerializeField] private UnityEngine.UI.Text descriptionText;
    [SerializeField] private UnityEngine.UI.Text highScoreText;
    [SerializeField] private UnityEngine.UI.Text lastPlayedText;
    [SerializeField] private UnityEngine.UI.Image gameIcon;
    [SerializeField] private GameObject dailyBonusIndicator;
    [SerializeField] private GameObject lockedOverlay;
    
    private MiniGameInfo gameInfo;
    
    public event Action OnButtonClicked;
    
    public void Setup(MiniGameInfo info, int highScore, string lastPlayed)
    {
        gameInfo = info;
        
        // Set texts
        if (nameText != null)
            nameText.text = info.gameName;
            
        if (descriptionText != null)
            descriptionText.text = info.description;
            
        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore}";
            
        if (lastPlayedText != null)
            lastPlayedText.text = $"Last Played: {lastPlayed}";
            
        // Set icon
        if (gameIcon != null && info.gameIcon != null)
            gameIcon.sprite = info.gameIcon;
            
        // Check daily bonus
        bool hasDailyBonus = info.isDailyBonusEnabled && 
                             MiniGameManager.Instance.IsDailyBonusAvailable(info.gameId);
                             
        if (dailyBonusIndicator != null)
            dailyBonusIndicator.SetActive(hasDailyBonus);
            
        // Check if locked
        bool isLocked = GameManager.Instance.GetPlayerLevel() < info.minPlayerLevel;
        
        if (lockedOverlay != null)
            lockedOverlay.SetActive(isLocked);
    }
    
    public void OnClick()
    {
        // Check if mini-game is locked
        if (GameManager.Instance.GetPlayerLevel() < gameInfo.minPlayerLevel)
        {
            // Show locked message
            UIManager.Instance.ShowMessage($"Reach level {gameInfo.minPlayerLevel} to unlock {gameInfo.gameName}!");
            return;
        }
        
        OnButtonClicked?.Invoke();
    }
}

// Extensions for UIManager to show messages
public static class UIManagerExtensions
{
    public static void ShowMessage(this UIManager uiManager, string message)
    {
        // In a real implementation, you would have a method in UIManager to show messages
        Debug.Log($"UI Message: {message}");
    }
}
