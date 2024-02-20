using System.Collections;
using System.Threading;

namespace LlamaFS.Command.Interrupts;

public class WaitForCycles : IEnumerator
{
    public object Current { get { return Frames; } }
    protected int Frames;
    private int StartFrames;

    public WaitForCycles(int milliseconds)
    {
        Frames = milliseconds;
        StartFrames = Frames;
    }

    public bool MoveNext()
    {
        Thread.Sleep(1);
        return Frames-- > 0;
    }

    public void Reset()
    {
        Frames = StartFrames;
    }
}