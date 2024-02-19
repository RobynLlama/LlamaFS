using System.Collections.Generic;

namespace LlamaFS.ENV;
public partial class VirtualEnvironment
{
    protected readonly List<int> MountedVFS = new();
    public int PrimaryVFS { get; protected set; }
    public int UUID { get; }
    protected Dictionary<string, string> variables = new()
    {
        {"$HOME",""},
        {"$CWD",@"\"}
    };

    public VirtualEnvironment(int UUID)
    {
        this.UUID = UUID;
    }
}