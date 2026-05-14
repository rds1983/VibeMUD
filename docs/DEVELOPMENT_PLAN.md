# Vibe MUD - Development Plan

## Overview

This document outlines the development roadmap for Vibe MUD, breaking the project into phases, milestones, and actionable tasks. The plan prioritizes getting a playable game loop functional early, then expanding content and features iteratively.

## Development Phases

### Phase 1: Project Foundation (Weeks 1-2)
**Goal**: Establish core project structure and basic game loop

#### Milestones
1. **M1.1**: Project Setup
   - [x] Git repository initialized
   - [x] README.md created
   - [x] Design document finalized
   - [ ] Create .csproj files and solution structure
   - [ ] Set up project directory layout (src/, content/, tests/)
   - [ ] Configure .gitignore for C# projects

2. **M1.2**: Core Models
   - [ ] Implement Character model (stats, inventory, equipment)
   - [ ] Implement NPC model (stats, AI behavior, loot)
   - [ ] Implement Room model (exits, NPCs, items, terrain)
   - [ ] Implement Area model (rooms, metadata, level range)
   - [ ] Implement Item model (equipment, consumables)
   - [ ] Implement Skill model
   - [ ] Write unit tests for all models

3. **M1.3**: Data Loading System
   - [ ] Implement JsonDataLoader (loads all content/ JSON files)
   - [ ] Create JSON schema validators
   - [ ] Implement SaveManager (persist character state)
   - [ ] Write tests for data loading and validation

#### Tasks
```
1.1.1 - Create VibeMUD.sln and project structure
1.1.2 - Set up .gitignore and configuration files
1.1.3 - Create GitHub repository (if using external repo)

1.2.1 - Implement Character class with properties and methods
1.2.2 - Implement NPC class with AI support
1.2.3 - Implement Room, Area, Item, Skill classes
1.2.4 - Add validation logic to all models
1.2.5 - Write comprehensive unit tests (40+ tests)

1.3.1 - Implement JsonDataLoader with System.Text.Json
1.3.2 - Create JSON schema validation
1.3.3 - Implement SaveManager for character persistence
1.3.4 - Write integration tests for data loading
```

#### Deliverables
- Working .NET 8.0 project structure
- All core models implemented with validation
- Data loading and persistence system functional
- 40+ passing unit tests

---

### Phase 2: Game Engine & Basic Systems (Weeks 3-4)
**Goal**: Implement game loop, command handler, and basic interactions

#### Milestones
1. **M2.1**: Game Engine
   - [ ] Implement GameState (holds current world state)
   - [ ] Implement GameEngine (main game loop)
   - [ ] Implement CommandHandler (parses and executes commands)
   - [ ] Create base Command abstraction
   - [ ] Write tests for game engine

2. **M2.2**: Movement System
   - [ ] Implement MovementSystem (move between rooms)
   - [ ] Add 6-direction support (N, S, E, W, U, D)
   - [ ] Implement room validation and exit checking
   - [ ] Add movement command handlers
   - [ ] Write tests for movement

3. **M2.3**: Basic Inventory & Equipment
   - [ ] Implement inventory management (add, remove, list)
   - [ ] Implement equipment system (equip, unequip, swap)
   - [ ] Create inventory commands
   - [ ] Add weight limit validation
   - [ ] Write tests for inventory system

#### Tasks
```
2.1.1 - Implement GameState class
2.1.2 - Implement GameEngine with main game loop
2.1.3 - Implement CommandHandler with command parsing
2.1.4 - Create base Command abstract class
2.1.5 - Implement HelpCommand, InfoCommand
2.1.6 - Write 20+ tests for game engine

2.2.1 - Implement MovementSystem class
2.2.2 - Create movement command handlers (move, north, south, etc.)
2.2.3 - Add room exit validation
2.2.4 - Test all 6 directions and boundary conditions
2.2.5 - Write 15+ movement tests

2.3.1 - Implement InventorySystem class
2.3.2 - Create inventory commands (inventory, drop, take, etc.)
2.3.3 - Implement EquipmentSystem
2.3.4 - Create equipment commands (equip, unequip, wear, remove)
2.3.5 - Write 15+ inventory/equipment tests
```

#### Deliverables
- Game engine with functional game loop
- Command parsing and execution system
- Movement between rooms fully functional
- Inventory and equipment management working
- 50+ new passing tests

---

### Phase 3: Combat System (Weeks 5-6)
**Goal**: Implement turn-based PvE combat with all mechanics

#### Milestones
1. **M3.1**: Combat Engine
   - [ ] Implement CombatSystem (turn-based combat)
   - [ ] Implement Combat state machine
   - [ ] Implement damage calculation
   - [ ] Add all 5 damage types
   - [ ] Implement resistances and vulnerabilities
   - [ ] Write extensive combat tests

2. **M3.2**: Combat Features
   - [ ] Implement NPC special attacks
   - [ ] Implement special flags (Undead, Ethereal, etc.)
   - [ ] Implement flee mechanic
   - [ ] Implement turn order (initiative)
   - [ ] Add combat logging/display
   - [ ] Write tests for all features

3. **M3.3**: Skills & Abilities
   - [ ] Create skill system (learn, use, costs)
   - [ ] Implement mana system
   - [ ] Implement cooldowns
   - [ ] Create combat commands (attack, cast, use item, flee)
   - [ ] Write tests for skills

#### Tasks
```
3.1.1 - Implement CombatSystem class
3.1.2 - Create Combat state machine (Initiative -> Turns -> Resolution)
3.1.3 - Implement damage calculation formula
3.1.4 - Add all 5 damage types (Physical, Fire, Ice, Lightning, Light)
3.1.5 - Implement resistance and vulnerability logic
3.1.6 - Write 25+ combat tests

3.2.1 - Implement special attack system
3.2.2 - Implement NPC flags (Undead, Ethereal, Undying, Giant, Beast)
3.2.3 - Implement flee mechanic with success chance
3.2.4 - Implement initiative calculation
3.2.5 - Add combat event logging
3.2.6 - Write 20+ tests for combat features

3.3.1 - Create Skill class and SkillSystem
3.3.2 - Implement mana costs and mana regeneration
3.3.3 - Implement ability cooldowns
3.3.4 - Create combat command handlers (attack, cast, use, flee)
3.3.5 - Write 15+ skill system tests
```

#### Deliverables
- Fully functional turn-based combat system
- All damage types and interactions working
- Special attacks and NPC flags implemented
- Skill/ability system with mana and cooldowns
- 60+ new combat tests

---

### Phase 4: Progression System (Week 7)
**Goal**: Implement leveling, experience, and stat growth

#### Milestones
1. **M4.1**: Experience & Leveling
   - [ ] Implement experience calculation
   - [ ] Implement level thresholds
   - [ ] Implement stat growth per level
   - [ ] Implement skill unlocks by level
   - [ ] Write progression tests

2. **M4.2**: Character Classes
   - [ ] Implement class-specific stat multipliers
   - [ ] Implement class-specific starting stats
   - [ ] Implement class-specific skill trees
   - [ ] Create base classes for all 6 classes
   - [ ] Write tests for class system

#### Tasks
```
4.1.1 - Create ProgressionSystem class
4.1.2 - Implement XP gain from combat
4.1.3 - Implement level thresholds (exponential scaling)
4.1.4 - Implement stat growth formulas
4.1.5 - Implement skill unlock system
4.1.6 - Write 20+ progression tests

4.2.1 - Create base ClassDefinition
4.2.2 - Implement all 6 classes (Warrior, Thief, Mage, Cleric, Monk, Druid)
4.2.3 - Implement class-specific skill trees
4.2.4 - Load classes from classes.json
4.2.5 - Write 15+ class system tests
```

#### Deliverables
- Experience system with exponential level scaling
- Stat growth and leveling system
- All 6 classes with unique progression paths
- 35+ new tests

---

### Phase 5: NPC AI System (Week 8)
**Goal**: Implement NPC behavior and intelligence

#### Milestones
1. **M5.1**: AI Behaviors
   - [ ] Implement base NPCBehavior class
   - [ ] Implement WanderBehavior
   - [ ] Implement PatrolBehavior
   - [ ] Implement AggressiveBehavior
   - [ ] Write behavior tests

2. **M5.2**: NPC Spawning & Respawning
   - [ ] Implement NPC spawning at game start
   - [ ] Implement NPC respawn timers
   - [ ] Track NPC positions and state
   - [ ] Write respawn tests

#### Tasks
```
5.1.1 - Create NPCBehavior abstract class
5.1.2 - Implement WanderBehavior (random room selection)
5.1.3 - Implement PatrolBehavior (predefined path)
5.1.4 - Implement AggressiveBehavior (attack on sight)
5.1.5 - Create NPCBehaviorSystem
5.1.6 - Write 20+ behavior tests

5.2.1 - Implement NPC spawn locations
5.2.2 - Implement respawn timer system
5.2.3 - Track NPC state and positions
5.2.4 - Create update loop for NPC behaviors
5.2.5 - Write 15+ respawn/spawn tests
```

#### Deliverables
- 3 AI behaviors fully functional
- NPC spawning and respawning system
- NPC state tracking and updates
- 35+ new tests

---

### Phase 6: Loot & Economy (Week 9)
**Goal**: Implement item drops, shops, and currency

#### Milestones
1. **M6.1**: Loot System
   - [ ] Implement LootSystem (generates drops from NPCs)
   - [ ] Implement probability-based item drops
   - [ ] Implement gold drops
   - [ ] Implement loot display
   - [ ] Write loot tests

2. **M6.2**: Shop System
   - [ ] Implement ShopSystem
   - [ ] Implement buy/sell mechanics
   - [ ] Implement shop inventory
   - [ ] Implement pricing with margins
   - [ ] Write shop tests

3. **M6.3**: Potions & Scrolls
   - [ ] Implement potion usage (healing, mana, effects)
   - [ ] Implement scroll usage (single-use spells)
   - [ ] Create consumable commands
   - [ ] Write consumable tests

#### Tasks
```
6.1.1 - Create LootSystem class
6.1.2 - Implement probability-based item selection
6.1.3 - Implement gold drop calculation
6.1.4 - Add loot to player on NPC defeat
6.1.5 - Write 15+ loot tests

6.2.1 - Create ShopSystem class
6.2.2 - Implement buy command
6.2.3 - Implement sell command
6.2.4 - Implement shop inventory management
6.2.5 - Implement pricing logic (margin multiplier)
6.2.6 - Write 15+ shop tests

6.3.1 - Implement potion effects (healing, mana, buffs)
6.3.2 - Implement scroll casting
6.3.3 - Create use/consume commands
6.3.4 - Add consumable validation
6.3.5 - Write 10+ consumable tests
```

#### Deliverables
- Functional loot system with probability drops
- Working shop system with buy/sell
- Potion and scroll systems
- 40+ new tests

---

### Phase 7: Game Content - Starter Area (Week 10)
**Goal**: Create starter city and first few areas

#### Milestones
1. **M7.1**: Starter City
   - [ ] Create 10+ starter city rooms
   - [ ] Create starter NPCs and shop
   - [ ] Create starter equipment
   - [ ] Set spawn point
   - [ ] Test complete starter flow

2. **M7.2**: Early Areas (Levels 5-20)
   - [ ] Create 3 areas with 40+ rooms each
   - [ ] Create NPCs for each area
   - [ ] Create area-specific loot tables
   - [ ] Balance difficulty curves
   - [ ] Test player progression

#### Tasks
```
7.1.1 - Create Starter City in areas.json (city_center, inn, shop, temple, etc.)
7.1.2 - Create starter NPCs (guard_captain, shopkeeper, town_crier, etc.)
7.1.3 - Create starter shop inventory
7.1.4 - Create starter equipment (iron_sword, leather_armor, etc.)
7.1.5 - Set spawn point in state
7.1.6 - Play through and balance starter experience

7.2.1 - Create Goblin Caves (levels 5-15)
7.2.2 - Create Forest of Whispers (levels 10-20)
7.2.3 - Create Crumbling Ruins (levels 15-25)
7.2.4 - Create NPCs and loot for each area
7.2.5 - Balance difficulty and loot progression
7.2.6 - Test complete progression through early areas
```

#### Deliverables
- Starter city fully implemented with quests and NPCs
- 3 early-game areas with 120+ rooms
- Balanced early progression curve
- Playable game from levels 1-25

---

### Phase 8: Mid & Late Game Content (Weeks 11-12)
**Goal**: Create remaining areas and content

#### Milestones
1. **M8.1**: Mid-Game Areas (Levels 25-60)
   - [ ] Create 6 mid-game areas with 40+ rooms each
   - [ ] Create mid-game NPCs and bosses
   - [ ] Create mid-game equipment progression
   - [ ] Balance difficulty

2. **M8.2**: Late-Game Areas (Levels 60-100)
   - [ ] Create 6 endgame areas with 40+ rooms each
   - [ ] Create endgame bosses and challenges
   - [ ] Create legendary equipment
   - [ ] Balance endgame progression

#### Tasks
```
8.1.1 - Create Swamplands (levels 25-35)
8.1.2 - Create Dark Forest (levels 30-40)
8.1.3 - Create Underground Mines (levels 35-45)
8.1.4 - Create Volcanic Wastes (levels 40-50)
8.1.5 - Create Ice Caverns (levels 45-55)
8.1.6 - Create Haunted Castle (levels 50-60)
8.1.7 - Create all NPCs and equipment
8.1.8 - Balance difficulty progression

8.2.1 - Create Abyssal Chasm (levels 55-65)
8.2.2 - Create Celestial Tower (levels 60-70)
8.2.3 - Create Lich's Tomb (levels 70-80)
8.2.4 - Create Void Realm (levels 80-100)
8.2.5 - Create boss NPCs and special encounters
8.2.6 - Create legendary equipment
8.2.7 - Create epic NPCs with special attacks
8.2.8 - Balance endgame content
```

#### Deliverables
- 15 total areas, 600+ rooms
- Complete content coverage for all levels 1-100
- Balanced difficulty progression
- Endgame challenges and legendary equipment

---

### Phase 9: Multiplayer & Networking (Week 13)
**Goal**: Implement multi-player support

#### Milestones
1. **M9.1**: Network Server
   - [ ] Implement async TCP server
   - [ ] Implement client-server communication
   - [ ] Implement player connection management
   - [ ] Write server tests

2. **M9.2**: Multi-Player State
   - [ ] Implement shared world state
   - [ ] Implement player visibility
   - [ ] Implement chat system
   - [ ] Implement party/grouping (optional)

#### Tasks
```
9.1.1 - Create TcpGameServer
9.1.2 - Implement async client handling
9.1.3 - Implement message protocol
9.1.4 - Implement connection/disconnection logic
9.1.5 - Write 15+ server tests

9.2.1 - Synchronize world state across clients
9.2.2 - Implement player visibility (show other players)
9.2.3 - Implement chat/communication system
9.2.4 - Implement party/group system (optional)
9.2.5 - Write 15+ multiplayer tests
```

#### Deliverables
- Working TCP game server
- Multiple simultaneous connections
- Shared world state
- Basic chat functionality

---

### Phase 10: Polish & Testing (Week 14)
**Goal**: Final testing, bug fixes, and optimization

#### Milestones
1. **M10.1**: Integration Testing
   - [ ] Full playthrough tests (1-100)
   - [ ] Stress testing (multiple players)
   - [ ] Edge case testing
   - [ ] Balance verification

2. **M10.2**: Bug Fixes & Optimization
   - [ ] Performance profiling
   - [ ] Memory leak detection
   - [ ] Bug fixes from testing
   - [ ] Documentation updates

3. **M10.3**: Release Preparation
   - [ ] Final content review
   - [ ] License file setup
   - [ ] Release notes
   - [ ] Final commit and tag

#### Tasks
```
10.1.1 - Play through game 1-100 multiple times
10.1.2 - Test multiplayer with concurrent players
10.1.3 - Test all combat mechanics
10.1.4 - Test all progression systems
10.1.5 - Document all bugs found
10.1.6 - Write 20+ integration tests

10.2.1 - Profile game performance
10.2.2 - Optimize hot paths
10.2.3 - Fix all reported bugs
10.2.4 - Update documentation
10.2.5 - Code cleanup

10.3.1 - Create LICENSE file (MIT)
10.3.2 - Create RELEASE_NOTES.md
10.3.3 - Final version bump
10.3.4 - Create git tag for v1.0
10.3.5 - Final documentation review
```

#### Deliverables
- Fully tested and stable game
- 100+ passing tests
- Complete documentation
- v1.0 release ready

---

## Timeline Summary

| Phase | Duration | Key Deliverable |
|-------|----------|-----------------|
| 1 | 2 weeks | Core models and data loading |
| 2 | 2 weeks | Game engine and basic systems |
| 3 | 2 weeks | Combat system |
| 4 | 1 week | Progression system |
| 5 | 1 week | NPC AI |
| 6 | 1 week | Loot and economy |
| 7 | 1 week | Starter content |
| 8 | 2 weeks | All remaining content |
| 9 | 1 week | Multiplayer support |
| 10 | 1 week | Polish and release |
| **Total** | **14 weeks** | **v1.0 Release** |

## Testing Strategy

### Unit Tests (All Phases)
- Minimum 80% code coverage
- Test all public methods
- Test edge cases and error conditions
- Organize by system (CombatTests, MovementTests, etc.)

### Integration Tests (Phases 2+)
- Test complete game flows
- Test system interactions
- Test data loading and persistence

### Gameplay Tests (Phases 7+)
- Manual playtesting
- Balance verification
- Content quality assurance
- Multiplayer testing

### Performance Testing (Phase 10)
- Load testing with concurrent players
- Memory profiling
- CPU profiling
- Network performance

## Git Workflow

### Commit Strategy
- Commit after completing each task
- Descriptive commit messages
- Group related tasks in feature branches
- Merge to main after phase completion

### Example Commit Messages
```
feat: Implement Character model with equipment slots

fix: Resolve inventory weight calculation error

test: Add 20 unit tests for combat system

docs: Update design document with combat mechanics

refactor: Simplify damage calculation formula

ci: Set up GitHub Actions for CI/CD
```

### Branching Strategy
```
main (production-ready)
├── phase-1 (merged after M1.3)
├── phase-2 (merged after M2.3)
├── phase-3 (merged after M3.3)
... (continues through phase-10)
```

## Risk Management

### Technical Risks
1. **JSON Performance**: Large content files may impact load time
   - **Mitigation**: Implement lazy loading and caching

2. **Multiplayer Synchronization**: Keeping player states in sync
   - **Mitigation**: Design clear update protocol early (Phase 9)

3. **Combat Balance**: PvE balance may be difficult to achieve
   - **Mitigation**: Extensive playtesting in Phase 10

### Schedule Risks
1. **Content Creation**: 600+ rooms take significant time
   - **Mitigation**: Parallelize content creation in Phase 8

2. **Unforeseen Bugs**: Complex systems may have hidden issues
   - **Mitigation**: Comprehensive testing throughout

## Success Criteria

- [ ] All 14 phases completed on schedule
- [ ] 100+ passing unit tests
- [ ] 600+ implemented rooms across 15 areas
- [ ] All 6 classes fully functional
- [ ] Level progression 1-100 complete
- [ ] Combat system fully balanced
- [ ] Multiplayer support working
- [ ] Complete documentation
- [ ] Clean, maintainable codebase
- [ ] v1.0 stable release

---

## Next Steps

1. Proceed with **Phase 1: Project Foundation**
2. Set up .NET 8.0 project structure
3. Create solution file and project layout
4. Begin implementing core models
5. Commit progress after each milestone

