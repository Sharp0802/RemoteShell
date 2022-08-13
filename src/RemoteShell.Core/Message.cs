using System.Text;

namespace RemoteShell.Core;

public class Message
{
    public static Encoding Encoding = Encoding.Unicode;

    public const string Separator = "!!!";

    public Message(string[] header, MessageBody body)
    {
        Header = header;
        Body = body;
    }
    
    public string[] Header { get; }
    public MessageBody Body { get; }
    
    public static bool TryDecode(byte[] raw, out Message? msg)
    {
        var rawStr = Encoding.GetString(raw).Split("!!!");
        if (rawStr.Length <= 0)
        {
            msg = null;
            return false;
        }

        if (!int.TryParse(rawStr[0], out var nHeader))
            nHeader = 0;
        
        if (rawStr.Length == 1) // only contains header length
        {
            if (nHeader == 0) // no header/body detected
                msg = new Message(Array.Empty<string>(), new MessageBody());
            else // header/body count mismatched
                msg = null;
        }
        else
        {
            if (nHeader == rawStr.Length - 1) // only contains header
            {
                msg = new Message(rawStr[1..], new MessageBody());
            }
            else if (nHeader == 0 /* 0 equals (rawStr.Length - 2) */) // only contains body
            {
                if (MessageBody.TryDecode(rawStr[1], out var body))
                    msg = new Message(Array.Empty<string>(), body);
                else
                    msg = null;
            }
            else if (nHeader == rawStr.Length - 3) // contains both of header and body
            {
                if (MessageBody.TryDecode(rawStr[^1], out var body))
                    msg = new Message(rawStr[1..^2], body);
                else
                    msg = null;
            }
            else // header/body count mismatched
            {
                msg = null;
            }
        }

        return msg is not null;
    }

    public byte[] Encode()
    {
        var builder = new StringBuilder();
        builder.Append(Header.Length.ToString());
        if (Header.Length > 0)
            builder.Append(Separator);
        foreach (var header in Header)
        {
            builder.Append(header);
            builder.Append(Separator);
        }

        builder.Append(Body.Encode());

        return Encoding.GetBytes(builder.ToString());
    }
}