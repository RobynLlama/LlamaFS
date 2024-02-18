using System.Collections.Generic;
using LlamaFS.VFS;
using UnityEngine;

namespace LlamaFS.ENV;
public class VirtualEnvironment
{
    public int HDD1 { get; }
    public int HDD2 { get; }
    public string CaseUUID;
    protected Dictionary<string, string> variables = new()
    {
        {"$HOME",""},
        {"$CWD",@"\"}
    };

    public VirtualEnvironment(string UUID)
    {
        CaseUUID = UUID;
        HDD1 = (UUID + "_HDD1").GetHashCode();
        HDD2 = (UUID + "_HDD2").GetHashCode();
    }

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

}