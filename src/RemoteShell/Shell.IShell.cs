using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using RemoteShell.Core;

namespace RemoteShell;

public partial class Shell : IShell
{
    private event Action<Message> MessageReceived;

    private const string InvalidStatusCode = "Invalid status code detected.";
    private const string InvalidBodyContentType = "Invalid body content type detected.";
    private const string IOFailed = "An I/O error occurred. IPC pipe is broken. Shell client may be terminated.";
    
    public Task<int> GetProperty<T>(string name, IShell.Out<T> dst, TimeSpan timeout)
    {
        var flag = new AutoResetEvent(false);
        var msg = ShellHelper.PropertyGetter(name, out var id).Encode();

        Unsafe.SkipInit(out T ret);
        Unsafe.SkipInit(out int status);
        Action<Message> handler = null!;
        handler = arg =>
        {
            if (!string.Equals(arg.Header[0], $"ID:{id}", StringComparison.Ordinal)) return;
            MessageReceived -= handler;
            
            var statusStr = arg.Header[1];
            statusStr = statusStr["STATUS:".Length..];
            if (!int.TryParse(statusStr, out status))
                throw new InvalidDataException(InvalidStatusCode);

            if (status >= 0) // succeeded
            {
                if (arg.Body.Bytes is null) throw new InvalidDataException(InvalidBodyContentType);
                using var stream = new MemoryStream(arg.Body.Bytes);
                ret = (T) new BinaryFormatter().Deserialize(stream);
            }
            
            flag.Set();
        };
        MessageReceived += handler;

        lock (PipeLocker)
        {
            if (!IpcPipe.TryWriteMessage(msg))
                throw new IOException(IOFailed);
        }

        return Task.Factory.StartNew(() =>
        {
            flag.WaitOne(timeout);
            dst.Invoke(ret);
            return status;
        });
    }

    public Task<int> SetProperty<T>(string name, T value, TimeSpan timeout)
    {
        var flag = new AutoResetEvent(false);
        var msg = ShellHelper.PropertySetter(name, value, out var id).Encode();

        Unsafe.SkipInit(out int status);
        Action<Message> handler = null!;
        handler = arg =>
        {
            if (!string.Equals(arg.Header[0], $"ID:{id}", StringComparison.Ordinal)) return;
            MessageReceived -= handler;

            var statusStr = arg.Header[1];
            statusStr = statusStr["STATUS:".Length..];
            if (!int.TryParse(statusStr, out status))
                throw new InvalidDataException(InvalidStatusCode);
            
            flag.Set();
        };
        MessageReceived += handler;

        lock (PipeLocker)
        {
            if (!IpcPipe.TryWriteMessage(msg))
                throw new IOException(IOFailed);
        }

        return Task.Factory.StartNew(() =>
        {
            flag.WaitOne(timeout);
            return status;
        });
    }
}