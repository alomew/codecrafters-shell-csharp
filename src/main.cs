class Program
{
    static void Main()
    {
        Console.Write("$ ");

        var wantedCommand = Console.ReadLine();
        
        Console.WriteLine($"{wantedCommand}: command not found");
    }
}
