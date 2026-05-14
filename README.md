# Vibe MUD

A text-based fantasy medieval Multi-User Dungeon (MUD) built with C# and .NET 8.0.

## Overview

Vibe MUD is a classic-style MUD where players explore a richly detailed fantasy world, fight monsters, gain experience, and progress through 100 levels of gameplay. The entire game world is defined through JSON configuration files, making it easy to expand, modify, and customize the experience.

**PvE Only** - Vibe MUD is designed as a cooperative, monster-hunting experience. All combat is between players and NPCs in a shared world.

## Features

- **6 Playable Classes**: Warrior, Thief, Mage, Cleric, Monk, and Druid with unique abilities
- **Level Progression**: Advance from level 1 to 100 with stat growth and skill unlocks
- **6-Directional Movement**: Navigate north, south, east, west, up, and down
- **15+ Expansive Areas**: Distinct regions designed for specific level ranges, from starter zones to endgame challenges
- **Dynamic NPC System**: 
  - Multiple AI behaviors (wandering, patrolling, aggressive)
  - Special attacks (deathrolls, breaths, curses)
  - Special flags (Undead, Ethereal, Undying)
- **Advanced Combat**:
  - 5 damage types: Physical, Fire, Ice, Lightning, Light
  - Resistances and vulnerabilities
  - Turn-based tactical combat
- **Equipment & Loot**: Over 40 items across 15 areas with probability-based drops
- **Economy**: In-game shops, potions, and scrolls
- **JSON-Based Content**: All game data is easily modifiable and distributable

## Technology Stack

- **Language**: C# (.NET 8.0)
- **Data Format**: JSON for all game content
- **Architecture**: Async/await for multiplayer support
- **Testing**: xUnit for unit tests
- **License**: MIT

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- A terminal or command prompt

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/VibeMUD.git
cd VibeMUD

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the game
dotnet run --project src/VibeMUD.csproj
```

### First Time Playing

1. Start the server
2. Connect via telnet or the in-game client
3. Create a character and choose a class
4. You'll spawn in Starter City - explore and fight!

## Project Structure

```
VibeMUD/
├── src/                          # Source code
│   ├── Core/                     # Game engine and state
│   ├── Models/                   # Data models
│   ├── Systems/                  # Game systems (combat, movement, etc.)
│   ├── Data/                     # JSON loading and saving
│   ├── Commands/                 # Command handler and implementations
│   └── AI/                       # NPC behavior systems
├── content/                      # Game content data
│   ├── classes.json             # Class definitions and stats
│   ├── areas.json               # World areas and rooms
│   ├── npcs.json                # NPC definitions
│   ├── items.json               # Equipment and loot
│   ├── potions.json             # Consumable potions
│   ├── scrolls.json             # Single-use spell scrolls
│   └── shops.json               # Shop definitions
├── tests/                        # Unit tests
├── docs/                         # Documentation
│   ├── DESIGN.md                # Technical design document
│   ├── Developer Guide.md       # For contributors
│   └── User Guide.md            # For players
├── README.md                     # This file
├── state.json                    # Project state tracking
└── LICENSE                       # MIT License
```

## Documentation

- **[Design Document](docs/DESIGN.md)** - Complete technical specification of all systems, models, and mechanics
- **[User Guide](docs/User%20Guide.md)** - How to play Vibe MUD, commands, and tips
- **[Developer Guide](docs/Developer%20Guide.md)** - How to contribute and extend the game

## Game World

### Areas by Level Range

1. **Starter City** (1-10) - Hub for new adventurers with shops and tutorials
2. **Goblin Caves** (5-15) - Cavern system with goblin tribes
3. **Forest of Whispers** (10-20) - Woodland with sprites and forest creatures
4. **Crumbling Ruins** (15-25) - Ancient structures with undead and golems
5. **Mountain Pass** (20-30) - High altitude with dwarves and rock elementals
6. **Swamplands** (25-35) - Murky marshes with crocodiles and will-o'-wisps
7. **Dark Forest** (30-40) - Dangerous woods with dryads and shadow beasts
8. **Underground Mines** (35-45) - Deep caverns with demons and corrupted creatures
9. **Volcanic Wastes** (40-50) - Lava fields with fire elementals and dragons
10. **Ice Caverns** (45-55) - Frozen realm with frost giants and ice spirits
11. **Haunted Castle** (50-60) - Cursed fortress with wraiths and dark knights
12. **Abyssal Chasm** (55-65) - Portal-filled abyss with demonic entities
13. **Celestial Tower** (60-70) - Floating tower with angels and celestial beings
14. **Lich's Tomb** (70-80) - Ancient necromancer's lair with powerful undead
15. **Void Realm** (80-100) - Ultimate challenge with ancient evils

## Classes

| Class | Playstyle | Strengths |
|-------|-----------|-----------|
| **Warrior** | Tank/Melee DPS | High health, heavy armor, powerful attacks |
| **Thief** | Melee DPS/Burst | High evasion, quick attacks, crowd control |
| **Mage** | Ranged DPS/Control | Area damage, crowd control, mana flexibility |
| **Cleric** | Support/Hybrid | Healing, protection, light-based damage |
| **Monk** | Mobile DPS/Control | Quick movement, multiple hits, meditation |
| **Druid** | Support/Hybrid | Healing, nature damage, shapeshifting |

## Combat System

Combat is turn-based and tactical:

1. Player initiates combat with an NPC
2. Initiative determined by dexterity
3. Players and NPCs take turns executing actions
4. Actions include: attack, cast spell, use item, flee
5. Damage calculated based on stats, equipment, resistances, and damage type
6. Combat ends when one side reaches 0 HP or flees

### Damage Types

- **Physical**: Standard melee and ranged attacks
- **Fire**: Weak to ice, strong against plant-based creatures
- **Ice**: Weak to fire, strong against undead
- **Lightning**: Affects metal armor, neutral effectiveness otherwise
- **Light**: Strong against undead creatures

## Contributing

Contributions are welcome! Please see the [Developer Guide](docs/Developer%20Guide.md) for guidelines on:
- Code style and conventions
- Adding new content (areas, NPCs, items)
- Implementing new features
- Running tests

## Roadmap

### Current Phase
- Core game systems implementation
- Content creation (areas, NPCs, items)

### Planned Features
- Player guilds and faction systems
- Custom NPC quest generation
- Dynamic weather and day/night cycles
- Player-run shops and auctions
- Crafting system
- Fishing, gathering, and other professions

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Community

Join us and explore the world of Vibe MUD! Report bugs, suggest features, and share your experiences.

---

**Happy adventuring!** 🗡️

