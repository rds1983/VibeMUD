# VibeMUD Agent Guide

This document explains how AI agents can understand and continue work on the VibeMUD project.

## Quick Start for Agents

### Understanding Project State

**Always check these files first** to understand what has been completed:

1. **`state.json`** - The single source of truth for project progress
   - `version`: Current build version (e.g., "0.4.0")
   - `status`: Current system focus (e.g., "progression_system_implemented")
   - `currentPhase`: What phase we're on (e.g., "Phase 4 Complete - Ready for Phase 5")
   - `completionStatus`: Detailed breakdown of what's implemented
   - `notes.accomplishments`: List of all completed features with phase indicators
   - Test count (bottom of completion status shows total tests passing)

2. **`CLAUDE.md`** - Project context and constraints
   - Coding patterns and conventions
   - What to avoid or prioritize
   - Class structure and system interactions

3. **`DESIGN.md`** - Architecture and technical specifications
   - System designs (combat, progression, movement, etc.)
   - Data models and JSON structures
   - Class progression systems

### Taking On New Work

When starting a new task:

1. **Read `state.json`** to understand what phase we're on and what's complete
2. **Check `nextSteps`** in state.json for the planned next work
3. **Verify all tests pass** - Run: `dotnet test`
4. **Work on the next incomplete feature**
5. **Keep state.json up to date** after completing a phase
6. **Commit with clear messages** explaining what was implemented
7. **Push to git** when work is complete

### File Organization

```
VibeMUD/
├── state.json                 ← PROJECT STATE (always read this first!)
├── CLAUDE.md                  ← AI context and preferences
├── docs/
│   ├── DESIGN.md             ← Technical architecture
│   ├── DEVELOPMENT_PLAN.md   ← Detailed phase breakdowns
│   └── AGENTS.md             ← This file
├── src/VibeMUD/
│   ├── Systems/              ← Game logic (Combat, Progression, etc.)
│   ├── Models/               ← Data models (Character, NPC, Item, etc.)
│   ├── Commands/             ← Command handlers
│   ├── Core/                 ← Game engine
│   └── Data/                 ← Data loading and persistence
├── tests/                    ← Comprehensive test suite
├── content/                  ← Game content JSON files
└── saves/                    ← Player character saves
```

## Understanding the Phase Structure

### Current Phases (as of v0.4.0)

- **Phase 1**: Core models and data systems ✅ (94 tests)
- **Phase 2**: Game engine with commands ✅ (139 tests)
- **Phase 3**: Full turn-based combat system ✅ (172 tests)
- **Phase 4**: Experience and leveling ✅ (195 tests)
- **Phase 5**: NPC AI behaviors (NEXT)
- **Phase 6**: Loot and economy systems
- **Phase 7-8**: Game content and areas
- **Phase 9**: Multiplayer networking
- **Phase 10**: Polish and release

## Test-Driven Development

This project uses **comprehensive testing**:

- Every system has unit tests
- Current test count: **195 passing tests**
- All tests must pass before committing
- Run: `dotnet test`

Expected test counts by phase:
- Phase 1: 94 tests
- Phase 2: 139 tests
- Phase 3: 172 tests
- Phase 4: 195 tests
- Phase 5: ~220+ tests (estimated)

## How to Update state.json

After completing a phase:

1. Update `version` (e.g., "0.4.0" → "0.5.0")
2. Update `status` to current system name
3. Update `currentPhase` to reflect completion and readiness for next
4. Update relevant system status in `completionStatus.systems`
5. Add accomplishments to `notes.accomplishments` with phase indicator (e.g., "✓ Phase 5: Feature name")
6. Update test count in notes

Example:
```json
{
  "version": "0.5.0",
  "status": "npc_behavior_system_implemented",
  "currentPhase": "Phase 5 Complete - Ready for Phase 6",
  "notes": {
    "phase5Completion": "✓ Phase 5 complete with NPC AI and 220 passing tests",
    "accomplishments": [
      "✓ Phase 5: NPC wandering behavior",
      "✓ Phase 5: NPC patrol paths",
      "✓ Phase 5: Aggressive NPC behavior"
    ]
  }
}
```

## Key Development Patterns

### System Implementation Pattern

1. **Create the System class** (e.g., `ProgressionSystem.cs`)
   - Static methods for logic
   - Clear, testable functions
   - No side effects except for intended game state changes

2. **Create comprehensive tests** (e.g., `ProgressionSystemTests.cs`)
   - Test normal cases
   - Test edge cases (level 1, level 100, etc.)
   - Test interactions with other systems

3. **Integrate into existing systems**
   - Update CombatSystem, GameEngine, etc. to use the new system
   - Add integration tests

4. **Update state.json**
   - Mark the feature as complete
   - Add to accomplishments list
   - Update test count

5. **Commit and push**
   - Clear commit message explaining what was implemented
   - Include test count in commit message

### Example: ProgressionSystem (Phase 4)

```csharp
// File: src/VibeMUD/Systems/ProgressionSystem.cs
public class ProgressionSystem
{
    // Experience calculation
    public static long GetExperienceForLevel(int level) { ... }
    
    // Award XP and handle leveling
    public static void AwardExperience(Character character, long xpGained) { ... }
    
    // Level up logic
    public static void LevelUp(Character character) { ... }
    
    // XP reward calculation
    public static long CalculateExperienceReward(Character character, NPC npc) { ... }
    
    // Character initialization
    public static void InitializeCharacter(Character character) { ... }
}
```

## Common Tasks for the Next Agent

### If working on Phase 5 (NPC Behavior)

Check state.json - it will tell you exactly what needs implementing:
- Implement wandering behavior
- Implement patrol behavior  
- Implement aggressive behavior
- Create NPCBehaviorSystem
- Add 20+ tests for NPC behaviors
- Integrate into game loop

Expected output:
- New `src/VibeMUD/Systems/NPCBehaviorSystem.cs`
- New `tests/VibeMUD.Tests/Systems/NPCBehaviorTests.cs`
- ~220 total tests passing (up from 195)

### If working on Phase 6 (Loot/Economy)

- Create `src/VibeMUD/Systems/LootSystem.cs`
- Create `src/VibeMUD/Systems/EconomySystem.cs`
- Add comprehensive tests
- Integrate NPC loot drops into combat
- Create shop system tests

## Communication Patterns

### Commit Messages

Use clear, consistent format:
```
Phase X: Feature name

- Brief description of what was implemented
- List of key components added
- Test count (e.g., "195 tests passing")
- Any notable design decisions
```

Example:
```
Phase 4: Implement progression system with experience and leveling

- Add ProgressionSystem with experience calculation and level progression
- Implement class-specific stat scaling for all 6 classes
- Add dynamic health/mana growth based on Constitution/Intelligence
- Integrate XP rewards into combat system
- All 195 tests passing

Features:
- Automatic leveling on reaching XP threshold
- Class progression with unique stat multipliers
- Experience scaling based on level difference
- Level cap at 100 with appropriate XP scaling
```

### Branching Strategy

- Work on `master` branch directly for this project (already established)
- Commit early and often
- Push after each completed phase
- No long-lived feature branches needed

## Git Workflow (Simplified)

```bash
# Read state.json to understand what to work on
cd D:\Projects\VibeMUD

# Make changes to implement the phase
# Write tests alongside implementation

# Run tests to verify
dotnet test

# Stage and commit
git add -A
git commit -m "Phase X: [description]"

# Push to remote
git push
```

## Debugging Tips

### Tests Failing?

1. Check that `state.json` matches actual implementation
2. Verify all new files are properly namespaced
3. Run `dotnet test --verbosity detailed` for detailed output
4. Check test assertions match the actual behavior

### Compilation Errors?

1. Ensure using statements are correct
2. Check that new Systems are added to relevant classes
3. Verify JSON data models match system expectations

### Git Issues?

- Always push after committing
- If conflicts occur, resolve manually then commit
- Check remote with `git status`

## Key Concepts

### What is state.json?

The **single source of truth** for project progress. It contains:
- What's been built (completionStatus)
- What phase we're on
- How many tests are passing
- What comes next (nextSteps)
- Historical accomplishments

An agent reading state.json can immediately understand the project's progress without diving into code.

### Why comprehensive tests?

Each system is thoroughly tested because:
1. Catches regressions immediately
2. Documents expected behavior
3. Allows safe refactoring
4. Proves the feature works

### Why these phases?

The phases are ordered for maximum dependency satisfaction:
- Build systems first (combat, progression)
- Add behaviors second (NPC AI)
- Implement economy third
- Fill content last (areas, NPCs, items)
- Only then handle multiplayer (networking needs stable single-player first)

## Success Criteria for Each Phase

When completing a phase, the agent should have:

- [ ] All code written and compiling
- [ ] Comprehensive tests (30+ tests typical)
- [ ] Total test count increases (e.g., 172 → 195)
- [ ] state.json updated with new version and accomplishments
- [ ] Commit message explaining the phase
- [ ] Changes pushed to git
- [ ] No broken tests or regressions

## Questions?

If unclear about:
- **What to do next**: Check `state.json` → `nextSteps`
- **How something works**: Check `DESIGN.md` → relevant section
- **Project preferences**: Check `CLAUDE.md`
- **Current status**: Check `state.json` → `completionStatus`

---

**Remember**: The agent's job is to reliably implement the next phase, keep state.json accurate, and ensure all tests pass. Simple, clear, testable code wins.
