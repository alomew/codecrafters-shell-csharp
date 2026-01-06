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
                }
            }

            Console.WriteLine(NotFoundMsg(wantedCommand));
            
            EndOfLoop: ;
        }
    }

    static string NotFoundMsg(string? command)
    {
        return $"{command}: command not found";
    }
}
