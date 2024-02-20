using System;
using System.IO;
using LlamaFS.VFS;

namespace LlamaFS.EXT;

public static class NodeSaveExt
{
    public static void Save(this Node node, Stream output)
    {
        StreamWriter stream = new(output);
        stream.WriteLine($"NODE:{node.nodeType}:{node.Parent}:{node.Name}:{node.UUID}");
        stream.Flush();
    }
}