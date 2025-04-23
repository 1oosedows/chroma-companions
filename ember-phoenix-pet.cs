// EmberPhoenix.cs - A fire-based pet with rebirth and healing abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmberPhoenix : PetBase
{
    [Header("Phoenix Specific")]
    [SerializeField] private float rebirthHealthBonus = 50f;
    [SerializeField] private int maxRebirthsPerDay = 1;
    [SerializeField] private GameObject flameEffectPrefab;
    [SerializeField] private GameObject rebirthEffectPrefab;
    [SerializeField] private GameObject healingFlamesPrefab;
    [SerializeField] private float emberGlowIntensity = 0.5f;
    
    private int rebirthsToday = 0;
    private bool isEmberGlowActive = false;
    private float emberGlowTimer = 0f;
    private List<GameObject> activeFlameEffects = new List<GameObject>();
    
    // Special phoenix abilities
    public enum PhoenixAbility
    {
        EmberGlow,      // Passive glow that provides bonuses
        HealingFlames,  // Heal self and others
        FirebirdDash,   // Quick dash leaving fire trail
        Rebirth,        // Resurrect from defeat with bonus
        SunburstFlare   // Ultimate attack that transforms
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, PhoenixAbility> levelAbilities = new Dictionary<int, PhoenixAbility>()
    {
        { 3, PhoenixAbility.EmberGlow },
        { 6, PhoenixAbility.HealingFlames },
        { 10, PhoenixAbility.FirebirdDash },
        { 15, PhoenixAbility.Rebirth },
        { 20, PhoenixAbility.SunburstFlare }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for phoenix
        element = PetElement.Fire; // Phoenix starts as Fire element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Phoenix have balanced stats with slight boost to health
        stats.health = 105f;
    }
    
    private void Update()
    {
        // Update ember glow if active
        if (isEmberGlowActive)
        {
            emberGlowTimer += Time.deltaTime;
            
            // Pulse the glow intensity
            if (flameEffectPrefab != null && activeFlameEffects.Count > 0)
            {
                float pulseIntensity = Mathf.Sin(emberGlowTimer * 2f) * 0.25f + emberGlowIntensity;
                
                foreach (GameObject flame in activeFlameEffects)
                {
                    if (flame != null)
                    {
                        // Adjust particle emission or light intensity
                        ParticleSystem particles = flame.GetComponent<ParticleSystem>();
                        if (particles != null)
                        {
                            var emission = particles.emission;
                            emission.rateOverTime = pulseIntensity * 20f;
                        }
                        
                        Light light = flame.GetComponent<Light>();
                        if (light != null)
                        {
                            light.intensity = pulseIntensity;
                        }
                    }
                }
            }
        }
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for phoenix-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // Initialize effects for ember glow
                if (levelAbility.Value == PhoenixAbility.EmberGlow)
                {
                    ActivateEmberGlow();
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Phoenix get extra happiness from fire-based toys
        if (toy.preferredBy == PetElement.Fire)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 100f);
            
            // Chance to activate fireball dash
            if (HasAbility(PhoenixAbility.FirebirdDash) && Random.Range(0, 100) < 30)
            {
                ActivateFirebirdDash();
            }
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Phoenix prefer hot/spicy foods
        if (food.preferredBy == PetElement.Fire)
        {
            stats.health = Mathf.Min(stats.health + 10f, 105f);
            stats.energy = Mathf.Min(stats.energy + 10f, 100f);
            
            // Stronger ember glow if active
            if (isEmberGlowActive)
            {
                emberGlowIntensity = Mathf.Min(emberGlowIntensity + 0.1f, 1f);
            }
        }
    }
    
    private void ActivateEmberGlow()
    {
        if (!HasAbility(PhoenixAbility.EmberGlow))
            return;
        
        isEmberGlowActive = true;
        emberGlowTimer = 0f;
        
        // Create ember effects
        if (flameEffectPrefab != null)
        {
            // Clear existing effects
            foreach (GameObject flame in activeFlameEffects)
            {
                if (flame != null)
                {
                    Destroy(flame);
                }
            }
            activeFlameEffects.Clear();
            
            // Create new subtle flame effect
            GameObject ember = Instantiate(flameEffectPrefab, transform.position, Quaternion.identity, transform);
            
            // Scale down the effect for subtle glow
            ember.transform.localScale = Vector3.one * 0.5f;
            
            // Adjust particle emission
            ParticleSystem particles = ember.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var emission = particles.emission;
                emission.rateOverTime = emberGlowIntensity * 20f;
            }
            
            activeFlameEffects.Add(ember);
        }
        
        UIManager.Instance.ShowMessage("Ember glow activated!");
    }
    
    public void ActivateHealingFlames()
    {
        if (!HasAbility(PhoenixAbility.HealingFlames))
            return;
        
        // Play animation
        animator.SetTrigger("HealingFlames");
        
        // Create healing flames effect
        if (healingFlamesPrefab != null)
        {
            GameObject healingFlames = Instantiate(healingFlamesPrefab, transform.position, Quaternion.identity);
            Destroy(healingFlames, 5f);
        }
        
        // Heal the phoenix
        float healAmount = 20f;
        stats.health = Mathf.Min(stats.health + healAmount, 105f);
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        UIManager.Instance.ShowMessage("Healing flames restore " + healAmount + " health!");
        
        // In a full game, this would also heal nearby pets
    }
    
    public void ActivateFirebirdDash()
    {
        if (!HasAbility(PhoenixAbility.FirebirdDash))
            return;
        
        // Play animation
        animator.SetTrigger("FirebirdDash");
        
        // Create dash effect
        StartCoroutine(FirebirdDashEffect());
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 20f, 0f);
        
        UIManager.Instance.ShowMessage("Firebird dash!");
    }
    
    private IEnumerator FirebirdDashEffect()
    {
        // Calculate dash path
        Vector3 startPos = transform.position;
        Vector3 dashDirection = transform.forward;
        Vector3 endPos = startPos + dashDirection * 5f;
        
        // Create fire trail
        if (flameEffectPrefab != null)
        {
            float dashDuration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dashDuration;
                
                // Move the phoenix
                transform.position = Vector3.Lerp(startPos, endPos, t);
                
                // Create fire trail
                GameObject fireTrail = Instantiate(flameEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fireTrail, 2f);
                
                yield return null;
            }
            
            // Ensure final position
            transform.position = endPos;
        }
        
        // In a full game, this would also check for items or bonuses along the path
        
        // Small chance to find item during dash
        if (Random.Range(0, 100) < 30)
        {
            int coinAmount = Random.Range(5, 10);
            GameManager.Instance.AddCurrency(coinAmount);
            UIManager.Instance.ShowMessage("Found " + coinAmount + " coins during dash!");
        }
    }
    
    public void TriggerRebirth()
    {
        if (!HasAbility(PhoenixAbility.Rebirth) || rebirthsToday >= maxRebirthsPerDay)
            return;
        
        // Check if near death
        if (stats.health > 20f)
            return;
        
        rebirthsToday++;
        
        // Play rebirth animation
        animator.SetTrigger("Rebirth");
        
        // Create rebirth effect
        if (rebirthEffectPrefab != null)
        {
            GameObject rebirthEffect = Instantiate(rebirthEffectPrefab, transform.position, Quaternion.identity);
            Destroy(rebirthEffect, 5f);
        }
        
        // Heal significantly
        stats.health = Mathf.Min(stats.health + rebirthHealthBonus, 105f);
        stats.energy = Mathf.Min(stats.energy + 50f, 100f);
        stats.happiness = Mathf.Min(stats.happiness + 30f, 100f);
        
        // Experience bonus
        AddExperience(30);
        
        UIManager.Instance.ShowMessage("Phoenix reborn from the ashes!");
    }
    
    public void ActivateSunburstFlare()
    {
        if (!HasAbility(PhoenixAbility.SunburstFlare))
            return;
        
        // This is the ultimate ability - massive transformation
        animator.SetTrigger("SunburstFlare");
        
        // Create massive fire effect
        StartCoroutine(SunburstFlareEffect());
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 50f, 0f);
        
        UIManager.Instance.ShowMessage("Sunburst flare activated!");
        
        // Major rewards
        int currencyAmount = 35 + stats.level * 2;
        GameManager.Instance.AddCurrency(currencyAmount);
        
        // Temporary element change
        PetElement originalElement = element;
        element = PetElement.Light; // Temporarily become Light element
        
        // Reset after delay
        StartCoroutine(ResetElement(originalElement, 30f));
        
        // Heal completely
        stats.health = 105f;
    }
    
    private IEnumerator SunburstFlareEffect()
    {
        // Create expanding fire wave
        for (int i = 0; i < 3; i++)
        {
            // Create ring of fire particles
            if (flameEffectPrefab != null)
            {
                int numParticles = 12;
                for (int j = 0; j < numParticles; j++)
                {
                    float angle = (360f / numParticles) * j * Mathf.Deg2Rad;
                    float radius = (i + 1) * 1.5f;
                    Vector3 position = transform.position + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0.5f,
                        Mathf.Sin(angle) * radius
                    );
                    
                    GameObject flameParticle = Instantiate(flameEffectPrefab, position, Quaternion.identity);
                    Destroy(flameParticle, 3f);
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // Transform visual effect
        if (rebirthEffectPrefab != null)
        {
            GameObject transformEffect = Instantiate(rebirthEffectPrefab, transform.position, Quaternion.identity);
            transformEffect.transform.localScale = Vector3.one * 2f;
            Destroy(transformEffect, 5f);
        }
    }
    
    private IEnumerator ResetElement(PetElement originalElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        element = originalElement;
        UIManager.Instance.ShowMessage("Phoenix returns to normal form.");
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset rebirth counter
        rebirthsToday = 0;
        
        // Phoenix recover faster in warm environments
        // In a full game, this would check environment temperature
        stats.health = Mathf.Min(stats.health + 5f, 105f);
        
        // Fade ember glow intensity
        if (isEmberGlowActive)
        {
            emberGlowIntensity = Mathf.Max(emberGlowIntensity - 0.1f, 0.5f);
        }
        
        // Auto-rebirth if health is critical and ability is available
        if (stats.health < 10f && HasAbility(PhoenixAbility.Rebirth) && rebirthsToday < maxRebirthsPerDay)
        {
            TriggerRebirth();
        }
    }
    
    // Helper to check if a specific phoenix ability is unlocked
    private bool HasAbility(PhoenixAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Get ember glow intensity for UI
    public float GetEmberGlowIntensity()
    {
        return isEmberGlowActive ? emberGlowIntensity : 0f;
    }
}
