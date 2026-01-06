using System.Data;

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
                var commandTerms = wantedCommand.Split(" ", 2);

                switch (commandTerms[0])
                {
                    case "exit":
                        return;
                    case "echo":
                    {
                        if (commandTerms.Length > 1)
                        {
                            Console.WriteLine(commandTerms[1]);
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
                            Console.WriteLine($"{commandTerms[1]}: not found");
                        }
                        
                        goto EndOfLoop;
                    }
                }
                
                if (SearchPATH(commandTerms[0]))
                {
                    goto EndOfLoop;
                }

                Console.WriteLine(NotFoundMsg(commandTerms[0]));
            }
            
            EndOfLoop: ;
        }
    }

    static bool SearchPATH(string commandName)
    {
        var PATH = Environment.GetEnvironmentVariable("PATH");

        if (PATH == null)
        {
            return false;
        }
        
        foreach (var dir in PATH.Split(Path.PathSeparator))
        {
            var execCandidate = Path.Combine(dir, commandName);

            if (Path.Exists(execCandidate))
            {
                UnixFileMode mode = File.GetUnixFileMode(execCandidate);

                if ((mode & UnixFileMode.UserExecute) != 0)
                {
                    Console.WriteLine($"{commandName} is {execCandidate}");
                    return true;
                }
            }
        }

        return false;
    }

    static string NotFoundMsg(string? command)
    {
        return $"{command}: command not found";
    }

    static List<string> _builtins = ["exit", "echo", "type"];
}
