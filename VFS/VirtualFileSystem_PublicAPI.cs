namespace LlamaFS.VFS;

public partial class VirtualFileSystem
{
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

    public void FileRead(int ID) => NodeOpen(ID);
    public void FileWrite(int ID, string contents)
    {
        if (contents.Length > MaxFileContentLength)
            NodeWrite(ID, contents.Substring(0, MaxFileContentLength));
        else
            NodeWrite(ID, contents);
    }
}