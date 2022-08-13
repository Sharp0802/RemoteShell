namespace RemoteShell;

public interface IShell
{
    public delegate void Out<in T>(T value);
    
    public Task<int> GetProperty<T>(string name, Out<T> dst, TimeSpan timeout);
    public Task<int> SetProperty<T>(string name, T value, TimeSpan timeout);

    public Task<int> ClearAsync(TimeSpan timeout);
    public Task<int> ReadLineAsync(Out<string?> dst, TimeSpan timeout);
    public Task<int> WriteAsync(string msg, TimeSpan timeout);
    public Task<int> WriteLineAsync(string msg, TimeSpan timeout);
    public Task<int> WriteLineAsync(TimeSpan timeout);
}