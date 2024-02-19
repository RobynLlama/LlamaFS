using System;
using System.Collections.Generic;

namespace LlamaFS.LOG;

public class LogManager
{
    //I LOVE SINGLETONS
    public static LogManager Instance { get; } = new();
    protected List<ILogWriter> logWriters = new();

    public LogManager() { }

    internal void WriteToLogs(LogLevel level, string message)
    {
        foreach (ILogWriter log in logWriters)
        {
            log.Log(level, message);
        }
    }

    public void RegisterLogWriter(ILogWriter writer)
    {
        if (logWriters.Contains(writer))
        {
            return;
        }

        logWriters.Add(writer);
    }
}