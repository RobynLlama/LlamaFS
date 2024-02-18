using System.Collections.Generic;

namespace LlamaFS.VFS;

public class VFSManager
{
    //I LOVE SINGLETONS
    public static VFSManager Instance;
    protected Dictionary<string, VirtualFileSystem> AllVFS = new();

    public VFSManager()
    {

    }

    public VirtualFileSystem GetOrCreateVFS(string UUID)
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