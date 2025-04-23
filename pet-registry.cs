// PetRegistry.cs - Registers and manages all available pet types
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PetRegistryEntry
{
    public string petId;
    public string displayName;
    public string description;
    public PetRarity rarity;
    public PetElement defaultElement;
    public Sprite previewImage;
    public GameObject petPrefab;
    public int unlockLevel;
    public int baseCost;
    public bool isLimited;
    public string[] specialAbilities;
}

public class PetRegistry : MonoBehaviour
{
    private static PetRegistry _instance;
    public static PetRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("PetRegistry instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Pet Entries")]
    [SerializeField] private List<PetRegistryEntry> registeredPets = new List<PetRegistryEntry>();
    
    // Dictionaries for faster lookup
    private Dictionary<string, PetRegistryEntry> petsById = new Dictionary<string, PetRegistryEntry>();
    private Dictionary<PetElement, List<PetRegistryEntry>> petsByElement = new Dictionary<PetElement, List<PetRegistryEntry>>();
    private Dictionary<PetRarity, List<PetRegistryEntry>> petsByRarity = new Dictionary<PetRarity, List<PetRegistryEntry>>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize dictionaries
        InitializeLookups();
    }
    
    private void InitializeLookups()
    {
        // Clear dictionaries
        petsById.Clear();
        petsByElement.Clear();
        petsByRarity.Clear();
        
        // Initialize element dictionary
        foreach (PetElement element in System.Enum.GetValues(typeof(PetElement)))
        {
            petsByElement[element] = new List<PetRegistryEntry>();
        }
        
        // Initialize rarity dictionary
        foreach (PetRarity rarity in System.Enum.GetValues(typeof(PetRarity)))
        {
            petsByRarity[rarity] = new List<PetRegistryEntry>();
        }
        
        // Populate dictionaries
        foreach (PetRegistryEntry pet in registeredPets)
        {
            petsById[pet.petId] = pet;
            petsByElement[pet.defaultElement].Add(pet);
            petsByRarity[pet.rarity].Add(pet);
        }
    }
    
    // Get methods
    public PetRegistryEntry GetPetById(string petId)
    {
        if (petsById.TryGetValue(petId, out PetRegistryEntry pet))
        {
            return pet;
        }
        
        Debug.LogWarning($"Pet with ID {petId} not found in registry!");
        return null;
    }
    
    public List<PetRegistryEntry> GetPetsByElement(PetElement element)
    {
        if (petsByElement.TryGetValue(element, out List<PetRegistryEntry> pets))
        {
            return pets;
        }
        
        return new List<PetRegistryEntry>();
    }
    
    public List<PetRegistryEntry> GetPetsByRarity(PetRarity rarity)
    {
        if (petsByRarity.TryGetValue(rarity, out List<PetRegistryEntry> pets))
        {
            return pets;
        }
        
        return new List<PetRegistryEntry>();
    }
    
    public List<PetRegistryEntry> GetAllPets()
    {
        return new List<PetRegistryEntry>(registeredPets);
    }
    
    public List<PetRegistryEntry> GetAvailablePets(int playerLevel)
    {
        List<PetRegistryEntry> availablePets = new List<PetRegistryEntry>();
        
        foreach (PetRegistryEntry pet in registeredPets)
        {
            if (playerLevel >= pet.unlockLevel && !pet.isLimited)
            {
                availablePets.Add(pet);
            }
        }
        
        return availablePets;
    }
    
    public List<PetRegistryEntry> GetStarterPets()
    {
        // Get pets available at level 1
        return GetAvailablePets(1);
    }
    
    public List<ShopItem> GeneratePetShopItems()
    {
        List<ShopItem> shopItems = new List<ShopItem>();
        int playerLevel = GameManager.Instance.GetPlayerLevel();
        
        foreach (PetRegistryEntry pet in registeredPets)
        {
            // Skip limited pets or pets above player level
            if (pet.isLimited || playerLevel < pet.unlockLevel)
                continue;
            
            ShopItem item = new ShopItem
            {
                itemId = pet.petId,
                displayName = pet.displayName,
                description = pet.description,
                icon = pet.previewImage,
                price = CalculatePetPrice(pet),
                itemType = ShopItemType.Pet,
                requiredLevel = pet.unlockLevel,
                isPremium = pet.rarity >= PetRarity.Epic
            };
            
            shopItems.Add(item);
        }
        
        return shopItems;
    }
    
    private int CalculatePetPrice(PetRegistryEntry pet)
    {
        // Base price modified by rarity
        int rarityMultiplier = 1;
        
        switch (pet.rarity)
        {
            case PetRarity.Common:
                rarityMultiplier = 1;
                break;
            case PetRarity.Uncommon:
                rarityMultiplier = 2;
                break;
            case PetRarity.Rare:
                rarityMultiplier = 4;
                break;
            case PetRarity.Epic:
                rarityMultiplier = 10;
                break;
            case PetRarity.Legendary:
                rarityMultiplier = 25;
                break;
        }
        
        return pet.baseCost * rarityMultiplier;
    }
    
    // Helper methods
    public void RegisterPetsWithShop()
    {
        if (ShopManager.Instance != null)
        {
            List<ShopItem> petShopItems = GeneratePetShopItems();
            // In a full implementation, this would add items to the shop
        }
    }
    
    // Runtime pet registration (for modding support)
    public void RegisterPet(PetRegistryEntry newPet)
    {
        // Check if pet ID already exists
        if (petsById.ContainsKey(newPet.petId))
        {
            Debug.LogWarning($"Pet with ID {newPet.petId} already exists in registry!");
            return;
        }
        
        // Add to registry
        registeredPets.Add(newPet);
        
        // Update lookups
        petsById[newPet.petId] = newPet;
        petsByElement[newPet.defaultElement].Add(newPet);
        petsByRarity[newPet.rarity].Add(newPet);
    }
}
