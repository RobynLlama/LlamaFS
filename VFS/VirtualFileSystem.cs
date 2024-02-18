using System.Collections.Generic;
using LlamaFS.VFS.Nodes;

namespace LlamaFS.VFS;
public class VirtualFileSystem
{

    private int NextFileID = 0;
    protected Dictionary<int, Node> FileTable = new();
    protected List<int> DeletedRecords = new();
    protected readonly int MaxFileSize = 350;
    protected readonly string UUID;
    protected readonly string MasterUUID;
    protected readonly bool Locked;

    public VirtualFileSystem(string UUID, string MasterUUID = "_", bool Locked = false)
    {
        this.UUID = UUID;
        this.MasterUUID = MasterUUID;
        this.Locked = Locked;

        if (MasterUUID != "_")
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
    protected T NodeGetMaster<T>(int ID) where T : Node
    {
        if (MasterUUID == "_")
            throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS");

        //Search for node by ID on the master
        return VFSManager.Instance.GetOrCreateVFS(MasterUUID).NodeGet<T>(ID, true);
    }
    #endregion

    #region Nodes
    /*********************************************
        NODES
    *********************************************/
    /// <summary>
    /// Gets a node from the current FS, optionally from Masters as well
    /// </summary>
    /// <typeparam name="T">A class derived from Node</typeparam>
    /// <param name="ID">The node ID</param>
    /// <param name="AllowMasters">If we should search masters</param>
    /// <returns></returns>
    public T NodeGet<T>(int ID, bool AllowMasters) where T : Node
    {
        //Does the node exist?
        if (!FileTable.ContainsKey(ID))
            //Explicitly deleted nodes do not fetch from master
            if (NodeIsDeleted(ID))
                throw new FileSystemNodeException(ID, UUID, "Node is explicitly deleted on VFS");
            //Only fetch from masters when allowed
            else if (AllowMasters)
                return NodeGetMaster<T>(ID);
            else
                throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS");

        //Return the node
        return FileTable[ID] as T;
    }

    public bool NodeIsDeleted(int ID)
    {
        return DeletedRecords.Contains(ID);
    }

    /// <summary>
    /// Deletes a node on the local FS (recursively for Dirs) or marks a
    /// masterFS node as explicitly deleted
    /// </summary>
    /// <param name="ID">Node ID of the file to be deleted</param>
    public void NodeDelete(int ID)
    {
        //Sanity: Is this node already explicitly deleted?
        if (DeletedRecords.Contains(ID))
        {
            throw new FileSystemNodeException(ID, UUID, "Node is explicitly deleted on VFS");
        }

        Node node;

        //Does the node exist on our LocalFS?
        node = NodeGet<Node>(ID, false);

        if (node is null)
        {
            //Master FS may have this file
            node = NodeGet<Node>(ID, true);

            //Master FS has this file
            if (node is not null)
            {
                //Mark this node as deleted
                DeletedRecords.Add(node.UUID);
            }

            throw new FileSystemNodeException(ID, UUID, "Node does not exist on VFS");
        }

        //Local FS has this file
        //Mark this node as deleted
        DeletedRecords.Add(node.UUID);

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

        if (node.Parent != 0)
        {
            DirNode parent = NodeGet<DirNode>(node.Parent, false);

            if (parent == null)
                return;

            //Remove from Parent
            parent.Children.Remove(node);
        }

        return;

    }
    #endregion

    #region DirNodes
    /*********************************************
        DIR NODES
    *********************************************/
    /* public int DirNodeCreate(int ParentID, string Name)
    {
        //Get the parent node from our FS only
        DirNode node = NodeGet<DirNode>(ParentID, false);
        NodeDelete
    } */
    #endregion

    #region FileNodes
    /*********************************************
        FILE NODES
    *********************************************/
    public int FileNodeCreate(int Parent, string Name, bool Binary = false)
    {
        //Sanity: Does the parent exist?
        if (!FileTable.ContainsKey(Parent))
        {
            throw new FileSystemNodeException(Parent, UUID, "Node parent does not exist");
        }

        //Sanity: Is the parent a DirNode?
        if (FileTable[Parent] is not DirNode parent)
        {
            throw new FileSystemNodeException(Parent, UUID, "Node parent is not assignable from DirNode");
        }

        //Sanity: Does the DirNode already contain something with this name?
        foreach (Node item in parent)
        {
            if (item.Name == Name)
                if (item.nodeType == NodeType.File)
                    return item.UUID;
                else
                    throw new FileSystemNodeException(Parent, UUID, $"Node parent already contains a child with name {Name} ");
        }

        int fileID = GetNextID();

        Node NewNode = new FileNode(Parent, Name, Binary, fileID);

        FileTable.Add(fileID, NewNode);
        parent.Children.Add(NewNode);

        return fileID;
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