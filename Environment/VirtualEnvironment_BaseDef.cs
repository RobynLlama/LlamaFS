using System.Collections.Generic;
using LlamaFS.VFS;
using LlamaFS.EXT;
using System.IO;
using LlamaFS.LOG;

namespace LlamaFS.ENV;
public partial class VirtualEnvironment
{
    protected readonly Dictionary<int, string> MountedVFS = new();
    public int rootVFS { get; protected set; }
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

    internal (Node node, NodeState state, int vfs) GetNodeFromPath(string Path)
    {
        //Resolve . and .. like a real filesystem
        ResolvePath(ref Path);

        //Start on the root VFS
        VirtualFileSystem vfs = VFSManager.Instance.GetVFS(rootVFS);

        //Split the string into parts for traversal
        string[] list = Path[1..].Split('/');

        //Set the current node to the root node of the VFS
        var current = vfs.NodeGet(0);

        foreach (string item in list)
        {

            if (string.IsNullOrEmpty(item))
                continue;

            if (current.state.IsNullorDeleted())
            {
                return (current.node, current.state, vfs.UUID);
            }

            //Console.WriteLine($"Looking for {item} in {current.node.Name}");
            //LogManager.Instance.WriteToStream(LogLevel.Info, $"Looking for {item} in {current.node.Name}");
            current = GetChildByName(vfs, current.node.UUID, item);

            //Check for softlinks
            if (current.node.nodeType == NodeType.Link)
            {
                //LogManager.Instance.WriteToStream(LogLevel.Info, $"Attempting to select a link");
                //Set the new target
                if (current.node.vfsUUID != 0)
                {
                    //LogManager.Instance.WriteToStream(LogLevel.Info, $"Changing to VFS {current.node.vfsUUID}");
                    vfs = VFSManager.Instance.GetVFS(current.node.vfsUUID);
                }

                //Get the new current node
                current = vfs.NodeGet(current.node.LinkUUID);
            }
        }

        return (current.node, current.state, vfs.UUID);
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
        ResolvePath(ref Path);
        string ParentPath = Path + "..";
        string Name;
        ResolvePath(ref ParentPath);

        Name = Path.Replace(ParentPath, "").Replace("/", "");
        VirtualFileSystem vfs = VFSManager.Instance.GetVFS(rootVFS);

        //LogManager.Instance.WriteToStream(LogLevel.Info, $"Parent: {ParentPath} Name: {Name}");

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
                VFSManager.Instance.GetVFS(info.vfs).FileCreate(info.node.UUID, Name);
                return true;
            case NodeType.Directory:
                VFSManager.Instance.GetVFS(info.vfs).DirCreate(info.node.UUID, Name);
                return true;
            default:
                return false;
        }
    }
}