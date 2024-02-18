using System.Collections.Generic;

namespace LlamaFS.VFS;
public class VirtualFileSystem
{

    private int NextFileID = 0;
    protected Dictionary<int, Node> FileTable = new();
    protected List<int> DeletedRecords = new();
    public int MaxFileSize { get; } = 350;
    public int UUID { get; }
    public int MasterUUID { get; protected set; }
    public bool Locked { get; protected set; }
    public enum NodeState
    {
        Null,
        Deleted,
        Local,
        Master
    };

    public VirtualFileSystem(int UUID, int MasterUUID = 0, bool Locked = false)
    {
        this.UUID = UUID;
        this.MasterUUID = MasterUUID;
        this.Locked = Locked;

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
    public void MountMasterVFS(int UUID)
    {
        VirtualFileSystem newMaster = VFSManager.Instance.GetOrCreateVFS(UUID);

        if (!newMaster.Locked)
            throw new FileSystemException(UUID, "Master VFS is not locked");

        MasterUUID = UUID;
    }
    #endregion

    #region Nodes
    /*********************************************
        NODES
    *********************************************/
    public (NodeState, Node) NodeGetState(int ID)
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

    /// <summary>
    /// Gets a node from the current FS, optionally from Masters as well
    /// </summary>
    /// <typeparam name="T">A class derived from Node</typeparam>
    /// <param name="ID">The node ID</param>
    /// <param name="AllowMasters">If we should search masters</param>
    /// <returns></returns>
    public T NodeGet<T>(int ID) where T : Node
    {
        NodeState state = NodeGetState(ID);

        return state switch
        {
            NodeState.Local => (T)FileTable[ID],
            NodeState.Master => VFSManager.Instance.GetVFS(MasterUUID).NodeGet<T>(ID),
            _ => throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS or any Master"),
        };
    }

    /* public bool NodeIsDeleted(int ID)
    {
        return DeletedRecords.Contains(ID);
    } */

    /// <summary>
    /// Deletes a node on the local FS (recursively for Dirs) or marks a
    /// masterFS node as explicitly deleted
    /// </summary>
    /// <param name="ID">Node ID of the file to be deleted</param>
    public void NodeDelete(int ID)
    {
        //Where does the node live?
        NodeState state = NodeGetState(ID);

        switch (state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(ID, UUID, "Node is explicitly deleted on VFS");
            case NodeState.Local:
                Node node = NodeGet<Node>(ID);
                //Add to deleted records
                DeletedRecords.Add(ID);

                //If the node is a Directory, delete its children
                if (node is DirNode dirNode)
                {
                    foreach (Node child in dirNode)
                    {
                        NodeDelete(child.UUID);
                    }
                }

                //Remove from FileTable
                FileTable.Remove(node.UUID);

                //Remove from Parent
                if (node.Parent != 0)
                {
                    if (NodeGetState(node.Parent) != NodeState.Local)
                        throw new FileSystemNodeException(node.Parent, UUID, "Node parent not on same VFS as child.");

                    DirNode parent = NodeGet<DirNode>(node.Parent);

                    parent.Children.Remove(node);
                }

                break;
            case NodeState.Master:
                //It only exists on a master so just mark it deleted
                DeletedRecords.Add(ID);
                break;
        }

    }
    #endregion

    #region DirNodes
    /*********************************************
        DIR NODES
    *********************************************/
    public int DirNodeCreate(int ParentID, string Name)
    {
        return 0;
    }
    #endregion

    #region FileNodes
    /*********************************************
        FILE NODES
    *********************************************/
    public int FileNodeCreate(int Parent, string Name, bool Binary = false)
    {
        NodeState parentState = NodeGetState(Parent);

        switch (parentState)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                throw new FileSystemNodeException(Parent, UUID, "Parent does not exist");
        }

        Node parentNode = NodeGet<Node>(Parent);

        //Sanity: Is parent a dir
        if (parentNode is not DirNode)
            throw new FileSystemNodeException(Parent, UUID, "Parent is not assignable from DirNode");

        int existingID = -1;

        //Sanity: Does this filename already exist?
        foreach (Node item in (DirNode)parentNode)
        {
            if (item.Name == Name)
            {
                existingID = item.UUID;

                if (item.nodeType != NodeType.File)
                    throw new FileSystemNodeException(item.UUID, UUID, "Existing child is not a file type node");
            }
        }

        //Create parent if needed
        if (parentState != NodeState.Local)
            NodeGet<DirNode>(DirNodeCreate(parentNode.Parent, parentNode.Name));

        //If the child already exists
        if (existingID > 0)
        {

            FileNode existingChild = NodeGet<FileNode>(existingID);

            switch (NodeGetState(existingID))
            {
                case NodeState.Local:
                    throw new FileSystemNodeException(existingID, UUID, "File already exists on local VFS");
                case NodeState.Master:
                    //The existing node is a file on a master so copy its properties
                    FileNode newNode = new(Parent, Name, existingChild.Binary, existingChild.UUID);
                    //Add the parent to the local FS and add ourself as a child;

                    return newNode.UUID;
            }

        }
        //The child does not already exist
        else
        {

        }
    }

    public int FileNodeWrite(int ID, string Contents)
    {
        //Sanity: Does the file exist?
        if (!FileTable.ContainsKey(ID))
        {
            throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS");
        }

        //Sanity: Is the node a FileNode?
        if (FileTable[ID] is not FileNode file)
        {
            throw new FileSystemNodeException(ID, UUID, "Node is not a directory");
        }

        //Check that the contents aren't too big
        if (Contents.Length > MaxFileSize)
            Contents = Contents.Substring(0, MaxFileSize);

        file.Contents = Contents;

        return 1;
    }
    #endregion
}