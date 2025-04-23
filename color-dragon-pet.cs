// ColorDragon.cs - A specific pet implementation
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDragon : PetBase
{
    [Header("Dragon Specific")]
    [SerializeField] private float flightEnergyCost = 10f;
    [SerializeField] private float colorChangeInterval = 3f;
    [SerializeField] private GameObject flameEffectPrefab;
    [SerializeField] private List<Color> availableColors = new List<Color>();
    
    private Color currentColor;
    private int colorIndex = 0;
    private GameObject activeFlameEffect;
    private bool isFlying = false;
    
    // Special dragon abilities
    public enum DragonAbility
    {
        ColorChange,
        Flight,
        FireBreath,
        ElementalShield,
        ColorBurst
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, DragonAbility> levelAbilities = new Dictionary<int, DragonAbility>()
    {
        { 3, DragonAbility.ColorChange },
        { 7, DragonAbility.Flight },
        { 10, DragonAbility.FireBreath },
        { 15, DragonAbility.ElementalShield },
        { 20, DragonAbility.ColorBurst }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for dragon
        if (availableColors.Count == 0)
        {
            // Default colors if none set in inspector
            availableColors.Add(Color.red);    // Fire
            availableColors.Add(Color.blue);   // Water
            availableColors.Add(Color.green);  // Nature
            availableColors.Add(Color.yellow); // Electric
            availableColors.Add(new Color(0.5f, 0, 1f)); // Dark/Purple
        }
        
        // Set initial color
        SetPetColor(availableColors[0]);
    }
    
    private void Start()
    {
        // Start color cycling if ability is unlocked
        if (HasAbility(DragonAbility.ColorChange))
        {
            StartCoroutine(CycleColors());
        }
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Dragons have higher initial energy but lower happiness
        stats.energy = 120f;
        stats.happiness = 80f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for dragon-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // If it's color change, start the cycle
                if (levelAbility.Value == DragonAbility.ColorChange && !IsInvoking("CycleColors"))
                {
                    StartCoroutine(CycleColors());
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Dragons get extra happiness from playing
        stats.happiness = Mathf.Min(stats.happiness + 5f, 100f);
        
        // Use special ability if it's unlocked
        if (HasAbility(DragonAbility.FireBreath) && Random.Range(0, 100) < 30)
        {
            ActivateFireBreath();
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Dragons gain more energy from food
        stats.energy = Mathf.Min(stats.energy + 10f, 120f);
        
        // If food element matches current color, extra bonus
        if (food.preferredBy == element)
        {
            stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
        }
    }
    
    public void ActivateFlight()
    {
        if (!HasAbility(DragonAbility.Flight) || stats.energy < flightEnergyCost)
            return;
        
        isFlying = true;
        stats.energy -= flightEnergyCost;
        stats.happiness = Mathf.Min(stats.happiness + 15f, 100f);
        
        // Play flight animation
        animator.SetBool("IsFlying", true);
        
        // Schedule landing
        Invoke("DeactivateFlight", 5f);
    }
    
    private void DeactivateFlight()
    {
        isFlying = false;
        animator.SetBool("IsFlying", false);
    }
    
    public void ActivateFireBreath()
    {
        if (!HasAbility(DragonAbility.FireBreath))
            return;
        
        animator.SetTrigger("FireBreath");
        
        // Create flame effect
        if (flameEffectPrefab && activeFlameEffect == null)
        {
            activeFlameEffect = Instantiate(flameEffectPrefab, transform.position + transform.forward, Quaternion.identity, transform);
            
            // Color the flame based on current dragon color
            ParticleSystem particles = activeFlameEffect.GetComponent<ParticleSystem>();
            if (particles)
            {
                var main = particles.main;
                main.startColor = currentColor;
            }
            
            // Destroy after short duration
            Destroy(activeFlameEffect, 2f);
        }
    }
    
    private void SetPetColor(Color newColor)
    {
        currentColor = newColor;
        
        // Update sprite color
        if (spriteRenderer)
        {
            spriteRenderer.color = newColor;
        }
        
        // Update pet element based on color
        UpdateElementFromColor(newColor);
    }
    
    private void UpdateElementFromColor(Color color)
    {
        // Determine element based on color
        if (color == Color.red)
        {
            element = PetElement.Fire;
        }
        else if (color == Color.blue)
        {
            element = PetElement.Water;
        }
        else if (color == Color.green)
        {
            element = PetElement.Nature;
        }
        else if (color == Color.yellow)
        {
            element = PetElement.Electric;
        }
        else if (color.g < 0.2f && color.b > 0.5f) // Purple/Dark
        {
            element = PetElement.Dark;
        }
        else
        {
            element = PetElement.Light;
        }
    }
    
    private IEnumerator CycleColors()
    {
        while (HasAbility(DragonAbility.ColorChange))
        {
            yield return new WaitForSeconds(colorChangeInterval);
            
            // Only change color if not in the middle of another action
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Eat") && 
                !animator.GetCurrentAnimatorStateInfo(0).IsName("Play"))
            {
                // Move to next color
                colorIndex = (colorIndex + 1) % availableColors.Count;
                SetPetColor(availableColors[colorIndex]);
                
                // Play color change effect
                animator.SetTrigger("ColorChange");
            }
        }
    }
    
    public void ActivateColorBurst()
    {
        if (!HasAbility(DragonAbility.ColorBurst))
            return;
        
        animator.SetTrigger("ColorBurst");
        
        // Change colors rapidly
        StartCoroutine(ColorBurstEffect());
    }
    
    private IEnumerator ColorBurstEffect()
    {
        float duration = 3f;
        float elapsed = 0f;
        float interval = 0.2f;
        
        while (elapsed < duration)
        {
            // Random color
            int randomIndex = Random.Range(0, availableColors.Count);
            SetPetColor(availableColors[randomIndex]);
            
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        
        // Return to previous color
        SetPetColor(availableColors[colorIndex]);
    }
    
    public void ActivateElementalShield()
    {
        if (!HasAbility(DragonAbility.ElementalShield))
            return;
        
        // Add shield effect
        animator.SetTrigger("Shield");
        
        // In a real game, this would add temporary protection
        // For demonstration, just add some health
        stats.health = Mathf.Min(stats.health + 20f, 100f);
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Dragons lose energy faster
        stats.energy = Mathf.Max(stats.energy - 5f, 0f);
        
        // But they gain health back naturally
        stats.health = Mathf.Min(stats.health + 5f, 100f);
    }
    
    // Helper to check if a specific dragon ability is unlocked
    private bool HasAbility(DragonAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    protected override void UpdateAccessoryVisual()
    {
        base.UpdateAccessoryVisual();
        
        // Additional dragon-specific accessory logic
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
