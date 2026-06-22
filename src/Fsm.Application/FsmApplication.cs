using Fsm.Parsing;
using Fsm.Presentation;
using Fsm.Validation;

namespace Fsm.Application;

public sealed class FsmApplication
{
    public const int SuccessExitCode = 0;
    public const int MissingInputExitCode = 1;
    public const int ParseErrorExitCode = 2;
    public const int ValidationErrorExitCode = 3;

    private readonly IFsmParser _parser;
    private readonly ValidationPipeline _validationPipeline;
    private readonly IFsmRenderer _renderer;
    private readonly IUserInterface _userInterface;

    public FsmApplication(
        IFsmParser parser,
        ValidationPipeline validationPipeline,
        IFsmRenderer renderer,
        IUserInterface userInterface)
    {
        _parser = parser;
        _validationPipeline = validationPipeline;
        _renderer = renderer;
        _userInterface = userInterface;
    }

    public static FsmApplication CreateDefault(IUserInterface userInterface)
    {
        return new FsmApplication(
            new FsmTextParser(),
            new ValidationPipeline(
            [
                new DeterministicTransitionValidator(),
                new InitialFinalTransitionValidator(),
                new UnreachableStateValidator(),
                new CompoundTargetValidator()
            ]),
            new ConsoleTextRenderer(),
            userInterface);
    }

    public int Run(string[] args)
    {
        var inputFilePath = ResolveInputFilePath(args);

        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            _userInterface.WriteLine("No FSM file path provided.");
            return MissingInputExitCode;
        }

        try
        {
            var diagram = _parser.ParseFile(ResolveExistingFilePath(inputFilePath));
            var validationResult = _validationPipeline.Validate(diagram);

            if (!validationResult.IsValid)
            {
                _userInterface.WriteLine("Validation errors:");

                foreach (var error in validationResult.Errors)
                {
                    _userInterface.WriteLine($"- {error.Code}: {error.Message}");
                }

                return ValidationErrorExitCode;
            }

            _userInterface.WriteLine(_renderer.RenderDiagram(diagram));
            return SuccessExitCode;
        }
        catch (ParseException exception)
        {
            _userInterface.WriteLine($"Parse error: {exception.Message}");
            return ParseErrorExitCode;
        }
        catch (IOException exception)
        {
            _userInterface.WriteLine($"Input error: {exception.Message}");
            return ParseErrorExitCode;
        }
        catch (UnauthorizedAccessException exception)
        {
            _userInterface.WriteLine($"Input error: {exception.Message}");
            return ParseErrorExitCode;
        }
    }

    private string? ResolveInputFilePath(string[] args)
    {
        return args.Length > 0 ? args[0] : _userInterface.ReadInputFilePath();
    }

    private static string ResolveExistingFilePath(string inputFilePath)
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
