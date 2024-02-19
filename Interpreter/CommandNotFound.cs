using System.Collections;
using LlamaFS.ENV;
using LlamaFS.LOG;

namespace LlamaFS.Command;

public class CommandNotFound : TerminalCommand
{
    public CommandNotFound(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        yield return $"Command not found {args[0]}";
    }
}