using System;
using System.Collections;
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

        string content = env.FileRead(path);
        string neoContent = string.Empty;
        int inBytes;
        int outBytes;
        byte[] data;
        byte[] bytes;

        switch (option)
        {
            case "-c":
                bytes = Encoding.ASCII.GetBytes(content);
                BrotliEncoder brotli = new();
                data = new byte[bytes.Length];
                brotli.Compress(bytes, data, out inBytes, out outBytes, true);
                brotli.Dispose();
                neoContent = Convert.ToBase64String(data);
                break;
            case "-d":
                bytes = Convert.FromBase64String(content);
                BrotliDecoder dec = new();
                data = new byte[bytes.Length];
                dec.Decompress(bytes, data, out inBytes, out outBytes);
                dec.Dispose();
                neoContent = Encoding.ASCII.GetString(data);
                break;
        }

        yield return neoContent;
        env.FileWrite(path, neoContent);
    }
}