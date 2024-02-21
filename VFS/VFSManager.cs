using System;
using System.Collections.Generic;

namespace LlamaFS.VFS;

public class VFSManager
{
    //I LOVE SINGLETONS
    public static VFSManager Instance { get; } = new();
    protected Dictionary<int, VirtualFileSystem> AllVFS = new();

    public VFSManager() { }

    public VirtualFileSystem CreateVFS(int UUID, int Master, int MaxFileName, int MaxFileContent)
    {
        if (AllVFS.ContainsKey(UUID))
        {
            throw new ArgumentException("UUID already exists in VFS list, cannot create", "UUID");
        }

        VirtualFileSystem NewVFS = new(UUID, Master);
        //Todo: logging callback

        AllVFS.Add(UUID, NewVFS);
        return NewVFS;
    }

    public VirtualFileSystem GetVFS(int UUID)
    {
        if (AllVFS.ContainsKey(UUID))
        {
            return AllVFS[UUID];
        }

        throw new ArgumentOutOfRangeException("UUID", "The UUID does not exist in VFS List");
    }

    public void AddVFS(VirtualFileSystem vfs)
    {
        if (AllVFS.ContainsKey(vfs.UUID))
            return;

        AllVFS.Add(vfs.UUID, vfs);
    }
}