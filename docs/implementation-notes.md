# Implementation Notes

## Architecture

The implementation follows the planned layered structure:

- Domain classes contain no parser, validation, rendering, or console code.
- Parser code builds the model through `FsmModelBuilder` instead of wiring relationships directly.
- Validation is a strategy pipeline of independent validators.
- Rendering is behind `IFsmRenderer` and uses domain visitor hooks.
- Console I/O is isolated behind `IUserInterface` and the CLI entry point.

## Design Patterns

- Composite: `CompoundState` contains nested `State` objects.
- Visitor: domain elements expose `Accept(IFsmElementVisitor)` for rendering traversal.
- Strategy: validators implement `IFsmValidator` and are composed in `ValidationPipeline`.
- Factory: `StateFactory` maps parsed state types to concrete state classes.
- Builder: `FsmModelBuilder` centralizes creation and reference resolution.

## Validators

Implemented validators:

- non-deterministic outgoing transitions,
- incoming transitions to initial states,
- outgoing transitions from final states,
- unreachable states,
- transitions ending at compound states.

## Run And Test

Run a valid sample:

```bash
dotnet run --project src/Fsm.Cli/Fsm.Cli.csproj -- "Test FSMs/example_lamp.fsm"
```

Run an invalid sample:

```bash
dotnet run --project src/Fsm.Cli/Fsm.Cli.csproj -- "Test FSMs/invalid_initial.fsm"
```

Run all tests:

```bash
dotnet test dpat_dylandegroot.sln
```

## Fixture Notes

The test project references `Test FSMs/*.fsm` and copies them to the test output as `SampleFsms`. Temporary Office lock files beginning with `~$` are ignored and are not copied.
