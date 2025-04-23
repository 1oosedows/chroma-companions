// EarthTortoise.cs - A defensive pet with earth and protection abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthTortoise : PetBase
{
    [Header("Tortoise Specific")]
    [SerializeField] private float shellDefenseBonus = 50f;
    [SerializeField] private int maxShellDefensesPerDay = 3;
    [SerializeField] private GameObject earthEffectPrefab;
    [SerializeField] private GameObject shellGlowPrefab;
    [SerializeField] private GameObject earthquakePrefab;
    [SerializeField] private float gardenGrowthRate = 0.1f; // Growth per day
    
    private int shellDefensesToday = 0;
    private bool isInShell = false;
    private float gardenGrowth = 0f; // 0-1 range for garden progress
    private List<GameObject> activeEarthEffects = new List<GameObject>();
    
    // Special tortoise abilities
    public enum TortoiseAbility
    {
        ShellDefense,    // Enhanced protection from shell
        EarthAffinity,   // Passive boost from earth connection
        TerrainShift,    // Manipulate earth for bonuses
        GardenGrowth,    // Grow plants that provide food
        EarthquakeStomp  // Ultimate attack that stuns
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, TortoiseAbility> levelAbilities = new Dictionary<int, TortoiseAbility>()
    {
        { 2, TortoiseAbility.ShellDefense },
        { 5, TortoiseAbility.EarthAffinity },
        { 9, TortoiseAbility.TerrainShift },
        { 13, TortoiseAbility.GardenGrowth },
        { 19, TortoiseAbility.EarthquakeStomp }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for tortoise
        element = PetElement.Earth; // Tortoise starts as Earth element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Tortoise have higher initial health but lower energy and slower movement
        stats.health = 130f;
        stats.energy = 80f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for tortoise-specific ability unlocks based on level
        foreach (var levelAbility in levelAbilities)
        {
            if (stats.level >= levelAbility.Key && !HasAbility(levelAbility.Value.ToString()))
            {
                // Unlock the ability
                abilities.Add(levelAbility.Value.ToString());
                
                // Notify about new ability
                string abilityName = levelAbility.Value.ToString();
                OnAbilityUnlocked?.Invoke(abilityName);
                
                // Initialize effects for earth affinity
                if (levelAbility.Value == TortoiseAbility.EarthAffinity)
                {
                    ActivateEarthAffinity();
                }
            }
        }
    }
    
    public override void Play(ToyItem toy)
    {
        base.Play(toy);
        
        // Tortoise prefer earth/rock toys
        if (toy.preferredBy == PetElement.Earth)
        {
            stats.happiness = Mathf.Min(stats.happiness + 15f, 100f);
            
            // Chance to activate terrain shift
            if (HasAbility(TortoiseAbility.TerrainShift) && Random.Range(0, 100) < 30)
            {
                ActivateTerrainShift();
            }
        }
        
        // Exit shell if in it
        if (isInShell)
        {
            ExitShell();
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Tortoise prefer vegetable foods
        if (food.preferredBy == PetElement.Earth || food.preferredBy == PetElement.Nature)
        {
            stats.health = Mathf.Min(stats.health + 15f, 130f);
            
            // Boost garden growth if ability unlocked
            if (HasAbility(TortoiseAbility.GardenGrowth))
            {
                gardenGrowth = Mathf.Min(gardenGrowth + 0.05f, 1f);
            }
        }
    }
    
    public void ActivateShellDefense()
    {
        if (!HasAbility(TortoiseAbility.ShellDefense) || isInShell || shellDefensesToday >= maxShellDefensesPerDay)
            return;
        
        shellDefensesToday++;
        isInShell = true;
        
        // Play animation
        animator.SetTrigger("EnterShell");
        
        // Create shell glow effect
        if (shellGlowPrefab != null)
        {
            GameObject shellGlow = Instantiate(shellGlowPrefab, transform.position, Quaternion.identity, transform);
            activeEarthEffects.Add(shellGlow);
        }
        
        // Defensive boost
        // In a full game, this would reduce incoming damage
        
        UIManager.Instance.ShowMessage("Shell defense activated!");
    }
    
    public void ExitShell()
    {
        if (!isInShell)
            return;
        
        isInShell = false;
        
        // Play animation
        animator.SetTrigger("ExitShell");
        
        // Remove shell glow effect
        foreach (GameObject effect in activeEarthEffects)
        {
            if (effect != null && effect.transform.parent == transform)
            {
                Destroy(effect);
            }
        }
        activeEarthEffects.Clear();
        
        UIManager.Instance.ShowMessage("Exited shell defense.");
    }
    
    public void ActivateEarthAffinity()
    {
        if (!HasAbility(TortoiseAbility.EarthAffinity))
            return;
        
        // Create subtle earth effect
        if (earthEffectPrefab != null)
        {
            GameObject earthEffect = Instantiate(earthEffectPrefab, 
                transform.position + Vector3.down * 0.5f, 
                Quaternion.identity, 
                transform);
            
            activeEarthEffects.Add(earthEffect);
        }
        
        // In a full game, this would provide passive bonuses on earth/natural terrain
        
        UIManager.Instance.ShowMessage("Earth affinity activated! Stronger on natural terrain.");
    }
    
    public void ActivateTerrainShift()
    {
        if (!HasAbility(TortoiseAbility.TerrainShift))
            return;
        
        // Play animation
        animator.SetTrigger("TerrainShift");
        
        // Create earth effect
        StartCoroutine(TerrainShiftEffect());
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        UIManager.Instance.ShowMessage("Terrain shift activated!");
        
        // Small chance to find item
        if (Random.Range(0, 100) < 40)
        {
            int coinAmount = Random.Range(5, 15);
            GameManager.Instance.AddCurrency(coinAmount);
            UIManager.Instance.ShowMessage("Found " + coinAmount + " coins in the earth!");
        }
    }
    
    private IEnumerator TerrainShiftEffect()
    {
        // Create rippling earth effect
        for (int i = 0; i < 3; i++)
        {
            if (earthEffectPrefab != null)
            {
                float radius = (i + 1) * 1.5f;
                int numRocks = 8;
                
                for (int j = 0; j < numRocks; j++)
                {
                    float angle = (360f / numRocks) * j * Mathf.Deg2Rad;
                    Vector3 position = transform.position + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0,
                        Mathf.Sin(angle) * radius
                    );
                    
                    GameObject rockEffect = Instantiate(earthEffectPrefab, position, Quaternion.identity);
                    
                    // Animate rock rising
                    StartCoroutine(AnimateRockRise(rockEffect));
                }
            }
            
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    private IEnumerator AnimateRockRise(GameObject rock)
    {
        if (rock == null)
            yield break;
        
        Vector3 startPos = rock.transform.position;
        Vector3 peakPos = startPos + Vector3.up * 0.5f;
        Vector3 endPos = startPos;
        
        // Rise up
        float riseTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < riseTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseTime;
            
            rock.transform.position = Vector3.Lerp(startPos, peakPos, t);
            
            yield return null;
        }
        
        // Hold briefly
        yield return new WaitForSeconds(0.2f);
        
        // Sink down
        elapsed = 0f;
        while (elapsed < riseTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseTime;
            
            rock.transform.position = Vector3.Lerp(peakPos, endPos, t);
            
            yield return null;
        }
        
        Destroy(rock);
    }
    
    public void ActivateGardenGrowth()
    {
        if (!HasAbility(TortoiseAbility.GardenGrowth))
            return;
        
        // Play animation
        animator.SetTrigger("GardenGrowth");
        
        // Create garden effect
        StartCoroutine(GardenGrowthEffect());
        
        // Use energy
        stats.energy = Mathf.Max(stats.energy - 20f, 0f);
        
        // In a full game, this would create food items
        
        // Generate rewards based on garden growth
        int foodAmount = Mathf.RoundToInt(gardenGrowth * 10f);
        int coinAmount = Mathf.RoundToInt(gardenGrowth * 5f);
        
        if (foodAmount > 0 || coinAmount > 0)
        {
            GameManager.Instance.AddCurrency(coinAmount);
            UIManager.Instance.ShowMessage("Your garden produced food and " + coinAmount + " coins!");
        }
        else
        {
            UIManager.Instance.ShowMessage("Your garden needs more care to produce rewards.");
        }
    }
    
    private IEnumerator GardenGrowthEffect()
    {
        // Create growing plants effect
        if (earthEffectPrefab != null)
        {
            int numPlants = Mathf.RoundToInt(gardenGrowth * 10f) + 2;
            
            for (int i = 0; i < numPlants; i++)
            {
                Vector3 position = transform.position + new Vector3(
                    Random.Range(-2f, 2f),
                    0,
                    Random.Range(-2f, 2f)
                );
                
                GameObject plant = Instantiate(earthEffectPrefab, position, Quaternion.identity);
                
                // Scale it to look like a plant
                plant.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                
                // Animate growth
                StartCoroutine(AnimatePlantGrowth(plant));
                
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
    
    private IEnumerator AnimatePlantGrowth(GameObject plant)
    {
        if (plant == null)
            yield break;
        
        Vector3 startScale = Vector3.zero;
        Vector3 fullScale = new Vector3(0.5f, 1f + gardenGrowth, 0.5f);
        
        // Grow up
        float growTime = 1f;
        float elapsed = 0f;
        
        while (elapsed < growTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growTime;
            
            plant.transform.localScale = Vector3.Lerp(startScale, fullScale, t);
            
            yield return null;
        }
        
        // Stay for a while
        yield return new WaitForSeconds(5f);
        
        // Wither away
        elapsed = 0f;
        while (elapsed < growTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growTime;
            
            plant.transform.localScale = Vector3.Lerp(fullScale, startScale, t);
            
            yield return null;
        }
        
        Destroy(plant);
    }
    
    public void ActivateEarthquakeStomp()
    {
        if (!HasAbility(TortoiseAbility.EarthquakeStomp))
            return;
        
        // This is the ultimate ability - massive earthquake
        animator.SetTrigger("EarthquakeStomp");
        
        // Create earthquake effect
        StartCoroutine(EarthquakeEffect());
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 50f, 0f);
        
        UIManager.Instance.ShowMessage("Earthquake stomp activated!");
        
        // Major rewards
        int currencyAmount = 25 + stats.level * 2;
        GameManager.Instance.AddCurrency(currencyAmount);
        
        UIManager.Instance.ShowMessage("The earthquake reveals " + currencyAmount + " coins!");
    }
    
    private IEnumerator EarthquakeEffect()
    {
        // Create camera shake effect
        StartCoroutine(CameraShakeEffect());
        
        // Create expanding earthquake wave
        if (earthquakePrefab != null)
        {
            GameObject quake = Instantiate(earthquakePrefab, transform.position, Quaternion.identity);
            
            // Expand the earthquake
            float duration = 3f;
            float elapsed = 0f;
            float startScale = 1f;
            float endScale = 10f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float currentScale = Mathf.Lerp(startScale, endScale, t);
                quake.transform.localScale = Vector3.one * currentScale;
                
                yield return null;
            }
            
            Destroy(quake);
        }
        
        // Create many rock effects
        if (earthEffectPrefab != null)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector3 position = transform.position + new Vector3(
                    Random.Range(-5f, 5f),
                    0,
                    Random.Range(-5f, 5f)
                );
                
                GameObject rock = Instantiate(earthEffectPrefab, position, Quaternion.identity);
                
                // Animate rock
                StartCoroutine(AnimateRockRise(rock));
                
                if (i % 5 == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
    
    private IEnumerator CameraShakeEffect()
    {
        // In a full game, this would shake the camera
        // For now, just wait
        yield return new WaitForSeconds(3f);
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset shell defenses counter
        shellDefensesToday = 0;
        
        // Grow garden if ability unlocked
        if (HasAbility(TortoiseAbility.GardenGrowth))
        {
            gardenGrowth = Mathf.Min(gardenGrowth + gardenGrowthRate, 1f);
        }
        
        // Tortoise recover more on earth/natural terrain
        // In a full game, this would check terrain type
        stats.health = Mathf.Min(stats.health + 10f, 130f);
    }
    
    public void TakeDamage(float damageAmount)
    {
        // If in shell, reduce damage
        if (isInShell && HasAbility(TortoiseAbility.ShellDefense))
        {
            float damage = damageAmount * (1f - (shellDefenseBonus / 100f));
            stats.health = Mathf.Max(stats.health - damage, 0f);
            
            UIManager.Instance.ShowMessage("Shell absorbed " + 
                Mathf.RoundToInt(damageAmount - damage) + " damage!");
        }
        else
        {
            // Full damage
            stats.health = Mathf.Max(stats.health - damageAmount, 0f);
        }
    }
    
    // Helper to check if a specific tortoise ability is unlocked
    private bool HasAbility(TortoiseAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Get garden growth for UI
    public float GetGardenGrowth()
    {
        return HasAbility(TortoiseAbility.GardenGrowth) ? gardenGrowth : 0f;
    }
    
    // Check if in shell
    public bool IsInShell()
    {
        return isInShell;
    }
}
