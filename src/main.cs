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
                var commandTerms = wantedCommand.Split(" ");

                {
                    switch (commandTerms[0])
                    {
                        case "exit":
                            return;
                        case "echo":
                        {
                            if (commandTerms.Length > 1)
                            {
                                Console.WriteLine(string.Join(' ', commandTerms[1..]));
                            }

                            goto EndOfLoop;
                        }
                        case "type":
                        {
                            if (_builtins.Contains(commandTerms[1]))
                            {
                                Console.WriteLine($"{commandTerms[1]} is a shell builtin");
                            }
                            else
                            {
                                var typeExecPath = SearchPATH(commandTerms[1]);

                                if (typeExecPath != null)
                                {
                                    Console.WriteLine($"{commandTerms[1]} is {typeExecPath}");
                                }
                                else
                                {
                                    Console.WriteLine($"{commandTerms[1]}: not found");
                                }
                            }

                            goto EndOfLoop;
                        }
                    }
                }
                
                var execPath = SearchPATH(commandTerms[0]);

                if (execPath != null)
                {
                    using var proc = new Process();
                    var startInfo = new ProcessStartInfo(commandTerms[0], commandTerms[1..])
                    {
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        CreateNoWindow = true,
                    };
                    proc.StartInfo = startInfo;
                    proc.Start();
                    proc.WaitForExit();
                    
                    goto EndOfLoop;
                }
                
                Console.WriteLine(NotFoundMsg(commandTerms[0]));
            }
            
            EndOfLoop: ;
        }
    }

    static string? SearchPATH(string commandName)
    {
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

    static string NotFoundMsg(string? command)
    {
        return $"{command}: command not found";
    }

    static List<string> _builtins = ["exit", "echo", "type"];
}
