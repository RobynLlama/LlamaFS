using System;
using System.Collections.Generic;
using System.Collections;
using LlamaFS.ENV;

namespace LlamaFS.Command;

public static class AllCommands
{
    internal static Dictionary<string, Type> List = new();

    public static void RegisterCommand(string commandKey, Type commandType)
    {
        if (!typeof(ITerminalCommand).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"{commandType.Name} must implement ITerminalCommand", nameof(commandType));
        }

        if (List.ContainsKey(commandKey))
        {
            throw new ArgumentException($"A command with key '{commandKey}' is already registered.", nameof(commandKey));
        }

        List.Add(commandKey, commandType);
    }

    public static ITerminalCommand GetCommandInstance(string commandKey, VirtualEnvironment env)
    {
        if (List.TryGetValue(commandKey, out var type))
        {
            if (typeof(TerminalCommand).IsAssignableFrom(type))
            {
                return Activator.CreateInstance(type, env) as TerminalCommand ?? throw new InvalidCastException("Unable to instance command type to TerminalCommand " + type.ToString());
            }
        }

        return new CommandNotFound(env);
        //throw new CommandNotFoundException($"Command '{commandKey}' not found.");
    }

    /* internal static void RegisterDefaultCommands()
    {
    } */

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

    public abstract IEnumerator RunCommand(string[] args);
}