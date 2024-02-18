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
    const int MaxFileName = 10;
    const int MaxFileLength = 300;
    public readonly NodeType nodeType;
    private string _name = string.Empty;
    private string _value = string.Empty;
    public string Name
    {
        get => _name;
        internal set
        {
            if (value.Length > MaxFileName)
            {
                _name = value.Substring(0, MaxFileName);
                return;
            }

            _name = value;
        }
    }
    public string Value
    {
        get => _value;
        internal set
        {
            if (value.Length > MaxFileLength)
            {
                _value = value.Substring(0, MaxFileLength);
                return;
            }

            _value = value;
        }
    }
    public int Parent { internal set; get; }
    public readonly int UUID;

    public Node(NodeType type, int parent, string name, int UUID)
    {
        nodeType = type;
        Parent = parent;
        Name = name;
    }

}