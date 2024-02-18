using System.Collections.Generic;

namespace LlamaFS.ENV;

public class EnvironmentManager
{
    //I LOVE SINGLETONS
    public static EnvironmentManager Instance;
    protected Dictionary<string, VirtualEnvironment> AllEnvs = new();

    public EnvironmentManager()
    {
        Instance = this;
    }

    public VirtualEnvironment GetOrCreateEnvironment(string UUID)
    {

        if (AllEnvs.ContainsKey(UUID))
        {
            //Todo: logging callback
            return AllEnvs[UUID];
        }

        //Todo: Logging callback

        VirtualEnvironment temp = new(UUID);
        AllEnvs.Add(UUID, temp);
        return temp;
    }
}