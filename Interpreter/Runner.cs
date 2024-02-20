using System;
using System.Collections;
using LlamaFS.LOG;

namespace LlamaFS.Command;
public class Runner
{
    public static IEnumerator RunProgram(IEnumerator process)
    {
        while (process.MoveNext())
        {

            if (process.Current is not null)
            {
                if (process.Current.GetType() == typeof(string))
                {
                    LogManager.Instance.WriteToStream(LogLevel.Info, process.Current.ToString(), LogStream.Output);
                }
                else if (typeof(IEnumerator).IsAssignableFrom(process.Current.GetType()))
                {
                    while (RunProgram((IEnumerator)process.Current).MoveNext()) ;
                }
            }

            yield return process.Current;
        }
    }
}