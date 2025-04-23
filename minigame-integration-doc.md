# Mini-Game Integration Guide

This document outlines how to integrate the mini-games we've created into the main Chroma Companions game structure.

## Overview

We've created the following components:

1. `MiniGameBase.cs` - The abstract base class that all mini-games extend
2. `MiniGameManager.cs` - Manages the collection of mini-games and their integration with the main game
3. Specific mini-game implementations:
   - `ColorMatchPuzzle.cs` - Match-3 style color matching game
   - `ColorRushRacer.cs` - Endless runner with color-matching mechanics
   - `BubblePopAdventure.cs` - Bubble shooter with fantasy theme

## Integration Steps

### 1. Create Mini-Game Prefabs

For each mini-game implementation:

1. Create a new scene for the mini-game
2. Add a GameObject with the mini-game component
3. Configure all settings in the inspector
4. Create prefabs for all required elements (blocks, bubbles, obstacles, etc.)
5. Save the entire mini-game as a prefab

### 2. Setup Mini-Game Manager

1. Add the `MiniGameManager` component to a persistent GameObject in your main scene
2. Configure the available mini-games list with prefabs for each game
3. Create UI elements for the mini-game selection panel
4. Create mini-game button prefabs

### 3. Connect to Game Manager

Update `GameManager.cs` to integrate mini-games:

```csharp
// In GameManager.cs

// Add reference to MiniGameManager
[SerializeField] private MiniGameManager miniGameManager;

private void Start()
{
    // Existing code...
    
    // Subscribe to mini-game events
    if (miniGameManager != null)
    {
        miniGameManager.OnMiniGameCompleted += HandleMiniGameCompleted;
    }
}

private void HandleMiniGameCompleted(MiniGameInfo gameInfo, int currencyEarned, int expEarned)
{
    // Update player stats based on mini-game rewards
    AddCurrency(currencyEarned);
    
    // Award experience to selected pet if available
    if (selectedPet != null)
    {
        selectedPet.AddExperience(expEarned);
    }
    
    // Optional: Update daily tasks or achievements
    // UserData.Instance.UpdateAchievementProgress("PlayMiniGames", 1);
}
```

### 4. Connect to Pet System

Update `PetBase.cs` to add mini-game interactions:

```csharp
// In PetBase.cs

// Add methods for mini-game bonuses
public virtual void ApplyMiniGameBoost(string miniGameId)
{
    // Each pet type can implement custom bonuses for specific mini-games
    // For example, a water element pet might get bonuses in bubble pop game
    
    switch (miniGameId)
    {
        case "bubblePop":
            if (element == PetElement.Water)
            {
                // Apply happiness boost
                stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
                OnHappinessChanged?.Invoke(stats.happiness);
            }
            break;
            
        case "colorMatch":
            if (element == PetElement.Light)
            {
                // Apply happiness boost
                stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
                OnHappinessChanged?.Invoke(stats.happiness);
            }
            break;
            
        case "colorRush":
            if (element == PetElement.Air)
            {
                // Apply happiness boost
                stats.happiness = Mathf.Min(stats.happiness + 10f, 100f);
                OnHappinessChanged?.Invoke(stats.happiness);
            }
            break;
    }
}
```

### 5. Add UI Entry Points

Update your main UI to add buttons that open the mini-game selection:

```csharp
// In your UI controller
public void OnPlayMiniGamesButtonClicked()
{
    MiniGameManager.Instance.ShowMiniGameSelection();
}
```

### 6. Add Daily Bonuses and Rewards

Update `MiniGameManager.cs` to support daily bonuses:

```csharp
// In MiniGameManager.cs, extend the HandleMiniGameCompleted method

private void HandleMiniGameCompleted(MiniGameInfo gameInfo, int currencyEarned, int expEarned)
{
    // Check for daily bonus
    if (gameInfo.isDailyBonusEnabled && IsDailyBonusAvailable(gameInfo.gameId))
    {
        // Award daily bonus
        int bonusCurrency = gameInfo.dailyBonusCurrency;
        GameManager.Instance.AddCurrency(bonusCurrency);
        
        UIManager.Instance.ShowMessage($"Daily Bonus: +{bonusCurrency} currency!");
    }
    
    // Rest of the existing code...
}
```

## Testing and Balance

When implementing mini-games, consider these balancing factors:

1. **Reward Scaling**: Ensure that rewards are proportional to the difficulty and time investment of each mini-game.

2. **Progression Impact**: Mini-games should contribute to overall progression but not completely overshadow the main pet care gameplay.

3. **Energy System**: Consider implementing an energy system to limit how many mini-games can be played in a session.

4. **Pet Bonuses**: Different pets should have unique advantages in certain mini-games, encouraging players to collect diverse pets.

## Guild Integration

To integrate with the guild system:

```csharp
// In GuildManager.cs

public void AddMiniGameContribution(string miniGameId, int score)
{
    // Award contribution points based on mini-game score
    int contributionPoints = score / 20; // Scale appropriately
    AwardContributionPoints(contributionPoints);
}
```

Then in MiniGameManager:

```csharp
// In MiniGameManager.cs

private void HandleMiniGameCompleted(MiniGameInfo gameInfo, int currencyEarned, int expEarned)
{
    // Existing code...
    
    // Add guild contribution if player is in a guild
    if (GuildManager.Instance.currentUserGuild != null)
    {
        GuildManager.Instance.AddMiniGameContribution(gameInfo.gameId, currentMiniGame.currentScore);
    }
}
```

## Monetization Opportunities

The mini-games provide several monetization opportunities:

1. **Extra Lives/Continues**: Players can purchase additional attempts when they fail a mini-game.

2. **Power-ups**: Special items that provide advantages in mini-games.

3. **Energy Refills**: If using an energy system, allow players to purchase refills.

4. **Special Themes**: Cosmetic themes for the mini-games.

5. **Exclusive Mini-Games**: Premium mini-games that require purchase or subscription.

## Next Steps

After implementing these mini-games, consider:

1. Adding leaderboards to encourage competition
2. Creating mini-game tournaments with special rewards
3. Developing more complex mini-games that incorporate pet abilities
4. Adding seasonal mini-game themes for holidays and events
