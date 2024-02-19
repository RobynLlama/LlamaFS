using System;
using System.Collections.Generic;

namespace LlamaFS.Command;
public class CommandDictionary
{
    private readonly Dictionary<string, Type> _dict = new Dictionary<string, Type>();

    public void Add(string key, Type value)
    {
        _dict.Add(key, value);
    }

    public Type GetValue(string key)
    {
        return _dict[key];
    }
}

public class CommandNotFoundException : Exception
{
    public CommandNotFoundException(string message) : base(message)
    {
    }
}