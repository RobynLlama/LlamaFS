using System.Collections.Generic;

namespace LlamaFS.VFS;

public class VFSManager
{
    //I LOVE SINGLETONS
    public static VFSManager Instance { get; } = new();
    protected Dictionary<int, VirtualFileSystem> AllVFS = new();

    public VFSManager() { }

    public VirtualFileSystem GetOrCreateVFS(int UUID)
    {
        if (AllVFS.ContainsKey(UUID))
        {
            //Todo: logging callback
            return AllVFS[UUID];
        }

        VirtualFileSystem NewVFS = new(UUID);
        //Todo: logging callback

        AllVFS.Add(UUID, NewVFS);
        return NewVFS;
    }
}