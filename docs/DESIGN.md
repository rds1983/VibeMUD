# Vibe MUD - Design Document

## Table of Contents
1. [Overview](#overview)
2. [Core Architecture](#core-architecture)
3. [Data Models](#data-models)
4. [Game Systems](#game-systems)
5. [World Design](#world-design)
6. [NPC System](#npc-system)
7. [Equipment & Items](#equipment--items)
8. [Economy & Shops](#economy--shops)
9. [Character Progression](#character-progression)
10. [Combat System](#combat-system)

---

## Overview

**Vibe MUD** is a text-based fantasy medieval Multi-User Dungeon (MUD) built with C# and .NET 8.0. Players explore a richly detailed world, fight monsters, gain experience, and progress through levels 1-100 across 15+ distinct areas. The entire game world is defined through JSON configuration files, allowing for easy expansion and modification.

### Key Features
- 6 playable classes with unique abilities
- 100-level progression system
- 6-directional movement (N, S, E, W, U, D)
- Dynamic NPC AI with special attacks and flags
- Multiple damage types and resistances
- Equipment loot system with probability-based drops
- In-game shops and economy
- Potion and scroll systems

---

## Core Architecture

### Technology Stack
- **Language**: C# (.NET 8.0)
- **Data Format**: JSON for all game content
- **Threading**: Async/await for multiplayer handling
- **Testing**: xUnit for unit tests

### Project Structure
```
VibeMUD/
├── src/
│   ├── Core/
│   │   ├── Game.cs
│   │   ├── GameEngine.cs
│   │   ├── GameState.cs
│   ├── Models/
│   │   ├── Character.cs
│   │   ├── NPC.cs
│   │   ├── Room.cs
│   │   ├── Area.cs
│   │   ├── Item.cs
│   │   ├── Skill.cs
│   ├── Systems/
│   │   ├── CombatSystem.cs
│   │   ├── ProgressionSystem.cs
│   │   ├── MovementSystem.cs
│   │   ├── NPCBehaviorSystem.cs
│   │   ├── LootSystem.cs
│   ├── Data/
│   │   ├── JsonDataLoader.cs
│   │   ├── SaveManager.cs
│   ├── Commands/
│   │   ├── CommandHandler.cs
│   │   ├── CombatCommands.cs
│   │   ├── MovementCommands.cs
│   │   ├── InventoryCommands.cs
│   ├── AI/
│   │   ├── NPCBehavior.cs
│   │   ├── PatrolBehavior.cs
│   │   ├── WanderBehavior.cs
│   │   ├── AggressiveBehavior.cs
├── content/
│   ├── classes.json
│   ├── areas.json
│   ├── npcs.json
│   ├── items.json
│   ├── potions.json
│   ├── scrolls.json
│   ├── shops.json
├── docs/
│   ├── DESIGN.md
│   ├── DEVELOPMENT_PLAN.md
│   ├── User Guide.md
│   ├── Developer Guide.md
├── saves/
│   ├── player_character_1.json
│   ├── player_character_2.json
├── tests/
│   ├── CombatSystemTests.cs
│   ├── ProgressionSystemTests.cs
│   ├── NPCBehaviorTests.cs
├── README.md
├── state.json (Project state: phases, completion status, test coverage)
├── AGENTS.md (Guide for AI agents to understand and continue the project)
├── CLAUDE.md (Project context for AI agents)
└── LICENSE
```

---

## Data Models

### Classes JSON (content/classes.json)
Defines the 6 playable classes with their base stats and progression.

```json
{
  "classes": [
    {
      "id": "warrior",
      "name": "Warrior",
      "description": "Strong melee combatant with heavy armor",
      "baseStats": {
        "health": 100,
        "mana": 30,
        "strength": 18,
        "dexterity": 10,
        "constitution": 16,
        "intelligence": 8,
        "wisdom": 12,
        "charisma": 10
      },
      "startingSkills": ["slash", "defend"],
      "armorTypes": ["leather", "chain", "plate"],
      "weaponTypes": ["sword", "axe", "mace"]
    }
  ]
}
```

### Character Model
- **Level**: 1-100
- **Experience Points**: Required for level progression
- **Stats**: STR, DEX, CON, INT, WIS, CHA (scale with level)
- **Health Points**: Based on class and constitution
- **Mana Points**: Based on class and intelligence
- **Equipment Slots**: Head, chest, hands, legs, feet, waist, back, main-hand, off-hand, ring1, ring2
- **Inventory**: List of items with weight limits
- **Skills**: Learned abilities based on class

### Area JSON (content/areas.json)
Defines game world areas with rooms and metadata.

```json
{
  "areas": [
    {
      "id": "starter_city",
      "name": "Starter City",
      "description": "A welcoming city for new adventurers",
      "levelRange": { "min": 1, "max": 10 },
      "spawnPoint": { "areaId": "starter_city", "roomId": "city_center" },
      "hasShop": true,
      "shopId": "starter_shop",
      "rooms": [
        {
          "id": "city_center",
          "title": "City Center",
          "description": "The heart of the city, bustling with activity",
          "exits": {
            "north": { "areaId": "starter_city", "roomId": "temple" },
            "south": { "areaId": "starter_city", "roomId": "market" },
            "east": { "areaId": "starter_city", "roomId": "inn" },
            "west": { "areaId": "starter_city", "roomId": "barracks" },
            "up": null,
            "down": null
          },
          "npcs": ["guard_captain", "town_crier"],
          "items": [],
          "terrain": "city"
        }
      ]
    }
  ]
}
```

### NPC JSON (content/npcs.json)
Defines NPCs with AI behaviors, stats, and special abilities.

```json
{
  "npcs": [
    {
      "id": "goblin_warrior",
      "name": "Goblin Warrior",
      "description": "A scrappy green warrior",
      "level": 3,
      "health": 20,
      "mana": 5,
      "stats": {
        "strength": 12,
        "dexterity": 11,
        "constitution": 10,
        "intelligence": 8,
        "wisdom": 9,
        "charisma": 7
      },
      "behavior": "aggressive",
      "patrolPath": null,
      "skills": ["slash", "bite"],
      "specialAttacks": ["bite_attack"],
      "flags": [],
      "lootTable": {
        "goldMin": 5,
        "goldMax": 15,
        "items": [
          { "itemId": "rusty_dagger", "probability": 0.3 },
          { "itemId": "leather_armor", "probability": 0.15 }
        ]
      },
      "resistances": {
        "physical": 0,
        "fire": 0,
        "ice": 0,
        "lightning": 0,
        "light": 0
      },
      "damageTypes": ["physical"],
      "respawnTime": 60
    }
  ]
}
```

### Item JSON (content/items.json)
Defines all equipment, consumables, and loot items.

```json
{
  "items": [
    {
      "id": "iron_sword",
      "name": "Iron Sword",
      "type": "weapon",
      "subtype": "sword",
      "slot": "main-hand",
      "weight": 3.5,
      "value": 50,
      "level": 1,
      "stats": {
        "damage": 8,
        "bonus_strength": 2
      },
      "damageType": "physical"
    },
    {
      "id": "leather_armor",
      "name": "Leather Armor",
      "type": "armor",
      "subtype": "chest",
      "slot": "chest",
      "weight": 5.0,
      "value": 30,
      "level": 1,
      "stats": {
        "armor": 5,
        "bonus_dexterity": 1
      },
      "resistances": {
        "fire": 0.05
      }
    }
  ]
}
```

### Potions & Scrolls JSON
```json
{
  "potions": [
    {
      "id": "health_potion_small",
      "name": "Small Health Potion",
      "heals": 25,
      "cost": 10,
      "weight": 0.5,
      "level": 1
    }
  ],
  "scrolls": [
    {
      "id": "scroll_fireball",
      "name": "Scroll of Fireball",
      "skill": "fireball",
      "manaCost": 30,
      "damage": 40,
      "damageType": "fire",
      "cost": 100,
      "weight": 0.2,
      "level": 5
    }
  ]
}
```

### Shops JSON (content/shops.json)
```json
{
  "shops": [
    {
      "id": "starter_shop",
      "name": "General Store",
      "keeper": "shopkeeper_bob",
      "inventory": [
        { "itemId": "iron_sword", "quantity": 10 },
        { "itemId": "leather_armor", "quantity": 15 },
        { "itemId": "health_potion_small", "quantity": 20 }
      ],
      "buySellMargin": 1.5
    }
  ]
}
```

---

## Game Systems

### Movement System
- **Directions**: north, south, east, west, up, down
- **Commands**: `move north`, `n` (shorthand)
- **Exit Validation**: Checks if exit exists in current room
- **Room Transitions**: Load new room state, display description and contents

### Combat System
- **Initiative**: Based on dexterity
- **Turn-based**: Players and NPCs take turns
- **Damage Calculation**: Base damage + stat modifiers - armor reduction
- **Damage Types**: Physical, fire, ice, lightning, light
- **Resistances & Vulnerabilities**: NPCs and players can have resistance to damage types
- **Special Attacks**: NPCs can execute special attacks with unique effects
- **Combat Resolution**: Tracks damage dealt, applies effects, ends combat when one side is defeated

### Progression System
- **Experience Gain**: Awarded for defeating NPCs
- **Level Thresholds**: Exponential scaling (e.g., level 2 = 1000 XP, level 3 = 2000 XP)
- **Stat Growth**: Stats increase by fixed amount per level
- **Skill Unlocks**: New skills learned at specific levels
- **Class-Specific Progression**: Each class has unique skill trees

### Loot System
- **Drop Probability**: Defined per item in NPC loot tables
- **Gold Drops**: Random range per NPC
- **Item Rarity**: Common (high drop chance) to Rare (low drop chance)
- **Level Scaling**: Dropped items match area level recommendations

---

## World Design

### Area Overview
**15+ Areas organized by level ranges:**

1. **Starter City** (1-10): Hub with shops, inn, and beginner dungeons nearby
2. **Goblin Caves** (5-15): Cavern system with goblin tribes
3. **Forest of Whispers** (10-20): Woodland with sprites and forest creatures
4. **Crumbling Ruins** (15-25): Ancient structures with undead and golems
5. **Mountain Pass** (20-30): High altitude terrain with dwarves and rock elementals
6. **Swamplands** (25-35): Murky marshes with crocodiles and will-o'-wisps
7. **Dark Forest** (30-40): Dangerous woods with dryads and shadow beasts
8. **Underground Mines** (35-45): Deep caverns with demons and corrupted creatures
9. **Volcanic Wastes** (40-50): Lava fields with fire elementals and dragons
10. **Ice Caverns** (45-55): Frozen realm with frost giants and ice spirits
11. **Haunted Castle** (50-60): Cursed fortress with wraiths and dark knights
12. **Abyssal Chasm** (55-65): Portal-filled abyss with demonic entities
13. **Celestial Tower** (60-70): Floating tower with angels and celestial beings
14. **Lich's Tomb** (70-80): Ancient necromancer's lair with powerful undead
15. **Void Realm** (80-100): Ultimate challenge area with ancient evils

### Room Requirements
- **Minimum 40 rooms per area**
- **Room features**: Title, description, exits (6-directional), NPCs, items, terrain type
- **Interconnected design**: Areas connect through specific exits
- **Logical progression**: Difficulty increases deeper into areas

---

## NPC System

### AI Behaviors

#### Wandering
- NPCs randomly move between adjacent rooms
- No specific pattern, frequency configurable

#### Patrolling
- NPCs follow a predefined path through rooms
- Return to start after reaching end
- Useful for guards and sentries

#### Aggressive
- NPCs attack players on sight
- Engage in combat immediately
- May patrol or wander between combats

### Special Attacks
Examples of NPC-specific attacks:

- **Crocodile Death Roll**: Physical damage + knockback effect
- **Dragon Breath**: Area-of-effect fire damage
- **Lich's Curse**: Applies curse status effect
- **Poison Bite**: Damage over time effect
- **Petrification Gaze**: Stuns player

### NPC Flags
Special characteristics affecting combat and interactions:

- **Undead**: Takes extra light damage (1.5x multiplier)
- **Ethereal**: Physical attacks deal 50% reduced damage
- **Undying**: Resurrects after 5 minutes if killed in lair
- **Giant**: Increased damage and health scaling
- **Beast**: Weakly resistant to magic (0.8x multiplier)

---

## Equipment & Items

### Equipment Categories
- **Weapons**: Swords, axes, maces, bows, staves (main-hand, off-hand)
- **Armor**: Chest, head, hands, legs, feet, waist, back
- **Accessories**: Rings (2 slots)

### Item Properties
- **Level Requirement**: Minimum player level to equip
- **Damage/Armor Stats**: Combat effectiveness
- **Stat Bonuses**: Strength, dexterity, etc.
- **Resistances**: Protection against damage types
- **Weight**: Affects carrying capacity
- **Rarity**: Common, uncommon, rare, legendary

### Consumables
- **Potions**: Health, mana, cure status effects
- **Scrolls**: Single-use spells that don't require mana
- **Temporary Buffs**: +10% damage for 5 minutes, etc.

---

## Economy & Shops

### Shop System
- **NPC Shopkeepers**: Fixed locations with inventory
- **Buying**: Players sell items to shops
- **Selling**: Players purchase items from shops
- **Margin**: Shop buys at lower price, sells at higher (e.g., 1.5x multiplier)
- **Limited Stock**: Shops have finite quantities

### Currency
- **Gold**: Primary currency for all transactions
- **Acquisition**: Looted from defeated NPCs, sold items
- **Spending**: Equipment, potions, scrolls, repairs

### Pricing Strategy
- **Base Value**: Each item has an inherent value
- **Level Scaling**: Higher-level items cost more
- **Supply/Demand**: Can be implemented for advanced economy

---

## Character Progression

### Level Progression (1-100)
- **Experience Curve**: Exponential scaling
  - Level 1: 0 XP (start)
  - Level 2: 1,000 XP
  - Level 10: 50,000 XP
  - Level 50: 5,000,000 XP
  - Level 100: 100,000,000 XP (or similar scaling)

### Stats Growth Per Level
- **Strength**: +1-2 per level (varies by class)
- **Dexterity**: +1-2 per level
- **Constitution**: +1 per level
- **Intelligence**: +1-2 per level
- **Wisdom**: +1 per level
- **Charisma**: +0-1 per level

### Skill System
- **Class-Based Skills**: Each class learns unique abilities
- **Skill Levels**: Skills can be improved with use (optional advanced system)
- **Mana/Resource Costs**: Abilities consume mana or resources
- **Cooldowns**: Some abilities have reuse timers

### Class Examples

**Warrior**: High STR/CON, heavy armor, sword & board
- Skills: Slash, Power Attack, Shield Bash, Whirlwind, Execution

**Mage**: High INT/WIS, light armor, staffs & scrolls
- Skills: Fireball, Frost Bolt, Lightning Storm, Teleport, Spellshield

**Thief**: High DEX/STR, light armor, dual-wield
- Skills: Backstab, Dodge, Shadow Clone, Pickpocket, Evasion

**Cleric**: WIS/INT, medium armor, maces & shields
- Skills: Heal, Holy Light, Resurrection, Smite, Divine Shield

**Monk**: STR/DEX/WIS, no armor, fist weapons
- Skills: Flying Kick, Iron Skin, Meditation, Chi Strike, Serenity

**Druid**: WIS/INT, medium armor, staffs & shapeshifting
- Skills: Entangle, Heal, Wild Shape, Regrowth, Nature's Wrath

---

## Combat System

### Combat Flow
1. **Initiation**: Player attacks NPC or vice versa
2. **Initiative Order**: Sort by dexterity stat
3. **Turn Loop**: Active combatant takes action
4. **Action Types**: Attack, cast spell, use item, flee
5. **Damage Resolution**: Calculate and apply damage
6. **Status Effects**: Apply special attack effects
7. **End Condition**: One combatant reaches 0 HP or flees successfully

### Damage Calculation
```
Final Damage = Base Damage × Stat Multiplier - Armor Reduction ± Resistances
```

### Damage Types & Interactions
- **Physical**: Standard melee/ranged attacks
- **Fire**: Weak to ice, strong against plants
- **Ice**: Weak to fire, strong against undead
- **Lightning**: Weak to nothing specific, strong against metal armor
- **Light**: Weak to darkness/shadow, strong against undead

### Resistances
- Each NPC and player has resistance values (0.0 - 1.0)
- 0.0 = normal damage, 0.5 = half damage, 1.5 = increased damage
- Example: Undead with light = 1.5 multiplier (takes extra damage)

### Combat Rewards
- **Experience**: Based on NPC level vs player level
- **Gold**: Random range from NPC's loot table
- **Items**: Probability-based drops from loot tables

---

## Technical Considerations

### JSON Data Loading
- Load all JSON at startup into memory
- Validate schema on load
- Support hot-reload for development
- Serialize/deserialize using System.Text.Json

### Persistence
- Save player characters to individual JSON files in `saves/` directory
- Track project development state in `state.json` (phases, completion status, test counts)
- Save game world state for respawns and NPC positions
- Implement transaction-like save system

### Multithreading
- Use async/await for player connections
- Thread-safe access to shared game state
- Proper locking for combat and NPC updates

### Testing Strategy
- Unit tests for each system (combat, progression, etc.)
- Integration tests for full game flows
- Mock data fixtures for testing

---

## Future Enhancements

- Player guilds and faction systems
- Custom NPC quest generation
- Dynamic weather and day/night cycles
- Player-run shops and auctions
- Crafting system
- Fishing, gathering, and other professions

