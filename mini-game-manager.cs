// MiniGameManager.cs - Handles mini-game functionality and rewards
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MiniGameType
{
    ColorMatch,
    RainbowRun,
    PetPuzzle,
    MemoryMatch,
    BubbleSort
}

[Serializable]
public class MiniGameData
{
    public string gameId;
    public string displayName;
    public MiniGameType gameType;
    public Sprite icon;
    public GameObject gamePrefab;
    public int unlockLevel;
    public int costToPlay;
    public List<MiniGameReward> rewards = new List<MiniGameReward>();
}

[Serializable]
public class MiniGameReward
{
    public int minScore;
    public int currencyReward;
    public int experienceReward;
    public float happinessBoost;
    public List<string> potentialItems = new List<string>();
    public float rareItemChance;
}

[Serializable]
public class PlayerMiniGameStats
{
    public string gameId;
    public int highScore;
    public int totalPlays;
    public int totalRewardsEarned;
    public DateTime lastPlayed;
}

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
    
    [Header("Mini-Games")]
    [SerializeField] private List<MiniGameData> availableGames = new List<MiniGameData>();
    
    [Header("Settings")]
    [SerializeField] private int dailyBonusThreshold = 3; // Play 3 games for a bonus
    [SerializeField] private Transform miniGameContainer;
    [SerializeField] private GameObject miniGameCanvas;
    
    // Current state
    private MiniGameData currentGame;
    private PetBase activePet;
    private Dictionary<string, PlayerMiniGameStats> playerStats = new Dictionary<string, PlayerMiniGameStats>();
    private int gamesPlayedToday = 0;
    
    // Events
    public Action<MiniGameData, int> OnGameCompleted;
    public Action<string, int> OnHighScoreAchieved;
    public Action<int> OnDailyBonusEarned;
    
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
        // Load saved mini-game stats
        LoadMiniGameStats();
        
        // Reset daily counter if needed
        CheckDailyReset();
    }
    
    public List<MiniGameData> GetAvailableMiniGames()
    {
        List<MiniGameData> unlocked = new List<MiniGameData>();
        int playerLevel = GameManager.Instance.GetPlayerLevel();
        
        foreach (MiniGameData game in availableGames)
        {
            if (playerLevel >= game.unlockLevel)
            {
                unlocked.Add(game);
            }
        }
        
        return unlocked;
    }
    
    public void StartMiniGame(string gameId, PetBase pet)
    {
        if (pet == null)
        {
            Debug.LogError("Cannot start mini-game without a pet!");
            return;
        }
        
        // Find the game data
        MiniGameData gameData = availableGames.Find(g => g.gameId == gameId);
        
        if (gameData == null)
        {
            Debug.LogError($"Mini-game with ID {gameId} not found!");
            return;
        }
        
        // Check if player has enough currency
        if (!GameManager.Instance.SpendCurrency(gameData.costToPlay))
        {
            UIManager.Instance.ShowMessage("Not enough currency to play this game!");
            return;
        }
        
        // Set current game and pet
        currentGame = gameData;
        activePet = pet;
        
        // Instantiate the game prefab
        if (gameData.gamePrefab != null && miniGameContainer != null)
        {
            // Show the mini-game canvas
            if (miniGameCanvas != null)
            {
                miniGameCanvas.SetActive(true);
            }
            
            GameObject gameInstance = Instantiate(gameData.gamePrefab, miniGameContainer);
            
            // Initialize the game with the pet
            MiniGameBase game = gameInstance.GetComponent<MiniGameBase>();
            if (game != null)
            {
                game.Initialize(pet);
                
                // Apply pet-specific bonuses
                ApplyPetBonuses(game, pet);
            }
        }
        else
        {
            Debug.LogError("Cannot start mini-game: Missing prefab or container!");
        }
    }
    
    private void ApplyPetBonuses(MiniGameBase game, PetBase pet)
    {
        // Apply type-specific bonuses based on pet type
        
        // Check for PuzzleFox bonuses
        PuzzleFox fox = pet as PuzzleFox;
        if (fox != null)
        {
            if (currentGame.gameType == MiniGameType.PetPuzzle || 
                currentGame.gameType == MiniGameType.MemoryMatch)
            {
                game.ApplyScoreMultiplier(1.0f + fox.GetPuzzleGameBonus());
            }
            
            if (currentGame.gameType == MiniGameType.RainbowRun)
            {
                game.ApplyTimeBonus(fox.GetTimedGameBonus());
            }
        }
        
        // Check general element-based bonuses
        switch (pet.Element)
        {
            case PetElement.Fire:
                // Fire pets get bonus in action games
                if (currentGame.gameType == MiniGameType.RainbowRun)
                {
                    game.ApplyScoreMultiplier(1.1f);
                }
                break;
                
            case PetElement.Water:
                // Water pets get bonus in bubble games
                if (currentGame.gameType == MiniGameType.BubbleSort)
                {
                    game.ApplyScoreMultiplier(1.15f);
                }
                break;
                
            case PetElement.Air:
                // Air pets get bonus in all timed games
                game.ApplyTimeBonus(0.05f);
                break;
                
            case PetElement.Light:
                // Light pets get small bonus in all games
                game.ApplyScoreMultiplier(1.05f);
                break;
        }
    }
    
    public void CompleteMiniGame(int score)
    {
        if (currentGame == null || activePet == null)
        {
            Debug.LogError("Cannot complete mini-game: No active game or pet!");
            return;
        }
        
        // Hide the mini-game canvas
        if (miniGameCanvas != null)
        {
            miniGameCanvas.SetActive(false);
        }
        
        // Update player stats
        UpdateGameStats(currentGame.gameId, score);
        
        // Calculate rewards
        MiniGameReward reward = GetRewardForScore(score);
        if (reward != null)
        {
            // Award currency
            GameManager.Instance.AddCurrency(reward.currencyReward);
            
            // Award experience to pet
            activePet.AddExperience(reward.experienceReward);
            
            // Boost pet happiness
            if (reward.happinessBoost > 0)
            {
                // Need to add this method to PetBase class
                // activePet.BoostHappiness(reward.happinessBoost);
            }
            
            // Check for item rewards
            CheckForItemRewards(reward);
        }
        
        // Record special stats for PuzzleFox
        if (activePet is PuzzleFox puzzleFox)
        {
            bool isHighScore = IsHighScore(currentGame.gameId, score);
            puzzleFox.RecordMiniGameScore(currentGame.gameId, score, isHighScore);
            
            // Check for bonus prizes
            if (puzzleFox.CheckPrizeFinder())
            {
                // Award a bonus item or currency
                GameManager.Instance.AddCurrency(UnityEngine.Random.Range(10, 30));
                UIManager.Instance.ShowMessage("Your fox found a bonus prize!");
            }
        }
        
        // Check for daily bonus
        gamesPlayedToday++;
        CheckDailyBonus();
        
        // Trigger event
        OnGameCompleted?.Invoke(currentGame, score);
        
        // Clear current game variables
        currentGame = null;
        activePet = null;
    }
    
    private MiniGameReward GetRewardForScore(int score)
    {
        // Find the highest tier reward that the score qualifies for
        MiniGameReward bestReward = null;
        
        foreach (MiniGameReward reward in currentGame.rewards)
        {
            if (score >= reward.minScore && (bestReward == null || reward.minScore > bestReward.minScore))
            {
                bestReward = reward;
            }
        }
        
        return bestReward;
    }
    
    private void CheckForItemRewards(MiniGameReward reward)
    {
        if (reward.potentialItems.Count == 0)
            return;
            
        // Calculate chance of getting an item
        float chance = reward.rareItemChance;
        
        // Roll for item
        if (UnityEngine.Random.Range(0f, 1f) <= chance)
        {
            // Select random item from potential rewards
            int itemIndex = UnityEngine.Random.Range(0, reward.potentialItems.Count);
            string itemId = reward.potentialItems[itemIndex];
            
            // In a real implementation, this would add the item to inventory
            // For now, just show a message
            UIManager.Instance.ShowMessage($"You won a special item: {itemId}!");
        }
    }
    
    private void UpdateGameStats(string gameId, int score)
    {
        // Create stats entry if it doesn't exist
        if (!playerStats.ContainsKey(gameId))
        {
            playerStats[gameId] = new PlayerMiniGameStats
            {
                gameId = gameId,
                highScore = 0,
                totalPlays = 0,
                totalRewardsEarned = 0,
                lastPlayed = DateTime.Now
            };
        }
        
        // Update stats
        PlayerMiniGameStats stats = playerStats[gameId];
        stats.totalPlays++;
        stats.lastPlayed = DateTime.Now;
        
        // Check for high score
        if (score > stats.highScore)
        {
            stats.highScore = score;
            OnHighScoreAchieved?.Invoke(gameId, score);
        }
        
        // Save stats
        SaveMiniGameStats();
    }
    
    private bool IsHighScore(string gameId, int score)
    {
        if (playerStats.ContainsKey(gameId))
        {
            return score > playerStats[gameId].highScore;
        }
        
        return true; // First time playing is always a high score
    }
    
    private void CheckDailyBonus()
    {
        if (gamesPlayedToday == dailyBonusThreshold)
        {
            // Award daily bonus
            int bonus = 50; // Base bonus
            
            // Higher player levels get better bonuses
            int playerLevel = GameManager.Instance.GetPlayerLevel();
            bonus += playerLevel * 5;
            
            GameManager.Instance.AddCurrency(bonus);
            OnDailyBonusEarned?.Invoke(bonus);
            
            UIManager.Instance.ShowMessage($"Daily mini-game bonus: {bonus} coins!");
        }
    }
    
    private void CheckDailyReset()
    {
        // Check if we need to reset the daily counter
        // In a real game, this would check against the last login day
        
        // For now, just simulate with random value for testing
        if (UnityEngine.Random.Range(0, 10) < 3)
        {
            gamesPlayedToday = 0;
        }
    }
    
    private void LoadMiniGameStats()
    {
        // In a real game, this would load from PlayerPrefs or server
        // For simplicity, just initialize with empty data for now
        playerStats.Clear();
        
        foreach (MiniGameData game in availableGames)
        {
            playerStats[game.gameId] = new PlayerMiniGameStats
            {
                gameId = game.gameId,
                highScore = 0,
                totalPlays = 0,
                totalRewardsEarned = 0,
                lastPlayed = DateTime.MinValue
            };
        }
    }
    
    private void SaveMiniGameStats()
    {
        // In a real game, this would save to PlayerPrefs or server
        // For simplicity, do nothing for now
        // This would be implemented with JSON serialization similar to other systems
    }
    
    // Base class that all mini-games will extend
    public abstract class MiniGameBase : MonoBehaviour
    {
        protected PetBase associatedPet;
        protected float scoreMultiplier = 1.0f;
        protected float timeBonus = 0f;
        
        public virtual void Initialize(PetBase pet)
        {
            associatedPet = pet;
        }
        
        public void ApplyScoreMultiplier(float multiplier)
        {
            scoreMultiplier *= multiplier;
        }
        
        public void ApplyTimeBonus(float bonus)
        {
            timeBonus += bonus;
        }
        
        protected virtual int CalculateFinalScore(int rawScore)
        {
            return Mathf.RoundToInt(rawScore * scoreMultiplier);
        }
        
        // Call this when game is completed
        protected void EndGame(int score)
        {
            int finalScore = CalculateFinalScore(score);
            MiniGameManager.Instance.CompleteMiniGame(finalScore);
        }
    }
}
