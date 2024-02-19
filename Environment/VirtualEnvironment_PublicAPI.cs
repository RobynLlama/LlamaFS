using System.Collections.Generic;

namespace LlamaFS.ENV;
public partial class VirtualEnvironment
{
    public string GetEnvVariable(string name)
    {
        if (variables.ContainsKey(name))
            return variables[name];
        else
            return "";
    }

    public void SetEnvVariable(string name, string value)
    {
        if (variables.ContainsKey(name))
            variables[name] = value;
        else
            variables.Add(name, value);
    }

    public bool MountPrimaryFilesystem(int UUID)
    {
        //Check if we're mounting the same FS
        if (PrimaryVFS == UUID)
            return false;

        //Unmount the FS if its already in our list to avoid duplicates
        UnmountFilesystem(UUID);

        //Set it to primary
        PrimaryVFS = UUID;
        return true;
    }

    public bool MountFilesystem(int UUID)
    {
        //Exit if we already have this FS in our list or primary
        if (MountedVFS.Contains(UUID) || PrimaryVFS == UUID)
        {
            return false;
        }

        MountedVFS.Add(UUID);
        return true;
    }

    public bool UnmountFilesystem(int UUID)
    {
        //Only Unmount FS that are not primary and in our list
        if (PrimaryVFS != UUID)
            if (MountedVFS.Contains(UUID))
            {
                MountedVFS.Remove(UUID);
                return true;
            }

        return false;
    }

}