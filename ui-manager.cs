// UIManager.cs - Handles UI elements and transitions
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UIManager instance not found!");
            }
            return _instance;
        }
    }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject petHomePanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject guildPanel;
    [SerializeField] private GameObject miniGamesPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject messagePanel;
    
    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI dayText;
    
    [Header("Pet Status UI")]
    [SerializeField] private Slider happinessSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI petNameText;
    [SerializeField] private TextMeshProUGUI petLevelText;
    [SerializeField] private Image petElementIcon;
    
    [Header("Message UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;
    
    [Header("References")]
    [SerializeField] private Sprite[] elementIcons;
    
    [Header("Animation")]
    [SerializeField] private float panelTransitionTime = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Current state
    private GameObject currentPanel;
    private PetBase selectedPet;
    private Coroutine messageCoroutine;
    
    // Events
    public Action<string> OnScreenChanged;
    
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
        // Hide all panels initially
        HideAllPanels();
        
        // Show main menu by default
        ShowPanel(mainMenuPanel);
        
        // Subscribe to events
        GameManager.Instance.OnCurrencyChanged += UpdateCurrencyDisplay;
        GameManager.Instance.OnDayChanged += UpdateDayDisplay;
        
        // Initial UI updates
        UpdatePlayerInfo();
        UpdateCurrencyDisplay(UserData.Instance.currency);
        UpdateDayDisplay(GameManager.Instance.GetCurrentDay());
    }
    
    private void HideAllPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (petHomePanel) petHomePanel.SetActive(false);
        if (shopPanel) shopPanel.SetActive(false);
        if (guildPanel) guildPanel.SetActive(false);
        if (miniGamesPanel) miniGamesPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (messagePanel) messagePanel.SetActive(false);
    }
    
    public void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        
        if (currentPanel != null)
        {
            // Animate transition
            StartCoroutine(TransitionPanels(currentPanel, panel));
        }
        else
        {
            // Just show the new panel
            panel.SetActive(true);
            currentPanel = panel;
            
            // Trigger event with panel name
            OnScreenChanged?.Invoke(GetPanelName(panel));
        }
    }
    
    private IEnumerator TransitionPanels(GameObject oldPanel, GameObject newPanel)
    {
        // Setup old panel for transition out
        CanvasGroup oldCanvasGroup = oldPanel.GetComponent<CanvasGroup>();
        if (oldCanvasGroup == null)
        {
            oldCanvasGroup = oldPanel.AddComponent<CanvasGroup>();
        }
        
        // Setup new panel for transition in
        newPanel.SetActive(true);
        CanvasGroup newCanvasGroup = newPanel.GetComponent<CanvasGroup>();
        if (newCanvasGroup == null)
        {
            newCanvasGroup = newPanel.AddComponent<CanvasGroup>();
        }
        newCanvasGroup.alpha = 0;
        
        // Animate transition
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        while (elapsedTime < panelTransitionTime)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / panelTransitionTime;
            float curvedT = transitionCurve.Evaluate(t);
            
            oldCanvasGroup.alpha = 1 - curvedT;
            newCanvasGroup.alpha = curvedT;
            
            yield return null;
        }
        
        // Ensure final state
        oldCanvasGroup.alpha = 0;
        newCanvasGroup.alpha = 1;
        oldPanel.SetActive(false);
        
        // Update current panel
        currentPanel = newPanel;
        
        // Reset canvas groups
        oldCanvasGroup.alpha = 1;
        
        // Trigger event with panel name
        OnScreenChanged?.Invoke(GetPanelName(newPanel));
    }
    
    private string GetPanelName(GameObject panel)
    {
        if (panel == mainMenuPanel) return "MainMenu";
        if (panel == petHomePanel) return "PetHome";
        if (panel == shopPanel) return "Shop";
        if (panel == guildPanel) return "Guild";
        if (panel == miniGamesPanel) return "MiniGames";
        if (panel == settingsPanel) return "Settings";
        
        return panel.name;
    }
    
    // Navigation methods
    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }
    
    public void ShowPetHome()
    {
        ShowPanel(petHomePanel);
    }
    
    public void ShowShop()
    {
        ShowPanel(shopPanel);
    }
    
    public void ShowGuild()
    {
        // Check if player can access guild features
        if (GuildManager.Instance.CanAccessGuildFeature())
        {
            ShowPanel(guildPanel);
        }
        else
        {
            GuildManager.Instance.ShowGuildAccessRequirements();
        }
    }
    
    public void ShowMiniGames()
    {
        ShowPanel(miniGamesPanel);
    }
    
    public void ShowSettings()
    {
        ShowPanel(settingsPanel);
    }
    
    // UI update methods
    public void UpdatePlayerInfo()
    {
        if (playerNameText)
        {
            playerNameText.text = UserData.Instance.displayName;
        }
        
        if (playerLevelText)
        {
            playerLevelText.text = "Level " + UserData.Instance.playerLevel;
        }
    }
    
    public void UpdateCurrencyDisplay(int amount)
    {
        if (currencyText)
        {
            currencyText.text = amount.ToString();
        }
    }
    
    public void UpdateDayDisplay(int day)
    {
        if (dayText)
        {
            dayText.text = "Day " + day;
        }
    }
    
    public void SelectPet(PetBase pet)
    {
        selectedPet = pet;
        UpdatePetStatusDisplay();
    }
    
    public void UpdatePetStatusDisplay()
    {
        if (selectedPet == null) return;
        
        // Update pet name and level
        if (petNameText)
        {
            petNameText.text = selectedPet.PetName;
        }
        
        if (petLevelText)
        {
            petLevelText.text = "Level " + selectedPet.Level;
        }
        
        // Update stat sliders
        if (happinessSlider)
        {
            happinessSlider.value = selectedPet.Happiness / 100f;
        }
        
        if (hungerSlider)
        {
            hungerSlider.value = selectedPet.Hunger / 100f;
        }
        
        if (energySlider)
        {
            energySlider.value = selectedPet.Energy / 100f;
        }
        
        if (healthSlider)
        {
            healthSlider.value = selectedPet.Health / 100f;
        }
        
        // Update element icon
        if (petElementIcon && elementIcons.Length > 0)
        {
            int elementIndex = (int)selectedPet.Element;
            
            if (elementIndex >= 0 && elementIndex < elementIcons.Length)
            {
                petElementIcon.sprite = elementIcons[elementIndex];
            }
        }
    }
    
    // Message display
    public void ShowMessage(string message)
    {
        if (messagePanel == null || messageText == null) return;
        
        // Stop any existing message
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        
        // Set message text
        messageText.text = message;
        
        // Show the panel
        messagePanel.SetActive(true);
        
        // Start the hide timer
        messageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }
    
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }
    
    // Helper methods
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
    
    // These PetBase properties would need to be added to the PetBase class
    // to support the UI updates above
}

// These extension properties should be added to PetBase class:
/*
public string PetName => petName;
public int Level => stats.level;
public float Happiness => stats.happiness;
public float Hunger => stats.hunger;
public float Energy => stats.energy;
public float Health => stats.health;
public PetElement Element => element;
*/
