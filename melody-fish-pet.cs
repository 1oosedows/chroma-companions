// MelodyFish.cs - A musical pet with rhythm-based abilities
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MelodyFish : PetBase
{
    [Header("Fish Specific")]
    [SerializeField] private float musicalBonusMultiplier = 0.2f; // 20% bonus to happiness from play
    [SerializeField] private float bubbleDuration = 5f;
    [SerializeField] private GameObject musicalNotesPrefab;
    [SerializeField] private GameObject bubbleEffectPrefab;
    [SerializeField] private int maxSongsPerDay = 3;
    
    private int songsPlayedToday = 0;
    private bool isBubbleActive = false;
    
    // Special fish abilities
    public enum FishAbility
    {
        Harmony,       // Music improves happiness of all pets
        BubbleShield,  // Protection from negative effects
        RhythmBoost,   // Bonus for rhythm mini-games
        OceanMelody,   // Generate currency when playing music
        ChoralSurge    // Group performance for major bonuses
    }
    
    // List of abilities unlocked at different levels
    private Dictionary<int, FishAbility> levelAbilities = new Dictionary<int, FishAbility>()
    {
        { 2, FishAbility.Harmony },
        { 5, FishAbility.BubbleShield },
        { 9, FishAbility.RhythmBoost },
        { 13, FishAbility.OceanMelody },
        { 17, FishAbility.ChoralSurge }
    };
    
    protected override void Awake()
    {
        base.Awake();
        
        // Additional setup for fish
        element = PetElement.Water; // Fish start as Water element
    }
    
    protected override void InitializeDefaultStats()
    {
        base.InitializeDefaultStats();
        
        // Fish have higher initial happiness but lower health
        stats.happiness = 110f;
        stats.health = 90f;
    }
    
    protected override void CheckForAbilityUnlock()
    {
        base.CheckForAbilityUnlock();
        
        // Check for fish-specific ability unlocks based on level
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
        
        // Fish get extra happiness from playing with music toys
        if (toy.preferredBy == PetElement.Water)
        {
            float bonusHappiness = 10f;
            
            // Apply musical bonus if ability is unlocked
            if (HasAbility(FishAbility.Harmony))
            {
                bonusHappiness *= (1f + musicalBonusMultiplier);
            }
            
            stats.happiness = Mathf.Min(stats.happiness + bonusHappiness, 110f);
            
            // Chance to play a song
            if (Random.Range(0, 100) < 40 && songsPlayedToday < maxSongsPerDay)
            {
                PlaySong();
            }
        }
    }
    
    public override void Feed(FoodItem food)
    {
        base.Feed(food);
        
        // Fish prefer water-based foods
        if (food.preferredBy == PetElement.Water)
        {
            stats.happiness = Mathf.Min(stats.happiness + 10f, 110f);
            stats.health = Mathf.Min(stats.health + 5f, 90f);
        }
    }
    
    public void PlaySong()
    {
        if (songsPlayedToday >= maxSongsPerDay)
            return;
        
        songsPlayedToday++;
        
        // Play animation
        animator.SetTrigger("Sing");
        
        // Create musical notes effect
        if (musicalNotesPrefab)
        {
            GameObject notes = Instantiate(musicalNotesPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(notes, 3f);
        }
        
        // Apply happiness boost to all active pets (if Harmony ability)
        if (HasAbility(FishAbility.Harmony))
        {
            // In a full game, this would affect all active pets
            // For now, just add extra happiness to this pet
            stats.happiness = Mathf.Min(stats.happiness + 5f, 110f);
        }
        
        // Generate currency if Ocean Melody ability
        if (HasAbility(FishAbility.OceanMelody))
        {
            int currencyAmount = Random.Range(1, 5);
            GameManager.Instance.AddCurrency(currencyAmount);
            UIManager.Instance.ShowMessage("Your fish's song generated " + currencyAmount + " coins!");
        }
    }
    
    public void ActivateBubbleShield()
    {
        if (!HasAbility(FishAbility.BubbleShield) || isBubbleActive)
            return;
        
        // Activate bubble shield
        isBubbleActive = true;
        
        // Visual effect
        animator.SetTrigger("BubbleShield");
        
        if (bubbleEffectPrefab)
        {
            GameObject bubble = Instantiate(bubbleEffectPrefab, transform.position, Quaternion.identity, transform);
            Destroy(bubble, bubbleDuration);
        }
        
        // In a real game, this would protect from negative status effects
        
        // Use some energy
        stats.energy = Mathf.Max(stats.energy - 15f, 0f);
        
        UIManager.Instance.ShowMessage("Bubble shield activated!");
        
        // Deactivate after duration
        StartCoroutine(DeactivateBubbleShield());
    }
    
    private IEnumerator DeactivateBubbleShield()
    {
        yield return new WaitForSeconds(bubbleDuration);
        isBubbleActive = false;
    }
    
    public float GetRhythmGameBonus()
    {
        if (HasAbility(FishAbility.RhythmBoost))
        {
            return 0.25f; // 25% bonus for rhythm mini-games
        }
        
        return 0f;
    }
    
    public void ActivateChoralSurge()
    {
        if (!HasAbility(FishAbility.ChoralSurge))
            return;
        
        // This is the ultimate ability - coordinated music performance
        animator.SetTrigger("ChoralSurge");
        
        // Use significant energy
        stats.energy = Mathf.Max(stats.energy - 50f, 0f);
        
        // Create enhanced musical notes effect
        if (musicalNotesPrefab)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1.5f), Random.Range(-1f, 1f));
                GameObject notes = Instantiate(musicalNotesPrefab, transform.position + offset, Quaternion.identity);
                Destroy(notes, 5f);
            }
        }
        
        // Major bonus
        int bonusAmount = stats.level * 5;
        GameManager.Instance.AddCurrency(bonusAmount);
        
        UIManager.Instance.ShowMessage("Choral surge creates beautiful harmony! +" + bonusAmount + " coins!");
        
        // In a full game, this would provide significant bonuses to all pets
    }
    
    public override void UpdateDailyStats()
    {
        base.UpdateDailyStats();
        
        // Reset songs counter
        songsPlayedToday = 0;
        
        // Fish recover more happiness naturally
        stats.happiness = Mathf.Min(stats.happiness + 10f, 110f);
    }
    
    public override void Rest()
    {
        base.Rest();
        
        // Fish recover more when resting in water
        stats.energy = Mathf.Min(stats.energy + 10f, 100f);
        stats.health = Mathf.Min(stats.health + 5f, 90f);
    }
    
    // Helper to check if a specific fish ability is unlocked
    private bool HasAbility(FishAbility ability)
    {
        return abilities.Contains(ability.ToString());
    }
    
    // Check if bubble shield is active
    public bool IsBubbleShieldActive()
    {
        return isBubbleActive && HasAbility(FishAbility.BubbleShield);
    }
}
