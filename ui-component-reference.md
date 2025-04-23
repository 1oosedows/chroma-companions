# Chroma Companions - UI Component Reference

This document provides a comprehensive reference for all UI components needed in the Chroma Companions game, including layout specifications, color schemes, and component hierarchies.

## Color Palette

### Primary Colors
- **Main Purple**: `#8A4FFF` - Primary brand color
- **Bright Orange**: `#FF7D3C` - Secondary accent color
- **Vibrant Pink**: `#FF3C9E` - Tertiary accent color
- **Electric Blue**: `#3C9EFF` - Information color

### Element Colors
- **Fire**: `#FF5C38` - Fire element color
- **Water**: `#38A7FF` - Water element color
- **Earth**: `#8D5C3F` - Earth element color
- **Air**: `#C9E8FF` - Air element color
- **Light**: `#FFED8A` - Light element color
- **Dark**: `#6B4299` - Dark element color
- **Nature**: `#5CCC5C` - Nature element color
- **Electric**: `#FFEB3B` - Electric element color

### UI Colors
- **Background**: `#F5F0FF` - Light background
- **Card Background**: `#FFFFFF` - Card/panel background
- **Text Primary**: `#333333` - Primary text color
- **Text Secondary**: `#666666` - Secondary text color
- **Success**: `#4CAF50` - Success/positive actions
- **Warning**: `#FF9800` - Warning messages
- **Error**: `#F44336` - Error messages
- **Inactive**: `#CCCCCC` - Disabled/inactive elements

## Typography

### Fonts
- **Main Font**: "Nunito" - For all UI text
- **Accent Font**: "Fredoka One" - For headers and emphasis

### Text Styles
- **Header 1**: Fredoka One, 32pt, `#333333`
- **Header 2**: Fredoka One, 24pt, `#333333`
- **Header 3**: Fredoka One, 18pt, `#333333`
- **Body Text**: Nunito Regular, 16pt, `#333333`
- **Small Text**: Nunito Regular, 14pt, `#666666`
- **Button Text**: Nunito Bold, 16pt, White

## Common UI Components

### Buttons

#### Primary Button
- **Background**: `#8A4FFF`
- **Text**: White, Nunito Bold, 16pt
- **Corner Radius**: 24px
- **Padding**: 16px horizontal, 12px vertical
- **States**:
  - Normal: `#8A4FFF`
  - Pressed: `#7040DC`
  - Disabled: `#CCCCCC`

#### Secondary Button
- **Border**: 2px `#8A4FFF`
- **Background**: Transparent
- **Text**: `#8A4FFF`, Nunito Bold, 16pt
- **Corner Radius**: 24px
- **Padding**: 16px horizontal, 12px vertical
- **States**:
  - Normal: Border `#8A4FFF`
  - Pressed: Background `#F0E6FF`
  - Disabled: Border `#CCCCCC`, Text `#CCCCCC`

#### Icon Button
- **Size**: 48x48px
- **Icon Size**: 24x24px
- **Background**: Circular, `#8A4FFF`
- **Icon Color**: White
- **States**:
  - Normal: `#8A4FFF`
  - Pressed: `#7040DC`
  - Disabled: `#CCCCCC`

### Cards

#### Pet Card
- **Size**: 160px wide, 200px tall
- **Background**: White
- **Corner Radius**: 16px
- **Shadow**: 0px 4px 8px rgba(0, 0, 0, 0.1)
- **Layout**:
  - Pet Image: 120x120px centered at top
  - Pet Name: Header 3, centered
  - Element Icon: 24x24px, right side
  - Rarity Stars: Bottom, centered

#### Information Card
- **Background**: White
- **Corner Radius**: 16px
- **Shadow**: 0px 4px 8px rgba(0, 0, 0, 0.1)
- **Padding**: 16px
- **Layout**:
  - Header: Header 2, top
  - Content: Body Text
  - Action Button: Bottom right (optional)

### Progress Bars

#### Stat Bar
- **Height**: 12px
- **Corner Radius**: 6px
- **Background**: `#E0E0E0`
- **Colors**:
  - Happiness: `#FF3C9E`
  - Hunger: `#FF7D3C`
  - Energy: `#FFEB3B`
  - Health: `#4CAF50`
- **Label**: Small Text, above bar

#### Experience Bar
- **Height**: 8px
- **Corner Radius**: 4px
- **Background**: `#E0E0E0`
- **Fill Color**: `#3C9EFF`
- **Label**: Small Text, Level indicator beside bar

### Navigation

#### Tab Bar
- **Height**: 60px
- **Background**: White
- **Shadow**: 0px -2px 8px rgba(0, 0, 0, 0.1)
- **Items**:
  - Icon: 24x24px
  - Label: Small Text
  - Active Tab: Icon and text in `#8A4FFF`
  - Inactive Tab: Icon and text in `#666666`

#### Top Bar
- **Height**: 60px
- **Background**: White
- **Shadow**: 0px 2px 8px rgba(0, 0, 0, 0.1)
- **Content**:
  - Title: Header 2, centered
  - Back Button: Left side (when needed)
  - Action Button: Right side (when needed)
  - Currency Display: Right side

## Screen Layouts

### Main Menu

```
+-----------------------------------+
|            GAME LOGO              |
|                                   |
|      [PLAY BUTTON]                |
|                                   |
|      [SHOP BUTTON]                |
|                                   |
|      [SETTINGS BUTTON]            |
|                                   |
|   Player Level        Currency    |
+-----------------------------------+
```

- **Background**: Gradient or animated background with pets
- **Logo**: Centered at top, 50% of screen width
- **Buttons**: Stacked vertically, centered
- **Player Info**: Bottom of screen

### Pet Home Screen

```
+-----------------------------------+
| [Back]    PET HOME    [Settings]  |
+-----------------------------------+
|                                   |
|         [ACTIVE PET VIEW]         |
|                                   |
+-----------------------------------+
| Happiness [=========]             |
| Hunger    [=======  ]             |
| Energy    [======   ]             |
+-----------------------------------+
|                                   |
| [FEED]   [PLAY]   [REST]          |
|                                   |
+-----------------------------------+
| [Pets]  [Shop]  [Games]  [Guild]  |
+-----------------------------------+
```

- **Active Pet**: Large, animated pet in the center
- **Stats**: Progress bars for happiness, hunger, energy
- **Actions**: Feed, play, rest buttons
- **Tab Bar**: Bottom navigation

### Pet Selection Screen

```
+-----------------------------------+
| [Back]   SELECT PET    [Filter]   |
+-----------------------------------+
|                                   |
| [Pet Card] [Pet Card] [Pet Card]  |
|                                   |
| [Pet Card] [Pet Card] [Pet Card]  |
|                                   |
+-----------------------------------+
|     [PREVIEW OF SELECTED PET]     |
|                                   |
|   Pet Name          Element       |
|   Level: 5                        |
|                                   |
|   [SELECT BUTTON]                 |
+-----------------------------------+
```

- **Grid View**: Scrollable grid of pet cards
- **Preview**: Shows selected pet with details
- **Select Button**: Confirms pet selection

### Mini-Game Screen

```
+-----------------------------------+
| [Back]    MINI GAMES              |
+-----------------------------------+
|                                   |
| [Game 1]  [Game 2]  [Game 3]      |
|                                   |
| [Game 4]  [Game 5]  [Game 6]      |
|                                   |
+-----------------------------------+
|     [ACTIVE PET]                  |
|     Pet Bonus: +15% Score         |
|                                   |
+-----------------------------------+
```

- **Game Grid**: Scrollable grid of available games
- **Active Pet**: Shows which pet is providing bonuses
- **Bonus Info**: Displays current pet bonus

### Shop Screen

```
+-----------------------------------+
| [Back]      SHOP      Currency    |
+-----------------------------------+
| [Pets] [Food] [Toys] [Accessories]|
+-----------------------------------+
|                                   |
| [Item]     [Item]     [Item]      |
|                                   |
| [Item]     [Item]     [Item]      |
|                                   |
+-----------------------------------+
|        [SPECIAL OFFERS]           |
+-----------------------------------+
```

- **Category Tabs**: Filters items by category
- **Item Grid**: Scrollable grid of purchasable items
- **Special Offers**: Highlighted promotional items

### Guild Screen

```
+-----------------------------------+
| [Back]     GUILD                  |
+-----------------------------------+
|                                   |
|     [GUILD INFORMATION CARD]      |
|                                   |
+-----------------------------------+
|                                   |
| [MEMBERS]         [EVENTS]        |
|                                   |
+-----------------------------------+
|                                   |
| [ACHIEVEMENTS]    [REWARDS]       |
|                                   |
+-----------------------------------+
```

- **Guild Info**: Card with guild name, level, description
- **Tabs**: Navigate between guild features
- **Member List**: Shows guild members with badges
- **Events**: Upcoming guild activities

## Component Hierarchies

### Pet Card Prefab

```
PetCard
├── Background
├── PetImage
├── NameText
├── ElementIcon
├── RarityStars
└── SelectButton
```

### Stat Bar Prefab

```
StatBar
├── Label
├── ValueText
├── BarBackground
└── BarFill
```

### Pet Detail View Prefab

```
PetDetailView
├── PetContainer
│   └── PetSprite
├── InfoPanel
│   ├── NameText
│   ├── ElementIcon
│   ├── RarityStars
│   ├── LevelDisplay
│   └── Description
├── StatsBars
│   ├── HappinessBar
│   ├── HungerBar
│   ├── EnergyBar
│   └── HealthBar
└── AbilitiesList
    ├── Ability1
    ├── Ability2
    └── Ability3
```

### Top Navigation Bar Prefab

```
TopNavigationBar
├── BackButton
├── TitleText
├── SettingsButton
└── CurrencyDisplay
    ├── CurrencyIcon
    └── CurrencyAmount
```

### Bottom Tab Bar Prefab

```
BottomTabBar
├── HomeTab
│   ├── HomeIcon
│   └── HomeLabel
├── PetsTab
│   ├── PetsIcon
│   └── PetsLabel
├── ShopTab
│   ├── ShopIcon
│   └── ShopLabel
├── GamesTab
│   ├── GamesIcon
│   └── GamesLabel
└── GuildTab
    ├── GuildIcon
    └── GuildLabel
```

## Animation Guidelines

### Pet Animations
- **Idle**: Subtle breathing motion, blinking
- **Happy**: Bouncing, particle effects
- **Hungry**: Slower movement, looking around
- **Tired**: Occasional yawning, droopy posture
- **Play**: Energetic jumping, spinning
- **Eat**: Chewing animation with food item
- **Level Up**: Glowing effect, size pulse

### UI Animations
- **Button Press**: Scale to 0.95, then back to 1.0
- **Screen Transition**: Fade or slide between panels
- **Card Appearance**: Scale from 0.8 to 1.0 with slight bounce
- **Error Message**: Shake animation
- **Reward**: Spinning, particle effects, scale up and down

## Responsive Layout Guidelines

### Phone Portrait (Default)
- Full layouts as shown above
- Button size: 56px height
- Card width: 160px

### Phone Landscape
- Move bottom navigation to right side
- Reduce pet display size
- Reorganize stat bars horizontally

### Tablet
- Two-column layout for most screens
- Larger pet display
- Show more items in grids
- Button size: 64px height
- Card width: 200px

## Accessibility Considerations

- **Text Size**: Support Dynamic Type (iOS) and larger font scales
- **Color Contrast**: All text meets WCAG AA standards
- **Touch Targets**: Minimum 44x44px for all interactive elements
- **Alternative Text**: Provide for all informational images
- **Sound Feedback**: Accompany visual feedback with audio cues

## Implementation Tips

- Use anchors and layout groups to ensure responsive behavior
- Create prefabs for all reusable components
- Implement a UI manager to handle transitions
- Use TextMeshPro for all text elements
- Implement DOTween for smooth animations
