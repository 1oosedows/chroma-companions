# Chroma Companions Mini-Game System

## Overview

The mini-game system for Chroma Companions provides engaging alternative gameplay modes that complement the core pet-raising experience. Players can earn rewards through these games to enhance their pets' development and earn currency for purchases.

## Components

### Core Classes

1. **MiniGameBase.cs** - Abstract base class that provides common functionality:
   - Score tracking
   - Timer management
   - Game state handling (start, pause, resume, end)
   - Reward calculation

2. **MiniGameManager.cs** - Central control system that:
   - Maintains the collection of available games
   - Handles game selection UI
   - Tracks player progress and high scores
   - Manages daily bonuses and rewards
   - Integrates with the main game economy

3. **MiniGameUI.cs** - User interface component that:
   - Displays score, time, and game status
   - Manages tutorial screens
   - Handles pause and game over states
   - Shows rewards and achievements

### Mini-Game Implementations

1. **ColorMatchPuzzle.cs** - Match-3 style puzzle game
   - Match colored blocks in groups of 3+ to score points
   - Special blocks with unique clearing abilities
   - Combo system for chain reactions

2. **ColorRushRacer.cs** - Endless runner with color-matching
   - Character runs automatically through a colorful obstacle course
   - Players must match their color to obstacles to pass through them
   - Speed increases over time for escalating difficulty

3. **BubblePopAdventure.cs** - Bubble shooter with strategic depth
   - Shoot colored bubbles to match and pop groups
   - Special bubble types (bomb, rainbow, star) with unique effects
   - Physics-based shooting mechanics and strategy

## Integration with Main Game

### Reward System

Mini-games provide several types of rewards:
- **Currency** - Used for purchases in the main game shop
- **Experience** - Applied to the player's active pet
- **Happiness Boosts** - Increases pet happiness based on game performance
- **Special Items** - Rare cosmetics or accessories unlocked through achievements

### Pet Bonuses

Each pet type provides unique bonuses in specific mini-games:

1. **Element Affinities**:
   - Water element pets excel in Bubble Pop Adventure
   - Light element pets gain advantages in Color Match Puzzle
   - Air element pets perform better in Color Rush Racer

2. **Special Abilities**:
   - Pets can provide power-ups or starting bonuses in their preferred games
   - Higher-level pets offer stronger bonuses
   - Legendary pets have unique game-changing abilities

### Guild Connections

Mini-games contribute to guild progression:

1. **Contribution Points** - Mini-game achievements generate guild contribution
2. **Guild Competitions** - Weekly mini-game tournaments for guild rewards
3. **Guild Bonuses** - Guild level provides multipliers for mini-game rewards

## Player Progression

Mini-games are integrated into the progression system:

1. **Level Gating** - Some mini-games are unlocked at specific player levels
2. **Difficulty Scaling** - Game difficulty increases with player level
3. **Daily Challenges** - Rotating objectives for bonus rewards
4. **Achievements** - Long-term goals across all mini-games

## Technical Integration

The mini-game system connects with these main game components:

1. **GameManager.cs** - For overall game state and currency management
2. **PetBase.cs** - To apply experience and bonuses to pets
3. **UserData.cs** - To save player progress and achievements
4. **GuildManager.cs** - For guild contribution and events

## Future Expansion

The system is designed for easy addition of new mini-games:

1. Create a new class that extends MiniGameBase
2. Implement the required abstract methods
3. Create prefabs and UI assets
4. Add to the MiniGameManager's available games list

## Game Balance Considerations

When designing new mini-games, consider:

1. **Time vs. Reward** - Balance rewards based on average play time
2. **Skill vs. Luck** - Mix skill-based and luck-based elements for broad appeal
3. **Progression Impact** - Ensure mini-games complement but don't overshadow the main game
4. **Pet Integration** - Create meaningful connections to the pet raising mechanics

## Monetization Opportunities

The mini-game system offers several monetization touchpoints:

1. **Energy System** - Limit plays with an energy system that refills over time
2. **Continues** - Allow players to continue failed games for premium currency
3. **Power-ups** - Sell special abilities and advantages for difficult levels
4. **Exclusive Games** - Create premium mini-games available through purchase
5. **Cosmetic Themes** - Offer visual themes and customization options

## Implementation Roadmap

1. **Phase 1**: Implement core mini-game framework and first game (Color Match Puzzle)
2. **Phase 2**: Add second and third games (Color Rush Racer and Bubble Pop Adventure)
3. **Phase 3**: Integrate with pet bonuses and guild systems
4. **Phase 4**: Implement achievements and daily challenges
5. **Phase 5**: Add monetization features and expanded content
