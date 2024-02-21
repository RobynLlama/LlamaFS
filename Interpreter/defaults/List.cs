using System.Collections;
using System.Collections.Generic;
using System.IO;
using LlamaFS.ENV;
using LlamaFS.VFS;

namespace LlamaFS.Command.Default;

public class ListDirectory : TerminalCommand
{
    public ListDirectory(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {

        List<Node> files = new();
        string path;

        if (args.Length == 1)
        {
            path = env.GetEnvVariable("$CWD");
        }
        else
        {
            path = args[1];
        }

        ProcessQuotedInput(ref path);
        //yield return $"Path: {path}";

        env.ListDirectory(path, files);

        yield return $"Contents of {path}\n";

        foreach (Node file in files)
        {
            switch (file.nodeType)
            {
                case NodeType.Directory:
                    yield return $" {file.Name}/";
                    break;
                default:
                    yield return $" {file.Name}";
                    break;

            }
        }

        yield return "\n";
    }
}