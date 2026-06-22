namespace Fsm.Application;

public interface IUserInterface
{
    string? ReadInputFilePath();

    void WriteLine(string message = "");
}
