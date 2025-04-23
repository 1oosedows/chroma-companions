// AntiTamperingManager.cs - Advanced protection against code manipulation and memory editing
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AntiTamperingManager : MonoBehaviour
{
    private static AntiTamperingManager _instance;
    public static AntiTamperingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("AntiTamperingManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Protection Settings")]
    [SerializeField] private bool enableCodeIntegrityChecks = true;
    [SerializeField] private bool enableMemoryProtection = true;
    [SerializeField] private bool enableTimeTamperingChecks = true;
    [SerializeField] private float integrityCheckInterval = 30f; // Seconds between checks
    
    [Header("Critical Game Objects")]
    [SerializeField] private List<GameObject> protectedGameObjects = new List<GameObject>();
    
    // Critical class checksums
    private Dictionary<string, string> classChecksums = new Dictionary<string, string>();
    
    // Memory protection
    private Dictionary<string, ProtectedValue<int>> protectedIntegers = new Dictionary<string, ProtectedValue<int>>();
    private Dictionary<string, ProtectedValue<float>> protectedFloats = new Dictionary<string, ProtectedValue<float>>();
    private Dictionary<string, ProtectedValue<bool>> protectedBooleans = new Dictionary<string, ProtectedValue<bool>>();
    
    // Time verification
    private DateTime lastRealTime;
    private float lastGameTime;
    private bool timeCheckInitialized = false;
    
    // Events
    public Action<TamperType, string> OnTamperingDetected;
    
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
        // Initialize protection systems
        InitializeProtection();
        
        // Start regular integrity checks
        if (enableCodeIntegrityChecks)
        {
            InvokeRepeating("PerformIntegrityCheck", integrityCheckInterval, integrityCheckInterval);
        }
        
        // Initialize time checking
        if (enableTimeTamperingChecks)
        {
            InitializeTimeChecking();
        }
    }
    
    private void Update()
    {
        // Check for time manipulation if enabled
        if (enableTimeTamperingChecks && timeCheckInitialized)
        {
            CheckForTimeManipulation();
        }
    }
    
    #region Initialization
    
    private void InitializeProtection()
    {
        // Register critical classes for integrity checking
        if (enableCodeIntegrityChecks)
        {
            RegisterCriticalClasses();
        }
        
        // Set up protection for game objects
        foreach (GameObject obj in protectedGameObjects)
        {
            if (obj != null)
            {
                MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    string className = component.GetType().FullName;
                    string checksum = ComputeObjectChecksum(component);
                    classChecksums[className + "_" + component.GetInstanceID()] = checksum;
                }
            }
        }
    }
    
    private void RegisterCriticalClasses()
    {
        // Register checksums for critical game classes
        RegisterClassChecksum(typeof(GameManager));
        RegisterClassChecksum(typeof(SecureUserData));
        RegisterClassChecksum(typeof(SecurityManager));
        RegisterClassChecksum(typeof(AntiTamperingManager));
        
        // Register other critical classes
        RegisterClassChecksum(typeof(PetBase));
        RegisterClassChecksum(typeof(GuildManager));
        RegisterClassChecksum(typeof(ShopManager));
    }
    
    private void RegisterClassChecksum(Type type)
    {
        string className = type.FullName;
        string checksum = ComputeTypeChecksum(type);
        classChecksums[className] = checksum;
        
        Debug.Log($"Registered checksum for {className}");
    }
    
    private void InitializeTimeChecking()
    {
        lastRealTime = DateTime.UtcNow;
        lastGameTime = Time.realtimeSinceStartup;
        timeCheckInitialized = true;
    }
    
    #endregion
    
    #region Integrity Checking
    
    private void PerformIntegrityCheck()
    {
        // Check protected objects for tampering
        CheckProtectedGameObjects();
        
        // Check class integrity
        CheckClassIntegrity();
        
        // Check memory values
        if (enableMemoryProtection)
        {
            CheckProtectedValues();
        }
    }
    
    private void CheckProtectedGameObjects()
    {
        foreach (GameObject obj in protectedGameObjects)
        {
            if (obj == null)
            {
                // Critical object has been destroyed
                TamperingDetected(TamperType.ObjectDestroyed, "Protected game object was destroyed");
                continue;
            }
            
            MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                string className = component.GetType().FullName;
                string instanceKey = className + "_" + component.GetInstanceID();
                
                if (classChecksums.ContainsKey(instanceKey))
                {
                    string storedChecksum = classChecksums[instanceKey];
                    string currentChecksum = ComputeObjectChecksum(component);
                    
                    if (storedChecksum != currentChecksum)
                    {
                        TamperingDetected(TamperType.ComponentModified, 
                            $"Component {className} on {obj.name} has been modified");
                    }
                }
            }
        }
    }
    
    private void CheckClassIntegrity()
    {
        // Check integrity of critical classes
        CheckTypeIntegrity(typeof(GameManager));
        CheckTypeIntegrity(typeof(SecureUserData));
        CheckTypeIntegrity(typeof(SecurityManager));
        CheckTypeIntegrity(typeof(AntiTamperingManager));
        
        // Check other critical classes
        CheckTypeIntegrity(typeof(PetBase));
        CheckTypeIntegrity(typeof(GuildManager));
        CheckTypeIntegrity(typeof(ShopManager));
    }
    
    private void CheckTypeIntegrity(Type type)
    {
        string className = type.FullName;
        
        if (classChecksums.ContainsKey(className))
        {
            string storedChecksum = classChecksums[className];
            string currentChecksum = ComputeTypeChecksum(type);
            
            if (storedChecksum != currentChecksum)
            {
                TamperingDetected(TamperType.CodeModified, $"Class {className} has been modified");
            }
        }
    }
    
    private void CheckProtectedValues()
    {
        // Check integers
        foreach (var entry in protectedIntegers)
        {
            if (!entry.Value.Validate())
            {
                TamperingDetected(TamperType.MemoryModified, 
                    $"Protected integer {entry.Key} has been tampered with");
                
                // Restore the value
                entry.Value.Restore();
            }
        }
        
        // Check floats
        foreach (var entry in protectedFloats)
        {
            if (!entry.Value.Validate())
            {
                TamperingDetected(TamperType.MemoryModified, 
                    $"Protected float {entry.Key} has been tampered with");
                
                // Restore the value
                entry.Value.Restore();
            }
        }
        
        // Check booleans
        foreach (var entry in protectedBooleans)
        {
            if (!entry.Value.Validate())
            {
                TamperingDetected(TamperType.MemoryModified, 
                    $"Protected boolean {entry.Key} has been tampered with");
                
                // Restore the value
                entry.Value.Restore();
            }
        }
    }
    
    private void CheckForTimeManipulation()
    {
        DateTime currentRealTime = DateTime.UtcNow;
        float currentGameTime = Time.realtimeSinceStartup;
        
        // Calculate time differences
        TimeSpan realTimeDiff = currentRealTime - lastRealTime;
        float gameTimeDiff = currentGameTime - lastGameTime;
        
        // Check for significant discrepancies
        if (gameTimeDiff > 0 && realTimeDiff.TotalSeconds > 0)
        {
            float timeRatio = (float)(gameTimeDiff / realTimeDiff.TotalSeconds);
            
            // If game time is running much faster or slower than real time
            if (timeRatio > 1.5f || timeRatio < 0.5f)
            {
                TamperingDetected(TamperType.TimeManipulation, 
                    $"Time manipulation detected. Ratio: {timeRatio}");
            }
        }
        
        // Check for time going backwards
        if (currentGameTime < lastGameTime || currentRealTime < lastRealTime)
        {
            TamperingDetected(TamperType.TimeManipulation, "Time reversal detected");
        }
        
        // Update timestamps
        lastRealTime = currentRealTime;
        lastGameTime = currentGameTime;
    }
    
    #endregion
    
    #region Utility Methods
    
    private string ComputeTypeChecksum(Type type)
    {
        // Create a simple checksum based on method signatures
        StringBuilder sb = new StringBuilder();
        
        // Add methods
        var methods = type.GetMethods(System.Reflection.BindingFlags.Public | 
                                      System.Reflection.BindingFlags.NonPublic | 
                                      System.Reflection.BindingFlags.Instance | 
                                      System.Reflection.BindingFlags.Static);
        
        foreach (var method in methods)
        {
            if (method.DeclaringType == type) // Only methods defined in this type, not inherited
            {
                sb.Append(method.Name);
                sb.Append(":");
                
                foreach (var param in method.GetParameters())
                {
                    sb.Append(param.ParameterType.Name);
                    sb.Append(",");
                }
                
                sb.Append(";");
            }
        }
        
        // Add fields
        var fields = type.GetFields(System.Reflection.BindingFlags.Public | 
                                     System.Reflection.BindingFlags.NonPublic | 
                                     System.Reflection.BindingFlags.Instance | 
                                     System.Reflection.BindingFlags.Static);
        
        foreach (var field in fields)
        {
            if (field.DeclaringType == type) // Only fields defined in this type, not inherited
            {
                sb.Append(field.Name);
                sb.Append(":");
                sb.Append(field.FieldType.Name);
                sb.Append(";");
            }
        }
        
        // Hash the combined string
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
    
    private string ComputeObjectChecksum(object obj)
    {
        // For simplicity, we're using the same method as type checksum
        // In a real implementation, this would also include instance-specific data
        return ComputeTypeChecksum(obj.GetType());
    }
    
    private void TamperingDetected(TamperType tamperType, string message)
    {
        Debug.LogWarning($"Tampering detected: {tamperType} - {message}");
        
        // Invoke the event
        OnTamperingDetected?.Invoke(tamperType, message);
        
        // Take action based on tampering type
        switch (tamperType)
        {
            case TamperType.CodeModified:
            case TamperType.ObjectDestroyed:
                // Critical tampering - these could force quit the application in a real game
                HandleCriticalTampering();
                break;
                
            case TamperType.ComponentModified:
            case TamperType.MemoryModified:
                // Try to recover
                HandleRecoverableTampering();
                break;
                
            case TamperType.TimeManipulation:
                // Log and notify server in a real game
                HandleTimeManipulation();
                break;
        }
    }
    
    private void HandleCriticalTampering()
    {
        // In a real game, this might:
        // 1. Log the event to a server
        // 2. Force the player to update the app
        // 3. Shut down the application
        
        Debug.LogError("Critical tampering detected!");
        
        // For this example, we'll just reload the current scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    private void HandleRecoverableTampering()
    {
        // Try to restore from backups or reset affected systems
        Debug.LogWarning("Attempting to recover from tampering...");
        
        // For User Data, try to restore from backup
        if (SecureUserData.Instance != null)
        {
            SecureUserData.Instance.RestoreFromBackup();
        }
    }
    
    private void HandleTimeManipulation()
    {
        // Reset timers and notify
        lastRealTime = DateTime.UtcNow;
        lastGameTime = Time.realtimeSinceStartup;
        
        Debug.LogWarning("Time manipulation detected and corrected");
    }
    
    #endregion
    
    #region Public API
    
    // Register a protected integer value
    public void ProtectInt(string key, int initialValue)
    {
        if (!enableMemoryProtection)
            return;
            
        if (protectedIntegers.ContainsKey(key))
        {
            protectedIntegers[key].SetValue(initialValue);
        }
        else
        {
            protectedIntegers[key] = new ProtectedValue<int>(initialValue);
        }
    }
    
    // Get a protected integer value
    public int GetProtectedInt(string key)
    {
        if (!enableMemoryProtection || !protectedIntegers.ContainsKey(key))
            return 0;
            
        ProtectedValue<int> protectedValue = protectedIntegers[key];
        
        // Validate before returning
        if (!protectedValue.Validate())
        {
            TamperingDetected(TamperType.MemoryModified, $"Protected integer {key} was accessed after tampering");
            protectedValue.Restore();
        }
        
        return protectedValue.GetValue();
    }
    
    // Update a protected integer value
    public void UpdateProtectedInt(string key, int newValue)
    {
        if (!enableMemoryProtection)
            return;
            
        if (protectedIntegers.ContainsKey(key))
        {
            protectedIntegers[key].SetValue(newValue);
        }
        else
        {
            ProtectInt(key, newValue);
        }
    }
    
    // Similar methods for float and bool
    public void ProtectFloat(string key, float initialValue)
    {
        if (!enableMemoryProtection)
            return;
            
        if (protectedFloats.ContainsKey(key))
        {
            protectedFloats[key].SetValue(initialValue);
        }
        else
        {
            protectedFloats[key] = new ProtectedValue<float>(initialValue);
        }
    }
    
    public float GetProtectedFloat(string key)
    {
        if (!enableMemoryProtection || !protectedFloats.ContainsKey(key))
            return 0f;
            
        ProtectedValue<float> protectedValue = protectedFloats[key];
        
        if (!protectedValue.Validate())
        {
            TamperingDetected(TamperType.MemoryModified, $"Protected float {key} was accessed after tampering");
            protectedValue.Restore();
        }
        
        return protectedValue.GetValue();
    }
    
    public void UpdateProtectedFloat(string key, float newValue)
    {
        if (!enableMemoryProtection)
            return;
            
        if (protectedFloats.ContainsKey(key))
        {
            protectedFloats[key].SetValue(newValue);
        }
        else
        {
            ProtectFloat(key, newValue);
        }
    }
    
    public void ProtectBool(string key, bool initialValue)
    {
        if (!enableMemoryProtection)
            return;
            
        if (protectedBooleans.ContainsKey(key))
        {
            protectedBooleans[key].SetValue(initialValue);
        }
        else
        {
            protectedBooleans[key] = new ProtectedValue<bool>(initialValue);
        }
    }
    
    public bool GetProtectedBool(string key)
    {
        if (!enableMemoryProtection || !protectedBooleans.ContainsKey(key))
            return false;
            
        ProtectedValue<bool> protectedValue = protectedBooleans[key];
        
        if (!protectedValue.Validate())
        {
            TamperingDetected(TamperType.MemoryModified, $"Protected boolean {key} was accessed after tampering");
            protectedValue.Restore();
        }
        
        return protectedValue.GetValue();
    }
    
    public void UpdateProtectedBool(string key, bool newValue)
    {
        if (!enableMemoryProtection)
            return;
            
        if (protectedBooleans.ContainsKey(key))
        {
            protectedBooleans[key].SetValue(newValue);
        }
        else
        {
            ProtectBool(key, newValue);
        }
    }
    
    // Register a game object for protection
    public void ProtectGameObject(GameObject obj)
    {
        if (obj != null && !protectedGameObjects.Contains(obj))
        {
            protectedGameObjects.Add(obj);
            
            // Register all components
            MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                string className = component.GetType().FullName;
                string checksum = ComputeObjectChecksum(component);
                classChecksums[className + "_" + component.GetInstanceID()] = checksum;
            }
        }
    }
    
    #endregion
}

// Protected value wrapper to prevent memory editing
public class ProtectedValue<T> where T : IEquatable<T>
{
    private T value;
    private T redundantValue; // Redundant copy for verification
    private int checksum;      // Checksum for verification
    
    public ProtectedValue(T initialValue)
    {
        SetValue(initialValue);
    }
    
    public void SetValue(T newValue)
    {
        value = newValue;
        redundantValue = newValue;
        checksum = ComputeChecksum(newValue);
    }
    
    public T GetValue()
    {
        // Validate before returning
        if (!Validate())
        {
            Restore();
        }
        
        return value;
    }
    
    public bool Validate()
    {
        // Check if value matches redundant copy
        if (!value.Equals(redundantValue))
        {
            return false;
        }
        
        // Check if checksum matches
        int currentChecksum = ComputeChecksum(value);
        return currentChecksum == checksum;
    }
    
    public void Restore()
    {
        // Use the redundant value (which might also be compromised,
        // but it's better than nothing)
        value = redundantValue;
        checksum = ComputeChecksum(value);
    }
    
    private int ComputeChecksum(T val)
    {
        // Simple checksum calculation
        return val.GetHashCode() ^ typeof(T).GetHashCode();
    }
}

public enum TamperType
{
    CodeModified,      // Game code has been modified
    ObjectDestroyed,   // A protected object has been destroyed
    ComponentModified, // A component has been modified
    MemoryModified,    // Protected memory values have been modified
    TimeManipulation   // System time has been manipulated
}
