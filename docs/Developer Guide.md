# VibeMUD Developer Guide

## Character Login & Persistence System

### Architecture Overview

The character authentication and persistence system is built on a state machine that guides players through login or character creation.

### Login State Machine

```
Initial Connection
    ↓
AskingName - Collect character name
    ↓
    ├─ Character exists? ─→ AskingPassword ─→ Password valid? ─→ Authenticated
    │                                              ↓
    │                                         Wrong (3 attempts) → Disconnect
    │
    └─ Character doesn't exist? ─→ AskingCreateConfirmation
                                    ↓
                            ├─ Yes → AskingNewPassword
                            │         ↓
                            │         ConfirmingPassword
                            │         ↓
                            │         AskingClass
                            │         ↓
                            │         (Create Character) → Authenticated
                            │
                            └─ No → (Back to AskingName)
```

### Key Components

#### 1. LoginState Enum (`Networking/LoginState.cs`)
Defines all possible states during authentication:
- `AskingName` - Waiting for character name input
- `AskingPassword` - Waiting for password (existing character)
- `AskingCreateConfirmation` - Waiting for new character confirmation
- `AskingNewPassword` - Waiting for password (new character)
- `ConfirmingPassword` - Waiting for password confirmation
- `AskingClass` - Waiting for class selection
- `Authenticated` - Character authenticated and in game

#### 2. PasswordHasher (`Utilities/PasswordHasher.cs`)
```csharp
public class PasswordHasher
{
    public string HashPassword(string password);           // BCrypt hash
    public bool VerifyPassword(string password, string hash); // Verify hash
}
```

**Security Details:**
- Uses BCrypt.Net-Next (industry standard)
- Work factor: 12 (balance of security and speed)
- Hash cost: ~100ms per hash (intentional slowness to prevent brute force)

#### 3. SaveManager Enhancements (`Data/SaveManager.cs`)

**New Methods:**
```csharp
public Character? LoadCharacterByName(string characterName);
public bool CharacterExistsByName(string characterName);
```

**File Storage:**
- Path: `players/` directory (created if doesn't exist)
- Filename format: `{lowercasename}.json`
- Example: `aragorn.json` for character "Aragorn"

**Character File Structure:**
```json
{
  "id": "client-uuid",
  "name": "Aragorn",
  "class": "warrior",
  "passwordHash": "$2b$12$...",
  "createdAt": "2026-05-15T12:34:56.789Z",
  "level": 1,
  "experiencePoints": 0,
  "health": 100,
  "maxHealth": 100,
  "gold": 0,
  ...other fields...
}
```

#### 4. ClientConnection Changes (`Networking/ClientConnection.cs`)

**New Properties:**
- `LoginState` - Current authentication state
- `AttemptedCharacterName` - Character name being authenticated
- `PasswordAttempt` - Password during creation (for confirmation)
- `WrongPasswordAttempts` - Failed login attempt counter

**New Methods:**
```csharp
public void SetLoginState(LoginState state);
public void SetAttemptedCharacterName(string name);
public void SetPasswordAttempt(string password);
public void IncrementWrongPasswordAttempts();
public void ResetLoginState();
public bool IsAuthenticatedCharacter();
```

#### 5. GameServer Login Flow (`Networking/GameServer.cs`)

**Main Method:**
```csharp
private async Task HandleLoginFlowAsync(string clientId, ClientConnection connection, string input)
```

Routes input to appropriate handler based on current login state.

**Handler Methods:**

| Handler | Purpose |
|---------|---------|
| `HandleAskingNameAsync()` | Validates name (3-20 chars), checks existence, determines next state |
| `HandleAskingPasswordAsync()` | Verifies password for existing character, enforces 3-attempt limit |
| `HandleCreateConfirmationAsync()` | Processes yes/no response for new character creation |
| `HandleNewPasswordAsync()` | Validates new password (min 4 chars), stores for confirmation |
| `HandleConfirmPasswordAsync()` | Verifies password confirmation matches |
| `HandleClassSelectionAsync()` | Validates class selection, creates character, saves to disk |
| `LoadCharacterIntoGameAsync()` | Sets character on connection, registers with server state |

### Data Flow

#### New Character Creation
```
User Input: "Aragorn"
    ↓
HandleAskingNameAsync()
    - Validate length (3-20)
    - Check existence via SaveManager.CharacterExistsByName()
    - If new: Prompt for creation confirmation
    ↓
User Input: "yes"
    ↓
HandleCreateConfirmationAsync()
    - Set state to AskingNewPassword
    ↓
User Input: "password123"
    ↓
HandleNewPasswordAsync()
    - Validate length (min 4)
    - Store in PasswordAttempt
    - Set state to ConfirmingPassword
    ↓
User Input: "password123"
    ↓
HandleConfirmPasswordAsync()
    - Compare with stored PasswordAttempt
    - If match: Set state to AskingClass
    ↓
User Input: "warrior"
    ↓
HandleClassSelectionAsync()
    - Validate class
    - Create Character object
    - Hash password: PasswordHasher.HashPassword()
    - Save to disk: SaveManager.SaveCharacter()
    - Load into game: LoadCharacterIntoGameAsync()
```

#### Existing Character Login
```
User Input: "Aragorn"
    ↓
HandleAskingNameAsync()
    - Character exists via SaveManager.CharacterExistsByName()
    - Set state to AskingPassword
    ↓
User Input: "password123"
    ↓
HandleAskingPasswordAsync()
    - Load character: SaveManager.LoadCharacterByName()
    - Verify password: PasswordHasher.VerifyPassword()
    - If valid: LoadCharacterIntoGameAsync()
    - If invalid: Increment attempt counter, re-prompt
```

### Character Saving

Characters are saved automatically at three points:

1. **Creation**: `SaveManager.SaveCharacter()` in `HandleClassSelectionAsync()`
2. **Gameplay**: `SaveManager.SaveCharacter()` after each command in `HandleCommandAsync()`
3. **Disconnection**: `SaveManager.SaveCharacter()` in `HandleClientDisconnectAsync()`

### Integration Points

#### Program.cs
```csharp
// Create SaveManager pointing to players directory
var playerSavePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "players");
var saveManager = new SaveManager(playerSavePath);

// Pass to GameServer
var server = new GameServer(gameState, commandHandler, saveManager);
```

#### Character Model
Added to existing Character class:
```csharp
public string PasswordHash { get; set; } = string.Empty;
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

### Error Handling

All error cases are handled gracefully:

| Error | Handling |
|-------|----------|
| Invalid character name length | Re-prompt with validation message |
| Character already exists (during creation) | Inform user, ask for new name |
| Character not found (during login) | Reset login state, re-ask for name |
| Wrong password | Increment counter, show remaining attempts |
| 3 wrong passwords | Automatic disconnect |
| Invalid class | Re-prompt with available options |
| File I/O error | Log error, send user message, disconnect if critical |

### Testing Checklist

- [ ] New character creation flow works end-to-end
- [ ] Character file created in correct location with correct format
- [ ] Password verification works on login
- [ ] Wrong password rejection (3 attempts max)
- [ ] Character stats persist between sessions
- [ ] Invalid class selection re-prompts
- [ ] Character names case-insensitive for login
- [ ] Password confirmation validation works
- [ ] Disconnection saves character
- [ ] BCrypt hashes are properly generated and verified

### Concurrency Considerations

The SaveManager is thread-safe:
- File operations use atomic writes
- Character existence checks happen before file creation
- Multiple simultaneous connection attempts to same character are handled by file system

The ClientConnection state machine is per-connection, so no shared state issues.

### Dependencies

- **BCrypt.Net-Next** v4.0.3 - Password hashing library
- **.NET 8.0** - Target framework

### Performance Notes

- BCrypt hashing intentionally takes ~100ms per password check (security feature)
- File I/O is only on creation, gameplay commands, and disconnection
- In-memory character state is maintained in GameState
- SaveManager uses JsonSerializer with indentation for readability (production could compress)

### Future Enhancements

1. **Character Deletion** - Add delete command with password confirmation
2. **Password Reset** - Admin command or email-based reset
3. **Login Timeout** - Auto-disconnect idle players
4. **Ban System** - Prevent login for banned characters
5. **Character Transfer** - Move character between accounts
6. **Encryption** - Encrypt character files at rest
7. **Backup System** - Automatic character backups
8. **Audit Logging** - Log all login attempts and password changes
