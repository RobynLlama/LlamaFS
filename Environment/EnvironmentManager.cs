using System;
using System.Collections.Generic;
using System.IO;

namespace LlamaFS.ENV;

public class EnvironmentManager
{
    //I LOVE SINGLETONS
    public static EnvironmentManager Instance { get; } = new();

    protected Dictionary<int, VirtualEnvironment> AllEnvs = new();

    public EnvironmentManager() { }

    public VirtualEnvironment Create(int UUID)
    {
        if (AllEnvs.ContainsKey(UUID))
        {
            throw new ArgumentException($"Duplicate UUID in EnvManager {UUID}", "UUID");
        }

        VirtualEnvironment env = new(UUID);
        AllEnvs.Add(UUID, env);
        return env;
    }

    public VirtualEnvironment Get(int UUID)
    {
        if (!AllEnvs.ContainsKey(UUID))
        {
            throw new ArgumentException($"UUID not in EnvManager {UUID}", "UUID");
        }

        return AllEnvs[UUID];
    }

    public void Save(Stream output)
    {
        foreach (int key in AllEnvs.Keys)
        {
            AllEnvs[key].Save(output);
        }
    }
}