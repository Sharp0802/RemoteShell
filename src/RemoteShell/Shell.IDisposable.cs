using System.Diagnostics;
using System.IO.Pipes;

namespace RemoteShell;

public partial class Shell : IDisposable
{
    private static int _identifier;

    public Shell()
    {
        var name = $"SHELL_{Interlocked.Increment(ref _identifier).ToString()}";
        IpcPipe = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Message);
        (ShellProcess, ShellDispose) = ShellLauncher.Launch(name);
    }

    private object PipeLocker { get; } = new();
    private NamedPipeServerStream IpcPipe { get; }
    
    private Process ShellProcess { get; }
    private Action ShellDispose { get; }

    public void Dispose()
    {
        IpcPipe.Disconnect();
        ShellProcess.Kill();
        
        IpcPipe.Dispose();
        ShellProcess.Dispose();
        ShellDispose.Invoke();
    }
}