using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using LlamaFS.ENV;

namespace LlamaFS.Command.Default;

public class Zip : TerminalCommand
{
    public Zip(VirtualEnvironment env) : base(env)
    {
    }

    public override IEnumerator RunCommand(string[] args)
    {
        if (args.Length < 3)
        {
            yield return "Usage: zip <option> <path>\n Options: -c, -d";
            yield break;
        }

        string option = args[1];
        string path = args[2];

        ProcessQuotedInput(ref path);

        //Resolve any . or .. characters
        //Console.WriteLine("resolving path");

        env.ResolvePath(ref path);

        //Console.WriteLine($"Final Path: {path}");

        MemoryStream file = env.FileOpen(path, VFS.NodeFileMode.IO);

        switch (option)
        {
            case "-c":
                //Setup streams
                MemoryStream compressed = new();
                GZipStream comp = new(compressed, CompressionMode.Compress);

                file.CopyTo(comp);
                comp.Flush();
                compressed.Position = 0;

                //Recreate the file for safety
                file = env.FileOpen(path, VFS.NodeFileMode.Overwrite);
                compressed.CopyTo(file);

                //Delete streams
                comp.Dispose();
                compressed.Dispose();
                break;
            case "-d":
                MemoryStream decompressed = new();
                GZipStream decomp = new(file, CompressionMode.Decompress);

                decomp.CopyTo(decompressed);
                decompressed.Position = 0;

                //Recreate file just in case
                file = env.FileOpen(path, VFS.NodeFileMode.Overwrite);
                decompressed.CopyTo(file);

                //cleanup
                decomp.Dispose();
                decompressed.Dispose();
                break;
        }

        yield return $"New file size: {file.Length}b";
    }
}