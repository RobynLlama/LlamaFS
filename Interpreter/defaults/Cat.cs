using System;
using System.Collections;
using System.IO;
using LlamaFS.ENV;

namespace LlamaFS.Command.Default;

public class Cat : TerminalCommand
{
    public Cat(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        if (args.Length < 2)
        {
            yield return "Usage: cat <path>";
            yield break;
        }

        string path = args[1];

        ProcessQuotedInput(ref path);

        //Resolve any . or .. characters
        //Console.WriteLine("resolving path");

        env.ResolvePath(ref path);

        //Console.WriteLine($"Final Path: {path}");
        string contents;

        StreamReader reader = new(env.FileOpen(path, VFS.NodeFileMode.IO));
        contents = reader.ReadToEnd();

        yield return contents;
    }
}