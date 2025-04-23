// SparkHamster.cs - An energetic pet with electricity-based abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkHamster : PetBase
{
    [Header("Hamster Specific")]
    [SerializeField] private float energyRegenSpeed = 2f; // 2x faster energy regeneration
    [SerializeField] private float wheelSpinEnergyGain = 5f;
    [SerializeField] private float speedBoostDuration = 10f;
    [SerializeField] private int maxSpeedBoostsPerDay = 5;
    [SerializeField] private GameObject electricSparkPrefab;
    [SerializeField] private GameObject powerSurgePrefab;
    
    private int speedBoostsToday = 0;
    private bool isSpeedBoostActive = false;
    private float wheelSpinCounter = 0f;
    
    // Special hamster abilities
    public enum HamsterAbility
    {
        EnergyStorage,   // Store more energy than normal
        SpeedBoost,      // Temporary speed boost for mini-games
        PowerSurge,      // Generate electricity for currency
        ElectricBarrier, // Protective shield
        HyperCharge      // Major boost to all stats temporarily
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, HamsterAbility> levelAbilities = new Dictionary<int, HamsterAbility>()
    {
        { 2, HamsterAbility.EnergyStorage },
        { 5, HamsterAbility.SpeedBoost },
        { 8, HamsterAbility.PowerSurge },
        { 12, HamsterAbility.ElectricBarrier },
        { 18, HamsterAbility.HyperCharge }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for hamster
        element = PetElement.Electric; // Hamsters start as Electric element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Hamsters have higher initial energy but lower health
        stats.energy = 120f;
        stats.health = 90f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for hamster-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // Update energy capacity if energy storage unlocked
                if (levelAbility.Value == HamsterAbility.EnergyStorage)
                {
                    stats.energy = 120f; // Increase max energy
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Hamsters love wheels and electric toys
        if (toy.preferredBy == PetElement.Electric)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 100f);
            
            // Simulate wheel spinning
            SpinWheel();
            
            // Random chance to activate speed boost
            if (HasAbility(HamsterAbility.SpeedBoost) && Random.Range(0, 100) < 40)
            {
                ActivateSpeedBoost();
            }
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Hamsters prefer energy-rich foods
        if (food.preferredBy == PetElement.Electric)
        {
            stats.energy = Mathf.Min(stats.energy + 20f, HasAbility(HamsterAbility.EnergyStorage) ? 120f : 100f);
            stats.happiness = Mathf.Min(stats.happiness + 5f, 100f);
        }
    }
    
    private void SpinWheel()
    {
        // Play animation
        animator.SetTrigger("SpinWheel");
        
        // Increment wheel counter
        wheelSpinCounter += 1f;
        
        // Gain energy from spinning
        stats.energy = Mathf.Min(stats.energy + wheelSpinEnergyGain, 
            HasAbility(HamsterAbility.EnergyStorage) ? 120f : 100f);
        
        // Create spark effect at high speeds
        if (wheelSpinCounter > 3f && electricSparkPrefab != null)
        {
            GameObject spark = Instantiate(electricSparkPrefab, transform.position, Quaternion.identity);
            Destroy(spark, 1f);
        }
        
        // Trigger power surge if counter is high enough
        if (wheelSpinCounter >= 5f && HasAbility(HamsterAbility.PowerSurge))
        {
            ActivatePowerSurge();
            wheelSpinCounter = 0f;
        }
    }
    
    public void ActivateSpeedBoost()
    {
        if (!HasAbility(HamsterAbility.SpeedBoost) || isSpeedBoostActive || speedBoostsToday >= maxSpeedBoostsPerDay)
            return;
        
        speedBoostsToday++;
        isSpeedBoostActive = true;
        
        // Play animation
        animator.SetTrigger("SpeedBoost");
        
        // Create electric effect
        if (electricSparkPrefab != null)
        {
            StartCoroutine(CreateSparkTrail());
        }
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        UIManager.Instance.ShowMessage("Speed boost activated!");
        
        // In a full game, this would boost speed in mini-games
        
        // Deactivate after duration
        StartCoroutine(DeactivateSpeedBoost());
    }
    
    private IEnumerator CreateSparkTrail()
    {
        float elapsed = 0f;
        
        while (elapsed < speedBoostDuration)
        {
            // Create spark effect behind pet
            GameObject spark = Instantiate(electricSparkPrefab, 
                transform.position - transform.forward * 0.5f, 
                Quaternion.identity);
            Destroy(spark, 0.5f);
            
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
    }
    
    private IEnumerator DeactivateSpeedBoost()
    {
        yield return new WaitForSeconds(speedBoostDuration);
        isSpeedBoostActive = false;
    }
    
    public void ActivatePowerSurge()
    {
        if (!HasAbility(HamsterAbility.PowerSurge))
            return;
        
        // Play animation
        animator.SetTrigger("PowerSurge");
        
        // Create power surge effect
        if (powerSurgePrefab != null)
        {
            GameObject surge = Instantiate(powerSurgePrefab, transform.position, Quaternion.identity);
            Destroy(surge, 3f);
        }
        
        // Generate currency based on energy level
        int energyPercent = Mathf.RoundToInt(stats.energy / 
            (HasAbility(HamsterAbility.EnergyStorage) ? 120f : 100f) * 100f);
        
        int currencyAmount = Mathf.RoundToInt(energyPercent / 10f);
        
        if (currencyAmount > 0)
        {
            GameManager.Instance.AddCurrency(currencyAmount);
            UIManager.Instance.ShowMessage("Power surge generated " + currencyAmount + " coins!");
        }
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 30f, 0f);
    }
    
    public void ActivateElectricBarrier()
    {
        if (!HasAbility(HamsterAbility.ElectricBarrier))
            return;
        
        // Play animation
        animator.SetTrigger("ElectricBarrier");
        
        // Create barrier effect
        if (electricSparkPrefab != null)
        {
            StartCoroutine(CreateBarrierEffect());
        }
        
        // In a full game, this would protect from negative effects
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 25f, 0f);
        
        UIManager.Instance.ShowMessage("Electric barrier activated!");
    }
    
    private IEnumerator CreateBarrierEffect()
    {
        float duration = 5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Create sparks in a circle around pet
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = 1f;
            Vector3 position = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                0.5f,
                Mathf.Sin(angle) * radius
            );
            
            GameObject spark = Instantiate(electricSparkPrefab, position, Quaternion.identity);
            Destroy(spark, 0.5f);
            
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
    }
    
    public void ActivateHyperCharge()
    {
        if (!HasAbility(HamsterAbility.HyperCharge))
            return;
        
        // This is the ultimate ability - major stat boost
        animator.SetTrigger("HyperCharge");
        
        // Create massive electric effect
        if (powerSurgePrefab != null && electricSparkPrefab != null)
        {
            GameObject surge = Instantiate(powerSurgePrefab, transform.position, Quaternion.identity);
            Destroy(surge, 5f);
            
            StartCoroutine(CreateIntenseSparkEffect());
        }
        
        // Boost stats
        stats.happiness = 100f;
        stats.health = Mathf.Min(stats.health + 30f, 90f);
        
        // Generate significant currency
        int currencyAmount = 20 + stats.level;
        GameManager.Instance.AddCurrency(currencyAmount);
        
        // Use all energy
        stats.energy = 0f;
        
        UIManager.Instance.ShowMessage("Hyper charge activated! +" + currencyAmount + " coins!");
    }
    
    private IEnumerator CreateIntenseSparkEffect()
    {
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Create intense spark effect
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(0f, 1.5f),
                    Random.Range(-1.5f, 1.5f)
                );
                
                GameObject spark = Instantiate(electricSparkPrefab, 
                    transform.position + offset, 
                    Quaternion.identity);
                Destroy(spark, 0.5f);
            }
            
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset speed boosts counter
        speedBoostsToday = 0;
        wheelSpinCounter = 0f;
        
        // Hamsters recover energy much faster
        float energyRegen = 20f * energyRegenSpeed;
        stats.energy = Mathf.Min(stats.energy + energyRegen, 
            HasAbility(HamsterAbility.EnergyStorage) ? 120f : 100f);
    }
    
    public override void Rest()
    {
        base.Rest();
        
        // Hamsters recover energy extremely efficiently when resting
        float restBonus = HasAbility(HamsterAbility.EnergyStorage) ? 40f : 30f;
        stats.energy = Mathf.Min(stats.energy + restBonus, 
            HasAbility(HamsterAbility.EnergyStorage) ? 120f : 100f);
    }
    
    // Helper to check if a specific hamster ability is unlocked
    private bool HasAbility(HamsterAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Check if speed boost is active
    public bool IsSpeedBoostActive()
    {
        return isSpeedBoostActive;
    }
    
    // Get energy percentage for mini-game bonuses
    public float GetEnergyPercentage()
    {
        return stats.energy / (HasAbility(HamsterAbility.EnergyStorage) ? 120f : 100f);
    }
}
