namespace VibeMUD.Commands;

using VibeMUD.Commands.Built_in;
using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Handles command parsing and execution
/// </summary>
public class CommandHandler
{
    private readonly Dictionary<string, Command> _commands = new();
    private readonly Dictionary<string, string> _aliases = new(); // alias -> command name

    public CommandHandler()
    {
        RegisterDefaultCommands();
    }

    /// <summary>
    /// Registers a command
    /// </summary>
    public void RegisterCommand(Command command)
    {
        _commands[command.Name.ToLower()] = command;

        foreach (var alias in command.Aliases)
        {
            _aliases[alias.ToLower()] = command.Name.ToLower();
        }
    }

    /// <summary>
    /// Parses and executes a command
    /// </summary>
    public CommandResult ExecuteCommand(Character character, GameState gameState, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return CommandResult.Error("Please enter a command.");

        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return CommandResult.Error("Please enter a command.");

        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        // Check if it's an alias
        if (_aliases.TryGetValue(commandName, out var resolvedCommand))
            commandName = resolvedCommand;

        // Find and execute command
        if (_commands.TryGetValue(commandName, out var command))
        {
            if (!character.IsAlive())
                return CommandResult.Error("You are dead. You cannot perform actions.");

            return command.Execute(character, gameState, args);
        }

        return CommandResult.Error($"Unknown command: {commandName}. Type 'help' for available commands.");
    }

    /// <summary>
    /// Gets all available commands
    /// </summary>
    public List<Command> GetAllCommands()
    {
        return _commands.Values.ToList();
    }

    /// <summary>
    /// Gets a command by name
    /// </summary>
    public Command? GetCommand(string name)
    {
        if (_commands.TryGetValue(name.ToLower(), out var command))
            return command;

        return null;
    }

    private void RegisterDefaultCommands()
    {
        // Information
        RegisterCommand(new HelpCommand());
        RegisterCommand(new LookCommand());
        RegisterCommand(new InfoCommand());
        RegisterCommand(new WhoCommand());

        // Movement
        RegisterCommand(new NorthCommand());
        RegisterCommand(new SouthCommand());
        RegisterCommand(new EastCommand());
        RegisterCommand(new WestCommand());
        RegisterCommand(new UpCommand());
        RegisterCommand(new DownCommand());

        // Inventory
        RegisterCommand(new InventoryCommand());
        RegisterCommand(new EquipCommand());
        RegisterCommand(new UnequipCommand());
        RegisterCommand(new DropCommand());
        RegisterCommand(new TakeCommand());

        // Combat
        RegisterCommand(new AttackCommand());
        RegisterCommand(new CastCommand());
        RegisterCommand(new UseCommand());
        RegisterCommand(new FleeCommand());

        // Communication
        RegisterCommand(new SayCommand());
        RegisterCommand(new ChatCommand());
    }
}
