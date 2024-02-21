using System;
using System.IO;
using System.Text;
using LlamaFS.VFS;

namespace LlamaFS.EXT;

public static class NodeSaveExt
{
    public static void Save(this Node node, Stream output)
    {
        StreamWriter stream = new(output);
        //stream.WriteLine($"NODE:{node.nodeType}:{node.Parent}:{node.Name}:{node.UUID}:{node.LinkUUID}:{node.vfsUUID}");
        stream.WriteLine($"NODE:{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{node.Parent}:{node.Name}:{node.UUID}:{node.LinkUUID}:{node.vfsUUID}"))}");
        stream.Flush();
    }
}