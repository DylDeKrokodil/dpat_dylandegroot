namespace Fsm.Tests.Fixtures;

public static class SampleFsmFiles
{
    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "SampleFsms");

    public static string PathFor(string fileName)
    {
        var path = Path.Combine(DirectoryPath, fileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Sample FSM file '{fileName}' was not copied to the test output.", path);
        }

        return path;
    }

    public static string Read(string fileName)
    {
        return File.ReadAllText(PathFor(fileName));
    }
}
