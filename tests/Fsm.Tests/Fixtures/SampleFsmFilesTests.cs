using Fsm.Tests.Fixtures;

namespace Fsm.Tests;

public class SampleFsmFilesTests
{
    [Theory]
    [InlineData("example_lamp.fsm")]
    [InlineData("example_user_account.fsm")]
    [InlineData("valid_compound.fsm")]
    [InlineData("valid_deterministic.fsm")]
    [InlineData("invalid_compound.fsm")]
    [InlineData("invalid_deterministic1.fsm")]
    [InlineData("invalid_deterministic2.fsm")]
    [InlineData("invalid_deterministic3.fsm")]
    [InlineData("invalid_final.fsm")]
    [InlineData("invalid_initial.fsm")]
    [InlineData("invalid_unreachable.fsm")]
    public void SampleFileIsAvailable(string fileName)
    {
        var contents = SampleFsmFiles.Read(fileName);

        Assert.Contains("STATE", contents);
    }

    [Fact]
    public void TemporaryOfficeLockFilesAreNotCopiedAsFixtures()
    {
        var fixtureFiles = Directory.EnumerateFiles(SampleFsmFiles.DirectoryPath);

        Assert.DoesNotContain(fixtureFiles, file => Path.GetFileName(file).StartsWith("~$", StringComparison.Ordinal));
    }
}
