using System;
using System.Collections.Generic;

namespace LlamaFS.LOG;

public class LogManager
{
    //I LOVE SINGLETONS
    public static LogManager Instance { get; } = new();
    protected Dictionary<LogStream, List<ILogWriter>> LogRegistry = new();

    public LogManager()
    {
        //Init registry
        LogRegistry.Add(LogStream.Logging, new());
        LogRegistry.Add(LogStream.Output, new());
    }

    internal void WriteToStream(LogLevel level, string message, LogStream stream = LogStream.Logging)
    {
        foreach (ILogWriter log in LogRegistry[stream])
        {
            log.Log(level, message);
        }
    }

    public void RegisterLogWriterToStream(LogStream stream, ILogWriter writer)
    {
        if (LogRegistry[stream].Contains(writer))
        {
            return;
        }

        LogRegistry[stream].Add(writer);
    }
}