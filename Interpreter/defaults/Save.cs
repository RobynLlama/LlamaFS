using System.Collections;
using LlamaFS.ENV;

namespace LlamaFS.Command.Default;

public class Save : TerminalCommand
{
    public Save(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        if (args.Length < 3)
        {
            yield return "Usage: save <path> <data>";
            yield break;
        }

        string path = args[1];
        string content = args[2];

        ProcessQuotedInput(ref path);
        ProcessQuotedInput(ref content);

        //Resolve any . or .. characters
        //Console.WriteLine("resolving path");

        env.ResolvePath(ref path);

        //Console.WriteLine($"Final Path: {path}");

        if (!env.FileWrite(path, content))
        {
            yield return "Failed to save to file";
        }
    }
}