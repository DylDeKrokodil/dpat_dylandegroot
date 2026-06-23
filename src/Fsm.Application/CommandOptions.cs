namespace Fsm.Application;

public sealed record CommandOptions(string? InputFilePath, bool Simulate)
{
    public static CommandOptions Parse(string[] args)
    {
        string? inputFilePath = null;
        var simulate = false;

        foreach (var argument in args)
        {
            if (argument is "--simulate" or "-s")
            {
                simulate = true;
                continue;
            }

            inputFilePath ??= argument;
        }

        return new CommandOptions(inputFilePath, simulate);
    }
}
