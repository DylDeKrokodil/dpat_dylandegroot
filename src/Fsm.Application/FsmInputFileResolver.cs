namespace Fsm.Application;

public sealed class FsmInputFileResolver : IInputFileResolver
{
    public string Resolve(string inputFilePath)
    {
        if (File.Exists(inputFilePath) || Path.IsPathRooted(inputFilePath))
        {
            return inputFilePath;
        }

        foreach (var searchRoot in GetSearchRoots())
        {
            var candidate = FindInAncestorDirectories(searchRoot, inputFilePath);

            if (candidate is not null)
            {
                return candidate;
            }
        }

        return inputFilePath;
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;
    }

    private static string? FindInAncestorDirectories(string startDirectory, string relativePath)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
