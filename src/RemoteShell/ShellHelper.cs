using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization.Formatters.Binary;
using RemoteShell.Core;

namespace RemoteShell;

public class ShellHelper
{
    private static int _identifier;
    private static int Identifier => Interlocked.Increment(ref _identifier);

    public static Message PropertyGetter(string name, out string id)
    {
        id = Identifier.ToString();
        
        return new Message(new[]
        {
            $"ID:{id}",
            $"PROPERTY:{name}",
            "GET"
        }, new MessageBody(name));
    }

    public static Message PropertySetter<T>(string name, T value, out string id)
    {
        id = Identifier.ToString();
        
        using var stream = new MemoryStream();
        if (value is not null) new BinaryFormatter().Serialize(stream, value);

        return new Message(new[]
        {
            $"ID:{id}",
            $"PROPERTY:{name}",
            "SET"
        }, new MessageBody(stream.ToArray()));
    }
}