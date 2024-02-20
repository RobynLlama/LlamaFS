using System;
namespace LlamaFS.Command;

public class CommandNotFoundException : Exception
{
    public CommandNotFoundException(string message) : base(message)
    {
    }
}