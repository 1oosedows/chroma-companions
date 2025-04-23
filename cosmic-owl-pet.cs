// CosmicOwl.cs - A wise pet with time-based abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmicOwl : PetBase
{
    [Header("Owl Specific")]
    [SerializeField] private float wisdomBonusMultiplier = 0.25f; // 25% bonus to experience gained
    [SerializeField] private float nightVisionDuration = 10f;
    [SerializeField] private GameObject starryEffectPrefab;
    [SerializeField] private int maxPropheciesPerDay = 3;
    
    private int propheciesGivenToday = 0;
    private bool isNightVisionActive = false;
    
    // Special owl abilities
    public enum OwlAbility
    {
        Wisdom,         // Bonus experience gain
        NightVision,    // Reveal hidden items in darkness
        TimePause,      // Pause timers briefly
        Prophecy,       // Predict mini-game patterns
        CosmicInsight   // Major gameplay hints
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, OwlAbility> levelAbilities = new Dictionary<int, OwlAbility>()
    {
        { 2, OwlAbility.Wisdom },
        { 5, OwlAbility.NightVision },
        { 9, OwlAbility.TimePause },
        { 12, OwlAbility.Prophecy },
        { 18, OwlAbility.CosmicInsight }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for owl
        element = PetElement.Light; // Owls start as Light element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Owls have higher initial health but lower energy
        stats.health = 120f;
        stats.energy = 90f;
    }
    
    private void Start()
    {
        // Apply wisdom bonus if ability is unlocked
        if (HasAbility(OwlAbility.Wisdom))
        {
            StartCoroutine(WisdomEffect());
        }
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for owl-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // If it's wisdom, start the effect
                if (levelAbility.Value == OwlAbility.Wisdom && !IsInvoking("WisdomEffect"))
                {
                    StartCoroutine(WisdomEffect());
                }
            }
        }
    }
    
    private IEnumerator WisdomEffect()
    {
        while (HasAbility(OwlAbility.Wisdom))
        {
            // Every hour of gameplay (in this example, every 5 minutes)
            yield return new WaitForSeconds(300f);
            
            // Grant extra experience if the pet is happy
            if (stats.happiness > 50f)
            {
                int bonusExp = Mathf.RoundToInt(5 * stats.level * wisdomBonusMultiplier);
                AddExperience(bonusExp);
                
                if (bonusExp > 0)
                {
                    UIManager.Instance.ShowMessage("Your owl's wisdom provided " + bonusExp + " bonus experience!");
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Owls prefer intellectual toys
        if (toy.preferredBy == PetElement.Light || toy.preferredBy == PetElement.Air)
        {
            stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
            
            // Random chance to activate prophecy
            if (HasAbility(OwlAbility.Prophecy) && Random.Range(0, 100) < 30)
            {
                ActivateProphecy();
            }
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Owls prefer certain foods
        if (food.preferredBy == PetElement.Light)
        {
            stats.happiness = Mathf.Min(stats.happiness + 5f, 100f);
            stats.energy = Mathf.Min(stats.energy + 10f, 90f);
        }
    }
    
    public void ActivateNightVision()
    {
        if (!HasAbility(OwlAbility.NightVision) || isNightVisionActive)
            return;
        
        // Activate night vision
        isNightVisionActive = true;
        
        // Visual effect
        animator.SetTrigger("NightVision");
        
        if (starryEffectPrefab)
        {
            GameObject effect = Instantiate(starryEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(effect, nightVisionDuration);
        }
        
        // In a real game, this would reveal hidden items
        
        // Use some energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        // Deactivate after duration
        StartCoroutine(DeactivateNightVision());
    }
    
    private IEnumerator DeactivateNightVision()
    {
        yield return new WaitForSeconds(nightVisionDuration);
        isNightVisionActive = false;
    }
    
    public void ActivateTimePause()
    {
        if (!HasAbility(OwlAbility.TimePause))
            return;
        
        // This would pause timers in mini-games
        animator.SetTrigger("TimePause");
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 25f, 0f);
        
        UIManager.Instance.ShowMessage("Time paused briefly!");
        
        // In a real game, this would affect active mini-games
    }
    
    public void ActivateProphecy()
    {
        if (!HasAbility(OwlAbility.Prophecy) || propheciesGivenToday >= maxPropheciesPerDay)
            return;
        
        propheciesGivenToday++;
        
        // Visual effect
        animator.SetTrigger("Prophecy");
        
        // Generate a "prophecy" (gameplay hint)
        string[] prophecies = new string[]
        {
            "Look for patterns of three in your next challenge.",
            "The next special item will appear in the upper right.",
            "Patience will be rewarded in your next encounter.",
            "A rare opportunity will present itself soon.",
            "Focus on blue elements in your next challenge."
        };
        
        string prophecy = prophecies[Random.Range(0, prophecies.Length)];
        UIManager.Instance.ShowMessage("Owl Prophecy: " + prophecy);
        
        // In a full game, these would be actual gameplay hints
    }
    
    public void ActivateCosmicInsight()
    {
        if (!HasAbility(OwlAbility.CosmicInsight))
            return;
        
        // This is the ultimate ability - major gameplay advantage
        animator.SetTrigger("CosmicInsight");
        
        // Use all energy
        stats.energy = 0f;
        
        // Major bonus
        GameManager.Instance.AddCurrency(stats.level * 10);
        
        UIManager.Instance.ShowMessage("Cosmic insight revealed hidden treasures!");
        
        // In a full game, this would provide significant advantages
        // like revealing all solutions or optimal paths
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset prophecies counter
        propheciesGivenToday = 0;
        
        // Owls recover more at night (would be based on device time in full game)
        bool isNightTime = Random.Range(0, 100) < 50; // Simulated for this example
        
        if (isNightTime)
        {
            stats.energy = Mathf.Min(stats.energy + 20f, 90f);
            stats.health = Mathf.Min(stats.health + 10f, 120f);
        }
    }
    
    // Helper to check if a specific owl ability is unlocked
    private bool HasAbility(OwlAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
}
