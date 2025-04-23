// RainbowBunny.cs - A specific pet implementation focused on resource generation
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowBunny : PetBase
{
    [Header("Bunny Specific")]
    [SerializeField] private float hopEnergyUsage = 5f;
    [SerializeField] private GameObject rainbowTrailPrefab;
    [SerializeField] private int maxResourcesPerDay = 5;
    
    private int resourcesGeneratedToday = 0;
    private float happinessThreshold = 70f; // Minimum happiness to generate resources
    
    // Special bunny abilities
    public enum BunnyAbility
    {
        RainbowTrail,
        ResourceGeneration, 
        SuperHop,
        LuckyCharm,
        ColorfulDreams
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, BunnyAbility> levelAbilities = new Dictionary<int, BunnyAbility>()
    {
        { 2, BunnyAbility.RainbowTrail },
        { 5, BunnyAbility.ResourceGeneration },
        { 8, BunnyAbility.SuperHop },
        { 12, BunnyAbility.LuckyCharm },
        { 18, BunnyAbility.ColorfulDreams }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for bunny
        element = PetElement.Light; // Bunnies start as Light element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Bunnies have higher initial happiness but lower energy
        stats.happiness = 120f;
        stats.energy = 80f;
    }
    
    private void Start()
    {
        // Check for resource generation on a timer
        if (HasAbility(BunnyAbility.ResourceGeneration))
        {
            InvokeRepeating("TryGenerateResource", 60f, 60f); // Check every minute
        }
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for bunny-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // If it's resource generation, start the timer
                if (levelAbility.Value == BunnyAbility.ResourceGeneration && !IsInvoking("TryGenerateResource"))
                {
                    InvokeRepeating("TryGenerateResource", 60f, 60f);
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Bunnies get extra happiness from playing
        stats.happiness = Mathf.Min(stats.happiness + 10f, 120f);
        
        // Random chance to hop around
        if (Random.Range(0, 100) < 70)
        {
            ActivateHop();
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Bunnies prefer plant/nature foods
        if (food.preferredBy == PetElement.Nature)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 120f);
            stats.health = Mathf.Min(stats.health + 5f, 100f);
        }
    }
    
    public override void Rest()
    {
        base.Rest();
        
        // Bunny dreams generate resources if they have the ColorfulDreams ability
        if (HasAbility(BunnyAbility.ColorfulDreams) && stats.happiness > happinessThreshold)
        {
            TryGenerateResource();
        }
    }
    
    public void ActivateHop()
    {
        if (stats.energy < hopEnergyUsage)
            return;
        
        stats.energy -= hopEnergyUsage;
        
        // Play hop animation
        animator.SetTrigger("Hop");
        
        // Create rainbow trail if the ability is unlocked
        if (HasAbility(BunnyAbility.RainbowTrail) && rainbowTrailPrefab)
        {
            GameObject trail = Instantiate(rainbowTrailPrefab, transform.position, Quaternion.identity);
            Destroy(trail, 2f); // Auto-destroy after a short time
        }
        
        // Extra effects for super hop
        if (HasAbility(BunnyAbility.SuperHop))
        {
            // Bigger jump in animation
            animator.SetFloat("HopHeight", 2f);
            
            // Higher chance to find a resource
            if (Random.Range(0, 100) < 40)
            {
                TryGenerateResource();
            }
        }
        else
        {
            animator.SetFloat("HopHeight", 1f);
        }
    }
    
    public void ActivateLuckyCharm()
    {
        if (!HasAbility(BunnyAbility.LuckyCharm))
            return;
        
        animator.SetTrigger("LuckyCharm");
        
        // Lucky charm increases the chance of generating a rare resource
        // In a real game, this would be implemented with the economy system
        GameManager.Instance.AddCurrency(Random.Range(5, 15));
    }
    
    private void TryGenerateResource()
    {
        if (!HasAbility(BunnyAbility.ResourceGeneration))
            return;
        
        // Check daily limit
        if (resourcesGeneratedToday >= maxResourcesPerDay)
            return;
        
        // Check happiness threshold
        if (stats.happiness < happinessThreshold)
            return;
        
        // Generate resource based on happiness level
        int resourceAmount = Mathf.RoundToInt(stats.happiness / 20f);
        
        // Lucky charm adds bonus
        if (HasAbility(BunnyAbility.LuckyCharm) && Random.Range(0, 100) < 30)
        {
            resourceAmount *= 2;
            
            // Visual effect for lucky bonus
            animator.SetTrigger("LuckyBonus");
        }
        
        // Add to player's currency
        GameManager.Instance.AddCurrency(resourceAmount);
        
        // Increment counter
        resourcesGeneratedToday++;
        
        // Small happiness cost for generating resources
        stats.happiness = Mathf.Max(stats.happiness - 5f, 0f);
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset daily resource counter
        resourcesGeneratedToday = 0;
        
        // Bunnies recover happiness faster
        stats.happiness = Mathf.Min(stats.happiness + 10f, 120f);
    }
    
    // Helper to check if a specific bunny ability is unlocked
    private bool HasAbility(BunnyAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    protected override void UpdateAccessoryVisual()
    {
        base.UpdateAccessoryVisual();
        
        // Additional bunny-specific accessory logic
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
