using System.Collections.Generic;
using LlamaFS.VFS;
using LlamaFS.EXT;
using System.IO;

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

    internal (Node node, NodeState state) GetNodeFromPath(string Path)
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

    internal (Node node, NodeState state) GetChildByName(VirtualFileSystem vfs, int Parent, string Name)
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

        return (new Node(), NodeState.Null);
    }

    internal bool MakeNode(NodeType type, string Path)
    {
        string ParentPath = Path + "..";
        string Name;
        ResolvePath(ref ParentPath);

        Name = Path.Replace(ParentPath, "").Replace("/", "");
        VirtualFileSystem vfs;

        (vfs, ParentPath) = ResolveMountedVFS(ParentPath);

        //LogManager.Instance.WriteToStream(LogLevel.Info, $"Parent: {ParentPath} Name: {Path}");

        var info = GetNodeFromPath(ParentPath);
        var child = GetNodeFromPath(Path);

        if (info.state.IsNullorDeleted())
        {
            //LogManager.Instance.WriteToStream(LogLevel.Warn, "Parent is null or deleted");
            return false;
        }

        if (!child.state.IsNullorDeleted())
        {
            //LogManager.Instance.WriteToStream(LogLevel.Warn, "Child already exists");
            return false;
        }

        switch (type)
        {
            case NodeType.File:
                vfs.FileCreate(info.node.UUID, Name);
                return true;
            case NodeType.Directory:
                vfs.DirCreate(info.node.UUID, Name);
                return true;
            default:
                return false;
        }
    }
}