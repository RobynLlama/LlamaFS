using System.Collections;
using System.Collections.Generic;
using LlamaFS.VFS;

namespace LlamaFS.ENV;
public partial class VirtualEnvironment
{

    #region Env
    public string GetEnvVariable(string name)
    {
        if (variables.ContainsKey(name))
            return variables[name];
        else
            return name;
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
    }
    #endregion

    #region File Commands
    public void ListDirectory(string Path, List<Node> children)
    {
        var directory = GetNodeFromPath(Path);

        //Console.WriteLine($"Listing: {Path}\nNodeID: {directory.node.UUID}");

        //Only operate on directories
        if (directory.node.nodeType != NodeType.Directory)
            return;

        ResolveMountedVFS(Path).vfs.NodeGetChildren(directory.node.UUID, children);
    }

    public bool MakeDirectory(string Path)
    {
        string ParentPath = Path + "..";
        string Name;
        ResolvePath(ref ParentPath);

        Name = Path.Replace(ParentPath, "").Replace("/", "");
        VirtualFileSystem vfs;

        (vfs, ParentPath) = ResolveMountedVFS(ParentPath);

        //Console.WriteLine($"Parent: {ParentPath} Name: {Path}");

        var info = GetNodeFromPath(ParentPath);
        var child = GetNodeFromPath(Path);

        if (info.state.IsNullorDeleted())
        {
            return false;
        }

        if (!child.state.IsNullorDeleted())
        {
            return false;
        }

        vfs.DirCreate(info.node.UUID, Name);

        return true;
    }

    public (Node node, VirtualFileSystem.NodeState state) StatPathNode(string Path)
    {
        return GetNodeFromPath(Path);
    }
    #endregion

    #region VFS Operations
    //Todo: Implement checking for mount points
    public (VirtualFileSystem vfs, string path) ResolveMountedVFS(string Path)
    {
        return (VFSManager.Instance.GetVFS(PrimaryVFS), Path);
    }
    public bool MountPrimaryFilesystem(int UUID)
    {
        //Check if we're mounting the same FS
        if (PrimaryVFS == UUID)
            return false;

        //Unmount the FS if its already in our list to avoid duplicates
        UnmountFilesystemByVFS(UUID);

        //Set it to primary
        PrimaryVFS = UUID;
        return true;
    }

    public bool MountFilesystem(int UUID, string path)
    {
        //Exit if we already have this FS in our list or primary
        if (PrimaryVFS == UUID || MountedVFS.ContainsValue(UUID) || MountedVFS.ContainsKey(path))
        {
            return false;
        }

        MountedVFS.Add(path, UUID);
        return true;
    }

    public bool UnmountFilesystemByPath(string path)
    {
        //Only unmount FS that are in our mount list
        if (MountedVFS.ContainsKey(path))
        {
            MountedVFS.Remove(path);
            return true;
        }

        return false;
    }

    public bool UnmountFilesystemByVFS(int UUID)
    {
        if (PrimaryVFS == UUID)
            return false;

        if (MountedVFS.ContainsValue(UUID))
        {

            IEnumerator keys = MountedVFS.Keys.GetEnumerator();

            while (keys.MoveNext())
            {
                string key = (string)keys.Current;

                if (MountedVFS[key] == UUID)
                {
                    MountedVFS.Remove(key);
                }
            }

            return true;
        }

        return false;
    }
    #endregion

}