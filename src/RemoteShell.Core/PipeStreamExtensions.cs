using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

namespace RemoteShell.Core;

public static class PipeStreamExtensions
{
    public static bool TryReadMessage(this PipeStream stream, [NotNullWhen(true)] out byte[]? msg)
    {
        if (stream.TransmissionMode is not PipeTransmissionMode.Message)
            throw new InvalidOperationException(
                "To read a message from pipe, TransmissionMode must be set to PipeTransmissionMode.Message.");

        try
        {
            using var buffer = new MemoryStream();
            var local = new byte[32];
            do
            {
                var len = stream.Read(local, 0, local.Length);
                buffer.Write(local, 0, len);
            } while (!stream.IsMessageComplete);

            msg = buffer.ToArray();
            return true;
        }
        catch (IOException)
        {
            msg = null;
            return false;
        }
    }

    public static bool TryWriteMessage(this PipeStream stream, byte[] msg)
    {
        try
        {
            stream.Write(msg, 0, msg.Length);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }
}