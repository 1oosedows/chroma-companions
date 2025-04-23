// SecurityManager.cs - Handles data encryption, authentication, and security measures
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SecurityManager : MonoBehaviour
{
    private static SecurityManager _instance;
    public static SecurityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("SecurityManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("Security Settings")]
    [SerializeField] private bool useEncryption = true;
    [SerializeField] private bool useServerAuthentication = true;
    [SerializeField] private bool useIntegrityChecks = true;
    [SerializeField] private bool enableTamperDetection = true;
    [SerializeField] private bool enableJailbreakDetection = true;
    
    // Server configuration
    [SerializeField] private string authServerUrl = "https://api.chromacompanions.com/auth";
    
    // Encryption keys (in production these would be securely generated and stored)
    private string encryptionKey = "ChangeThisToASecureKeyInProduction!";
    private string ivKey = "ChangeThisIVToo!";
    
    // Integrity tracking
    private Dictionary<string, string> fileHashes = new Dictionary<string, string>();
    private bool securityInitialized = false;
    
    // User authentication
    private string authToken = "";
    private DateTime tokenExpiration;
    
    // Events
    public Action OnSecurityBreach;
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
    }
    
    private void Start()
    {
        // Initialize security features
        InitializeSecurity();
    }
    
    public void InitializeSecurity()
    {
        if (securityInitialized)
            return;
        
        // Generate a unique device identifier
        string deviceId = GetSecureDeviceIdentifier();
        
        // Set up encryption
        if (useEncryption)
        {
            // In a real app, you would securely generate and store keys
            // This is a simplified example
            encryptionKey = GenerateSecureKey(deviceId);
            ivKey = GenerateSecureIV(deviceId);
        }
        
        // Perform integrity checks
        if (useIntegrityChecks)
        {
            StartCoroutine(PerformIntegrityChecks());
        }
        
        // Check for device tampering
        if (enableTamperDetection)
        {
            DetectTampering();
        }
        
        // Check for jailbreak/root
        if (enableJailbreakDetection)
        {
            if (IsDeviceRootedOrJailbroken())
            {
                OnSecurityWarning?.Invoke("Unsecured device detected. Some features may be limited.");
            }
        }
        
        securityInitialized = true;
    }
    
    #region Encryption Methods
    
    // Encrypt data before saving
    public string EncryptData(string data)
    {
        if (!useEncryption || string.IsNullOrEmpty(data))
            return data;
        
        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(ivKey);
                
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                
                return Convert.ToBase64String(encryptedBytes);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error encrypting data: " + e.Message);
            return data; // Return original data on error
        }
    }
    
    // Decrypt data after loading
    public string DecryptData(string encryptedData)
    {
        if (!useEncryption || string.IsNullOrEmpty(encryptedData))
            return encryptedData;
        
        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(ivKey);
                
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error decrypting data: " + e.Message);
            return encryptedData; // Return encrypted data on error
        }
    }
    
    // Hash data for validation
    public string ComputeHash(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder builder = new StringBuilder();
            
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }
            
            return builder.ToString();
        }
    }
    
    private string GenerateSecureKey(string seed)
    {
        // In a real app, you would use a more secure method to generate keys
        // This is a simplified example
        string combinedSeed = seed + SystemInfo.deviceUniqueIdentifier + Application.version;
        
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedSeed));
            return Convert.ToBase64String(hashBytes).Substring(0, 32);
        }
    }
    
    private string GenerateSecureIV(string seed)
    {
        // Generate a different IV using a variant of the seed
        string variantSeed = seed + "IV_VARIANT" + SystemInfo.deviceModel;
        
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(variantSeed));
            return Convert.ToBase64String(hashBytes).Substring(0, 16);
        }
    }
    
    #endregion
    
    #region Authentication Methods
    
    // Authenticate with server
    public IEnumerator AuthenticateUser(string username, string password, Action<bool, string> callback)
    {
        if (!useServerAuthentication)
        {
            callback(true, "Server authentication disabled");
            yield break;
        }
        
        // Create auth request
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", ComputeHash(password)); // Send hashed password
        form.AddField("device_id", SystemInfo.deviceUniqueIdentifier);
        
        using (UnityWebRequest www = UnityWebRequest.Post(authServerUrl + "/login", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Authentication error: " + www.error);
                callback(false, "Authentication failed: Network error");
            }
            else
            {
                try
                {
                    // Parse response
                    string response = www.downloadHandler.text;
                    // In a real app, parse JSON response to get token and expiration
                    // For this example, we'll simulate a successful auth
                    authToken = "sample_auth_token";
                    tokenExpiration = DateTime.Now.AddHours(24);
                    
                    callback(true, "Authentication successful");
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing auth response: " + e.Message);
                    callback(false, "Authentication failed: Invalid response");
                }
            }
        }
    }
    
    public bool IsAuthenticated()
    {
        if (!useServerAuthentication)
            return true;
            
        return !string.IsNullOrEmpty(authToken) && DateTime.Now < tokenExpiration;
    }
    
    public void LogOut()
    {
        authToken = "";
    }
    
    public string GetAuthToken()
    {
        return authToken;
    }
    
    #endregion
    
    #region Integrity and Security Checks
    
    private IEnumerator PerformIntegrityChecks()
    {
        // In a real app, you would validate critical game files
        // This is a simplified example
        
        // Check app signature (Android)
        #if UNITY_ANDROID
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            using (AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>(
                "getPackageInfo", Application.identifier, 64)) // 64 is PackageManager.GET_SIGNATURES
            {
                AndroidJavaObject[] signatures = packageInfo.Get<AndroidJavaObject[]>("signatures");
                if (signatures.Length > 0)
                {
                    byte[] bytes = signatures[0].Call<byte[]>("toByteArray");
                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] hash = sha.ComputeHash(bytes);
                        string signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                        
                        // Compare with expected signature
                        // In a real app, you would check against your known good signature
                        string expectedSignature = "your_app_signature_hash";
                        
                        if (expectedSignature != "your_app_signature_hash" && signature != expectedSignature)
                        {
                            // App has been tampered with or signature doesn't match
                            OnSecurityBreach?.Invoke();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking app signature: " + e.Message);
        }
        #endif
        
        yield return null;
    }
    
    private void DetectTampering()
    {
        // Detect common tampering tools
        #if UNITY_ANDROID
        try
        {
            string[] suspiciousPackages = new string[]
            {
                "com.chelpus.lackypatch",
                "com.dimonvideo.luckypatcher",
                "com.forpda.lp",
                "com.android.vending.billing.InAppBillingService.LUCK",
                "com.android.vending.billing.InAppBillingService.CLON",
                "com.android.vending.billing.InAppBillingService.LOCK",
                "com.android.vending.billing.InAppBillingService.CRAC",
                "eu.chainfire.supersu",
                "com.noshufou.android.su",
                "com.koushikdutta.superuser",
                "com.zachspong.temprootremovejb",
                "com.ramdroid.appquarantine"
            };
            
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            {
                foreach (string packageName in suspiciousPackages)
                {
                    try
                    {
                        packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                        // If we get here, package exists
                        OnSecurityWarning?.Invoke("Suspicious app detected: " + packageName);
                    }
                    catch
                    {
                        // Package not found, which is good
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in tamper detection: " + e.Message);
        }
        #endif
    }
    
    private bool IsDeviceRootedOrJailbroken()
    {
        #if UNITY_ANDROID
        try
        {
            // Check for common root indicators
            string[] paths = new string[]
            {
                "/system/app/Superuser.apk",
                "/system/xbin/su",
                "/system/bin/su",
                "/sbin/su",
                "/system/su",
                "/system/bin/.ext/.su",
                "/system/xbin/.ext/.su",
                "/system/bin/failsafe/su",
                "/data/local/su",
                "/data/local/xbin/su",
                "/data/local/bin/su"
            };
            
            foreach (string path in paths)
            {
                if (System.IO.File.Exists(path))
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in root detection: " + e.Message);
        }
        #endif
        
        #if UNITY_IOS
        try
        {
            // Check for common jailbreak indicators
            string[] paths = new string[]
            {
                "/Applications/Cydia.app",
                "/Library/MobileSubstrate/MobileSubstrate.dylib",
                "/bin/bash",
                "/usr/sbin/sshd",
                "/etc/apt",
                "/usr/bin/ssh"
            };
            
            foreach (string path in paths)
            {
                if (System.IO.Directory.Exists(path) || System.IO.File.Exists(path))
                {
                    return true;
                }
            }
            
            // Try to write to a restricted location
            string testPath = "/private/jailbreak_test.txt";
            try
            {
                System.IO.File.WriteAllText(testPath, "Jailbreak Test");
                System.IO.File.Delete(testPath);
                return true; // If we can write, device is jailbroken
            }
            catch
            {
                // Expected behavior on non-jailbroken device
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in jailbreak detection: " + e.Message);
        }
        #endif
        
        return false;
    }
    
    private string GetSecureDeviceIdentifier()
    {
        // Create a more secure device identifier than just the SystemInfo.deviceUniqueIdentifier
        string baseIdentifier = SystemInfo.deviceUniqueIdentifier;
        string hardwareInfo = SystemInfo.deviceModel + SystemInfo.processorType + SystemInfo.systemMemorySize;
        
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] combined = Encoding.UTF8.GetBytes(baseIdentifier + hardwareInfo);
            byte[] hashBytes = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    #endregion
    
    #region Public API
    
    // Secure save data method
    public void SecureSave(string key, string data)
    {
        // Add some random salt to avoid identical encrypted values for identical data
        string timestamp = DateTime.Now.Ticks.ToString();
        string salted = data + "|" + timestamp;
        
        // Encrypt the data
        string encryptedData = EncryptData(salted);
        
        // Compute a hash for integrity checking
        string dataHash = ComputeHash(data);
        
        // Save the encrypted data
        PlayerPrefs.SetString(key, encryptedData);
        
        // Save the hash for integrity verification
        PlayerPrefs.SetString(key + "_hash", dataHash);
        
        PlayerPrefs.Save();
    }
    
    // Secure load data method
    public string SecureLoad(string key)
    {
        // Load the encrypted data
        if (!PlayerPrefs.HasKey(key))
            return null;
            
        string encryptedData = PlayerPrefs.GetString(key);
        
        // Decrypt the data
        string decryptedData = DecryptData(encryptedData);
        
        // Remove salt
        string[] parts = decryptedData.Split('|');
        if (parts.Length > 0)
        {
            string data = parts[0];
            
            // Verify data integrity if hash exists
            if (PlayerPrefs.HasKey(key + "_hash"))
            {
                string savedHash = PlayerPrefs.GetString(key + "_hash");
                string currentHash = ComputeHash(data);
                
                if (savedHash != currentHash)
                {
                    // Data has been tampered with
                    Debug.LogWarning("Data integrity check failed for key: " + key);
                    OnSecurityWarning?.Invoke("Data integrity check failed");
                    return null;
                }
            }
            
            return data;
        }
        
        return decryptedData;
    }
    
    // Enable or disable security features
    public void SetSecurityLevel(SecurityLevel level)
    {
        switch (level)
        {
            case SecurityLevel.Low:
                useEncryption = true;
                useServerAuthentication = false;
                useIntegrityChecks = false;
                enableTamperDetection = false;
                enableJailbreakDetection = false;
                break;
                
            case SecurityLevel.Medium:
                useEncryption = true;
                useServerAuthentication = true;
                useIntegrityChecks = true;
                enableTamperDetection = false;
                enableJailbreakDetection = false;
                break;
                
            case SecurityLevel.High:
                useEncryption = true;
                useServerAuthentication = true;
                useIntegrityChecks = true;
                enableTamperDetection = true;
                enableJailbreakDetection = true;
                break;
        }
    }
    
    #endregion
}

public enum SecurityLevel
{
    Low,
    Medium,
    High
}
