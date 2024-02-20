using System.Collections;
using LlamaFS.ENV;

namespace LlamaFS.Command;

public class CommandNotFound : TerminalCommand
{
    public CommandNotFound(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        yield return $"Command not found {args[0]}";

        foreach (string key in AllCommands.CommandRegistry.Keys)
        {
            yield return key;
        }
    }
}