# Unity 2D 

A top-down action game inspired by Hyper Light Drifter, UltraKill etc, featuring dynamic combat, momentum-based movement, and a combo system.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Controls](#controls)
- [Game Systems](#game-systems)
- [Installation](#installation)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Save System](#save-system)
- [Contributing](#contributing)

## Features

- **Momentum-Based Movement**: Isaac-like inertia system with smooth acceleration and deceleration
- **Dynamic Dash System**: Quick dash with speed boost and knee slide on double-tap
- **Combo System**: Chain attacks to increase movement and attack speed
- **Save/Load System**: Full game state persistence using Unity's PlayerPrefs
- **Enemy AI**: Basic enemy system with health and position saving
- **Health System**: Modular health component with damage and healing
- **Hit Feedback**: Visual and mechanical feedback for attacks and damage

## Getting Started

### Prerequisites

- Unity 2020.3 LTS or newer
- Basic understanding of Unity's 2D physics system
- C# programming knowledge



## Controls

| Action | Key |
|--------|-----|
| Movement | WASD / Arrow Keys |
| Light Attack | J |
| Heavy Attack | K |
| Dash | Shift (single tap) |
| Slide | Shift (double tap) |
| Quick Save | F5 |
| Quick Load | F9 |

## Game Systems

### Movement System

The player controller features a sophisticated movement system:

- **Inertia**: Smooth acceleration and deceleration similar to The Binding of Isaac
- **Combo Speed Scaling**: Higher combo levels increase speed but reduce control (ice effect)
- **Post-Dash Speed Boost**: Gain temporary speed increase after dashing
- **Speed Decay**: Bonus speed gradually returns to normal when idle

#### Configuration Parameters

```csharp
[Header("Movement")]
baseMoveSpeed = 4.5f;              // Base movement speed
acceleration = 20f;                 // How fast player accelerates
deceleration = 25f;                 // How fast player decelerates
comboIceMultiplier = 0.3f;         // Ice effect strength at high combo
postDashSpeedBonus = 1.5f;         // Speed multiplier after dash (1.5 = +50%)
speedBonusDecay = 0.8f;            // How fast bonus speed decays
```

### Dash System

Two types of dashes:

**Normal Dash (Single Shift)**
- Quick burst of speed in movement direction
- Grants speed boost after completion
- Short cooldown

**Knee Slide (Double Shift)**
- Longer duration with gradual deceleration
- Player loses control during slide
- Can be canceled by attacking
- Starts at dash speed and smoothly decays

#### Configuration Parameters

```csharp
[Header("Dash")]
dashSpeed = 15f;                   // Dash/slide speed
dashDuration = 0.15f;              // Dash duration
dashCooldown = 0.8f;               // Cooldown between dashes
doubleTapWindow = 0.3f;            // Time window for double-tap

[Header("Slide")]
slideDuration = 0.6f;              // Slide duration
slideDecayRate = 8f;               // Speed decay during slide
```

### Combat System

**Chain Attack System**
- Light attacks (J): Fast, low damage
- Heavy attacks (K): Slower, high damage
- Attacks can interrupt slides
- Combo system increases attack speed

**Hit Reaction**
- Knockback on damage
- Stagger animation
- Combo interruption on hit

### Health System

The `Health` component provides:
- Current/Max HP tracking
- Damage and healing methods
- Events for health changes and death
- IDamageable interface implementation

### Save System

Complete game state persistence:

**Saved Data**
- Player health and position
- Current combo level
- Active scene
- All enemy states (health, positions)

**Usage**
```csharp
// Save game
GameManager.Instance.Save();

// Load game
GameManager.Instance.Load();
```

## Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs      # Main player controller
│   │   ├── ComboSystem.cs           # Combo mechanics
│   │   ├── ChainAttack.cs           # Attack system
│   │   └── AttackFeedback.cs        # Visual feedback
│   ├── Core/
│   │   ├── Health.cs                # Health component
│   │   ├── IDamageable.cs           # Damage interface
│   │   └── GameManager.cs           # Game state manager
│   ├── Enemy/
│   │   └── EnemyAI.cs               # Enemy behavior
│   ├── UI/
│   │   └── PlayerUI.cs              # UI elements
│   └── Data/
│       └── SaveData.cs              # Save data structures
├── Scenes/
│   └── MainScene.unity              # Main game scene
└── Prefabs/
    ├── Player.prefab
    └── Enemy.prefab
```


### Save Data Structure

```csharp
public class SaveData
{
    public int playerHealth;      // Current HP
    public int comboLevel;        // Combo level
    public float playerPosX;      // X position
    public float playerPosY;      // Y position
    public int currentScene;      // Scene index
    public List<EnemySaveData> enemies;
}
```

### Enemy Save Data

```csharp
public class EnemySaveData
{
    public string enemyID;        // Unique identifier
    public int currentHealth;     // Current HP
    public float posX;            // X position
    public float posY;            // Y position
}
```



## Performance Considerations

- Use object pooling for projectiles/effects if needed
- Limit FindObjectOfType calls (only in save/load)
- Consider using coroutines for complex animations
- Profile in Unity Profiler for bottlenecks

## Known Issues

- Enemy ID generation uses GetInstanceID() which may not be consistent across game restarts
- Save system uses PlayerPrefs which has platform-specific limitations
- No encryption on save data (implement if needed)

## Future Improvements

- [ ] Add enemy pooling system
- [ ] Implement more attack types
- [ ] Add particles and VFX
- [ ] Create combo UI display
- [ ] Add sound effects
- [ ] Implement damage numbers
- [ ] Add more movement abilities
- [ ] Create boss AI system
- [ ] Add weapon variety
- [ ] Implement difficulty settings




## Contact
kartoxa9373

**Note**: This is an active development project. Features and API may change.
