class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var wantedCommand = Console.ReadLine();

            switch (wantedCommand)
            {
                case "exit":
                    return;
            }
        
            Console.WriteLine($"{wantedCommand}: command not found");
        }
    }
}
