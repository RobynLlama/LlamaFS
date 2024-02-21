using System;
using System.Collections.Generic;
using System.IO;

namespace LlamaFS.VFS;

public class VFSManager
{
    //I LOVE SINGLETONS
    public static VFSManager Instance { get; } = new();
    protected Dictionary<int, VirtualFileSystem> AllVFS = new();

    public VFSManager() { }

    public VirtualFileSystem Create(int UUID, int Master)
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

    public VirtualFileSystem Get(int UUID)
    {
        if (AllVFS.ContainsKey(UUID))
        {
            return AllVFS[UUID];
        }

        throw new ArgumentOutOfRangeException("UUID", $"The UUID does not exist in VFS List {UUID}");
    }

    public void AddVFS(VirtualFileSystem vfs)
    {
        if (AllVFS.ContainsKey(vfs.UUID))
            return;

        AllVFS.Add(vfs.UUID, vfs);
    }

    public void Save(Stream output)
    {
        foreach (int key in AllVFS.Keys)
        {
            AllVFS[key].Save(output);
        }
    }
}