using System.Collections;
using LlamaFS.ENV;

namespace LlamaFS.Command.Default;

public class MKFile : TerminalCommand
{
    public MKFile(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        if (args.Length < 2)
        {
            yield return "Usage: mkfile <path>";
            yield break;
        }

        string path = args[1];

        path = path[1..^1];
        path = path.Replace("\\\"", "\"");

        //Resolve any . or .. characters
        //Console.WriteLine("resolving path");

        ProcessQuotedInput(ref path);

        //Console.WriteLine($"Final Path: {path}");

        yield break;

        if (!env.MakeDirectory(path))
        {
            yield return "Failed to create directory";
        }
    }
}