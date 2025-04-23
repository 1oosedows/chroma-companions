// PetBase.cs - Base class for all pet types
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PetStats
{
    public float happiness;
    public float hunger;
    public float energy;
    public float health;
    public int level;
    public int experience;
    
    // Experience needed for next level
    public int NextLevelThreshold => 100 * level + 50;
}

public enum PetRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum PetElement
{
    Fire,
    Water,
    Earth,
    Air,
    Light,
    Dark,
    Nature,
    Electric
}

public abstract class PetBase : MonoBehaviour
{
    [SerializeField] protected string petName;
    [SerializeField] protected string description;
    [SerializeField] protected PetStats stats;
    [SerializeField] protected PetRarity rarity;
    [SerializeField] protected PetElement element;
    [SerializeField] protected List<string> abilities = new List<string>();
    
    [SerializeField] protected Sprite petSprite;
    [SerializeField] protected RuntimeAnimatorController petAnimator;
    
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected bool isInteractable = true;
    
    // Collection of unlocked decorative items
    [SerializeField] protected List<string> unlockedAccessories = new List<string>();
    [SerializeField] protected string equippedAccessory;
    
    // Events
    public Action<float> OnHappinessChanged;
    public Action<int> OnLevelUp;
    public Action<string> OnAbilityUnlocked;
    
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Initialize with default values if new pet
        if (stats.level == 0)
        {
            InitializeDefaultStats();
        }
    }
    
    protected virtual void InitializeDefaultStats()
    {
        stats.happiness = 100f;
        stats.hunger = 100f;
        stats.energy = 100f;
        stats.health = 100f;
        stats.level = 1;
        stats.experience = 0;
    }
    
    public virtual void Feed(FoodItem food)
    {
        stats.hunger = Mathf.Min(stats.hunger + food.hungerValue, 100f);
        stats.happiness = Mathf.Min(stats.happiness + food.happinessBonus, 100f);
        
        // Play eating animation
        animator.SetTrigger("Eat");
    }
    
    public virtual void Play(ToyItem toy)
    {
        stats.happiness = Mathf.Min(stats.happiness + toy.happinessBonus, 100f);
        stats.energy = Mathf.Max(stats.energy - toy.energyCost, 0f);
        
        // Play playing animation
        animator.SetTrigger("Play");
        
        OnHappinessChanged?.Invoke(stats.happiness);
    }
    
    public virtual void Rest()
    {
        stats.energy = Mathf.Min(stats.energy + 25f, 100f);
        
        // Play sleeping animation
        animator.SetTrigger("Sleep");
    }
    
    public virtual void AddExperience(int expAmount)
    {
        stats.experience += expAmount;
        
        // Check for level up
        while (stats.experience >= stats.NextLevelThreshold)
        {
            stats.experience -= stats.NextLevelThreshold;
            LevelUp();
        }
    }
    
    protected virtual void LevelUp()
    {
        stats.level++;
        stats.health = 100f;
        stats.energy = 100f;
        
        // Check for new ability unlock
        CheckForAbilityUnlock();
        
        OnLevelUp?.Invoke(stats.level);
    }
    
    protected virtual void CheckForAbilityUnlock()
    {
        // Override in derived classes for specific level-based abilities
    }
    
    public virtual void EquipAccessory(string accessoryId)
    {
        if (unlockedAccessories.Contains(accessoryId))
        {
            equippedAccessory = accessoryId;
            // Update visual representation
            UpdateAccessoryVisual();
        }
    }
    
    protected virtual void UpdateAccessoryVisual()
    {
        // Override in derived classes to update pet appearance
    }
    
    // Called each game day to simulate time passing
    public virtual void UpdateDailyStats()
    {
        stats.hunger = Mathf.Max(stats.hunger - 10f, 0f);
        stats.happiness = Mathf.Max(stats.happiness - 5f, 0f);
        stats.energy = Mathf.Min(stats.energy + 20f, 100f);
        
        // If hunger is too low, happiness decreases faster
        if (stats.hunger < 20f)
        {
            stats.happiness = Mathf.Max(stats.happiness - 10f, 0f);
        }
        
        // If happiness is high, generate small experience bonus
        if (stats.happiness > 80f)
        {
            AddExperience(5);
        }
    }
    
    // Serialization methods for saving/loading
    public virtual PetSaveData GetSaveData()
    {
        return new PetSaveData
        {
            petID = this.GetType().Name,
            petName = this.petName,
            stats = this.stats,
            rarity = this.rarity,
            element = this.element,
            abilities = new List<string>(this.abilities),
            unlockedAccessories = new List<string>(this.unlockedAccessories),
            equippedAccessory = this.equippedAccessory
        };
    }
    
    public virtual void LoadFromSaveData(PetSaveData saveData)
    {
        this.petName = saveData.petName;
        this.stats = saveData.stats;
        this.rarity = saveData.rarity;
        this.element = saveData.element;
        this.abilities = new List<string>(saveData.abilities);
        this.unlockedAccessories = new List<string>(saveData.unlockedAccessories);
        this.equippedAccessory = saveData.equippedAccessory;
        
        UpdateAccessoryVisual();
    }
}

// FoodItem.cs - Base class for food items
[Serializable]
public class FoodItem
{
    public string id;
    public string displayName;
    public Sprite icon;
    public float hungerValue;
    public float happinessBonus;
    public float healthBonus;
    public int price;
    public PetElement preferredBy; // Pets of this element get a bonus
}

// ToyItem.cs - Base class for toy items
[Serializable]
public class ToyItem
{
    public string id;
    public string displayName;
    public Sprite icon;
    public float happinessBonus;
    public float energyCost;
    public int price;
    public PetElement preferredBy; // Pets of this element get a bonus
}

// PetSaveData.cs - Data structure for serialization
[Serializable]
public class PetSaveData
{
    public string petID;
    public string petName;
    public PetStats stats;
    public PetRarity rarity;
    public PetElement element;
    public List<string> abilities;
    public List<string> unlockedAccessories;
    public string equippedAccessory;
}