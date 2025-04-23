// CrystalDeer.cs - A graceful pet with nature and crystal abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDeer : PetBase
{
    [Header("Deer Specific")]
    [SerializeField] private float healingPulseAmount = 10f;
    [SerializeField] private int maxHealingPulsesPerDay = 3;
    [SerializeField] private GameObject crystalGlowPrefab;
    [SerializeField] private GameObject naturePrefab;
    [SerializeField] private float crystalGrowthRate = 0.1f; // Crystal growth per day
    
    private int healingPulsesToday = 0;
    private float crystalGrowth = 0f; // 0-1 range for crystal coverage
    private List<PetBase> nearbyPets = new List<PetBase>();
    
    // Special deer abilities
    public enum DeerAbility
    {
        NatureTouch,     // Grows plants that provide food
        CrystalGrowth,   // Grows crystals that provide currency
        HealingPulse,    // Heals other pets
        Serenity,        // Calms all pets and increases happiness
        NaturalResonance // Major boost to all pet stats temporarily
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, DeerAbility> levelAbilities = new Dictionary<int, DeerAbility>()
    {
        { 3, DeerAbility.NatureTouch },
        { 6, DeerAbility.CrystalGrowth },
        { 10, DeerAbility.HealingPulse },
        { 14, DeerAbility.Serenity },
        { 20, DeerAbility.NaturalResonance }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for deer
        element = PetElement.Nature; // Deer start as Nature element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Deer have higher initial health and happiness
        stats.health = 110f;
        stats.happiness = 110f;
        stats.energy = 90f;
    }
    
    private void Update()
    {
        // Update crystal glow intensity based on growth
        if (HasAbility(DeerAbility.CrystalGrowth) && crystalGlowPrefab != null)
        {
            ParticleSystem particles = crystalGlowPrefab.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var emission = particles.emission;
                emission.rateOverTime = crystalGrowth * 20f; // Adjust emission rate
            }
        }
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for deer-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // Initialize special effects based on ability
                if (levelAbility.Value == DeerAbility.CrystalGrowth && crystalGlowPrefab != null)
                {
                    GameObject crystalEffect = Instantiate(crystalGlowPrefab, transform.position, Quaternion.identity, transform);
                    // Start with minimal particles
                    ParticleSystem particles = crystalEffect.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        var emission = particles.emission;
                        emission.rateOverTime = 0f;
                    }
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Deer get extra happiness from nature toys
        if (toy.preferredBy == PetElement.Nature)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 110f);
            
            // Chance to activate nature touch
            if (HasAbility(DeerAbility.NatureTouch) && Random.Range(0, 100) < 30)
            {
                ActivateNatureTouch();
            }
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Deer prefer plant-based foods
        if (food.preferredBy == PetElement.Nature)
        {
            stats.happiness = Mathf.Min(stats.happiness + 10f, 110f);
            stats.health = Mathf.Min(stats.health + 10f, 110f);
            
            // Faster crystal growth if ability unlocked
            if (HasAbility(DeerAbility.CrystalGrowth))
            {
                crystalGrowth = Mathf.Min(crystalGrowth + 0.05f, 1f);
            }
        }
    }
    
    public void ActivateNatureTouch()
    {
        if (!HasAbility(DeerAbility.NatureTouch))
            return;
        
        // Play animation
        animator.SetTrigger("NatureTouch");
        
        // Create nature effect
        if (naturePrefab != null)
        {
            GameObject nature = Instantiate(naturePrefab, transform.position + new Vector3(0, 0, 0.5f), Quaternion.identity);
            Destroy(nature, 10f);
        }
        
        // In a full game, this would generate food items
        // For now, add happiness
        stats.happiness = Mathf.Min(stats.happiness + 5f, 110f);
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 10f, 0f);
        
        UIManager.Instance.ShowMessage("Nature touch creates fresh plants!");
    }
    
    public void ActivateHealingPulse()
    {
        if (!HasAbility(DeerAbility.HealingPulse) || healingPulsesToday >= maxHealingPulsesPerDay)
            return;
        
        healingPulsesToday++;
        
        // Play animation
        animator.SetTrigger("HealingPulse");
        
        // Visual effect
        StartCoroutine(HealingPulseEffect());
        
        // Heal this pet
        stats.health = Mathf.Min(stats.health + healingPulseAmount, 110f);
        
        // In a full game, this would heal nearby pets
        // For now, just show a message
        UIManager.Instance.ShowMessage("Healing pulse restores health!");
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
    }
    
    private IEnumerator HealingPulseEffect()
    {
        // Create expanding circle effect
        float duration = 2f;
        float maxRadius = 3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float radius = Mathf.Lerp(0, maxRadius, elapsedTime / duration);
            
            // In a full implementation, this would be a shader effect
            // For now, just wait
            
            yield return null;
        }
    }
    
    public void ActivateSerenity()
    {
        if (!HasAbility(DeerAbility.Serenity))
            return;
        
        // Play animation
        animator.SetTrigger("Serenity");
        
        // Create calming effect
        StartCoroutine(SerenityEffect());
        
        // In a full game, this would increase happiness of all pets
        // For now, just add extra happiness to this pet
        stats.happiness = Mathf.Min(stats.happiness + 20f, 110f);
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 25f, 0f);
        
        UIManager.Instance.ShowMessage("Serenity calms all pets!");
    }
    
    private IEnumerator SerenityEffect()
    {
        // Create calming aura effect
        float duration = 5f;
        
        // In a full implementation, this would be a visual effect
        // For now, just wait
        
        yield return new WaitForSeconds(duration);
    }
    
    public void ActivateNaturalResonance()
    {
        if (!HasAbility(DeerAbility.NaturalResonance))
            return;
        
        // This is the ultimate ability - major stat boost
        animator.SetTrigger("NaturalResonance");
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 50f, 0f);
        
        // Major boost to stats
        stats.happiness = 110f;
        stats.health = 110f;
        
        // Gain experience
        AddExperience(30);
        
        // Generate currency based on crystal growth
        if (HasAbility(DeerAbility.CrystalGrowth))
        {
            int crystalBonus = Mathf.RoundToInt(crystalGrowth * 50f);
            GameManager.Instance.AddCurrency(crystalBonus);
            UIManager.Instance.ShowMessage("Crystal resonance generates " + crystalBonus + " coins!");
        }
        
        UIManager.Instance.ShowMessage("Natural resonance revitalizes your pet!");
        
        // In a full game, this would boost all pets in the area
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset healing pulses counter
        healingPulsesToday = 0;
        
        // Grow crystals if ability unlocked
        if (HasAbility(DeerAbility.CrystalGrowth))
        {
            crystalGrowth = Mathf.Min(crystalGrowth + crystalGrowthRate, 1f);
            
            // Generate currency based on crystal growth
            int crystalCurrency = Mathf.RoundToInt(crystalGrowth * 5f);
            if (crystalCurrency > 0)
            {
                GameManager.Instance.AddCurrency(crystalCurrency);
                UIManager.Instance.ShowMessage("Your deer's crystals generated " + crystalCurrency + " coins!");
            }
        }
        
        // Deer recover more in natural environments
        // In a full game, this would check environment type
        stats.health = Mathf.Min(stats.health + 5f, 110f);
    }
    
    // Helper to check if a specific deer ability is unlocked
    private bool HasAbility(DeerAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Get crystal growth for UI display
    public float GetCrystalGrowth()
    {
        return crystalGrowth;
    }
}
