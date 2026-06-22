namespace Fsm.Application;

public sealed class ConsoleUserInterface : IUserInterface
{
    public string? ReadInputFilePath()
    {
        Console.Write("FSM file path: ");
        return Console.ReadLine();
    }

    public void WriteLine(string message = "")
    {
        Console.WriteLine(message);
    }
}
