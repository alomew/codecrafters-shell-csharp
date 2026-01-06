class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");

            var wantedCommand = Console.ReadLine();
        
            Console.WriteLine($"{wantedCommand}: command not found");
        }
    }
}
