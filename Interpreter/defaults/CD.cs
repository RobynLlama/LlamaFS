using System.Collections;
using LlamaFS.ENV;
using LlamaFS.VFS;
using LlamaFS.EXT;

namespace LlamaFS.Command.Default;

public class ChangeDirectory : TerminalCommand
{
    public ChangeDirectory(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        if (args.Length < 2)
        {
            yield return "Usage: CD <path>";
            yield break;
        }

        string path = args[1];

        ProcessQuotedInput(ref path);
        env.ResolvePath(ref path);

        //Console.WriteLine($"Final Path: {path}");

        var info = env.StatPathNode(path);

        if (info.type != NodeType.Directory)
        {
            yield return "Error: path is not a directory";
            yield break;
        }

        if (info.state.IsNullorDeleted())
        {
            yield return "Error: path is null or deleted";
            yield break;
        }

        //yield return $"Setting CWD: {path}";
        env.SetEnvVariable("$CWD", path);
    }
}