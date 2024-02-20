using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LlamaFS.VFS;

namespace LlamaFS.ENV;
public partial class VirtualEnvironment
{
    protected readonly Dictionary<string, int> MountedVFS = new();
    public int PrimaryVFS { get; protected set; }
    public int UUID { get; }
    protected Dictionary<string, string> variables = new()
    {
        {"$HOME",""},
        {"$CWD",@"/"}
    };

    public VirtualEnvironment(int UUID)
    {
        this.UUID = UUID;
    }

    internal (Node node, VirtualFileSystem.NodeState state) GetNodeFromPath(string Path)
    {
        //Console.WriteLine("GetNodeFromPath:");

        //Resolve . and .. like a real filesystem
        ResolvePath(ref Path);

        //Trim the path to just the final VFS if we're not on the primary
        (VirtualFileSystem vfs, Path) = ResolveMountedVFS(Path);

        //Split the string into parts for traversal
        string[] list = Path[1..].Split('/');

        //Set the current node to the root node of the VFS
        var current = vfs.NodeGet(0);

        //Console.WriteLine($"Resolved Path: {Path}");

        foreach (string item in list)
        {

            if (string.IsNullOrEmpty(item))
                continue;

            if (current.state.IsNullorDeleted())
            {
                return current;
            }

            //Console.WriteLine($"Looking for {item} in {current.node.Name}");
            current = GetChildByName(vfs, current.node.UUID, item);
        }

        return current;
    }

    internal (Node node, VirtualFileSystem.NodeState state) GetChildByName(VirtualFileSystem vfs, int Parent, string Name)
    {
        List<Node> children = new();
        vfs.NodeGetChildren(Parent, children);

        foreach (Node child in children)
        {
            if (child.Name == Name)
            {
                return vfs.NodeGet(child.UUID);
            }
        }

        return (new Node(), VirtualFileSystem.NodeState.Null);
    }
}