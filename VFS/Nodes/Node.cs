using System;

namespace LlamaFS.VFS.Nodes;

public enum NodeType
{
    None,
    Directory,
    File,
    Device,
    Link
}
public class Node
{
    public readonly NodeType nodeType;
    public string Name { private set; get; }
    public int Parent { protected set; get; }
    public readonly int UUID;

    public Node(NodeType type, int parent, string name, int UUID)
    {
        nodeType = type;
        Parent = parent;
        this.UUID = UUID;
        Rename(name);
    }

    private void NameCheck()
    {
        if (Name.Length > 10)
        {
            Name = Name.Substring(0, 10);
        }
    }

    private void Rename(string name)
    {
        Name = name;
        NameCheck();
    }

}