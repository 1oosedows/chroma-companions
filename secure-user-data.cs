// SecureUserData.cs - Enhanced version of UserData with security features
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

[Serializable]
public class SecureUserData : MonoBehaviour
{
    private static SecureUserData _instance;
    public static SecureUserData Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("SecureUserData instance not found!");
            }
            return _instance;
        }
    }
    
    // User identification - immutable once set
    private string _userId;
    public string userId 
    { 
        get { return _userId; }
        private set { _userId = value; }
    }
    
    private string _displayName;
    public string displayName
    {
        get { return _displayName; }
        set 
        {
            if (string.IsNullOrEmpty(value))
                return;
                
            _displayName = value;
            // Schedule a save when changing important data
            ScheduleSave();
        }
    }
    
    // Game progress with validation
    private int _playerLevel = 1;
    public int playerLevel
    {
        get { return _playerLevel; }
        set 
        {
            // Validate level change
            if (value < _playerLevel)
            {
                // Level should never decrease
                Debug.LogWarning("Attempted to decrease player level");
                TriggerSecurityWarning("Invalid level change detected");
                return;
            }
            
            if (value > _playerLevel + 5)
            {
                // Suspicious large level jump
                Debug.LogWarning("Unusually large level increase");
                TriggerSecurityWarning("Suspicious level change detected");
            }
            
            _playerLevel = value;
            ScheduleSave();
        }
    }
    
    private int _playerExp = 0;
    public int playerExp
    {
        get { return _playerExp; }
        set 
        {
            // Validate exp change
            if (value < 0)
            {
                Debug.LogWarning("Attempted to set negative exp");
                TriggerSecurityWarning("Invalid experience value detected");
                return;
            }
            
            // Check for unusually large exp gains
            if (value > _playerExp + 1000)
            {
                Debug.LogWarning("Unusually large exp increase");
                TriggerSecurityWarning("Suspicious experience gain detected");
            }
            
            _playerExp = value;
            ScheduleSave();
        }
    }
    
    private int _currency = 0;
    public int currency
    {
        get { return _currency; }
        set 
        {
            // Validate currency change
            if (value < 0)
            {
                Debug.LogWarning("Attempted to set negative currency");
                TriggerSecurityWarning("Invalid currency value detected");
                return;
            }
            
            // Check for unusually large currency increases
            if (value > _currency + 10000)
            {
                Debug.LogWarning("Unusually large currency increase");
                TriggerSecurityWarning("Suspicious currency gain detected");
            }
            
            _currency = value;
            ScheduleSave();
        }
    }
    
    private int _currentGameDay = 1;
    public int currentGameDay
    {
        get { return _currentGameDay; }
        set 
        {
            // Validate day progression
            if (value < _currentGameDay)
            {
                Debug.LogWarning("Attempted to go back in game days");
                TriggerSecurityWarning("Invalid game day change detected");
                return;
            }
            
            // Check for unusually large day jumps
            if (value > _currentGameDay + 30)
            {
                Debug.LogWarning("Unusually large game day increase");
                TriggerSecurityWarning("Suspicious game day change detected");
            }
            
            _currentGameDay = value;
            ScheduleSave();
        }
    }
    
    // Owned items with validation
    private Dictionary<string, PetSaveData> _ownedPets = new Dictionary<string, PetSaveData>();
    public Dictionary<string, PetSaveData> ownedPets 
    { 
        get { return new Dictionary<string, PetSaveData>(_ownedPets); } // Return a copy to prevent direct modification
    }
    
    private List<string> _ownedItems = new List<string>();
    public List<string> ownedItems 
    { 
        get { return new List<string>(_ownedItems); } // Return a copy to prevent direct modification
    }
    
    private List<string> _completedAchievements = new List<string>();
    public List<string> completedAchievements 
    { 
        get { return new List<string>(_completedAchievements); } // Return a copy to prevent direct modification
    }
    
    // Settings and preferences
    private bool _musicEnabled = true;
    public bool musicEnabled
    {
        get { return _musicEnabled; }
        set { _musicEnabled = value; ScheduleSave(); }
    }
    
    private bool _sfxEnabled = true;
    public bool sfxEnabled
    {
        get { return _sfxEnabled; }
        set { _sfxEnabled = value; ScheduleSave(); }
    }
    
    private bool _notificationsEnabled = true;
    public bool notificationsEnabled
    {
        get { return _notificationsEnabled; }
        set { _notificationsEnabled = value; ScheduleSave(); }
    }
    
    // Flags
    public bool IsInitialized { get; private set; }
    
    // Security-related
    private string playerDataHash;
    private DateTime lastSaveTime;
    private bool isDataTampered = false;
    private int suspiciousActivityCount = 0;
    private bool saveScheduled = false;
    private float saveDelay = 5.0f; // Seconds to wait before saving after changes
    private float saveTimer = 0f;
    
    // File paths
    private string primarySavePath;
    private string backupSavePath;
    
    // Events
    public Action OnDataLoaded;
    public Action OnDataSaved;
    public Action<string> OnSecurityWarning;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Setup save paths
        SetupSavePaths();
        
        // Try to load saved data
        LoadData();
    }
    
    private void Update()
    {
        // Handle delayed save
        if (saveScheduled)
        {
            saveTimer -= Time.deltaTime;
            if (saveTimer <= 0f)
            {
                saveScheduled = false;
                SaveData();
            }
        }
    }
    
    private void SetupSavePaths()
    {
        // Set up secure paths for saving data
        primarySavePath = Path.Combine(Application.persistentDataPath, "user_data.dat");
        backupSavePath = Path.Combine(Application.persistentDataPath, "user_data_backup.dat");
    }
    
    public void Initialize(int startingCurrency)
    {
        // Generate a unique user ID if not already set
        if (string.IsNullOrEmpty(userId))
        {
            userId = Guid.NewGuid().ToString();
        }
        
        // Set default display name
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Player" + UnityEngine.Random.Range(1000, 9999);
        }
        
        // Set starting values
        _playerLevel = 1;
        _playerExp = 0;
        _currency = startingCurrency;
        _currentGameDay = 1;
        
        IsInitialized = true;
        SaveData();
    }
    
    public async void LoadData()
    {
        bool dataLoaded = false;
        
        // Try to load from primary file first
        if (File.Exists(primarySavePath))
        {
            try
            {
                string encryptedData = await ReadFileAsync(primarySavePath);
                if (!string.IsNullOrEmpty(encryptedData))
                {
                    string decryptedData = SecurityManager.Instance.DecryptData(encryptedData);
                    
                    // Validate data integrity with hash
                    int separatorIndex = decryptedData.LastIndexOf("|HASH:");
                    if (separatorIndex != -1)
                    {
                        string jsonData = decryptedData.Substring(0, separatorIndex);
                        string dataHash = decryptedData.Substring(separatorIndex + 6);
                        
                        string computedHash = SecurityManager.Instance.ComputeHash(jsonData);
                        if (dataHash == computedHash)
                        {
                            // Data is valid, parse it
                            ParseUserData(jsonData);
                            playerDataHash = dataHash;
                            dataLoaded = true;
                        }
                        else
                        {
                            Debug.LogWarning("Data integrity check failed. Hash mismatch.");
                            isDataTampered = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid data format. Hash not found.");
                        isDataTampered = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading primary save data: " + e.Message);
                isDataTampered = true;
            }
        }
        
        // If primary load failed, try backup
        if (!dataLoaded && File.Exists(backupSavePath))
        {
            try
            {
                string encryptedData = await ReadFileAsync(backupSavePath);
                if (!string.IsNullOrEmpty(encryptedData))
                {
                    string decryptedData = SecurityManager.Instance.DecryptData(encryptedData);
                    
                    // Validate data integrity with hash
                    int separatorIndex = decryptedData.LastIndexOf("|HASH:");
                    if (separatorIndex != -1)
                    {
                        string jsonData = decryptedData.Substring(0, separatorIndex);
                        string dataHash = decryptedData.Substring(separatorIndex + 6);
                        
                        string computedHash = SecurityManager.Instance.ComputeHash(jsonData);
                        if (dataHash == computedHash)
                        {
                            // Data is valid, parse it
                            ParseUserData(jsonData);
                            playerDataHash = dataHash;
                            dataLoaded = true;
                            
                            // Restore primary from backup
                            await WriteFileAsync(primarySavePath, encryptedData);
                        }
                        else
                        {
                            Debug.LogWarning("Backup data integrity check failed. Hash mismatch.");
                            isDataTampered = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid backup data format. Hash not found.");
                        isDataTampered = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading backup save data: " + e.Message);
                isDataTampered = true;
            }
        }
        
        // If data was tampered with, handle it
        if (isDataTampered)
        {
            TriggerSecurityWarning("Save data may have been tampered with");
            
            // Reset to default state or take other security action
            // For this example, we'll just continue with a new save
            if (!dataLoaded)
            {
                Initialize(100);
            }
        }
        
        IsInitialized = true;
        OnDataLoaded?.Invoke();
    }
    
    private void ParseUserData(string jsonData)
    {
        try
        {
            // Parse general user data
            JsonUtility.FromJsonOverwrite(jsonData, this);
            
            // Parse pets dictionary separately (Unity JsonUtility doesn't support dictionaries)
            int petsStartIndex = jsonData.IndexOf("\"ownedPets\":[");
            if (petsStartIndex != -1)
            {
                int petsEndIndex = FindMatchingBracket(jsonData, petsStartIndex + 12);
                if (petsEndIndex != -1)
                {
                    string petsJson = jsonData.Substring(petsStartIndex + 12, petsEndIndex - (petsStartIndex + 12));
                    PetDictionarySave petsSave = JsonUtility.FromJson<PetDictionarySave>("{\"entries\":" + petsJson + "}");
                    
                    _ownedPets.Clear();
                    foreach (PetSaveEntry entry in petsSave.entries)
                    {
                        _ownedPets.Add(entry.key, entry.value);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing user data: " + e.Message);
            throw; // Re-throw to be caught by the caller
        }
    }
    
    private int FindMatchingBracket(string text, int openBracketIndex)
    {
        int nestLevel = 0;
        
        for (int i = openBracketIndex; i < text.Length; i++)
        {
            if (text[i] == '[')
            {
                nestLevel++;
            }
            else if (text[i] == ']')
            {
                nestLevel--;
                if (nestLevel == 0)
                {
                    return i;
                }
            }
        }
        
        return -1; // No matching bracket found
    }
    
    public async void SaveData()
    {
        // Generate the JSON data
        string jsonData = JsonUtility.ToJson(this);
        
        // Save pets dictionary separately
        PetDictionarySave petsSave = new PetDictionarySave();
        petsSave.entries = new List<PetSaveEntry>();
        
        foreach (var entry in _ownedPets)
        {
            petsSave.entries.Add(new PetSaveEntry { key = entry.Key, value = entry.Value });
        }
        
        string petsJson = JsonUtility.ToJson(petsSave);
        
        // Combine data and add integrity hash
        string combinedJson = jsonData + "|PETS:" + petsJson;
        string dataHash = SecurityManager.Instance.ComputeHash(combinedJson);
        string dataWithHash = combinedJson + "|HASH:" + dataHash;
        
        // Encrypt the data
        string encryptedData = SecurityManager.Instance.EncryptData(dataWithHash);
        
        // Save to primary location
        try
        {
            await WriteFileAsync(primarySavePath, encryptedData);
            
            // Also save to backup location
            await WriteFileAsync(backupSavePath, encryptedData);
            
            lastSaveTime = DateTime.Now;
            playerDataHash = dataHash;
            
            OnDataSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving data: " + e.Message);
            TriggerSecurityWarning("Error saving data: " + e.Message);
        }
    }
    
    // Safe methods to modify collections
    public void AddPet(string petId, PetSaveData petData)
    {
        _ownedPets[petId] = petData;
        ScheduleSave();
    }
    
    public void RemovePet(string petId)
    {
        if (_ownedPets.ContainsKey(petId))
        {
            _ownedPets.Remove(petId);
            ScheduleSave();
        }
    }
    
    public void UpdatePet(string petId, PetSaveData petData)
    {
        if (_ownedPets.ContainsKey(petId))
        {
            _ownedPets[petId] = petData;
            ScheduleSave();
        }
    }
    
    public void AddItem(string itemId)
    {
        if (!_ownedItems.Contains(itemId))
        {
            _ownedItems.Add(itemId);
            ScheduleSave();
        }
    }
    
    public void RemoveItem(string itemId)
    {
        if (_ownedItems.Contains(itemId))
        {
            _ownedItems.Remove(itemId);
            ScheduleSave();
        }
    }
    
    public void AddAchievement(string achievementId)
    {
        if (!_completedAchievements.Contains(achievementId))
        {
            _completedAchievements.Add(achievementId);
            ScheduleSave();
        }
    }
    
    public void AddExperience(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to add negative experience");
            TriggerSecurityWarning("Invalid experience addition detected");
            return;
        }
        
        playerExp += amount;
        
        // Check for level up (simple formula)
        int expNeeded = 100 * playerLevel;
        
        while (playerExp >= expNeeded)
        {
            playerExp -= expNeeded;
            playerLevel++;
            expNeeded = 100 * playerLevel;
        }
        
        ScheduleSave();
    }
    
    public void SetDisplayName(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            displayName = newName;
            ScheduleSave();
        }
    }
    
    // Helper methods
    private async Task<string> ReadFileAsync(string filePath)
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading file: " + e.Message);
            throw;
        }
    }
    
    private async Task WriteFileAsync(string filePath, string content)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                await writer.WriteAsync(content);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing file: " + e.Message);
            throw;
        }
    }
    
    private void ScheduleSave()
    {
        saveScheduled = true;
        saveTimer = saveDelay;
    }
    
    private void TriggerSecurityWarning(string message)
    {
        suspiciousActivityCount++;
        OnSecurityWarning?.Invoke(message);
        
        // If too many suspicious activities, take action
        if (suspiciousActivityCount >= 5)
        {
            // In a real app, you might log the user out, report to the server, etc.
            Debug.LogWarning("Too many suspicious activities detected!");
        }
    }
    
    // Validate the current state of the data
    public bool ValidateDataIntegrity()
    {
        string jsonData = JsonUtility.ToJson(this);
        
        // Save pets dictionary separately
        PetDictionarySave petsSave = new PetDictionarySave();
        petsSave.entries = new List<PetSaveEntry>();
        
        foreach (var entry in _ownedPets)
        {
            petsSave.entries.Add(new PetSaveEntry { key = entry.Key, value = entry.Value });
        }
        
        string petsJson = JsonUtility.ToJson(petsSave);
        
        // Combine data
        string combinedJson = jsonData + "|PETS:" + petsJson;
        string currentHash = SecurityManager.Instance.ComputeHash(combinedJson);
        
        // Compare with stored hash
        return currentHash == playerDataHash;
    }
    
    // For emergency recovery
    public void RestoreFromBackup()
    {
        if (File.Exists(backupSavePath))
        {
            LoadData(); // Will try primary first, then backup
        }
        else
        {
            Debug.LogError("No backup file found for recovery");
        }
    }
}

// Helper classes for serialization (same as in original UserData)
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
