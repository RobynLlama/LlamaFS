using System;
using System.Collections.Generic;

namespace LlamaFS.Command;
public class CommandDictionary
{
    private Dictionary<string, object> _dict = new Dictionary<string, object>();

    public void Add<T>(string key, T value) where T : TerminalCommand
    {
        _dict.Add(key, value);
    }

    public T GetValue<T>(string key) where T : TerminalCommand
    {
        return _dict[key] as T;
    }
}

public class CommandNotFoundException : Exception
{
    public CommandNotFoundException(string message) : base(message)
    {
    }
}