using System.Collections;
using LlamaFS.ENV;

namespace LlamaFS.Command.Default;

public class Echo : TerminalCommand
{
    public Echo(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {

        if (args.Length < 2)
        {
            yield return "Usage: Echo <message>";
            yield break;
        }

        string arg = args[1];
        ProcessQuotedInput(ref arg);

        env.ResolveEnvVariables(ref arg);

        yield return arg;
    }
}