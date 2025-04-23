// MiniGameUI.cs - UI system for mini-games
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameUI : MonoBehaviour
{
    [Header("General UI Elements")]
    [SerializeField] private Text gameTitleText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Image timerBar;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text currencyRewardText;
    [SerializeField] private Text expRewardText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;
    
    [Header("Game Start UI")]
    [SerializeField] private GameObject gameStartPanel;
    [SerializeField] private Text gameDescriptionText;
    [SerializeField] private Text controlsText;
    [SerializeField] private Button startButton;
    
    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Button nextTutorialButton;
    [SerializeField] private Button skipTutorialButton;
    [SerializeField] private Image[] tutorialPages;
    
    // References
    private MiniGameBase miniGame;
    private int currentTutorialPage = 0;
    
    // Game state
    private bool isPaused = false;
    private bool isGameActive = false;
    
    public void Initialize(MiniGameBase game, string title, string description, string controls)
    {
        miniGame = game;
        
        // Set text values
        if (gameTitleText != null)
            gameTitleText.text = title;
            
        if (gameDescriptionText != null)
            gameDescriptionText.text = description;
            
        if (controlsText != null)
            controlsText.text = controls;
            
        // Initialize score and time
        UpdateScore(0);
        UpdateTime(game.timeLimit);
        
        // Hide panels initially
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (gameStartPanel != null)
            gameStartPanel.SetActive(true);
            
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
            
        // Set up button listeners
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
            
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
            
        if (startButton != null)
            startButton.onClick.AddListener(ShowTutorial);
            
        if (nextTutorialButton != null)
            nextTutorialButton.onClick.AddListener(NextTutorialPage);
            
        if (skipTutorialButton != null)
            skipTutorialButton.onClick.AddListener(StartGame);
            
        // Subscribe to game events
        miniGame.OnScoreChanged += UpdateScore;
        miniGame.OnTimeChanged += UpdateTime;
        miniGame.OnGameCompleted += ShowGameOver;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (miniGame != null)
        {
            miniGame.OnScoreChanged -= UpdateScore;
            miniGame.OnTimeChanged -= UpdateTime;
            miniGame.OnGameCompleted -= ShowGameOver;
        }
        
        // Remove button listeners
        if (pauseButton != null)
            pauseButton.onClick.RemoveAllListeners();
            
        if (retryButton != null)
            retryButton.onClick.RemoveAllListeners();
            
        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
            
        if (startButton != null)
            startButton.onClick.RemoveAllListeners();
            
        if (nextTutorialButton != null)
            nextTutorialButton.onClick.RemoveAllListeners();
            
        if (skipTutorialButton != null)
            skipTutorialButton.onClick.RemoveAllListeners();
    }
    
    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
    
    private void UpdateTime(float time)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
        
        if (timerBar != null && miniGame != null)
        {
            timerBar.fillAmount = time / miniGame.timeLimit;
        }
    }
    
    private void TogglePause()
    {
        isPaused = !isPaused;
        
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
            
        if (isPaused)
            miniGame.PauseGame();
        else
            miniGame.ResumeGame();
    }
    
    private void ShowTutorial()
    {
        if (gameStartPanel != null)
            gameStartPanel.SetActive(false);
            
        if (tutorialPanel != null && tutorialPages.Length > 0)
        {
            tutorialPanel.SetActive(true);
            currentTutorialPage = 0;
            
            // Show first page
            for (int i = 0; i < tutorialPages.Length; i++)
            {
                tutorialPages[i].gameObject.SetActive(i == 0);
            }
        }
        else
        {
            // No tutorial pages, just start the game
            StartGame();
        }
    }
    
    private void NextTutorialPage()
    {
        currentTutorialPage++;
        
        if (currentTutorialPage >= tutorialPages.Length)
        {
            // End of tutorial, start game
            StartGame();
        }
        else
        {
            // Show next page
            for (int i = 0; i < tutorialPages.Length; i++)
            {
                tutorialPages[i].gameObject.SetActive(i == currentTutorialPage);
            }
        }
    }
    
    private void StartGame()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
            
        isGameActive = true;
        miniGame.StartGame();
    }
    
    private void ShowGameOver(int currencyReward, int expReward)
    {
        isGameActive = false;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update text fields
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {miniGame.currentScore}";
                
            if (highScoreText != null)
            {
                int highScore = GetHighScore();
                
                if (miniGame.currentScore > highScore)
                {
                    // New high score
                    SetHighScore(miniGame.currentScore);
                    highScoreText.text = $"New High Score: {miniGame.currentScore}";
                }
                else
                {
                    highScoreText.text = $"High Score: {highScore}";
                }
            }
            
            if (currencyRewardText != null)
                currencyRewardText.text = $"Currency: +{currencyReward}";
                
            if (expRewardText != null)
                expRewardText.text = $"Experience: +{expReward}";
        }
    }
    
    private int GetHighScore()
    {
        string key = $"MiniGame_{miniGame.GetType().Name}_HighScore";
        return PlayerPrefs.GetInt(key, 0);
    }
    
    private void SetHighScore(int score)
    {
        string key = $"MiniGame_{miniGame.GetType().Name}_HighScore";
        PlayerPrefs.SetInt(key, score);
        PlayerPrefs.Save();
    }
    
    private void RetryGame()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Reset game
        miniGame.EndGame();
        
        // Short delay before restarting
        StartCoroutine(DelayedRestart());
    }
    
    private IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(0.5f);
        StartGame();
    }
    
    private void ExitGame()
    {
        // End game and return to main game
        miniGame.EndGame();
        
        // Let the MiniGameManager handle the transition back
        MiniGameManager.Instance.ReturnToMainGame();
    }
    
    // Called by pause menu buttons
    public void OnResumeButtonClicked()
    {
        TogglePause();
    }
    
    public void OnRestartButtonClicked()
    {
        if (isPaused)
        {
            isPaused = false;
            pausePanel.SetActive(false);
        }
        
        RetryGame();
    }
    
    public void OnQuitButtonClicked()
    {
        ExitGame();
    }
}
