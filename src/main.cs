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

                Console.WriteLine(NotFoundMsg(commandTerms[0]));
            }
            
            EndOfLoop: ;
        }
    }

    static string NotFoundMsg(string? command)
    {
        return $"{command}: command not found";
    }

    static List<string> _builtins = ["exit", "echo", "type"];
}
