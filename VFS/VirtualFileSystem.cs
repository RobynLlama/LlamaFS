using System.Collections.Generic;

namespace LlamaFS.VFS;
public class VirtualFileSystem
{

    private int NextFileID = 0;
    protected Dictionary<int, Node> FileTable = new();
    protected List<int> DeletedRecords = new();
    public int MaxFileSize { get; } = 350;
    public int UUID { get; }
    public int MasterUUID { get; protected set; } = 0;
    public bool Locked { get; protected set; } = false;
    public int MaxFileNameLength { get; protected set; } = 12;
    public int MaxFileContentLength { get; protected set; } = 300;
    public enum NodeState
    {
        Null,
        Deleted,
        Local,
        Master
    };

    public VirtualFileSystem(int UUID, int MasterUUID, bool Locked, int MaxFileName, int MaxFileLength)
    {
        this.UUID = UUID;
        this.MasterUUID = MasterUUID;
        this.Locked = Locked;
        MaxFileNameLength = MaxFileName;
        MaxFileContentLength = MaxFileLength;

        if (MasterUUID != 0)
        {
            NextFileID = VFSManager.Instance.GetOrCreateVFS(MasterUUID).NextFileID;
        };

    }


    #region InternalFunctions
    /*********************************************
        INTERNAL FUNCTIONS
    *********************************************/
    protected int GetNextID()
    {
        NextFileID++;
        return NextFileID;
    }
    #endregion

    #region MasterFunctions
    /*********************************************
        MASTER FUNCTIONS
    *********************************************/
    /* public void MountMasterVFS(int UUID)
    {
        VirtualFileSystem newMaster = VFSManager.Instance.GetOrCreateVFS(UUID);

        if (!newMaster.Locked)
            throw new FileSystemException(UUID, "Master VFS is not locked");

        MasterUUID = UUID;
    } */
    #endregion

    #region Nodes
    /*********************************************
        NODES
    *********************************************/
    protected NodeState NodeGetState(int ID)
    {
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

    protected void NodeGetChildren(int Parent, List<Node> nodes)
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
                throw new FileSystemNodeException(ID, UUID, "Trying to delete a non-existant file");
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
                throw new FileSystemNodeException(ID, UUID, "Trying to delete a non-existant file");
        }

        NodeInfo.node.Value = contents;
        FileTable[ID] = NodeInfo.node;
    }
    #endregion

    #region Public
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
            _ => throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS or any Master"),
        };
    }
    public int FileCreate(int Parent, string Name)
    {
        //Enforce name length
        if (Name.Length > MaxFileNameLength)
            Name = Name.Substring(0, MaxFileNameLength);

        return NodeCreate(NodeType.File, Parent, Name);
    }

    public int DirCreate(int Parent, string Name)
    {
        //Enforce name length
        if (Name.Length > MaxFileNameLength)
            Name = Name.Substring(0, MaxFileNameLength);

        return NodeCreate(NodeType.Directory, Parent, Name);
    }

    public void FileRemove(int ID) => NodeDelete(ID);
    public void DirRemove(int ID) => NodeDeleteTree(ID);

    public void Rename(int ID, string Name)
    {
        //Enforce name length
        if (Name.Length > MaxFileNameLength)
            Name = Name.Substring(0, MaxFileNameLength);

        NodeRename(ID, Name);
    }

    public void FileRead(int ID) => NodeOpen(ID);
    public void FileWrite(int ID, string contents)
    {
        if (contents.Length > MaxFileContentLength)
            NodeWrite(ID, contents.Substring(0, MaxFileContentLength));
        else
            NodeWrite(ID, contents);
    }
    #endregion
}