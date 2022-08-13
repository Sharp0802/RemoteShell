using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RemoteShell.Core;

public class MessageBody
{
    public MessageBody()
    {
        IsEmpty = true;
        IsRaw = true;
    }
    
    public MessageBody(string value)
    {
        String = value;
        IsEmpty = false;
        IsRaw = false;
    }

    public MessageBody(byte[] bytes)
    {
        Bytes = bytes;
        IsEmpty = false;
        IsRaw = true;
    }
    
    public bool IsEmpty { get; }
    
    public bool IsRaw { get; }

    public string? String { get; }

    public byte[]? Bytes { get; }

    public static bool TryDecode(string raw, [NotNullWhen(true)] out MessageBody? body)
    {
        if (raw.Length == 0)
        {
            body = new MessageBody();
            return true;
        }

        if (raw.Length < 6)
        {
            body = null;
            return false;
        }

        var rawId = raw[..5];
        var empty = raw.Length == 6;

        if (string.Equals(rawId, "RAW:T:", StringComparison.Ordinal))
        {
            body = empty 
                ? new MessageBody(Array.Empty<byte>()) 
                : new MessageBody(Convert.FromBase64String(raw[6..]));
            return true;
        }
        else if (string.Equals(rawId, "RAW:F:", StringComparison.Ordinal))
        {
            body = empty
                ? new MessageBody(string.Empty)
                : new MessageBody(Encoding.Unicode.GetString(Convert.FromBase64String(raw[6..])));
            return true;
        }
        else
        {
            body = null;
            return false;
        }
    }
    
    public string Encode()
    {
        var builder = new StringBuilder();
        if (!IsEmpty)
        {
            builder.Append(IsRaw ? "RAW:T:" : "RAW:F:")
                   .Append(Convert.ToBase64String(IsRaw ? Bytes! : Encoding.Unicode.GetBytes(String!)));
        }
        return builder.ToString();
    }
}