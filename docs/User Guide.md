# VibeMUD User Guide

## Getting Started

### Connecting to the Server

1. Use a telnet client to connect to the server:
   ```
   telnet localhost 9999
   ```

2. You'll be greeted with the welcome message and prompted to enter your character name.

## Character Creation

### For New Players

When you enter a character name that doesn't exist, you'll be asked if you want to create a new character:

```
No character named 'Aragorn' found.
Would you like to create a new character? (yes/no)
```

Follow these steps:

1. **Enter Character Name** (3-20 characters)
   - Example: `Aragorn`

2. **Confirm Character Creation** (yes/no)
   - Type: `yes`

3. **Enter Password** (minimum 4 characters)
   - Type: `password123`

4. **Confirm Password**
   - Type: `password123` again

5. **Choose Class** from available options:
   - `warrior` - Strong melee fighter
   - `thief` - Fast and sneaky
   - `mage` - Magical spellcaster
   - `cleric` - Holy healer
   - `monk` - Martial artist
   - `druid` - Nature magic user
   - Example: `warrior`

After confirmation, you'll be placed in the game!

## Character Login

### For Returning Players

1. Enter your character name
2. Enter your password
3. You're logged in!

### Security

- **Password Attempts**: You have 3 attempts to enter the correct password
- **Case Sensitivity**: Character names are case-insensitive for login
- **Password Requirements**: Minimum 4 characters

## In-Game Commands

Once logged in, you can use various commands. Type `help` to see available commands.

Common commands:
- `look` - Examine your surroundings
- `north`, `south`, `east`, `west` - Move directions
- `inventory` - Check what you're carrying
- `say <message>` - Speak to other players
- `help` - Get command help
- `quit` - Disconnect (character is automatically saved)

## Character Persistence

Your character is automatically saved:
- When you disconnect/quit
- After game actions that change your stats
- After each command you execute

When you reconnect with the same character name and password, all your progress is restored:
- Level, experience points, gold
- Equipment and inventory
- Current location
- Skills and stats

## Troubleshooting

### Wrong Password

If you enter an incorrect password:
- You'll see an error message with remaining attempts
- You have 3 total attempts
- After 3 failed attempts, you'll be disconnected
- Simply reconnect to try again

### Character Name Not Found

- Check spelling and capitalization (login is case-insensitive but character names are specific)
- If you're a new player, you'll be offered to create a character

### Connection Issues

- Ensure the server is running on port 9999
- Check your telnet client connection
- Verify firewall settings

## Tips for New Players

1. **Save Frequently**: Disconnect periodically to ensure your progress is saved
2. **Explore**: Use movement commands to explore different areas
3. **Interact**: Use `say` and `chat` commands to interact with other players
4. **Check Help**: Type `help` to see all available commands
5. **Remember Your Password**: You'll need it to log back in to your character
