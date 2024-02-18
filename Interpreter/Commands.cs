using System;
using System.Collections.Generic;
using System.Collections;
using LlamaFS.VFS;
using LlamaFS.ENV;

namespace LlamaFS.Command;

public static class AllCommands
{
    private static Dictionary<string, Type> List = new();

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
            return Activator.CreateInstance(type, env) as ITerminalCommand;
        }

        throw new CommandNotFoundException($"Command '{commandKey}' not found.");
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

    public virtual IEnumerator RunCommand(string[] args)
    {
        yield break;
    }
}