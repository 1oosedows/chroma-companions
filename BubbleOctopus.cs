// BubbleOctopus.cs - A multitasking pet with bubble and water abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleOctopus : PetBase
{
    [Header("Octopus Specific")]
    [SerializeField] private float bubbleShieldHealth = 30f;
    [SerializeField] private int maxBubbleShieldsPerDay = 3;
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private GameObject inkCloudPrefab;
    [SerializeField] private GameObject waterWavePrefab;
    [SerializeField] private float multitaskBonus = 0.2f; // 20% bonus for multiple actions
    
    private int bubbleShieldsToday = 0;
    private bool isBubbleShieldActive = false;
    private float currentBubbleShieldHealth = 0f;
    private List<GameObject> activeBubbles = new List<GameObject>();
    
    // Special octopus abilities
    public enum OctopusAbility
    {
        Multitasking,     // Can perform multiple actions at once
        BubbleShield,     // Protective bubble
        InkCloud,         // Defensive escape mechanism
        WaterControl,     // Control water for bonuses
        BubbleTsunami     // Ultimate wave attack
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, OctopusAbility> levelAbilities = new Dictionary<int, OctopusAbility>()
    {
        { 2, OctopusAbility.Multitasking },
        { 5, OctopusAbility.BubbleShield },
        { 9, OctopusAbility.InkCloud },
        { 13, OctopusAbility.WaterControl },
        { 18, OctopusAbility.BubbleTsunami }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for octopus
        element = PetElement.Water; // Octopus starts as Water element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Octopus have higher initial health but lower energy
        stats.health = 110f;
        stats.energy = 90f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for octopus-specific ability unlocks based on level
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
        
        // Octopus get extra happiness from water toys
        if (toy.preferredBy == PetElement.Water)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 100f);
            
            // Chance to create bubbles
            if (Random.Range(0, 100) < 50)
            {
                CreateBubbles();
            }
        }
        
        // Apply multitasking bonus if applicable
        if (HasAbility(OctopusAbility.Multitasking))
        {
            // In a full game, this would allow performing multiple actions
            // For now, just add a bonus to happiness and experience
            stats.happiness = Mathf.Min(stats.happiness + 5f * multitaskBonus, 100f);
            AddExperience(Mathf.RoundToInt(5 * multitaskBonus));
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Octopus prefer seafood
        if (food.preferredBy == PetElement.Water)
        {
            stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
            stats.health = Mathf.Min(stats.health + 10f, 110f);
        }
        
        // Apply multitasking bonus if applicable
        if (HasAbility(OctopusAbility.Multitasking))
        {
            // Bonus to food effects
            stats.hunger = Mathf.Min(stats.hunger + 5f * multitaskBonus, 100f);
        }
    }
    
    private void CreateBubbles()
    {
        // Play animation
        animator.SetTrigger("CreateBubbles");
        
        // Create bubble effects
        if (bubblePrefab != null)
        {
            StartCoroutine(BubbleEffect());
        }
    }
    
    private IEnumerator BubbleEffect()
    {
        int numBubbles = Random.Range(3, 7);
        
        for (int i = 0; i < numBubbles; i++)
        {
            // Random position around the octopus
            Vector3 offset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-0.5f, 0.5f)
            );
            
            GameObject bubble = Instantiate(bubblePrefab, 
                transform.position + offset, 
                Quaternion.identity);
            
            // Make bubbles float up
            StartCoroutine(FloatBubble(bubble));
            
            // Add to active bubbles
            activeBubbles.Add(bubble);
            
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private IEnumerator FloatBubble(GameObject bubble)
    {
        float duration = Random.Range(2f, 4f);
        float elapsed = 0f;
        
        Vector3 startPos = bubble.transform.position;
        Vector3 targetPos = startPos + new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(2f, 3f),
            Random.Range(-1f, 1f)
        );
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            bubble.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            yield return null;
        }
        
        // Pop the bubble
        Destroy(bubble);
        activeBubbles.Remove(bubble);
    }
    
    public void ActivateBubbleShield()
    {
        if (!HasAbility(OctopusAbility.BubbleShield) || isBubbleShieldActive || 
            bubbleShieldsToday >= maxBubbleShieldsPerDay)
            return;
        
        bubbleShieldsToday++;
        isBubbleShieldActive = true;
        currentBubbleShieldHealth = bubbleShieldHealth;
        
        // Play animation
        animator.SetTrigger("BubbleShield");
        
        // Create bubble shield effect
        if (bubblePrefab != null)
        {
            GameObject shield = Instantiate(bubblePrefab, transform.position, Quaternion.identity, transform);
            
            // Scale up the bubble
            shield.transform.localScale = Vector3.one * 3f;
            
            // Make it semi-transparent
            Renderer shieldRenderer = shield.GetComponent<Renderer>();
            if (shieldRenderer != null)
            {
                Color shieldColor = shieldRenderer.material.color;
                shieldColor.a = 0.5f;
                shieldRenderer.material.color = shieldColor;
            }
            
            // Add to active bubbles
            activeBubbles.Add(shield);
        }
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        UIManager.Instance.ShowMessage("Bubble shield activated!");
    }
    
    public void ActivateInkCloud()
    {
        if (!HasAbility(OctopusAbility.InkCloud))
            return;
        
        // Play animation
        animator.SetTrigger("InkCloud");
        
        // Create ink cloud effect
        if (inkCloudPrefab != null)
        {
            GameObject inkCloud = Instantiate(inkCloudPrefab, 
                transform.position + Vector3.down * 0.5f, 
                Quaternion.identity);
            
            Destroy(inkCloud, 5f);
        }
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 20f, 0f);
        
        // In a full game, this would provide defensive benefits
        // For now, just show a message
        UIManager.Instance.ShowMessage("Ink cloud released!");
        
        // Small chance to find item in the confusion
        if (Random.Range(0, 100) < 20)
        {
            int coinAmount = Random.Range(5, 15);
            GameManager.Instance.AddCurrency(coinAmount);
            UIManager.Instance.ShowMessage("Found " + coinAmount + " coins in the ink cloud!");
        }
    }
    
    public void ActivateWaterControl()
    {
        if (!HasAbility(OctopusAbility.WaterControl))
            return;
        
        // Play animation
        animator.SetTrigger("WaterControl");
        
        // Create water effect
        if (waterWavePrefab != null)
        {
            GameObject waterWave = Instantiate(waterWavePrefab, 
                transform.position, 
                Quaternion.identity);
            
            // Make wave move forward
            StartCoroutine(MoveWaterWave(waterWave));
        }
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 25f, 0f);
        
        UIManager.Instance.ShowMessage("Water control activated!");
        
        // Heal the pet
        stats.health = Mathf.Min(stats.health + 15f, 110f);
    }
    
    private IEnumerator MoveWaterWave(GameObject wave)
    {
        float duration = 3f;
        float elapsed = 0f;
        
        Vector3 startPos = wave.transform.position;
        Vector3 targetPos = startPos + transform.forward * 5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            wave.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            yield return null;
        }
        
        Destroy(wave);
    }
    
    public void ActivateBubbleTsunami()
    {
        if (!HasAbility(OctopusAbility.BubbleTsunami))
            return;
        
        // This is the ultimate ability - massive wave attack
        animator.SetTrigger("BubbleTsunami");
        
        // Create massive water and bubble effect
        StartCoroutine(BubbleTsunamiEffect());
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 50f, 0f);
        
        UIManager.Instance.ShowMessage("Bubble tsunami activated!");
        
        // Major rewards
        int currencyAmount = 25 + stats.level * 2;
        GameManager.Instance.AddCurrency(currencyAmount);
        
        UIManager.Instance.ShowMessage("The tsunami washes up " + currencyAmount + " coins!");
    }
    
    private IEnumerator BubbleTsunamiEffect()
    {
        // Create multiple waves
        for (int i = 0; i < 3; i++)
        {
            if (waterWavePrefab != null)
            {
                GameObject waterWave = Instantiate(waterWavePrefab, 
                    transform.position, 
                    Quaternion.identity);
                
                // Scale up the wave
                waterWave.transform.localScale = Vector3.one * (i + 1) * 1.5f;
                
                // Make wave move forward
                StartCoroutine(MoveWaterWave(waterWave));
            }
            
            // Create many bubbles
            if (bubblePrefab != null)
            {
                for (int j = 0; j < 10; j++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-2f, 2f),
                        Random.Range(0.5f, 2f),
                        Random.Range(-2f, 2f)
                    );
                    
                    GameObject bubble = Instantiate(bubblePrefab, 
                        transform.position + offset, 
                        Quaternion.identity);
                    
                    // Make bubbles float in random directions
                    StartCoroutine(FloatBubble(bubble));
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        // If bubble shield is active, damage the shield first
        if (isBubbleShieldActive)
        {
            currentBubbleShieldHealth -= damageAmount;
            
            // Check if shield breaks
            if (currentBubbleShieldHealth <= 0f)
            {
                // Break the shield
                BreakBubbleShield();
                
                // Any remaining damage goes to pet
                float remainingDamage = Mathf.Abs(currentBubbleShieldHealth);
                if (remainingDamage > 0)
                {
                    stats.health = Mathf.Max(stats.health - remainingDamage, 0f);
                }
            }
        }
        else
        {
            // Damage goes directly to pet
            stats.health = Mathf.Max(stats.health - damageAmount, 0f);
        }
    }
    
    private void BreakBubbleShield()
    {
        isBubbleShieldActive = false;
        currentBubbleShieldHealth = 0f;
        
        // Remove shield visual
        foreach (GameObject bubble in activeBubbles)
        {
            if (bubble != null && bubble.transform.parent == transform)
            {
                // Pop animation here if available
                Destroy(bubble);
            }
        }
        
        UIManager.Instance.ShowMessage("Bubble shield broken!");
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset bubble shields counter
        bubbleShieldsToday = 0;
        
        // If shield is active, restore some health
        if (isBubbleShieldActive)
        {
            currentBubbleShieldHealth = Mathf.Min(currentBubbleShieldHealth + 10f, bubbleShieldHealth);
        }
        
        // Octopus recover better in water
        // In a full game, this would check if in water environment
        stats.health = Mathf.Min(stats.health + 10f, 110f);
    }
    
    // Helper to check if a specific octopus ability is unlocked
    private bool HasAbility(OctopusAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Get shield percentage for UI
    public float GetShieldPercentage()
    {
        if (!isBubbleShieldActive)
            return 0f;
            
        return currentBubbleShieldHealth / bubbleShieldHealth;
    }
} 