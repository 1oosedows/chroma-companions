// GameManager.cs - Core game controller
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Game Settings")]
    [SerializeField] private float dayDurationInMinutes = 15f;
    [SerializeField] private int startingCurrency = 100;
    
    [Header("References")]
    [SerializeField] private Transform petParent;
    
    // Game state
    private bool isGamePaused = false;
    private float timeSinceLastDayUpdate = 0f;
    private int currentDay = 1;
    private Dictionary<string, PetBase> activePets = new Dictionary<string, PetBase>();
    
    // Events
    public Action<int> OnDayChanged;
    public Action<int> OnCurrencyChanged;
    public Action<PetBase> OnPetAdopted;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize user data if needed
        if (!UserData.Instance.IsInitialized)
        {
            UserData.Instance.Initialize(startingCurrency);
        }
    }
    
    private void Start()
    {
        // Load saved pets
        LoadSavedPets();
        
        // Subscribe to important events
        UserData.Instance.OnDataLoaded += OnUserDataLoaded;
    }
    
    private void Update()
    {
        if (!isGamePaused)
        {
            // Update game day cycle
            timeSinceLastDayUpdate += Time.deltaTime;
            
            // Check if a new day should start
            if (timeSinceLastDayUpdate >= dayDurationInMinutes * 60f)
            {
                AdvanceDay();
                timeSinceLastDayUpdate = 0f;
            }
        }
    }
    
    private void OnUserDataLoaded()
    {
        // Called when user data is loaded from storage
        currentDay = UserData.Instance.currentGameDay;
        LoadSavedPets();
    }
    
    private void AdvanceDay()
    {
        currentDay++;
        
        // Update all pets for the new day
        foreach (var pet in activePets.Values)
        {
            pet.UpdateDailyStats();
        }
        
        // Update user data
        UserData.Instance.currentGameDay = currentDay;
        UserData.Instance.SaveData();
        
        OnDayChanged?.Invoke(currentDay);
    }
    
    public int GetCurrentDay()
    {
        return currentDay;
    }
    
    public int GetPlayerLevel()
    {
        return UserData.Instance.playerLevel;
    }
    
    public void AddCurrency(int amount)
    {
        int newAmount = UserData.Instance.currency + amount;
        UserData.Instance.currency = newAmount;
        
        OnCurrencyChanged?.Invoke(newAmount);
        UserData.Instance.SaveData();
    }
    
    public bool SpendCurrency(int amount)
    {
        if (UserData.Instance.currency >= amount)
        {
            UserData.Instance.currency -= amount;
            OnCurrencyChanged?.Invoke(UserData.Instance.currency);
            UserData.Instance.SaveData();
            return true;
        }
        
        return false;
    }
    
    public PetBase AdoptPet(string petPrefabId)
    {
        // Load pet prefab
        GameObject petPrefab = Resources.Load<GameObject>($"Pets/{petPrefabId}");
        
        if (petPrefab == null)
        {
            Debug.LogError($"Pet prefab not found: {petPrefabId}");
            return null;
        }
        
        // Create pet instance
        GameObject petObject = Instantiate(petPrefab, petParent);
        PetBase newPet = petObject.GetComponent<PetBase>();
        
        if (newPet == null)
        {
            Debug.LogError($"PetBase component not found on prefab: {petPrefabId}");
            Destroy(petObject);
            return null;
        }
        
        // Generate a unique ID for this pet
        string petId = System.Guid.NewGuid().ToString();
        
        // Add to active pets
        activePets.Add(petId, newPet);
        
        // Add to user data
        UserData.Instance.ownedPets.Add(petId, newPet.GetSaveData());
        UserData.Instance.SaveData();
        
        OnPetAdopted?.Invoke(newPet);
        
        return newPet;
    }
    
    private void LoadSavedPets()
    {
        // Clear existing pets
        foreach (var pet in activePets.Values)
        {
            Destroy(pet.gameObject);
        }
        activePets.Clear();
        
        // Load pets from saved data
        foreach (var petEntry in UserData.Instance.ownedPets)
        {
            string petId = petEntry.Key;
            PetSaveData petData = petEntry.Value;
            
            // Load pet prefab
            GameObject petPrefab = Resources.Load<GameObject>($"Pets/{petData.petID}");
            
            if (petPrefab == null)
            {
                Debug.LogError($"Pet prefab not found: {petData.petID}");
                continue;
            }
            
            // Create pet instance
            GameObject petObject = Instantiate(petPrefab, petParent);
            PetBase pet = petObject.GetComponent<PetBase>();
            
            if (pet == null)
            {
                Debug.LogError($"PetBase component not found on prefab: {petData.petID}");
                Destroy(petObject);
                continue;
            }
            
            // Load saved data
            pet.LoadFromSaveData(petData);
            
            // Add to active pets
            activePets.Add(petId, pet);
        }
    }
    
    public void SaveGame()
    {
        // Update pet data
        foreach (var petEntry in activePets)
        {
            string petId = petEntry.Key;
            PetBase pet = petEntry.Value;
            
            UserData.Instance.ownedPets[petId] = pet.GetSaveData();
        }
        
        // Save user data
        UserData.Instance.SaveData();
    }
    
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        // Save when app is put in background
        if (pauseStatus)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}

// UserData.cs - Handles user progress and preferences
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData : MonoBehaviour
{
    private static UserData _instance;
    public static UserData Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UserData instance not found!");
            }
            return _instance;
        }
    }
    
    // User identification
    public string userId;
    public string displayName;
    
    // Game progress
    public int playerLevel = 1;
    public int playerExp = 0;
    public int currency = 0;
    public int currentGameDay = 1;
    
    // Owned items
    public Dictionary<string, PetSaveData> ownedPets = new Dictionary<string, PetSaveData>();
    public List<string> ownedItems = new List<string>();
    public List<string> completedAchievements = new List<string>();
    
    // Settings and preferences
    public bool musicEnabled = true;
    public bool sfxEnabled = true;
    public bool notificationsEnabled = true;
    
    // Flags
    public bool IsInitialized { get; private set; }
    
    // Events
    public Action OnDataLoaded;
    public Action OnDataSaved;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Try to load saved data
        LoadData();
    }
    
    public void Initialize(int startingCurrency)
    {
        // Generate a unique user ID if not already set
        if (string.IsNullOrEmpty(userId))
        {
            userId = System.Guid.NewGuid().ToString();
        }
        
        // Set default display name
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Player" + UnityEngine.Random.Range(1000, 9999);
        }
        
        // Set starting values
        playerLevel = 1;
        playerExp = 0;
        currency = startingCurrency;
        currentGameDay = 1;
        
        IsInitialized = true;
        SaveData();
    }
    
    public void LoadData()
    {
        // In a real game, this would load from PlayerPrefs, a file, or a server
        // For this example, we'll use PlayerPrefs for simplicity
        
        if (PlayerPrefs.HasKey("UserData"))
        {
            string jsonData = PlayerPrefs.GetString("UserData");
            JsonUtility.FromJsonOverwrite(jsonData, this);
            
            // Load dictionary separately (Unity JsonUtility doesn't support dictionaries)
            if (PlayerPrefs.HasKey("UserPets"))
            {
                string petsJson = PlayerPrefs.GetString("UserPets");
                PetDictionarySave petsSave = JsonUtility.FromJson<PetDictionarySave>(petsJson);
                
                ownedPets.Clear();
                foreach (PetSaveEntry entry in petsSave.entries)
                {
                    ownedPets.Add(entry.key, entry.value);
                }
            }
            
            IsInitialized = true;
            OnDataLoaded?.Invoke();
        }
    }
    
    public void SaveData()
    {
        // Save main user data
        string jsonData = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("UserData", jsonData);
        
        // Save pets dictionary separately
        PetDictionarySave petsSave = new PetDictionarySave();
        petsSave.entries = new List<PetSaveEntry>();
        
        foreach (var entry in ownedPets)
        {
            petsSave.entries.Add(new PetSaveEntry { key = entry.Key, value = entry.Value });
        }
        
        string petsJson = JsonUtility.ToJson(petsSave);
        PlayerPrefs.SetString("UserPets", petsJson);
        
        PlayerPrefs.Save();
        OnDataSaved?.Invoke();
    }
    
    public void AddExperience(int amount)
    {
        playerExp += amount;
        
        // Check for level up (simple formula)
        int expNeeded = 100 * playerLevel;
        
        while (playerExp >= expNeeded)
        {
            playerExp -= expNeeded;
            playerLevel++;
            expNeeded = 100 * playerLevel;
        }
        
        SaveData();
    }
    
    public void SetDisplayName(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            displayName = newName;
            SaveData();
        }
    }
}

// Helper classes for serialization
[Serializable]
public class PetDictionarySave
{
    public List<PetSaveEntry> entries;
}

[Serializable]
public class PetSaveEntry
{
    public string key;
    public PetSaveData value;
}