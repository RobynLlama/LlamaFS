using LlamaFS.VFS;

namespace LlamaFS.EXT;

public static class NodeStateExt
{
    public static bool IsNullorDeleted(this VirtualFileSystem.NodeState state)
    {
        return state == VirtualFileSystem.NodeState.Null || state == VirtualFileSystem.NodeState.Deleted;
    }
}