using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace RemoteShell;

public static class ShellLauncher
{
    private static string ExecutablePath => IntPtr.Size == 8
        ? "RemoteShell.Resources.Shell.x64.exe"
        : "RemoteShell.Resources.Shell.x86.exe";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static (Process Process, Action Dispose) Launch(string ipcKey)
    {
        var path = Path.Combine(Path.GetTempPath(), $"shell_{Guid.NewGuid():N}.exe");
        using var res = Assembly.GetExecutingAssembly().GetManifestResourceStream(ExecutablePath);
        if (res is null) throw new MissingManifestResourceException("cannot find embedded shell.");
        using var file = new FileStream(path, FileMode.Create);
        res.CopyTo(file);

        var proc = Process.Start(new ProcessStartInfo(path, $" {ipcKey}"));
        if (proc is null) throw new FileLoadException("cannot start shell process.");
        return (proc, () => File.Delete(path));
    }
}