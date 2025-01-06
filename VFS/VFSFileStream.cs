using System;
using System.IO;
using LlamaFS.VFS;

public class VFSFileStream(MemoryStream memoryStream, int DiskID, int NodeID) : Stream
{
    private MemoryStream _memoryStream = memoryStream;
    protected readonly int DiskID = DiskID;
    protected readonly int NodeID = NodeID;

    public override bool CanRead => _memoryStream.CanRead;

    public override bool CanSeek => _memoryStream.CanSeek;

    public override bool CanWrite => _memoryStream.CanWrite;

    public override long Length => _memoryStream.Length;

    public override long Position
    {
        get => _memoryStream.Position;
        set => _memoryStream.Position = value;
    }

    public override void Flush()
    {
        _memoryStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _memoryStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _memoryStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _memoryStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _memoryStream.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        //Do not dispose the memory stream
    }

    public override void Close()
    {
        //Do not close the memory stream
    }

    public void DeleteFile()
    {
        VFSManager.Instance.Get(DiskID).FileRemove(NodeID);
        _memoryStream.Dispose();
    }

    internal void RecycleUnderlyingStream()
    {
        _memoryStream.Dispose();
        _memoryStream = new();
    }
}
