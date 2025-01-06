using LlamaFS.VFS;

namespace LlamaFS.EXT;

public static class NodeStateExt
{
    public static bool IsNullOrDeleted(this NodeState state)
    {
        return state == NodeState.Null || state == NodeState.Deleted;
    }
}
