using System.Collections;
using LlamaFS.ENV;

namespace LlamaFS.Command.Default;

public class MKDir : TerminalCommand
{
    public MKDir(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        if (args.Length < 2)
        {
            yield return "Usage: mkdir <path>";
            yield break;
        }

        string path = args[1];

        ProcessQuotedInput(ref path);

        //Resolve any . or .. characters
        //Console.WriteLine("resolving path");

        env.ResolvePath(ref path);

        //Console.WriteLine($"Final Path: {path}");

        if (!env.MakeDirectory(path))
        {
            yield return "Failed to create directory";
        }
    }
}