using System.Diagnostics;

namespace codecraftersshell;

public interface IStreamDirector : IDisposable
{
    public void Handler(Object sender, DataReceivedEventArgs e)
    {
        WriteLine(e.Data);
    }

    public void WriteLine(string? line);
}

public class ConsoleDirector : IStreamDirector
{
    public void WriteLine(string? line)
    {
        if (line is not null)
        {
            Console.WriteLine(line);
        }
    }
    
    public void Dispose()
    {}
}

public class FileDirector : IStreamDirector
{
    private readonly StreamWriter _directedFile;

    public FileDirector(string filePath)
    {
        _directedFile = File.CreateText(filePath);
        _directedFile.AutoFlush = true;
    }

    public void WriteLine(string? line)
    {
        if (line is not null)
        {
            _directedFile.WriteLine(line);
        }
    }
    
    public void Dispose()
    {
        _directedFile.Dispose();
    }
}