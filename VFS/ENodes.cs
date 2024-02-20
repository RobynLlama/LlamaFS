namespace LlamaFS.VFS;

public enum NodeState
{
    Root,
    Null,
    Deleted,
    Local,
    Master
};

/// <summary>
/// IO - Sets cursor to start of file
/// Append - Sets cursor to end of file
/// Overwrite - Clears file, sets cursor to start
/// </summary>
public enum NodeFileMode
{
    IO,
    Append,
    Overwrite
}