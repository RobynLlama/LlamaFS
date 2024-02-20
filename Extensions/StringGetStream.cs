using System.IO;

namespace LlamaFS.EXT;

public static class GetStreamExtension
{
    public static Stream GetStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}