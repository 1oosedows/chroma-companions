# chroma-companions
# Chroma Companions

Chroma Companions is a colorful pet collection and care mobile game built in Unity. It features a variety of unique pets, mini-games, and a positive social guild system.

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or newer
- Visual Studio or other C# IDE
- Git for version control

### Installation

1. Clone the repository

```
git clone https://github.com/yourusername/chroma-companions.git
```

2. Open the project in Unity

   - Start Unity Hub
   - Click "Add" and select the cloned project folder
   - Open the project
3. Open the Main scene

   - Navigate to Assets/Scenes
   - Open MainScene.unity

### Project Structure

```
Assets/
├── Animations/
├── Audio/
├── Graphics/
├── Prefabs/
│   ├── Pets/
│   ├── UI/
├── Resources/
├── Scenes/
├── Scripts/
│   ├── Core/       # Core game systems
│   ├── Pets/       # Pet-related scripts
│   ├── MiniGames/  # Mini-game scripts
│   ├── Guild/      # Guild system
│   ├── UI/         # UI components
│   ├── Utils/      # Utility scripts
├── Settings/
```

## Core Game Systems

### Pet System

The pet system includes:

- Base pet functionality (PetBase.cs)
- Multiple pet types with unique abilities
- Pet registry system for managing pet types
- Pet factory for creating pet instances
- Pet selection for new players

### Mini-Game System

- Core mini-game framework
- Color matching puzzle game
- Integration with pet abilities

### Guild System

- Social features with positive community focus
- Level-gated access (level 15+)
- Badge-based reward system
- Guild events

### Shop System

- Purchase new pets, food, toys, and accessories
- Integration with the game economy

## Development Guide

### Adding a New Pet Type

1. Create a new class that extends `PetBase`

```csharp
public class NewPetType : PetBase
{
    // Implement unique behaviors
}
```

2. Add the pet type to PetFactory.RegisterPetTypes()

```csharp
petTypes.Add("NewPetType", typeof(NewPetType));
```

3. Create a registration method in PetFactory

```csharp
private void RegisterNewPetType()
{
    string petId = "NewPetType";
    GameObject prefab = CreatePetPrefab(petId, PetElement.YourElement, spriteIndex);
  
    PetRegistryEntry entry = new PetRegistryEntry
    {
        petId = petId,
        displayName = "New Pet Type",
        description = "Description of the new pet",
        // Set other properties...
    };
  
    PetRegistry.Instance.RegisterPet(entry);
}
```

4. Add the registration call in RegisterAllPets()

```csharp
public void RegisterAllPets()
{
    // Existing registrations...
    RegisterNewPetType();
}
```

### Adding a New Mini-Game

1. Create a new class that extends `MiniGameManager.MiniGameBase`

```csharp
public class NewMiniGame : MiniGameManager.MiniGameBase
{
    // Implement game logic
  
    public override void Initialize(PetBase pet)
    {
        base.Initialize(pet);
        // Custom initialization
    }
  
    // Add game-specific methods
}
```

2. Create a prefab for the mini-game in the Unity editor
3. Add it to the MiniGameManager's available games list

## Building the Game

### Android Build

1. Open Build Settings (File > Build Settings)
2. Switch platform to Android
3. Configure Player Settings:
   - Set company and product name
   - Configure orientation (Portrait)
   - Set minimum API level (Android 5.0+)
4. Click Build or Build And Run

### iOS Build

1. Open Build Settings (File > Build Settings)
2. Switch platform to iOS
3. Configure Player Settings
4. Click Build
5. Open the generated Xcode project
6. Configure signing and capabilities
7. Build to device or simulator

## Contributing

1. Fork the repository
2. Create a feature branch
   ```
   git checkout -b feature/your-feature-name
   ```
3. Commit your changes
   ```
   git commit -m "Add your feature"
   ```
4. Push to the branch
   ```
   git push origin feature/your-feature-name
   ```
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

* Inspired by games like Neopets, Tamagotchi, and Animal Crossing
* Thanks to all contributors
