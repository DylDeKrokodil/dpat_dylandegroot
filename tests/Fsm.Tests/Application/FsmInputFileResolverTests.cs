using Fsm.Application;
using Fsm.Tests.Fixtures;

namespace Fsm.Tests.Application;

public class FsmInputFileResolverTests
{
    private readonly FsmInputFileResolver _resolver = new();

    [Fact]
    public void ResolveReturnsExistingAbsolutePath()
    {
        var path = SampleFsmFiles.PathFor("example_lamp.fsm");

        var resolvedPath = _resolver.Resolve(path);

        Assert.Equal(path, resolvedPath);
    }

    [Fact]
    public void ResolveReturnsOriginalPathWhenNoCandidateExists()
    {
        const string path = "does-not-exist.fsm";

        var resolvedPath = _resolver.Resolve(path);

        Assert.Equal(path, resolvedPath);
    }
}
