// PuzzleFox.cs - A specific pet implementation that excels at mini-games
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleFox : PetBase
{
    [Header("Fox Specific")]
    [SerializeField] private float puzzleBonus = 0.2f; // 20% bonus for puzzle games
    [SerializeField] private float trickEnergyCost = 5f;
    [SerializeField] private GameObject sparkleEffectPrefab;
    [SerializeField] private int maxTricksPerDay = 5;
    
    private int tricksPerformedToday = 0;
    private List<MiniGameScore> recentGameScores = new List<MiniGameScore>();
    
    // Special fox abilities
    public enum FoxAbility
    {
        SmartBonus,     // Bonus to puzzle game scores
        PrizeFinder,    // Chance to find rare items
        QuickReflex,    // Bonus for timed mini-games
        TreasureHunter, // Can find hidden treasure in the world
        MasterTrick     // Special performance for extra rewards
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, FoxAbility> levelAbilities = new Dictionary<int, FoxAbility>()
    {
        { 3, FoxAbility.SmartBonus },
        { 6, FoxAbility.PrizeFinder },
        { 9, FoxAbility.QuickReflex },
        { 14, FoxAbility.TreasureHunter },
        { 20, FoxAbility.MasterTrick }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for fox
        element = PetElement.Air; // Foxes start as Air element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Foxes have balanced stats with slightly higher energy
        stats.energy = 110f;
        stats.happiness = 90f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for fox-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Foxes gain more experience from playing
        AddExperience(10);
        
        // Random chance to perform a trick
        if (Random.Range(0, 100) < 60)
        {
            PerformTrick();
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Foxes are omnivorous but prefer meat
        if (food.preferredBy == PetElement.Fire || food.preferredBy == PetElement.Dark)
        {
            stats.happiness = Mathf.Min(stats.happiness + 5f, 100f);
            stats.energy = Mathf.Min(stats.energy + 10f, 110f);
        }
    }
    
    public void PerformTrick()
    {
        if (stats.energy < trickEnergyCost || tricksPerformedToday >= maxTricksPerDay)
            return;
        
        stats.energy -= trickEnergyCost;
        tricksPerformedToday++;
        
        // Play trick animation
        animator.SetTrigger("Trick");
        
        // Create sparkle effect
        if (sparkleEffectPrefab)
        {
            GameObject sparkle = Instantiate(sparkleEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(sparkle, 2f);
        }
        
        // Add a small amount of currency
        GameManager.Instance.AddCurrency(Random.Range(1, 5));
        
        // Extra effects for master trick
        if (HasAbility(FoxAbility.MasterTrick) && Random.Range(0, 100) < 30)
        {
            PerformMasterTrick();
        }
    }
    
    public void PerformMasterTrick()
    {
        if (!HasAbility(FoxAbility.MasterTrick))
            return;
        
        animator.SetTrigger("MasterTrick");
        
        // Visual effects
        if (sparkleEffectPrefab)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1.5f), Random.Range(-1f, 1f));
                GameObject sparkle = Instantiate(sparkleEffectPrefab, transform.position + offset, Quaternion.identity);
                Destroy(sparkle, 2f);
            }
        }
        
        // Significant reward
        GameManager.Instance.AddCurrency(Random.Range(10, 25));
        
        // Bonus experience
        AddExperience(25);
    }
    
    public void StartTreasureHunt()
    {
        if (!HasAbility(FoxAbility.TreasureHunter) || stats.energy < 30f)
            return;
        
        // This would be connected to a mini-game or world exploration in the full game
        // For now just simulate the result
        
        stats.energy -= 30f;
        animator.SetTrigger("Dig");
        
        // Simulate treasure hunting success
        int successRate = Mathf.RoundToInt(stats.happiness / 10f) + 20; // 20-30% base chance
        
        if (Random.Range(0, 100) < successRate)
        {
            // Found treasure!
            GameManager.Instance.AddCurrency(Random.Range(20, 50));
            
            // In a full game, this would also potentially award rare items
            
            // Add experience for success
            AddExperience(30);
        }
        else
        {
            // Found nothing - small consolation prize
            GameManager.Instance.AddCurrency(Random.Range(1, 5));
            
            // Small experience amount
            AddExperience(5);
        }
    }
    
    // Record scores from mini-games
    public void RecordMiniGameScore(string gameId, int score, bool isHighScore)
    {
        MiniGameScore gameScore = new MiniGameScore
        {
            gameId = gameId,
            score = score,
            isHighScore = isHighScore,
            timestamp = System.DateTime.Now
        };
        
        recentGameScores.Add(gameScore);
        
        // Keep only last 10 scores
        if (recentGameScores.Count > 10)
        {
            recentGameScores.RemoveAt(0);
        }
        
        // Add happiness for high scores
        if (isHighScore)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 100f);
            AddExperience(20);
        }
    }
    
    // Calculate bonus for puzzle mini-games
    public float GetPuzzleGameBonus()
    {
        float bonus = 0f;
        
        if (HasAbility(FoxAbility.SmartBonus))
        {
            bonus += puzzleBonus;
        }
        
        return bonus;
    }
    
    // Calculate bonus for timed mini-games
    public float GetTimedGameBonus()
    {
        float bonus = 0f;
        
        if (HasAbility(FoxAbility.QuickReflex))
        {
            bonus += 0.15f; // 15% time bonus
        }
        
        return bonus;
    }
    
    // Check for prize finding 
    public bool CheckPrizeFinder()
    {
        if (!HasAbility(FoxAbility.PrizeFinder))
            return false;
        
        // Base 10% chance, increased by happiness
        float chance = 10f + (stats.happiness / 10f);
        
        return Random.Range(0, 100) < chance;
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset tricks counter
        tricksPerformedToday = 0;
        
        // Foxes recover energy faster
        stats.energy = Mathf.Min(stats.energy + 15f, 110f);
    }
    
    // Helper to check if a specific fox ability is unlocked
    private bool HasAbility(FoxAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    protected override void UpdateAccessoryVisual()
    {
        base.UpdateAccessoryVisual();
        
        // Additional fox-specific accessory logic
        if (!string.IsNullOrEmpty(equippedAccessory))
        {
            // Find child game object with the accessory name
            Transform accessoryTransform = transform.Find(equippedAccessory);
            
            if (accessoryTransform != null)
            {
                // Show only the equipped accessory
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("Accessory_"))
                    {
                        child.gameObject.SetActive(child.name == equippedAccessory);
                    }
                }
            }
        }
    }
}

// Class to track mini-game performance
[System.Serializable]
public class MiniGameScore
{
    public string gameId;
    public int score;
    public bool isHighScore;
    public System.DateTime timestamp;
}
