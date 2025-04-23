// GameStartupManager.cs - Handles initialization of the game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartupManager : MonoBehaviour
{
    [Header("Referenced Managers")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PetRegistry petRegistry;
    [SerializeField] private PetFactory petFactory;
    [SerializeField] private GuildManager guildManager;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private MiniGameManager miniGameManager;
    [SerializeField] private NotificationManager notificationManager;
    
    [Header("First-Time Setup")]
    [SerializeField] private PetSelectionManager petSelectionManager;
    [SerializeField] private GameObject tutorialManager;
    
    [Header("Setup Settings")]
    [SerializeField] private bool showSplashScreen = true;
    [SerializeField] private float splashScreenDuration = 3f;
    [SerializeField] private GameObject splashScreenObject;
    
    private void Start()
    {
        // First check that all required managers exist
        EnsureManagersExist();
        
        // Start initialization sequence
        StartCoroutine(InitializeGameSequence());
    }
    
    private void EnsureManagersExist()
    {
        // Check for essential managers, create if missing
        if (gameManager == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManager = gameManagerObj.AddComponent<GameManager>();
            Debug.LogWarning("GameManager not assigned, creating a new one.");
        }
        
        if (uiManager == null)
        {
            GameObject uiManagerObj = new GameObject("UIManager");
            uiManager = uiManagerObj.AddComponent<UIManager>();
            Debug.LogWarning("UIManager not assigned, creating a new one.");
        }
        
        if (petRegistry == null)
        {
            GameObject petRegistryObj = new GameObject("PetRegistry");
            petRegistry = petRegistryObj.AddComponent<PetRegistry>();
            Debug.LogWarning("PetRegistry not assigned, creating a new one.");
        }
        
        // Other managers are technically optional but recommended
        if (petFactory == null)
        {
            Debug.LogWarning("PetFactory not assigned. Pet creation may not work properly.");
        }
        
        if (guildManager == null)
        {
            Debug.LogWarning("GuildManager not assigned. Guild functionality will be unavailable.");
        }
        
        if (shopManager == null)
        {
            Debug.LogWarning("ShopManager not assigned. Shop functionality will be unavailable.");
        }
        
        if (miniGameManager == null)
        {
            Debug.LogWarning("MiniGameManager not assigned. Mini-games will be unavailable.");
        }
        
        if (notificationManager == null)
        {
            Debug.LogWarning("NotificationManager not assigned. Notifications will be unavailable.");
        }
        
        if (petSelectionManager == null)
        {
            Debug.LogWarning("PetSelectionManager not assigned. First-time pet selection will be unavailable.");
        }
    }
    
    private IEnumerator InitializeGameSequence()
    {
        // Show splash screen if enabled
        if (showSplashScreen && splashScreenObject != null)
        {
            splashScreenObject.SetActive(true);
            yield return new WaitForSeconds(splashScreenDuration);
            splashScreenObject.SetActive(false);
        }
        
        // Initialize user data first
        yield return StartCoroutine(InitializeUserData());
        
        // Initialize pet system
        yield return StartCoroutine(InitializePetSystem());
        
        // Initialize other systems
        yield return StartCoroutine(InitializeGameSystems());
        
        // Check if this is first launch
        bool isFirstLaunch = IsFirstLaunch();
        
        if (isFirstLaunch)
        {
            // Show first-time experience
            ShowFirstTimeExperience();
        }
        else
        {
            // Show regular main menu
            ShowMainMenu();
        }
    }
    
    private IEnumerator InitializeUserData()
    {
        // Wait for UserData to initialize
        while (UserData.Instance == null || !UserData.Instance.IsInitialized)
        {
            yield return null;
        }
        
        Debug.Log("User data initialized.");
    }
    
    private IEnumerator InitializePetSystem()
    {
        // Wait for PetRegistry to be ready
        while (PetRegistry.Instance == null)
        {
            yield return null;
        }
        
        // Initialize pet factory if available
        if (petFactory != null)
        {
            petFactory.RegisterAllPets();
            Debug.Log("Pet factory initialized and pets registered.");
        }
        
        yield return null;
    }
    
    private IEnumerator InitializeGameSystems()
    {
        // Initialize optional systems
        
        // Shop
        if (shopManager != null)
        {
            // Register pets with shop
            if (petRegistry != null)
            {
                petRegistry.RegisterPetsWithShop();
            }
            
            Debug.Log("Shop system initialized.");
        }
        
        // Guild
        if (guildManager != null)
        {
            Debug.Log("Guild system initialized.");
        }
        
        // Mini-games
        if (miniGameManager != null)
        {
            Debug.Log("Mini-game system initialized.");
        }
        
        // Notifications
        if (notificationManager != null)
        {
            Debug.Log("Notification system initialized.");
        }
        
        yield return null;
    }
    
    private bool IsFirstLaunch()
    {
        // Check if user has any pets
        if (UserData.Instance != null)
        {
            return UserData.Instance.ownedPets.Count == 0;
        }
        
        return true;
    }
    
    private void ShowFirstTimeExperience()
    {
        Debug.Log("Showing first-time user experience.");
        
        // Show pet selection screen if available
        if (petSelectionManager != null)
        {
            petSelectionManager.ShowPetSelection();
        }
        else
        {
            // Fallback to main menu if pet selection not available
            ShowMainMenu();
        }
        
        // Show tutorial if available
        if (tutorialManager != null)
        {
            tutorialManager.SetActive(true);
        }
    }
    
    private void ShowMainMenu()
    {
        Debug.Log("Showing main menu.");
        
        // Show main menu via UI manager
        if (uiManager != null)
        {
            uiManager.ShowMainMenu();
        }
    }
}
