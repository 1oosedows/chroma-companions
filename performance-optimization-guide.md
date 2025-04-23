# Chroma Companions - Performance Optimization Guide

This guide provides best practices and strategies for optimizing the performance of Chroma Companions on mobile devices, ensuring smooth gameplay and efficient battery usage across a wide range of devices.

## Performance Targets

The game should aim to meet the following performance targets on reference devices:

| Device Tier | FPS Target | Loading Time | Memory Usage |
|-------------|------------|--------------|--------------|
| High-end    | 60 FPS     | < 3 seconds  | < 300 MB     |
| Mid-range   | 60 FPS     | < 5 seconds  | < 200 MB     |
| Low-end     | 30 FPS     | < 10 seconds | < 150 MB     |

## Asset Optimization

### Textures

- **Use Sprite Atlases**: Combine related sprites into texture atlases
  ```csharp
  // In Unity Editor:
  // 1. Create Sprite Atlas (Window > 2D > Sprite Atlas)
  // 2. Drag sprites into Objects field
  // 3. Configure compression and format options
  ```

- **Texture Compression**:
  - Android: Use ETC2 or ASTC (for newer devices)
  - iOS: Use PVRTC or ASTC
  - Set compression quality based on importance:
    - UI elements: Higher quality
    - Background elements: Lower quality

- **Texture Sizes**:
  - Pet sprites: 512x512 max (high-tier), 256x256 (low-tier)
  - UI elements: 256x256 max
  - Icons: 128x128 max
  - Use power-of-two dimensions (256, 512, etc.)

- **Implement Mipmap Streaming**:
  ```csharp
  // Enable in Project Settings > Player > Other Settings:
  // - Texture Streaming: Enabled
  // - Mipmap Streaming: Enabled
  ```

### Audio

- **Use Appropriate Formats**:
  - Background music: Use compressed formats (OGG, MP3)
  - Sound effects: Use PCM or ADPCM for short sounds
  
- **Audio Compression**:
  - Set compression quality in Import Settings
  - Background music: 128kbps, Vorbis/MP3
  - Sound effects: 96kbps, ADPCM

- **Memory Management**:
  - Set "Load In Background" to true for music
  - Use "Preload Audio Data" only for critical sound effects
  - Implement audio pool for frequently used sounds

### Animations

- **Sprite Animation Optimization**:
  - Use keyframe reduction in animations
  - Share animation clips when possible
  - Use lower frame rates for background elements (12-15 FPS)
  
- **Animation Culling**:
  ```csharp
  // Set culling mode in Animator Component
  animator.cullingMode = AnimatorCullingMode.CullCompletely;
  ```

## Code Optimization

### Memory Management

- **Object Pooling**: Implement pooling for frequently created/destroyed objects:
  ```csharp
  public class ObjectPool : MonoBehaviour
  {
      [SerializeField] private GameObject prefab;
      [SerializeField] private int poolSize = 20;
      
      private List<GameObject> pool;
      
      private void Awake()
      {
          pool = new List<GameObject>(poolSize);
          
          // Pre-instantiate objects
          for (int i = 0; i < poolSize; i++)
          {
              GameObject obj = Instantiate(prefab);
              obj.SetActive(false);
              pool.Add(obj);
          }
      }
      
      public GameObject GetObject()
      {
          // Find inactive object in pool
          foreach (GameObject obj in pool)
          {
              if (!obj.activeInHierarchy)
              {
                  obj.SetActive(true);
                  return obj;
              }
          }
          
          // If no inactive objects, create new one
          GameObject newObj = Instantiate(prefab);
          pool.Add(newObj);
          return newObj;
      }
      
      public void ReturnObject(GameObject obj)
      {
          obj.SetActive(false);
      }
  }
  ```

- **Avoid Allocations in Update**: Minimize garbage collection by avoiding allocations in frequently called methods:
  ```csharp
  // Bad practice:
  void Update()
  {
      Vector3 newPosition = new Vector3(x, y, z); // Allocation every frame
      transform.position = newPosition;
  }
  
  // Good practice:
  private Vector3 positionCache; // Reusable variable
  
  void Update()
  {
      positionCache.Set(x, y, z); // No allocation
      transform.position = positionCache;
  }
  ```

- **Cache Component References**: Store frequently accessed components:
  ```csharp
  // Cache components in Awake
  private Transform cachedTransform;
  private Renderer cachedRenderer;
  
  private void Awake()
  {
      cachedTransform = transform;
      cachedRenderer = GetComponent<Renderer>();
  }
  
  // Use cached references
  private void Update()
  {
      cachedTransform.Rotate(0, 1, 0);
      cachedRenderer.material.color = Color.red;
  }
  ```

### CPU Optimization

- **Coroutines for Distributed Processing**:
  ```csharp
  // Instead of processing everything at once
  private IEnumerator ProcessPetsOverTime()
  {
      foreach (PetBase pet in activePets)
      {
          pet.UpdateDailyStats();
          yield return null; // Wait for next frame
      }
  }
  ```

- **Optimize Update Methods**:
  ```csharp
  // Use less frequent updates for non-critical systems
  private float checkTimer = 0f;
  private float checkInterval = 0.5f; // Check twice per second
  
  private void Update()
  {
      checkTimer += Time.deltaTime;
      
      if (checkTimer >= checkInterval)
      {
          checkTimer = 0f;
          // Perform less frequent operations
          CheckPetNeeds();
      }
      
      // Only perform critical operations every frame
      HandlePlayerInput();
  }
  ```

- **Implement Variable Update Rates**:
  ```csharp
  // Adjust update frequency based on device performance
  private void AdjustUpdateRate()
  {
      // Detect device tier based on SystemInfo
      if (IsLowEndDevice())
      {
          // Reduce update frequency for background systems
          petUpdateInterval = 1.0f; // Once per second
          backgroundAnimationFrameRate = 10; // 10 FPS for background
      }
      else
      {
          petUpdateInterval = 0.5f; // Twice per second
          backgroundAnimationFrameRate = 15; // 15 FPS for background
      }
  }
  ```

### Rendering Optimization

- **Disable Unnecessary Renderers**:
  ```csharp
  // Disable renderers for offscreen elements
  private void OnBecameInvisible()
  {
      cachedRenderer.enabled = false;
  }
  
  private void OnBecameVisible()
  {
      cachedRenderer.enabled = true;
  }
  ```

- **Use Sprite Batching**:
  - Ensure sprites use the same material
  - Keep sprites in the same sorting layer and order
  - Use Dynamic Batching for small meshes

- **Optimize UI Rendering**:
  ```csharp
  // Reduce Canvas overdraw
  // Set Canvas property "Pixel Perfect" to true
  
  // Disable Canvas when not visible
  canvas.enabled = false; // When panel is hidden
  
  // Use CanvasGroup for fading to avoid rebuilding the canvas
  canvasGroup.alpha = 0.5f; // Fade without rebuild
  ```

## Loading and Initialization Optimization

### Asynchronous Loading

- **Implement Asynchronous Scene Loading**:
  ```csharp
  private IEnumerator LoadSceneAsync(string sceneName)
  {
      // Show loading screen
      loadingScreen.SetActive(true);
      
      // Start loading scene in background
      AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
      asyncLoad.allowSceneActivation = false;
      
      // Wait until loading is almost complete
      while (asyncLoad.progress < 0.9f)
      {
          // Update loading progress
          loadingBar.fillAmount = asyncLoad.progress;
          yield return null;
      }
      
      // Wait a bit to ensure smooth transition
      yield return new WaitForSeconds(0.2f);
      
      // Activate scene
      asyncLoad.allowSceneActivation = true;
      
      // Hide loading screen after transition
      loadingScreen.SetActive(false);
  }
  ```

- **Staggered Initialization**:
  ```csharp
  private IEnumerator StaggeredInitialization()
  {
      // Initialize critical systems first
      InitializeUserData();
      yield return null;
      
      // Then initialize pet system
      InitializePetSystem();
      yield return null;
      
      // Then UI
      InitializeUI();
      yield return null;
      
      // Then non-critical systems
      StartCoroutine(InitializeNonCriticalSystems());
      
      // Game is ready to play now
      ShowMainMenu();
  }
  ```

### Resource Loading

- **Lazy Loading**: Only load resources when needed:
  ```csharp
  // Load pet prefabs only when they're first used
  private Dictionary<string, GameObject> loadedPetPrefabs = new Dictionary<string, GameObject>();
  
  public GameObject GetPetPrefab(string petId)
  {
      if (!loadedPetPrefabs.ContainsKey(petId))
      {
          // Load from Resources only if not already loaded
          GameObject prefab = Resources.Load<GameObject>($"Pets/{petId}");
          loadedPetPrefabs[petId] = prefab;
      }
      
      return loadedPetPrefabs[petId];
  }
  ```

- **Addressable Assets**: For larger games, use the Addressable Asset System:
  ```csharp
  // Load asset asynchronously
  private async Task<GameObject> LoadPetAsync(string petAddressableKey)
  {
      AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(petAddressableKey);
      await handle.Task;
      return handle.Result;
  }
  ```

## Runtime Performance Monitoring

### FPS Counter

```csharp
public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    
    private float updateInterval = 0.5f;
    private float accumulatedTime = 0;
    private int frameCount = 0;
    
    private void Update()
    {
        accumulatedTime += Time.unscaledDeltaTime;
        frameCount++;
        
        if (accumulatedTime >= updateInterval)
        {
            float fps = frameCount / accumulatedTime;
            frameCount = 0;
            accumulatedTime = 0;
            
            fpsText.text = $"FPS: {Mathf.Round(fps)}";
            
            // Color-code based on performance
            if (fps >= 55) fpsText.color = Color.green;
            else if (fps >= 30) fpsText.color = Color.yellow;
            else fpsText.color = Color.red;
        }
    }
}
```

### Memory Usage Monitor

```csharp
public class MemoryMonitor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI memoryText;
    
    private float updateInterval = 1.0f;
    private float timer = 0;
    
    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        
        if (timer >= updateInterval)
        {
            timer = 0;
            UpdateMemoryDisplay();
        }
    }
    
    private void UpdateMemoryDisplay()
    {
        float totalMemoryMB = GC.GetTotalMemory(false) / (1024f * 1024f);
        memoryText.text = $"Mem: {totalMemoryMB:F1} MB";
    }
}
```

## Security and Performance Balance

### Optimizing Security Components

- **Selective Checks**: Perform intensive security checks less frequently:
  ```csharp
  // Example for AntiTamperingManager
  private float integrityCheckInterval = 60f; // Check once per minute, not every frame
  ```

- **Batch Encryption Operations**: Don't encrypt/decrypt small pieces individually:
  ```csharp
  // Inefficient - many small encryptions
  foreach (var petData in petList)
  {
      string encrypted = SecurityManager.Instance.EncryptData(JsonUtility.ToJson(petData));
      // Save encrypted data
  }
  
  // More efficient - encrypt all at once
  List<PetSaveData> allPets = new List<PetSaveData>(petList);
  string allPetsJson = JsonUtility.ToJson(new PetListWrapper { pets = allPets });
  string encrypted = SecurityManager.Instance.EncryptData(allPetsJson);
  // Save single encrypted string
  ```

### Tiered Security Levels

- **Adjust Security Based on Device Performance**:
  ```csharp
  private void ConfigureSecurityForDevice()
  {
      if (IsLowEndDevice())
      {
          // Use lighter security for low-end devices
          SecurityManager.Instance.SetSecurityLevel(SecurityLevel.Medium);
      }
      else
      {
          // Full security for more capable devices
          SecurityManager.Instance.SetSecurityLevel(SecurityLevel.High);
      }
  }
  ```

## Quality Settings Optimization

### Dynamic Quality Adjustment

- **Detect Device Tier Automatically**:
  ```csharp
  public enum DeviceTier { Low, Medium, High }
  
  public DeviceTier DetectDeviceTier()
  {
      // Get device info
      int processorCount = SystemInfo.processorCount;
      int systemMemorySize = SystemInfo.systemMemorySize;
      
      // Simple classification logic
      if (processorCount >= 6 && systemMemorySize >= 4000)
      {
          return DeviceTier.High;
      }
      else if (processorCount >= 4 && systemMemorySize >= 2000)
      {
          return DeviceTier.Medium;
      }
      else
      {
          return DeviceTier.Low;
      }
  }
  ```

- **Apply Settings Based on Device Tier**:
  ```csharp
  public void ApplyQualitySettings(DeviceTier tier)
  {
      switch (tier)
      {
          case DeviceTier.Low:
              Application.targetFrameRate = 30;
              QualitySettings.SetQualityLevel(0); // Lowest quality
              ReduceParticleEffects();
              DisablePostProcessing();
              break;
              
          case DeviceTier.Medium:
              Application.targetFrameRate = 60;
              QualitySettings.SetQualityLevel(1); // Medium quality
              ReduceParticleEffects();
              EnableBasicPostProcessing();
              break;
              
          case DeviceTier.High:
              Application.targetFrameRate = 60;
              QualitySettings.SetQualityLevel(2); // Highest quality
              EnableFullEffects();
              EnableAdvancedPostProcessing();
              break;
      }
  }
  ```

### Battery Usage Optimization

- **Detect Low Power Mode**:
  ```csharp
  private bool IsInLowPowerMode()
  {
      // iOS
      #if UNITY_IOS
      return UnityEngine.iOS.Device.lowPowerModeEnabled;
      #endif
      
      // Android (approximation via battery level)
      #if UNITY_ANDROID
      try
      {
          using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
          using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
          using (AndroidJavaObject batteryIntent = activity.Call<AndroidJavaObject>("registerReceiver", null, new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED")))
          {
              int level = batteryIntent.Call<int>("getIntExtra", "level", 0);
              int scale = batteryIntent.Call<int>("getIntExtra", "scale", 100);
              float batteryPct = level * 100f / scale;
              return batteryPct <= 20f; // Assume low power mode if battery <= 20%
          }
      }
      catch
      {
          return false;
      }
      #endif
      
      return false;
  }
  ```

- **Adjust Settings for Low Power Mode**:
  ```csharp
  private void UpdateForPowerMode()
  {
      if (IsInLowPowerMode())
      {
          // Reduce power usage
          Application.targetFrameRate = 30;
          EnableBatterySavingMode();
      }
      else
      {
          // Use normal settings based on device tier
          ApplyQualitySettings(currentDeviceTier);
      }
  }
  
  private void EnableBatterySavingMode()
  {
      // Reduce update frequency
      SetLowFrequencyUpdates();
      
      // Reduce visual effects
      DisableNonEssentialEffects();
      
      // Reduce brightness (where applicable)
      ReduceUIBrightness();
  }
  ```

## Build Size Optimization

### Asset Inclusion Management

- **Strip Unused Assets**:
  - In Build Settings, enable "Strip Engine Code"
  - Use Asset Bundle Browser to identify unused assets

- **Include Only Necessary Resources**:
  - Move unused assets to AssetBundles for optional download
  - Use addressables for content that can be downloaded later

- **Compress Textures Appropriately**:
  - Set "Use Crunch Compression" for textures
  - Use texture atlases to reduce overhead

### Code Stripping

- **Enable IL2CPP Stripping**:
  - Use "Medium" to "High" stripping level
  - Test thoroughly after stripping to ensure no broken functionality

- **Manual Code Stripping**:
  ```csharp
  // Add strip pragma to exclude code from certain builds
  #if !UNITY_ANDROID
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  private void AndroidOnlyFunction()
  {
      // This function won't be included in non-Android builds
  }
  #endif
  ```

## Profiling Guidelines

1. **Identify Bottlenecks First**:
   - Use Unity Profiler to find slowest methods
   - Focus optimization efforts on these areas

2. **Profile on Target Devices**:
   - Real-world performance differs from Editor
   - Use Development Builds with Profiler enabled

3. **Regular Performance Testing**:
   - Test on lowest-spec target devices
   - Create automated performance tests

4. **Performance Regression Detection**:
   - Record baseline performance metrics
   - Compare against baselines after major changes

## Platform-Specific Optimizations

### Android Optimizations

- **Multi-threading for Android**:
  ```csharp
  // Use System.Threading.Tasks for background work
  async Task ProcessDataAsync()
  {
      await Task.Run(() => {
          // Heavy work here
          ProcessLargeDataSet();
      });
      
      // Back on main thread
      UpdateUI();
  }
  ```

- **Optimize for Fragmentation**:
  - Use adaptive quality settings based on device capability
  - Provide fallbacks for unsupported features

### iOS Optimizations

- **Respect iOS Thermal State**:
  ```csharp
  #if UNITY_IOS
  private void OnThermalStateChange(string state)
  {
      switch (state)
      {
          case "Nominal":
              // Normal settings
              break;
          case "Fair":
              // Slightly reduce quality
              break;
          case "Serious":
              // Significantly reduce quality
              break;
          case "Critical":
              // Minimum settings, save power
              break;
      }
  }
  #endif
  ```

- **Optimize for Metal API**:
  - Enable Metal support in Player Settings
  - Use appropriate texture formats for Metal
  - Optimize shaders for Metal API

## Final Checklist

Before releasing, verify these performance aspects:

- [ ] Frame rate stays within target on all supported devices
- [ ] Loading times are acceptable (use loading screens for longer loads)
- [ ] Memory usage remains stable during extended sessions
- [ ] Battery drain is reasonable (test 30-minute sessions)
- [ ] Build size is optimized for platform guidelines
- [ ] Security measures don't impact gameplay experience
- [ ] All texture, audio, and animation assets are optimized
- [ ] Code is profiled and hotspots are addressed

Remember that optimization is an ongoing process. Continue monitoring performance metrics after release and be prepared to address new issues as they arise.
