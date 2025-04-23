// MiniGameBase.cs - Abstract base class for all mini-games
using System;
using System.Collections;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] protected string gameName;
    [SerializeField] protected string description;
    [SerializeField] protected int difficultyLevel = 1;
    [SerializeField] protected int currencyReward = 10;
    [SerializeField] protected int experienceReward = 5;
    [SerializeField] protected float timeLimit = 60f; // Time in seconds
    
    // Game state
    protected bool isGameActive = false;
    protected bool isGamePaused = false;
    protected float gameTimer = 0f;
    protected int currentScore = 0;
    
    // Events
    public Action<int, int> OnGameCompleted; // currency, experience
    public Action<int> OnScoreChanged;
    public Action<float> OnTimeChanged;
    public Action OnGameStarted;
    public Action OnGamePaused;
    public Action OnGameResumed;
    
    // References
    [SerializeField] protected GameObject gameUI;
    
    // Abstract methods that derived games must implement
    public abstract void StartGame();
    public abstract void PauseGame();
    public abstract void ResumeGame();
    public abstract void EndGame();
    
    protected virtual void Update()
    {
        if (isGameActive && !isGamePaused)
        {
            // Update timer
            gameTimer -= Time.deltaTime;
            OnTimeChanged?.Invoke(gameTimer);
            
            // Check if time ran out
            if (gameTimer <= 0f)
            {
                gameTimer = 0f;
                EndGame();
            }
        }
    }
    
    // Common functionality
    protected virtual void InitializeGame()
    {
        currentScore = 0;
        gameTimer = timeLimit;
        isGameActive = true;
        isGamePaused = false;
        
        // Update UI
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(gameTimer);
        
        // Show game UI
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }
        
        OnGameStarted?.Invoke();
    }
    
    // Add points to the current score
    public virtual void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    // Award rewards based on score and difficulty when game ends
    protected virtual void AwardRewards()
    {
        // Calculate rewards based on score and difficulty
        int currencyEarned = Mathf.Max(1, currencyReward * difficultyLevel * currentScore / 100);
        int expEarned = Mathf.Max(1, experienceReward * difficultyLevel * currentScore / 100);
        
        // Award to player
        GameManager.Instance.AddCurrency(currencyEarned);
        
        // If we had a reference to the current active pet, we could add experience
        // GameManager.Instance.AddPetExperience(expEarned);
        
        // Trigger event
        OnGameCompleted?.Invoke(currencyEarned, expEarned);
    }
    
    // Common game ending logic
    protected virtual void FinalizeGame()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        
        // Award rewards based on performance
        AwardRewards();
        
        // Hide game UI after a short delay
        StartCoroutine(DelayedUIHide(1.5f));
    }
    
    private IEnumerator DelayedUIHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
    }
}
