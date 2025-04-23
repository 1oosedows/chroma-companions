// PetFactory.cs - Generates pet prefabs and registers them with the system
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFactory : MonoBehaviour
{
    [Header("Pet Prefab References")]
    [SerializeField] private GameObject basePetPrefab;
    [SerializeField] private Transform petsContainer;
    
    [Header("Pet Resources")]
    [SerializeField] private Sprite[] petSprites;
    [SerializeField] private RuntimeAnimatorController[] petAnimators;
    [SerializeField] private GameObject[] petEffects;
    
    // Element color mapping
    private Dictionary<PetElement, Color> elementColors = new Dictionary<PetElement, Color>()
    {
        { PetElement.Fire, new Color(1f, 0.3f, 0.1f) },       // Orange-red
        { PetElement.Water, new Color(0.2f, 0.4f, 1f) },      // Blue
        { PetElement.Earth, new Color(0.6f, 0.4f, 0.2f) },    // Brown
        { PetElement.Air, new Color(0.8f, 0.8f, 1f) },        // Light blue
        { PetElement.Light, new Color(1f, 0.9f, 0.6f) },      // Yellow
        { PetElement.Dark, new Color(0.4f, 0.1f, 0.6f) },     // Purple
        { PetElement.Nature, new Color(0.3f, 0.8f, 0.3f) },   // Green
        { PetElement.Electric, new Color(1f, 0.9f, 0.2f) }    // Yellow
    };
    
    // Pet type lookup
    private Dictionary<string, System.Type> petTypes = new Dictionary<string, System.Type>();
    
    private void Awake()
    {
        // Register all pet types
        RegisterPetTypes();
    }
    
    private void Start()
    {
        // Create all pet prefabs and register them
        RegisterAllPets();
    }
    
    private void RegisterPetTypes()
    {
        // Register each pet class type with its ID
        petTypes.Add("ColorDragon", typeof(ColorDragon));
        petTypes.Add("RainbowBunny", typeof(RainbowBunny));
        petTypes.Add("PuzzleFox", typeof(PuzzleFox));
        petTypes.Add("CosmicOwl", typeof(CosmicOwl));
        petTypes.Add("MelodyFish", typeof(MelodyFish));
        petTypes.Add("CrystalDeer", typeof(CrystalDeer));
        petTypes.Add("SparkHamster", typeof(SparkHamster));
        petTypes.Add("ShadowCat", typeof(ShadowCat));
        petTypes.Add("BubbleOctopus", typeof(BubbleOctopus));
        petTypes.Add("EmberPhoenix", typeof(EmberPhoenix));
        petTypes.Add("EarthTortoise", typeof(EarthTortoise));
    }
    
    public void RegisterAllPets()
    {
        if (PetRegistry.Instance == null)
        {
            Debug.LogError("PetRegistry not found!");
            return;
        }
        
        // Register each pet type with the registry
        RegisterColorDragon();
        RegisterRainbowBunny();
        RegisterPuzzleFox();
        RegisterCosmicOwl();
        RegisterMelodyFish();
        RegisterCrystalDeer();
        RegisterSparkHamster();
        RegisterShadowCat();
        RegisterBubbleOctopus();
        RegisterEmberPhoenix();
        RegisterEarthTortoise();
    }
    
    private GameObject CreatePetPrefab(string petId, PetElement element, int spriteIndex)
    {
        if (basePetPrefab == null)
        {
            Debug.LogError("Base pet prefab not set!");
            return null;
        }
        
        // Create a new pet prefab
        GameObject petPrefab = Instantiate(basePetPrefab);
        petPrefab.name = petId + "_Prefab";
        
        // Set parent to pets container if available
        if (petsContainer != null)
        {
            petPrefab.transform.SetParent(petsContainer);
        }
        
        // Get pet component
        System.Type petType = petTypes[petId];
        PetBase petComponent = petPrefab.AddComponent(petType) as PetBase;
        
        // Setup visual components
        SpriteRenderer spriteRenderer = petPrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && petSprites != null && spriteIndex < petSprites.Length)
        {
            spriteRenderer.sprite = petSprites[spriteIndex];
            
            // Apply element color tint
            if (elementColors.TryGetValue(element, out Color tint))
            {
                spriteRenderer.color = tint;
            }
        }
        
        // Setup animator
        Animator animator = petPrefab.GetComponent<Animator>();
        if (animator != null && petAnimators != null && spriteIndex < petAnimators.Length)
        {
            animator.runtimeAnimatorController = petAnimators[spriteIndex];
        }
        
        // Deactivate the prefab (it should only be used as a template)
        petPrefab.SetActive(false);
        
        return petPrefab;
    }
    
    private void RegisterColorDragon()
    {
        string petId = "ColorDragon";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Fire, 0);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Color Dragon",
            description = "A magical dragon that can change colors and harness different elemental powers. Excels at adapting to different situations.",
            rarity = PetRarity.Rare,
            defaultElement = PetElement.Fire,
            previewImage = petSprites != null && petSprites.Length > 0 ? petSprites[0] : null,
            petPrefab = prefab,
            unlockLevel = 1, // Available as starter
            baseCost = 150,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Color Change - Cycle through different elemental colors",
                "Flight - Temporarily fly for enhanced mobility",
                "Fire Breath - Powerful attack that creates flame effects",
                "Elemental Shield - Protection from negative effects",
                "Color Burst - Ultimate ability that rapidly changes colors for massive effects"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterRainbowBunny()
    {
        string petId = "RainbowBunny";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Light, 1);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Rainbow Bunny",
            description = "A cheerful bunny that creates rainbows as it hops around. Specializes in generating resources and boosting happiness.",
            rarity = PetRarity.Uncommon,
            defaultElement = PetElement.Light,
            previewImage = petSprites != null && petSprites.Length > 1 ? petSprites[1] : null,
            petPrefab = prefab,
            unlockLevel = 1, // Available as starter
            baseCost = 100,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Rainbow Trail - Creates colorful paths that boost happiness",
                "Resource Generation - Passively creates resources over time",
                "Super Hop - Enhanced jumping ability with special effects",
                "Lucky Charm - Increases chances of finding rare items",
                "Colorful Dreams - Generate resources while sleeping"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterPuzzleFox()
    {
        string petId = "PuzzleFox";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Air, 2);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Puzzle Fox",
            description = "A clever fox with exceptional problem-solving abilities. Provides bonuses for mini-games and can find hidden treasures.",
            rarity = PetRarity.Uncommon,
            defaultElement = PetElement.Air,
            previewImage = petSprites != null && petSprites.Length > 2 ? petSprites[2] : null,
            petPrefab = prefab,
            unlockLevel = 1, // Available as starter
            baseCost = 100,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Smart Bonus - Increases scores in puzzle mini-games",
                "Prize Finder - Chance to find rare items",
                "Quick Reflex - Bonus for timed challenges",
                "Treasure Hunter - Can find hidden treasures in the world",
                "Master Trick - Special performance for extra rewards"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterCosmicOwl()
    {
        string petId = "CosmicOwl";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Light, 3);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Cosmic Owl",
            description = "A wise owl with celestial powers that can manipulate time and predict the future. Provides valuable gameplay insights.",
            rarity = PetRarity.Rare,
            defaultElement = PetElement.Light,
            previewImage = petSprites != null && petSprites.Length > 3 ? petSprites[3] : null,
            petPrefab = prefab,
            unlockLevel = 5,
            baseCost = 200,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Wisdom - Passive bonus to experience gain",
                "Night Vision - Reveal hidden items in darkness",
                "Time Pause - Pause timers briefly in mini-games",
                "Prophecy - Predict patterns in challenges",
                "Cosmic Insight - Major gameplay advantage"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterMelodyFish()
    {
        string petId = "MelodyFish";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Water, 4);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Melody Fish",
            description = "A musical fish that creates harmonious tunes underwater. Excels at rhythm-based challenges and provides mood boosts.",
            rarity = PetRarity.Uncommon,
            defaultElement = PetElement.Water,
            previewImage = petSprites != null && petSprites.Length > 4 ? petSprites[4] : null,
            petPrefab = prefab,
            unlockLevel = 3,
            baseCost = 120,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Harmony - Music improves happiness of all pets",
                "Bubble Shield - Protection from negative effects",
                "Rhythm Boost - Bonus for rhythm mini-games",
                "Ocean Melody - Generate currency when playing music",
                "Choral Surge - Group performance for major bonuses"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterCrystalDeer()
    {
        string petId = "CrystalDeer";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Nature, 5);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Crystal Deer",
            description = "A majestic deer with crystalline antlers that can heal and grow plants. Creates serene environments wherever it goes.",
            rarity = PetRarity.Rare,
            defaultElement = PetElement.Nature,
            previewImage = petSprites != null && petSprites.Length > 5 ? petSprites[5] : null,
            petPrefab = prefab,
            unlockLevel = 8,
            baseCost = 180,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Nature Touch - Grows plants that provide food",
                "Crystal Growth - Grows crystals that provide currency",
                "Healing Pulse - Heals other pets",
                "Serenity - Calms all pets and increases happiness",
                "Natural Resonance - Major boost to all pet stats temporarily"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterSparkHamster()
    {
        string petId = "SparkHamster";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Electric, 6);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Spark Hamster",
            description = "An energetic hamster that generates electricity as it runs. Provides speed boosts and powers electrical devices.",
            rarity = PetRarity.Common,
            defaultElement = PetElement.Electric,
            previewImage = petSprites != null && petSprites.Length > 6 ? petSprites[6] : null,
            petPrefab = prefab,
            unlockLevel = 2,
            baseCost = 80,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Energy Storage - Store more energy than normal",
                "Speed Boost - Temporary speed boost for mini-games",
                "Power Surge - Generate electricity for currency",
                "Electric Barrier - Protective shield",
                "Hyper Charge - Major boost to all stats temporarily"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterShadowCat()
    {
        string petId = "ShadowCat";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Dark, 7);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Shadow Cat",
            description = "A mysterious cat that can blend with shadows and create illusions. Excels at finding hidden treasures and stealth.",
            rarity = PetRarity.Rare,
            defaultElement = PetElement.Dark,
            previewImage = petSprites != null && petSprites.Length > 7 ? petSprites[7] : null,
            petPrefab = prefab,
            unlockLevel = 7,
            baseCost = 160,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Night Stalker - Find items in the dark",
                "Stealth - Become invisible temporarily",
                "Shadow Clone - Create illusions",
                "Treasure Finder - Chance to find extra rewards",
                "Void Walk - Ultimate teleport and prize finding"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterBubbleOctopus()
    {
        string petId = "BubbleOctopus";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Water, 8);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Bubble Octopus",
            description = "A clever octopus that can create protective bubbles and manipulate water. Excels at multitasking and defensive abilities.",
            rarity = PetRarity.Uncommon,
            defaultElement = PetElement.Water,
            previewImage = petSprites != null && petSprites.Length > 8 ? petSprites[8] : null,
            petPrefab = prefab,
            unlockLevel = 4,
            baseCost = 130,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Multitasking - Can perform multiple actions at once",
                "Bubble Shield - Protective bubble",
                "Ink Cloud - Defensive escape mechanism",
                "Water Control - Control water for bonuses",
                "Bubble Tsunami - Ultimate wave attack"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterEmberPhoenix()
    {
        string petId = "EmberPhoenix";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Fire, 9);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Ember Phoenix",
            description = "A majestic phoenix with flames that can heal and regenerate. Can rise from defeat with enhanced abilities.",
            rarity = PetRarity.Epic,
            defaultElement = PetElement.Fire,
            previewImage = petSprites != null && petSprites.Length > 9 ? petSprites[9] : null,
            petPrefab = prefab,
            unlockLevel = 10,
            baseCost = 250,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Ember Glow - Passive glow that provides bonuses",
                "Healing Flames - Heal self and others",
                "Firebird Dash - Quick dash leaving fire trail",
                "Rebirth - Resurrect from defeat with bonus",
                "Sunburst Flare - Ultimate attack that transforms"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    private void RegisterEarthTortoise()
    {
        string petId = "EarthTortoise";
        GameObject prefab = CreatePetPrefab(petId, PetElement.Earth, 10);
        
        PetRegistryEntry entry = new PetRegistryEntry
        {
            petId = petId,
            displayName = "Earth Tortoise",
            description = "A sturdy tortoise with earth-manipulating abilities. Provides superior defense and can grow useful plants.",
            rarity = PetRarity.Uncommon,
            defaultElement = PetElement.Earth,
            previewImage = petSprites != null && petSprites.Length > 10 ? petSprites[10] : null,
            petPrefab = prefab,
            unlockLevel = 6,
            baseCost = 140,
            isLimited = false,
            specialAbilities = new string[] 
            { 
                "Shell Defense - Enhanced protection from shell",
                "Earth Affinity - Passive boost from earth connection",
                "Terrain Shift - Manipulate earth for bonuses",
                "Garden Growth - Grow plants that provide food",
                "Earthquake Stomp - Ultimate attack that stuns"
            }
        };
        
        PetRegistry.Instance.RegisterPet(entry);
    }
    
    // Helper method to get a pet prefab by ID
    public GameObject GetPetPrefab(string petId)
    {
        if (PetRegistry.Instance != null)
        {
            PetRegistryEntry entry = PetRegistry.Instance.GetPetById(petId);
            if (entry != null)
            {
                return entry.petPrefab;
            }
        }
        
        return null;
    }
}
