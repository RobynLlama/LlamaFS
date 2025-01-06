using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using LlamaFS.EXT;
using LlamaFS.LOG;

namespace LlamaFS.VFS;
public partial class VirtualFileSystem
{

    private int NextFileID = 0;
    protected Dictionary<int, Node> FileTable = new();
    protected readonly Dictionary<int, MemoryStream> NodeData = new();
    protected List<int> DeletedRecords = new();
    public int UUID { get; }
    public int MasterUUID { get; protected set; } = 0;
    public static int MaxFileNameLength = 12;
    public static int MaxFileContent = 300;
    private Node rootNode = new(NodeType.Directory, -1, "ROOT", 0);

    public VirtualFileSystem(int UUID, int MasterUUID)
    {
        this.UUID = UUID;
        this.MasterUUID = MasterUUID;

        if (MasterUUID != 0)
        {
            NextFileID = VFSManager.Instance.Get(MasterUUID).NextFileID;
        };

    }


    #region InternalFunctions
    /*********************************************
        INTERNAL FUNCTIONS
    *********************************************/
    internal int GetNextID()
    {
        if (MasterUUID != 0)
            return VFSManager.Instance.Get(MasterUUID).GetNextID();

        return ++NextFileID;
    }
    #endregion

    #region Nodes
    /*********************************************
        NODES
    *********************************************/
    /// <summary>
    /// Returns a node information tuple about a specific node ID
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    /// <exception cref="FileSystemNodeException"></exception>
    internal (Node node, NodeState state) NodeGet(int ID)
    {
        NodeState state = NodeGetState(ID);

        return state switch
        {
            NodeState.Local => (FileTable[ID], state),
            NodeState.Master => (VFSManager.Instance.Get(MasterUUID).NodeGet(ID).node, state),
            NodeState.Root => (rootNode, NodeState.Root),
            _ => throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS or any Master"),
        };
    }
    protected NodeState NodeGetState(int ID)
    {

        if (ID == 0)
            return NodeState.Root;

        if (FileTable.ContainsKey(ID))
            return NodeState.Local;
        else if (DeletedRecords.Contains(ID))
            return NodeState.Deleted;

        if (MasterUUID == 0)
            return NodeState.Null;
        else
        {
            NodeState state = VFSManager.Instance.Get(MasterUUID).NodeGetState(ID);
            if (state == NodeState.Local || state == NodeState.Master)
                return NodeState.Master;

            else return state;
        }
    }

    internal void NodeGetChildren(int Parent, List<Node> nodes)
    {
        foreach (Node item in FileTable.Values)
        {
            if (item.Parent == Parent)
            {
                if (!nodes.Contains(item))
                    nodes.Add(item);
            }
        }

        if (MasterUUID != 0)
        {
            VFSManager.Instance.Get(MasterUUID).NodeGetChildren(Parent, nodes);
        }
    }

    protected int NodeCreate(NodeType type, int Parent, string Name)
    {
        //Get parent info
        var ParentInfo = NodeGet(Parent);

        //Is the Parent valid?
        switch (ParentInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(Parent, UUID, "Parent node does not exist");
        }

        List<Node> nodes = new();
        NodeGetChildren(Parent, nodes);

        foreach (Node item in nodes)
        {
            if (item.Name == Name)
            {
                //Oopsie
                throw new FileSystemNodeException(item.UUID, UUID, "Child node with same name already exists");
            }
        }

        //We're clear to make a new node
        Node newNode = new(type, Parent, Name, GetNextID());
        //LogManager.Instance.WriteToLogs(LogLevel.Info, $"Adding node: {newNode.Name},{newNode.Parent},{newNode.UUID}");
        FileTable.Add(newNode.UUID, newNode);
        return newNode.UUID;
    }

    internal int LinkCreate(int Parent, string Name, int TargetVFS, int TargetNode)
    {
        int parentInfo = NodeCreate(NodeType.Link, Parent, Name);
        LinkUpdate(parentInfo, TargetVFS, TargetNode);

        return parentInfo;
    }

    internal bool LinkUpdate(int ID, int TargetVFS, int TargetNode, NodeType type = NodeType.Link)
    {
        var nodeInfo = NodeGet(ID);

        if (nodeInfo.state.IsNullOrDeleted())
        {
            return false;
        }

        if (nodeInfo.node.nodeType == NodeType.Link || nodeInfo.node.nodeType == NodeType.Directory)
        {
            Node oldNode = nodeInfo.node;
            FileTable[ID] = new(type, oldNode.Parent, oldNode.Name, oldNode.UUID, TargetNode, TargetVFS);
        }

        return false;
    }

    protected void NodeDeleteTree(int ID)
    {
        var NodeInfo = NodeGet(ID);

        switch (NodeInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Trying to delete a non-existant file");
            case NodeState.Root:
                throw new FileSystemNodeException(ID, UUID, "Trying to delete root pseudo-node");
        }

        //Get children
        List<Node> nodes = new();
        NodeGetChildren(ID, nodes);

        foreach (Node item in nodes)
        {
            NodeDeleteTree(item.UUID);
        }

        DeletedRecords.Add(ID);
        FileTable.Remove(ID);
    }

    protected void NodeDelete(int ID)
    {
        var NodeInfo = NodeGet(ID);

        switch (NodeInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Trying to delete a non-existant file");
            case NodeState.Root:
                throw new FileSystemNodeException(ID, UUID, "Trying to delete root pseudo-node");
        }

        DeletedRecords.Add(ID);
        FileTable.Remove(ID);
        //Todo: Remove MemoryStream here
    }

    protected void NodeRename(int ID, string Name)
    {
        var NodeInfo = NodeGet(ID);

        switch (NodeInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Trying to rename a non-existant file");
            case NodeState.Root:
                throw new FileSystemNodeException(ID, UUID, "Trying to rename root pseudo-node");
        }

        Node oldNode = NodeInfo.node;
        FileTable[ID] = new Node(oldNode.nodeType, oldNode.Parent, Name, oldNode.UUID, oldNode.LinkUUID, oldNode.vfsUUID);
    }

    protected MemoryStream NodeOpen(int ID, NodeFileMode mode)
    {
        //Todo: copy on read?
        var NodeInfo = NodeGet(ID);

        switch (NodeInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Trying to read a non-existant file");
            case NodeState.Root:
                throw new FileSystemNodeException(ID, UUID, "Trying to read root pseudo-node");
        }

        MemoryStream fileHandle;

        if (!NodeData.TryGetValue(ID, out fileHandle))
        {
            fileHandle = new();
            NodeData[ID] = fileHandle;
        }


        switch (mode)
        {
            case NodeFileMode.IO:
                fileHandle.Position = 0;
                break;
            case NodeFileMode.Append:
                fileHandle.Position = fileHandle.Length;
                break;
            case NodeFileMode.Overwrite:
                fileHandle.Dispose();
                fileHandle = new();
                NodeData[ID] = fileHandle;
                break;
        }

        return fileHandle;
    }

    /* protected void NodeWrite(int ID, string contents)
    {
        var NodeInfo = NodeGet(ID);

        switch (NodeInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Trying to write a non-existant file");
            case NodeState.Root:
                throw new FileSystemNodeException(ID, UUID, "Trying to write root pseudo-node");
        }

        NodeInfo.node.Value = contents

        FileTable[ID] = NodeInfo.node;
    } */
    #endregion
}
