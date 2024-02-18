using System;

namespace LlamaFS.VFS;

public class FileSystemException : Exception
{
    public readonly int FileSystemUUID;
    public FileSystemException(int VFS, string Message) : base(Message)
    {
        FileSystemUUID = VFS;
    }

    public FileSystemException(int VFS, string Message, Exception exception) : base(Message, exception)
    {
        FileSystemUUID = VFS;
    }

}
public class FileSystemNodeException : FileSystemException
{
    public readonly int NodeUUID;

    public FileSystemNodeException(int Node, int VFS, string Message) : base(VFS, Message)
    {
        NodeUUID = Node;
    }

    public FileSystemNodeException(int Node, int VFS, string Message, Exception exception) : base(VFS, Message, exception)
    {
        NodeUUID = Node;
    }

}