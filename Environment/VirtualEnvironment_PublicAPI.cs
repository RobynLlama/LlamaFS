using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LlamaFS.EXT;
using LlamaFS.LOG;
using LlamaFS.VFS;

namespace LlamaFS.ENV;
public partial class VirtualEnvironment
{

    public void Save(Stream output)
    {
        StreamWriter stream = new(output);

        string mounts = string.Empty;

        foreach (int key in MountedVFS.Keys)
        {
            mounts += $"{key},{MountedVFS[key]}";
        }

        string vars = string.Empty;

        foreach (string key in variables.Keys)
        {
            vars += $"{key},{variables[key]}";
        }

        //stream.WriteLine($"ENV:{UUID}:{rootVFS}:{mounts}:{vars}");
        stream.WriteLine($"ENV:{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UUID}:{rootVFS}:{mounts}:{vars}"))}");
        stream.Flush();
    }

    #region Env
    public string GetEnvVariable(string name)
    {
        if (variables.ContainsKey(name))
            return variables[name];
        else
            return string.Empty;
    }

    public void SetEnvVariable(string name, string value)
    {
        if (variables.ContainsKey(name))
            variables[name] = value;
        else
            variables.Add(name, value);
    }

    public void ResolveEnvVariables(ref string value)
    {
        foreach (string key in variables.Keys)
        {
            value = value.Replace(key, variables[key]);
        }
    }

    public void ResolvePath(ref string path)
    {
        //Console.WriteLine($"Start Path: {path}");
        ResolveEnvVariables(ref path);

        //check if path is relative
        if (path[0] != '/')
        {
            path = GetEnvVariable("$CWD") + path;
        }

        string[] strings = path.Split('/');
        path = "/";

        Stack<string> list = new();

        string prev;

        foreach (string item in strings)
        {
            if (string.IsNullOrEmpty(item))
                continue;

            switch (item)
            {
                case ".":
                    continue;
                case "..":
                    //delete the previous entry
                    if (list.Count == 0)
                        continue;

                    prev = list.Pop();
                    path = path.Substring(0, path.Length - (prev.Length + 1));
                    break;
                default:
                    //Add an entry to the path
                    //Console.WriteLine($"Pushing {item}");
                    list.Push(item);
                    path += $"{item}/";
                    break;
            }

            //Just to be sure :)
            if (path[^1] != '/')
                path += "/";
        }
        //LogManager.Instance.WriteToStream(LogLevel.Info, $"Resolved: {path}");
    }
    #endregion

    #region File Commands
    public void ListDirectory(string Path, List<Node> children)
    {
        ResolvePath(ref Path);
        var directory = GetNodeFromPath(Path);

        //Console.WriteLine($"Listing: {Path}\nNodeID: {directory.node.UUID}");

        //Only operate on directories
        if (directory.node.nodeType != NodeType.Directory)
            return;

        VFSManager.Instance.Get(directory.vfs).NodeGetChildren(directory.node.UUID, children);
    }

    public bool MakeFile(string Path) => MakeNode(NodeType.File, Path);
    public bool MakeDirectory(string Path) => MakeNode(NodeType.Directory, Path);
    /* public bool FileWrite(string Path, string Content)
    {
        var info = GetNodeFromPath(Path);

        if (info.state.IsNullorDeleted())
        {
            return false;
        }

        ResolveMountedVFS(Path).vfs.FileWrite(info.node.UUID, Content);
        return true;

    } */
    public VFSFileStream FileOpen(string Path, NodeFileMode mode)
    {
        var info = GetNodeFromPath(Path);

        if (info.state.IsNullOrDeleted())
        {
            throw new FileSystemException(info.vfs, "Unable to open file for reading");
        }

        return VFSManager.Instance.Get(info.vfs).FileOpen(info.node.UUID, mode);
    }

    public (NodeState state, NodeType type) StatPathNode(string Path)
    {
        var info = GetNodeFromPath(Path);
        return (info.state, info.node.nodeType);
    }

    public bool DeleteFile(string Path)
    {
        var info = GetNodeFromPath(Path);

        if (info.state.IsNullOrDeleted())
            return false;

        if (info.node.nodeType != NodeType.File)
            return false;

        VFSManager.Instance.Get(info.vfs).FileRemove(info.node.UUID);
        return true;
    }

    public bool DeleteDirectory(string Path)
    {
        var info = GetNodeFromPath(Path);

        if (info.state.IsNullOrDeleted())
            return false;

        if (info.node.nodeType != NodeType.Directory)
            return false;

        VFSManager.Instance.Get(info.vfs).DirRemove(info.node.UUID);
        return true;
    }
    #endregion

    #region VFS Operations
    //Todo: Implement checking for mount points
    /* public (VirtualFileSystem vfs, string path) ResolveMountedVFS(string Path)
    {
        int size = 0;
        string mount_point = string.Empty;
        int ID = 0;

        foreach (int vfs in MountedVFS.Keys)
        {
            if (Path.StartsWith(MountedVFS[vfs]))
            {
                if (MountedVFS[vfs].Length > size)
                {
                    size = MountedVFS[vfs].Length;
                    mount_point = Path.Replace(MountedVFS[vfs], "");
                    ID = vfs;
                }
            }
        }
    } */

    public bool MountFilesystem(int UUID, string path)
    {

        //Resolve path
        ResolvePath(ref path);

        //Mount to primary if we target root
        if (path == "/")
        {
            rootVFS = UUID;
            return true;
        }

        //Exit if we already have this FS in our list
        if (MountedVFS.ContainsKey(UUID))
        {
            return false;
        }

        //Exit if this exact path is already used
        foreach (int key in MountedVFS.Keys)
        {
            if (MountedVFS[key] == path)
                return false;
        }

        var info = GetNodeFromPath(path);

        switch (info.state)
        {
            case NodeState.Null:
            case NodeState.Deleted:
                //Clear to add a new link
                string ParentPath = path + "/..";
                ResolvePath(ref ParentPath);

                var parentInfo = GetNodeFromPath(ParentPath);

                if (parentInfo.state.IsNullOrDeleted())
                    return false;

                string childName = path.Replace(ParentPath, "");

                VFSManager.Instance.Get(info.vfs).LinkCreate(parentInfo.node.Parent, childName, UUID, 0);
                MountedVFS.Add(UUID, path);

                return true;
            case NodeState.Local:
            case NodeState.Master:
                //Check if the file is a directory
                if (info.node.nodeType != NodeType.Directory)
                {
                    return false;
                }

                VFSManager.Instance.Get(info.vfs).LinkUpdate(info.node.UUID, UUID, 0, NodeType.Link);
                MountedVFS.Add(UUID, path);

                return true;
            default:
                return false;
        }

    }

    public bool UnmountFilesystem(int UUID)
    {
        if (!MountedVFS.ContainsKey(UUID))
        {
            return false;
        }

        string unmountPath = MountedVFS[UUID];

        foreach (int key in MountedVFS.Keys)
        {
            if (MountedVFS[key].StartsWith(unmountPath))
                if (MountedVFS[key] != unmountPath)
                {
                    LogManager.Instance.WriteToStream(LogLevel.Warn, "Unable to unmount {UUID} because another FS is mounted below its root");
                    return false;
                }
        }

        var info = GetNodeFromPath(unmountPath);

        //Probably should be safer here but w/e
        VFSManager.Instance.Get(info.vfs).LinkUpdate(info.node.UUID, 0, 0, NodeType.Directory);
        //Remove from mounted list
        MountedVFS.Remove(UUID);

        return true;
    }
    #endregion

}
