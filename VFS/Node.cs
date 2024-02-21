using System.IO;

namespace LlamaFS.VFS;

public enum NodeType
{
    None,
    Directory,
    File,
    Device,
    Link
}
public struct Node
{
    public readonly NodeType nodeType;
    public readonly int Parent;
    public readonly string Name;
    public readonly int LinkUUID;
    public readonly int vfsUUID;
    public readonly int UUID;

    public Node(NodeType type, int parent, string name, int UUID, int Link = 0, int VFS = 0)
    {
        nodeType = type;
        Parent = parent;
        Name = name;
        this.UUID = UUID;
        LinkUUID = Link;
        vfsUUID = VFS;
    }


}