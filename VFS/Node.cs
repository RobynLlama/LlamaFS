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
    public int Parent { internal set; get; }
    public string Name;
    public readonly int LinkUUID;
    public readonly int UUID;

    public Node(NodeType type, int parent, string name, int UUID)
    {
        nodeType = type;
        Parent = parent;
        Name = name;
        this.UUID = UUID;
    }

    public Node(Node node, string name)
    {
        nodeType = node.nodeType;
        Parent = node.Parent;
        Name = name;
        UUID = node.UUID;
    }

}