using System;

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
    public string Name;
    public string Value;
    public int Parent { internal set; get; }
    public readonly int UUID;

    public Node(NodeType type, int parent, string name, int UUID)
    {
        nodeType = type;
        Parent = parent;
        Name = name;
        this.UUID = UUID;
        Value = string.Empty;
    }

    public Node(Node node, string name)
    {
        nodeType = node.nodeType;
        Parent = node.Parent;
        Name = name;
        Value = node.Value;
        UUID = node.UUID;
    }

}