using codecraftersshell.ast;

namespace codecraftersshell;

using System.Diagnostics;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var wantedCommand = Console.ReadLine();

            if (wantedCommand != null)
            {
                var ast = new CommandLineParser(wantedCommand).Parse();

                if (ast is InvalidAst)
                {
                    Console.WriteLine("could not parse line");
                }
                else if (ast is EmptyAst)
                { }
                else if (ast is CommandNode or RedirectNode)
                {
                    Func<IStreamDirector> outputDirectorFactory;
                    CommandNode commandNode;

                    switch (ast)
                    {
                        case RedirectNode redirectNode:
                            outputDirectorFactory = () => new FileDirector(redirectNode.FileNode.FilePath);
                            commandNode = redirectNode.CommandNode;
                            break;
                        case CommandNode cNode:
                            outputDirectorFactory = () => new ConsoleDirector();
                            commandNode = cNode;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(ast.GetType().Name);
                    }
                    
                    using var outputDirector = outputDirectorFactory();
                    using IStreamDirector errorDirector = new ConsoleDirector();

                    switch (commandNode.Command)
                    {
                        case "exit":
                        {
                            return;
                        }
                        case "echo":
                        {
                            if (commandNode.Arguments.Any())
                            {
                                outputDirector.WriteLine(string.Join(' ', commandNode.Arguments));
                            }

                            goto EndOfLoop;
                        }
                        case "type":
                        {
                            if (commandNode.Arguments.Any())
                            {
                                var firstArg = commandNode.Arguments.First();
                                if (_builtins.Contains(firstArg))
                                {
                                    outputDirector.WriteLine($"{firstArg} is a shell builtin");
                                }
                                else
                                {
                                    var typeExecPath = SearchPATH(firstArg);

                                    if (typeExecPath != null)
                                    {
                                        outputDirector.WriteLine($"{firstArg} is {typeExecPath}");
                                    }
                                    else
                                    {
                                        outputDirector.WriteLine($"{firstArg}: not found");
                                    }
                                }
                            }

                            goto EndOfLoop;
                        }
                        case "pwd":
                        {
                            outputDirector.WriteLine(Environment.CurrentDirectory);

                            goto EndOfLoop;
                        }
                        case "cd":
                        {
                            var targetDir = commandNode.Arguments.FirstOrDefault("~");
                            if (targetDir == "~")
                            {
                                targetDir = Environment.GetEnvironmentVariable("HOME");
                            }

                            if (Directory.Exists(targetDir))
                            {
                                Directory.SetCurrentDirectory(targetDir);
                            }
                            else
                            {
                                outputDirector.WriteLine($"cd: {targetDir}: No such file or directory");
                            }

                            goto EndOfLoop;
                        }
                    }

                    var execPath = SearchPATH(commandNode.Command);

                    if (execPath != null)
                    {
                        using var proc = new Process();
                        var startInfo = new ProcessStartInfo(commandNode.Command, commandNode.Arguments)
                        {
                            RedirectStandardInput = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                        };
                        proc.StartInfo = startInfo;

                        // downside of this stream stuff: we can only think about lines at a time
                        // so we have to be okay to lose interactivity
                        proc.OutputDataReceived += outputDirector.Handler;
                        proc.ErrorDataReceived += errorDirector.Handler;
                        
                        proc.Start();
                        
                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();

                        proc.WaitForExit();
                        
                        goto EndOfLoop;
                    }

                    outputDirector.WriteLine($"{commandNode.Command}: command not found");
                }
            }

            EndOfLoop: ;
        }
    }

    static string? SearchPATH(string commandName)
    {
        if (OperatingSystem.IsWindows())
        {
            throw new Exception("we don't use windows");
        }
        var PATH = Environment.GetEnvironmentVariable("PATH");

        if (PATH == null)
        {
            return null;
        }
        
        foreach (var dir in PATH.Split(Path.PathSeparator))
        {
            var execCandidate = Path.Combine(dir, commandName);

            if (Path.Exists(execCandidate))
            {
                UnixFileMode mode = File.GetUnixFileMode(execCandidate);

                if ((mode & UnixFileMode.UserExecute) != 0)
                {
                    return execCandidate;
                }
            }
        }

        return null;
    }

    static List<string> _builtins = ["exit", "echo", "type", "pwd", "cd"];
}
