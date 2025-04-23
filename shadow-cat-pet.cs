// ShadowCat.cs - A stealthy pet with shadow and illusion abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCat : PetBase
{
    [Header("Cat Specific")]
    [SerializeField] private float stealthDuration = 8f;
    [SerializeField] private int maxStealthUsesPerDay = 3;
    [SerializeField] private GameObject shadowEffectPrefab;
    [SerializeField] private GameObject illusionEffectPrefab;
    [SerializeField] private float treasureFinderChance = 0.25f; // 25% chance to find items
    
    private int stealthUsesToday = 0;
    private bool isStealthActive = false;
    private List<GameObject> activeIllusions = new List<GameObject>();
    
    // Special cat abilities
    public enum CatAbility
    {
        NightStalker,     // Find items in the dark
        Stealth,          // Become invisible temporarily
        ShadowClone,      // Create illusions
        TreasureFinder,   // Chance to find extra rewards
        VoidWalk          // Ultimate teleport and prize finding
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, CatAbility> levelAbilities = new Dictionary<int, CatAbility>()
    {
        { 2, CatAbility.NightStalker },
        { 5, CatAbility.Stealth },
        { 9, CatAbility.ShadowClone },
        { 13, CatAbility.TreasureFinder },
        { 18, CatAbility.VoidWalk }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for cat
        element = PetElement.Dark; // Cats start as Dark element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Cats have higher initial happiness but lower health
        stats.happiness = 100f;
        stats.health = 90f;
        stats.energy = 110f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for cat-specific ability unlocks based on level
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
        
        // Cats get extra happiness from toys that move
        if (toy.preferredBy == PetElement.Dark || toy.preferredBy == PetElement.Air)
        {
            stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
            
            // Chance to activate stealth
            if (HasAbility(CatAbility.Stealth) && Random.Range(0, 100) < 30)
            {
                ActivateStealth();
            }
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Cats prefer meat-based foods
        if (food.preferredBy == PetElement.Dark || food.preferredBy == PetElement.Fire)
        {
            stats.health = Mathf.Min(stats.health + 10f, 90f);
            stats.energy = Mathf.Min(stats.energy + 10f, 110f);
        }
    }
    
    public void ActivateStealth()
    {
        if (!HasAbility(CatAbility.Stealth) || isStealthActive || stealthUsesToday >= maxStealthUsesPerDay)
            return;
        
        stealthUsesToday++;
        isStealthActive = true;
        
        // Play animation
        animator.SetTrigger("Stealth");
        
        // Visual effect - fade to shadow
        StartCoroutine(StealthEffect());
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        UIManager.Instance.ShowMessage("Stealth activated!");
        
        // In a full game, this would have gameplay benefits like finding hidden items
    }
    
    private IEnumerator StealthEffect()
    {
        // Fade to shadow effect
        float duration = 1f;
        float elapsed = 0f;
        
        Color originalColor = spriteRenderer.color;
        Color targetColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            spriteRenderer.color = Color.Lerp(originalColor, targetColor, t);
            
            yield return null;
        }
        
        // Create shadow effect
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity, transform);
            Destroy(shadow, stealthDuration);
        }
        
        // Stay in stealth for the duration
        yield return new WaitForSeconds(stealthDuration);
        
        // Fade back
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            spriteRenderer.color = Color.Lerp(targetColor, originalColor, t);
            
            yield return null;
        }
        
        // Ensure color is reset
        spriteRenderer.color = originalColor;
        isStealthActive = false;
        
        // Chance to find treasure if treasure finder ability is unlocked
        if (HasAbility(CatAbility.TreasureFinder) && Random.Range(0f, 1f) < treasureFinderChance)
        {
            FindTreasure();
        }
    }
    
    public void ActivateShadowClone()
    {
        if (!HasAbility(CatAbility.ShadowClone))
            return;
        
        // Play animation
        animator.SetTrigger("ShadowClone");
        
        // Create illusions
        StartCoroutine(CreateShadowClones());
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 20f, 0f);
        
        UIManager.Instance.ShowMessage("Shadow clones created!");
    }
    
    private IEnumerator CreateShadowClones()
    {
        // Clear existing illusions
        foreach (GameObject illusion in activeIllusions)
        {
            if (illusion != null)
            {
                Destroy(illusion);
            }
        }
        activeIllusions.Clear();
        
        // Create 2-3 illusions around the pet
        int numIllusions = Random.Range(2, 4);
        
        for (int i = 0; i < numIllusions; i++)
        {
            // Calculate position in a circle around the pet
            float angle = (360f / numIllusions) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 1.5f;
            
            if (illusionEffectPrefab != null)
            {
                GameObject illusion = Instantiate(illusionEffectPrefab, 
                    transform.position + offset, 
                    Quaternion.identity);
                
                activeIllusions.Add(illusion);
                
                // Scale the illusion up
                float duration = 0.5f;
                float elapsed = 0f;
                
                Transform illusionTransform = illusion.transform;
                illusionTransform.localScale = Vector3.zero;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    
                    illusionTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    
                    yield return null;
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        // Wait for illusion duration
        yield return new WaitForSeconds(10f);
        
        // Fade out illusions
        foreach (GameObject illusion in activeIllusions)
        {
            if (illusion != null)
            {
                StartCoroutine(FadeOutIllusion(illusion));
            }
        }
    }
    
    private IEnumerator FadeOutIllusion(GameObject illusion)
    {
        SpriteRenderer illSprite = illusion.GetComponent<SpriteRenderer>();
        
        if (illSprite != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            
            Color startColor = illSprite.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                illSprite.color = Color.Lerp(startColor, endColor, t);
                
                yield return null;
            }
        }
        
        Destroy(illusion);
    }
    
    private void FindTreasure()
    {
        // Play animation
        animator.SetTrigger("FindTreasure");
        
        // Determine reward amount based on level
        int baseReward = Random.Range(5, 10);
        int levelBonus = Mathf.FloorToInt(stats.level / 2);
        int totalReward = baseReward + levelBonus;
        
        // Award currency
        GameManager.Instance.AddCurrency(totalReward);
        
        UIManager.Instance.ShowMessage("Your cat found a treasure! +" + totalReward + " coins!");
    }
    
    public void ActivateNightStalker()
    {
        if (!HasAbility(CatAbility.NightStalker))
            return;
        
        // Play animation
        animator.SetTrigger("NightStalker");
        
        // Create shadow effect
        if (shadowEffectPrefab != null)
        {
            GameObject shadow = Instantiate(shadowEffectPrefab, 
                transform.position + Vector3.down * 0.5f, 
                Quaternion.identity);
            Destroy(shadow, 5f);
        }
        
        // In a full game, this would reveal hidden items at night
        // For now, just add extra happiness
        stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
        
        UIManager.Instance.ShowMessage("Night stalker activated! Your cat can find hidden secrets!");
    }
    
    public void ActivateVoidWalk()
    {
        if (!HasAbility(CatAbility.VoidWalk))
            return;
        
        // This is the ultimate ability - teleport and find rare items
        animator.SetTrigger("VoidWalk");
        
        // Create teleport effect
        StartCoroutine(VoidWalkEffect());
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 50f, 0f);
        
        UIManager.Instance.ShowMessage("Void walk activated!");
        
        // Major reward
        int currencyAmount = 30 + stats.level * 2;
        GameManager.Instance.AddCurrency(currencyAmount);
        
        UIManager.Instance.ShowMessage("Your cat found rare treasures in the void! +" + currencyAmount + " coins!");
        
        // In a full game, this would also grant rare items
    }
    
    private IEnumerator VoidWalkEffect()
    {
        // Fade out
        float duration = 0.5f;
        float elapsed = 0f;
        
        Color startColor = spriteRenderer.color;
        Color invisibleColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            spriteRenderer.color = Color.Lerp(startColor, invisibleColor, t);
            
            yield return null;
        }
        
        // Teleport to a random position
        Vector3 originalPos = transform.position;
        Vector3 teleportOffset = new Vector3(
            Random.Range(-3f, 3f),
            0,
            Random.Range(-3f, 3f)
        );
        
        transform.position += teleportOffset;
        
        // Create void effect at both positions
        if (shadowEffectPrefab != null)
        {
            GameObject originVoid = Instantiate(shadowEffectPrefab, originalPos, Quaternion.identity);
            GameObject destinationVoid = Instantiate(shadowEffectPrefab, transform.position, Quaternion.identity);
            
            Destroy(originVoid, 2f);
            Destroy(destinationVoid, 2f);
        }
        
        // Wait briefly
        yield return new WaitForSeconds(0.5f);
        
        // Fade back in
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            spriteRenderer.color = Color.Lerp(invisibleColor, startColor, t);
            
            yield return null;
        }
        
        // Ensure color is reset
        spriteRenderer.color = startColor;
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset stealth counter
        stealthUsesToday = 0;
        
        // Cats are more energetic at night
        // In a full game, this would check device time
        bool isNightTime = Random.Range(0, 100) < 50; // Simulated for this example
        
        if (isNightTime)
        {
            stats.energy = Mathf.Min(stats.energy + 15f, 110f);
        }
    }
    
    // Helper to check if a specific cat ability is unlocked
    private bool HasAbility(CatAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Check if stealth is active
    public bool IsStealthActive()
    {
        return isStealthActive;
    }
}
