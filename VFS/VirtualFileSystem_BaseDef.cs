using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using LlamaFS.LOG;

namespace LlamaFS.VFS;
public partial class VirtualFileSystem
{

    private int NextFileID = 0;
    protected Dictionary<int, Node> FileTable = new();
    protected List<int> DeletedRecords = new();
    public int MaxFileSize { get; } = 350;
    public int UUID { get; }
    public int MasterUUID { get; protected set; } = 0;
    public int MaxFileNameLength { get; protected set; } = 12;
    public int MaxFileContentLength { get; protected set; } = 300;
    private Node rootNode = new(NodeType.Directory, 0, "ROOT", 0);
    public enum NodeState
    {
        Root,
        Null,
        Deleted,
        Local,
        Master
    };

    public VirtualFileSystem(int UUID, int MasterUUID, int MaxFileName, int MaxFileLength)
    {
        this.UUID = UUID;
        this.MasterUUID = MasterUUID;
        MaxFileNameLength = MaxFileName;
        MaxFileContentLength = MaxFileLength;

        if (MasterUUID != 0)
        {
            NextFileID = VFSManager.Instance.GetVFS(MasterUUID).NextFileID;
        };

    }


    #region InternalFunctions
    /*********************************************
        INTERNAL FUNCTIONS
    *********************************************/
    protected int GetNextID()
    {
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
            NodeState.Master => (VFSManager.Instance.GetVFS(MasterUUID).NodeGet(ID).node, state),
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
            NodeState state = VFSManager.Instance.GetVFS(MasterUUID).NodeGetState(ID);
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
            VFSManager.Instance.GetVFS(MasterUUID).NodeGetChildren(Parent, nodes);
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

        NodeInfo.node.Name = Name;
        FileTable[ID] = NodeInfo.node;
    }

    protected string NodeOpen(int ID)
    {
        var NodeInfo = NodeGet(ID);

        switch (NodeInfo.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Trying to read a non-existant file");
            case NodeState.Root:
                throw new FileSystemNodeException(ID, UUID, "Trying to read root pseudo-node");
        }

        return NodeInfo.node.Value;
    }

    protected void NodeWrite(int ID, string contents)
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

        NodeInfo.node.Value = contents;
        FileTable[ID] = NodeInfo.node;
    }
    #endregion
}