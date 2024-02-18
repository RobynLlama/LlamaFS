namespace LlamaFS.VFS.Nodes;
public class FileNode : Node
{
    public string Contents { internal set; get; }
    public bool Binary { internal set; get; }

    public FileNode(int Parent, string Name, bool bin, int UUID) : base(NodeType.Directory, Parent, Name, UUID)
    {
        Binary = bin;
    }

}