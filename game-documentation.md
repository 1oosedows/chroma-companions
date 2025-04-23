# Chroma Companions Documentation

## Overview

Chroma Companions is a mobile pet collection and care game with colorful pets, mini-games, and a guild system designed to foster positive community interactions. The game features a variety of pet types, each with unique abilities and interactions, combined with mini-games and progression systems to keep players engaged.

## Core Systems

### GameManager and UserData

The heart of the game is the `GameManager` which controls the game state, economy, and interactions between systems. The `UserData` class stores all player progress, including:

- Pet collection
- Currency and resources
- Game progress (days, levels)
- Achievement tracking
- Settings and preferences

These two components work together to ensure game state is maintained across sessions.

### Pet System

The pet system is the primary focus of the game and includes:

1. **PetBase**: Abstract base class that defines the core pet functionality
   - Stats (happiness, hunger, energy, health)
   - Interaction methods (feed, play, rest)
   - Experience and level progression
   - Ability unlocking system

2. **Pet Types**: Each extends PetBase with unique behaviors
   - ColorDragon: Changes colors and elements
   - RainbowBunny: Generates resources from happiness
   - PuzzleFox: Excels at mini-games
   - CosmicOwl: Time-based abilities
   - MelodyFish: Music-based abilities
   - CrystalDeer: Healing and growth
   - SparkHamster: Energy and electricity
   - ShadowCat: Stealth and treasure finding
   - BubbleOctopus: Defensive and multitasking
   - EmberPhoenix: Rebirth and fire abilities
   - EarthTortoise: Defense and terraforming

3. **PetRegistry**: Centralized system for registering and tracking all pet types
   - Stores metadata about each pet type
   - Provides lookups by element, rarity, etc.
   - Determines availability based on player level

4. **PetFactory**: Creates pet prefabs at runtime
   - Registers all pet types with PetRegistry
   - Handles visual customization
   - Creates templates for pet instantiation

5. **PetSelectionManager**: Handles initial pet selection for new players
   - Shows available starter pets
   - Provides preview and information
   - Initiates the pet adoption process

### Guild System

The guild system promotes positive social interaction:

- Level gating (minimum level 15)
- Mandatory community guidelines tutorial
- Probation period for new members
- Badge-based recognition instead of visible trust scores
- Different member ranks (Novice, Member, Mentor, Elder, Leader)
- Guild events and activities
- Mentor/mentee relationships

### Mini-Game System

The mini-game system provides engaging activities:

- **MiniGameManager**: Handles mini-game selection, initialization, and rewards
- **MiniGameBase**: Abstract base class for all mini-games
- **Game Types**: Different mini-games with unique mechanics
  - ColorMatchMiniGame: Match-3 style puzzle game
  - (Other mini-games would be implemented similarly)
- **Pet Integration**: Pets provide bonuses to mini-games based on their abilities
- **Reward System**: Performance-based rewards with pet-specific bonuses

### Shop System

The shop allows players to purchase new pets, food, toys, and accessories:

- **ShopManager**: Handles inventory and transactions
- **ShopItem**: Data structure for all purchasable items
- **Categories**: Different sections for different item types
- **Pet Registry Integration**: Automatically populates shop with available pets

### Notification System

The notification system encourages player retention:

- Pet care reminders
- Daily bonus notifications
- Guild event notifications
- Special feature unlocks

## Architecture

### Component Relationships

```
GameManager
├── UserData
├── PetRegistry
│   └── PetFactory
├── ShopManager
├── GuildManager
├── MiniGameManager
├── NotificationManager
└── UIManager
```

### Initialization Flow

1. **GameStartupManager**: Initializes all systems in the correct order
2. **UserData Initialization**: Loads saved progress or creates new profile
3. **Pet System Initialization**: Registers all pet types
4. **First-Time Experience**: Shows pet selection for new players
5. **Main Menu**: Provides access to all game features

### Saving and Loading

- Player progress is saved automatically during important events
- Progress is also saved when the app is paused or closed
- Data is stored in PlayerPrefs for simplicity (would use server storage in production)

## Customization and Extensibility

### Adding New Pets

To add a new pet type:

1. Create a new class that extends `PetBase`
2. Implement unique abilities and behaviors
3. Add the pet type to `PetFactory.RegisterPetTypes()`
4. Create a registration method in `PetFactory` (e.g., `RegisterNewPetType()`)
5. Add associated assets (sprites, prefabs, etc.)

### Adding New Mini-Games

To add a new mini-game:

1. Create a new class that extends `MiniGameManager.MiniGameBase`
2. Implement game mechanics and UI
3. Create a prefab for the mini-game
4. Add the mini-game to `MiniGameManager.availableGames`

## UI System

The UI system handles all player interactions:

- **UIManager**: Controls screens, transitions, and messages
- **Panel System**: Different panels for different game features
- **Pet Status Display**: Shows current pet stats and state
- **Animation**: Smooth transitions between screens

## Future Development

### Planned Features

- Additional pet types with new elements and abilities
- More mini-games with varying mechanics
- Enhanced guild features including cooperative activities
- Pet breeding/fusion system
- Seasonal events and limited-time pets
- Expanded customization options

### Monetization

The game is designed with several monetization touchpoints:

- Premium pets (higher rarity)
- Special accessories and customizations
- Time-savers and boosters
- Optional ad viewing for bonuses
- Battle pass / premium subscription

## Best Practices

### Code Style

- Use SerializeField for Unity Inspector exposure
- Implement Singleton pattern for manager classes
- Use events for cross-system communication
- Keep UI and game logic separated
- Document all public methods and properties

### Performance

- Pool frequently instantiated objects
- Disable off-screen UI elements
- Use efficient data structures for lookups
- Minimize Update() method operations
- Use coroutines for time-dependent operations

## References

- Unity Manual: https://docs.unity3d.com/Manual/
- Unity Scripting API: https://docs.unity3d.com/ScriptReference/
- Design Inspiration: Neopets, Tamagotchi, Animal Crossing
