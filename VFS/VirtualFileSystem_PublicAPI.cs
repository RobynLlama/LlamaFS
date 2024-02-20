using System.IO;
using LlamaFS.EXT;

namespace LlamaFS.VFS;

public partial class VirtualFileSystem
{
    public void Save(Stream output)
    {
        StreamWriter stream = new(output);

        string delList = string.Empty;

        foreach (int item in DeletedRecords)
        {
            delList += item.ToString();
        }

        stream.WriteLine($"VFS:{NextFileID}:{UUID}:{MasterUUID}:{MaxFileNameLength}:{MaxFileContentLength}:{delList}");
        stream.Flush();

        foreach (Node item in FileTable.Values)
        {
            item.Save(output);
        }
    }
    public int FileCreate(int Parent, string Name)
    {
        //Enforce name length
        if (Name.Length > MaxFileNameLength)
            Name = Name.Substring(0, MaxFileNameLength);

        return NodeCreate(NodeType.File, Parent, Name);
    }

    public int DirCreate(int Parent, string Name)
    {
        //Enforce name length
        if (Name.Length > MaxFileNameLength)
            Name = Name.Substring(0, MaxFileNameLength);

        return NodeCreate(NodeType.Directory, Parent, Name);
    }

    public void FileRemove(int ID) => NodeDelete(ID);
    public void DirRemove(int ID) => NodeDeleteTree(ID);

    public void Rename(int ID, string Name)
    {
        //Enforce name length
        if (Name.Length > MaxFileNameLength)
            Name = Name.Substring(0, MaxFileNameLength);

        NodeRename(ID, Name);
    }

    public MemoryStream FileOpen(int ID, NodeFileMode mode) => NodeOpen(ID, mode);
    /* public void FileWrite(int ID, string contents)
    {
        if (contents.Length > MaxFileContentLength)
            NodeWrite(ID, contents.Substring(0, MaxFileContentLength));
        else
            NodeWrite(ID, contents);
    } */
    public (Node node, NodeState state) GetRaw(int ID) => NodeGet(ID);
}