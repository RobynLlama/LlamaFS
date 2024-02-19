namespace LlamaFS.LOG;

public interface ILogWriter
{
    void Log(LogLevel level, string message);
}