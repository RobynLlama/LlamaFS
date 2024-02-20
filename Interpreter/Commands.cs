using System;
using System.Collections.Generic;
using System.Collections;
using LlamaFS.ENV;
using LlamaFS.Command.Default;

namespace LlamaFS.Command;

public static class AllCommands
{
    internal static Dictionary<string, Type> CommandRegistry = new();
    internal static Dictionary<string, Type> DefaultCommands = new()
    {
        {"cd", typeof(ChangeDirectory)},
        {"echo", typeof(Echo)},
        {"ls", typeof(ListDirectory)},
        {"mkdir", typeof(MKDir)},
        {"mkfile", typeof(MKFile)},
        {"save", typeof(Save)},
        {"cat", typeof(Cat)},
        {"zip", typeof(Zip)},
    };
    public static bool UseDefaultCommands = true;

    public static void RegisterCommand(string commandKey, Type commandType)
    {
        if (!typeof(ITerminalCommand).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"{commandType.Name} must implement ITerminalCommand", nameof(commandType));
        }

        if (CommandRegistry.ContainsKey(commandKey))
        {
            throw new ArgumentException($"A command with key '{commandKey}' is already registered.", nameof(commandKey));
        }

        CommandRegistry.Add(commandKey, commandType);
    }

    public static ITerminalCommand GetCommandInstance(string commandKey, VirtualEnvironment env)
    {
        if (CommandRegistry.TryGetValue(commandKey, out var registeredCommand))
        {
            if (typeof(TerminalCommand).IsAssignableFrom(registeredCommand))
            {
                return Activator.CreateInstance(registeredCommand, env) as TerminalCommand ?? throw new InvalidCastException("Unable to instance command type to TerminalCommand " + registeredCommand.ToString());
            }
        }
        else if (UseDefaultCommands)
            if (DefaultCommands.TryGetValue(commandKey, out var defaultCommand))
            {
                if (typeof(TerminalCommand).IsAssignableFrom(defaultCommand))
                {
                    return Activator.CreateInstance(defaultCommand, env) as TerminalCommand ?? throw new InvalidCastException("Unable to instance command type to TerminalCommand " + defaultCommand.ToString());
                }
            }

        return new CommandNotFound(env);
        //throw new CommandNotFoundException($"Command '{commandKey}' not found.");
    }

}

public interface ITerminalCommand
{
    IEnumerator RunCommand(string[] args);
}

public abstract class TerminalCommand : ITerminalCommand
{
    protected VirtualEnvironment env;

    public TerminalCommand(VirtualEnvironment env)
    {
        this.env = env;
    }

    public virtual void ProcessQuotedInput(ref string input)
    {
        //Trim first and last character if its quoted
        if (input[0] == '"')
            input = input[1..^1];

        //replace escaped quotes with real ones
        input = input.Replace("\\\"", "\"");
    }
    public abstract IEnumerator RunCommand(string[] args);
}