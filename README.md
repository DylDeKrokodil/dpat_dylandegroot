# FSM Viewer

C# console application for reading, validating, and rendering finite state machine definitions from the assignment `.fsm` format.

## Run

```bash
dotnet run --project src/Fsm.Cli/Fsm.Cli.csproj -- "Test FSMs/example_lamp.fsm"
```

If no file path is passed, the CLI prompts for one.

## Test

```bash
dotnet test dpat_dylandegroot.sln
```

## Structure

- `src/Fsm.Domain`: FSM model, state hierarchy, transitions, actions, visitor hooks.
- `src/Fsm.Building`: model construction and reference resolution.
- `src/Fsm.Parsing`: tokenizer and parser for assignment `.fsm` files.
- `src/Fsm.Validation`: pluggable validation pipeline and validators.
- `src/Fsm.Presentation`: text rendering through visitor-based traversal.
- `src/Fsm.Application`: application flow and user interface abstraction.
- `src/Fsm.Cli`: console entry point.
- `tests/Fsm.Tests`: unit, integration, fixture, and smoke tests.
