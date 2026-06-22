namespace Fsm.Application;

public interface IUserInterface
{
    string? ReadInputFilePath();

    string? ReadLine();

    void WriteLine(string message = "");
}
