using System;

namespace LlamaFS.VFS;

public class FileSystemException : Exception
{
    public readonly string FileSystemUUID;
    public FileSystemException(string VFS, string Message) : base(Message)
    {
        FileSystemUUID = VFS;
    }

    public FileSystemException(string VFS, string Message, Exception exception) : base(Message, exception)
    {
        FileSystemUUID = VFS;
    }

}
public class FileSystemNodeException : FileSystemException
{
    public readonly int NodeUUID;

    public FileSystemNodeException(int Node, string VFS, string Message) : base(VFS, Message)
    {
        NodeUUID = Node;
    }

    public FileSystemNodeException(int Node, string VFS, string Message, Exception exception) : base(VFS, Message, exception)
    {
        NodeUUID = Node;
    }

}