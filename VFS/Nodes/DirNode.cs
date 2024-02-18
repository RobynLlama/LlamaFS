using System.Collections;
using System.Collections.Generic;

namespace LlamaFS.VFS.Nodes;
public class DirNode : Node, IEnumerable
{
    internal List<Node> Children = new();

    public DirNode(int Parent, string Name, int UUID) : base(NodeType.Directory, Parent, Name, UUID)
    {

    }

    public IEnumerator GetEnumerator()
    {
        return Children.GetEnumerator();
    }
}