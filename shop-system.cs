// ShopManager.cs - Handles in-game store functionality
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    private static ShopManager _instance;
    public static ShopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("ShopManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> availablePets = new List<ShopItem>();
    [SerializeField] private List<ShopItem> availableFood = new List<ShopItem>();
    [SerializeField] private List<ShopItem> availableToys = new List<ShopItem>();
    [SerializeField] private List<ShopItem> availableAccessories = new List<ShopItem>();
    
    [Header("UI References")]
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private RectTransform petsContainer;
    [SerializeField] private RectTransform foodContainer;
    [SerializeField] private RectTransform toysContainer;
    [SerializeField] private RectTransform accessoriesContainer;
    [SerializeField] private Button petsTabButton;
    [SerializeField] private Button foodTabButton;
    [SerializeField] private Button toysTabButton;
    [SerializeField] private Button accessoriesTabButton;
    [SerializeField] private GameObject purchaseConfirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmPurchaseButton;
    [SerializeField] private Button cancelPurchaseButton;
    
    // Current state
    private ShopCategory currentCategory = ShopCategory.Pets;
    private ShopItem selectedItem;
    
    // Events
    public Action<ShopItem> OnItemPurchased;
    public Action<FoodItem> OnFoodPurchased;
    public Action<ToyItem> OnToyPurchased;
    public Action<string> OnAccessoryPurchased;
    public Action<string> OnPetPurchased;
    
    // Enum for shop categories
    public enum ShopCategory
    {
        Pets,
        Food,
        Toys,
        Accessories
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Set up UI button listeners
        if (petsTabButton) petsTabButton.onClick.AddListener(() => ShowCategory(ShopCategory.Pets));
        if (foodTabButton) foodTabButton.onClick.AddListener(() => ShowCategory(ShopCategory.Food));
        if (toysTabButton) toysTabButton.onClick.AddListener(() => ShowCategory(ShopCategory.Toys));
        if (accessoriesTabButton) accessoriesTabButton.onClick.AddListener(() => ShowCategory(ShopCategory.Accessories));
        
        if (confirmPurchaseButton) confirmPurchaseButton.onClick.AddListener(ConfirmPurchase);
        if (cancelPurchaseButton) cancelPurchaseButton.onClick.AddListener(CancelPurchase);
        
        if (purchaseConfirmationPanel) purchaseConfirmationPanel.SetActive(false);
        
        // Initialize shop with pets category
        ShowCategory(ShopCategory.Pets);
    }
    
    public void ShowCategory(ShopCategory category)
    {
        currentCategory = category;
        
        // Clear all containers
        ClearContainers();
        
        // Show items for selected category
        switch (category)
        {
            case ShopCategory.Pets:
                PopulateItemContainer(petsContainer, availablePets);
                break;
                
            case ShopCategory.Food:
                PopulateItemContainer(foodContainer, availableFood);
                break;
                
            case ShopCategory.Toys:
                PopulateItemContainer(toysContainer, availableToys);
                break;
                
            case ShopCategory.Accessories:
                PopulateItemContainer(accessoriesContainer, availableAccessories);
                break;
        }
        
        // Update tab selection visual (would need additional UI components)
        UpdateTabVisuals();
    }
    
    private void ClearContainers()
    {
        // Deactivate all containers
        if (petsContainer) petsContainer.gameObject.SetActive(false);
        if (foodContainer) foodContainer.gameObject.SetActive(false);
        if (toysContainer) toysContainer.gameObject.SetActive(false);
        if (accessoriesContainer) accessoriesContainer.gameObject.SetActive(false);
        
        // Clear existing items - optional if containers are reused
        if (petsContainer) ClearContainer(petsContainer);
        if (foodContainer) ClearContainer(foodContainer);
        if (toysContainer) ClearContainer(toysContainer);
        if (accessoriesContainer) ClearContainer(accessoriesContainer);
    }
    
    private void ClearContainer(RectTransform container)
    {
        if (container == null) return;
        
        // Remove all children
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void PopulateItemContainer(RectTransform container, List<ShopItem> items)
    {
        if (container == null || shopItemPrefab == null) return;
        
        // Activate the container
        container.gameObject.SetActive(true);
        
        // Create item UI for each item
        foreach (ShopItem item in items)
        {
            // Skip items that don't meet level requirements
            if (GameManager.Instance.GetPlayerLevel() < item.requiredLevel)
            {
                continue;
            }
            
            GameObject itemObj = Instantiate(shopItemPrefab, container);
            SetupItemUI(itemObj, item);
        }
    }
    
    private void SetupItemUI(GameObject itemObj, ShopItem item)
    {
        // Find components
        Image itemIcon = itemObj.transform.Find("ItemIcon")?.GetComponent<Image>();
        TextMeshProUGUI itemName = itemObj.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemPrice = itemObj.transform.Find("ItemPrice")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemDescription = itemObj.transform.Find("ItemDescription")?.GetComponent<TextMeshProUGUI>();
        Button purchaseButton = itemObj.transform.Find("PurchaseButton")?.GetComponent<Button>();
        
        // Set values
        if (itemIcon && item.icon) itemIcon.sprite = item.icon;
        if (itemName) itemName.text = item.displayName;
        if (itemPrice) itemPrice.text = item.price.ToString();
        if (itemDescription) itemDescription.text = item.description;
        
        // Set button listener
        if (purchaseButton)
        {
            purchaseButton.onClick.AddListener(() => OnItemSelected(item));
        }
    }
    
    private void OnItemSelected(ShopItem item)
    {
        selectedItem = item;
        
        // Show confirmation dialog
        if (purchaseConfirmationPanel && confirmationText)
        {
            confirmationText.text = $"Purchase {item.displayName} for {item.price} coins?";
            purchaseConfirmationPanel.SetActive(true);
        }
        else
        {
            // No confirmation panel, proceed directly
            ProcessPurchase(item);
        }
    }
    
    private void ConfirmPurchase()
    {
        if (selectedItem != null)
        {
            ProcessPurchase(selectedItem);
            selectedItem = null;
        }
        
        if (purchaseConfirmationPanel)
        {
            purchaseConfirmationPanel.SetActive(false);
        }
    }
    
    private void CancelPurchase()
    {
        selectedItem = null;
        
        if (purchaseConfirmationPanel)
        {
            purchaseConfirmationPanel.SetActive(false);
        }
    }
    
    private void ProcessPurchase(ShopItem item)
    {
        // Check if player has enough currency
        if (!GameManager.Instance.SpendCurrency(item.price))
        {
            UIManager.Instance.ShowMessage("Not enough currency!");
            return;
        }
        
        // Process purchase based on item type
        switch (item.itemType)
        {
            case ShopItemType.Pet:
                PurchasePet(item);
                break;
                
            case ShopItemType.Food:
                PurchaseFood(item);
                break;
                
            case ShopItemType.Toy:
                PurchaseToy(item);
                break;
                
            case ShopItemType.Accessory:
                PurchaseAccessory(item);
                break;
        }
        
        // Global purchase event
        OnItemPurchased?.Invoke(item);
    }
    
    private void PurchasePet(ShopItem item)
    {
        // Create pet
        PetBase newPet = GameManager.Instance.AdoptPet(item.itemId);
        
        if (newPet != null)
        {
            UIManager.Instance.ShowMessage($"You've adopted a new {item.displayName}!");
            
            // Event
            OnPetPurchased?.Invoke(item.itemId);
        }
        else
        {
            // Refund on error
            GameManager.Instance.AddCurrency(item.price);
            UIManager.Instance.ShowMessage("Error adopting pet. Please try again.");
        }
    }
    
    private void PurchaseFood(ShopItem item)
    {
        // In a full game, this would add to inventory
        // For this example, we'll create a temporary food item
        FoodItem food = ScriptableObject.CreateInstance<FoodItem>();
        food.id = item.itemId;
        food.displayName = item.displayName;
        food.icon = item.icon;
        food.price = item.price;
        
        // Set default values - in a real game these would be configured in the ScriptableObject
        food.hungerValue = 25f;
        food.happinessBonus = 10f;
        food.healthBonus = 5f;
        
        UIManager.Instance.ShowMessage($"Purchased {item.displayName}!");
        
        // Event
        OnFoodPurchased?.Invoke(food);
    }
    
    private void PurchaseToy(ShopItem item)
    {
        // In a full game, this would add to inventory
        // For this example, we'll create a temporary toy item
        ToyItem toy = ScriptableObject.CreateInstance<ToyItem>();
        toy.id = item.itemId;
        toy.displayName = item.displayName;
        toy.icon = item.icon;
        toy.price = item.price;
        
        // Set default values - in a real game these would be configured in the ScriptableObject
        toy.happinessBonus = 20f;
        toy.energyCost = 15f;
        
        UIManager.Instance.ShowMessage($"Purchased {item.displayName}!");
        
        // Event
        OnToyPurchased?.Invoke(toy);
    }
    
    private void PurchaseAccessory(ShopItem item)
    {
        // In a full game, this would add to inventory
        UIManager.Instance.ShowMessage($"Purchased {item.displayName}!");
        
        // Event
        OnAccessoryPurchased?.Invoke(item.itemId);
    }
    
    private void UpdateTabVisuals()
    {
        // This would update the tab button visuals to show the active tab
        // Implementation depends on the UI design
        // Example: Change button colors or activate indicator objects
    }
    
    // Method to get available items (e.g., for other UI elements)
    public List<ShopItem> GetAvailableItems(ShopCategory category)
    {
        switch (category)
        {
            case ShopCategory.Pets:
                return availablePets;
                
            case ShopCategory.Food:
                return availableFood;
                
            case ShopCategory.Toys:
                return availableToys;
                
            case ShopCategory.Accessories:
                return availableAccessories;
                
            default:
                return new List<ShopItem>();
        }
    }
}

// Shop item data
[Serializable]
public class ShopItem
{
    public string itemId;
    public string displayName;
    public string description;
    public Sprite icon;
    public int price;
    public ShopItemType itemType;
    public int requiredLevel;
    public bool isLimited;
    public bool isPremium;
}

public enum ShopItemType
{
    Pet,
    Food,
    Toy,
    Accessory
}

// Note: FoodItem and ToyItem are already defined in PetBase.cs
// They would be converted to ScriptableObjects in a full implementation
