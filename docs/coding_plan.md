# FSM Coding Plan

This plan turns the assignment PDF, `requirements.md`, `ai_execution_plan.md`, and the current class design into small implementation slices. The goal is to keep every change reviewable, testable, and easy to commit without mixing unrelated concerns.

## Guiding Rules

- Build the must-have viewer first. Do not start a nice-to-have until the baseline is complete.
- Keep the model independent from parsing, validation, rendering, and console I/O.
- Prefer small commits that leave the project buildable.
- Add tests as soon as a layer has behavior worth protecting.
- Keep extension points explicit: parser, validators, renderer, and application orchestration should be replaceable.
- Choose one language and stick to its conventions. The current design is C#-oriented, so this plan assumes C# unless the team chooses Java before Step 1.

## Milestone 0: Repository And Tooling

### Step 0.1: Initialize project structure

Create the solution and projects only.

Suggested C# structure:

- `src/Fsm.Domain`
- `src/Fsm.Building`
- `src/Fsm.Parsing`
- `src/Fsm.Validation`
- `src/Fsm.Presentation`
- `src/Fsm.Application`
- `tests/Fsm.Tests`

Definition of done:

- The solution builds.
- The test project runs with one placeholder test.
- No FSM behavior exists yet.

Suggested commit:

`chore: create FSM solution structure`

### Step 0.2: Add sample input files to test assets

Copy or reference the provided `.fsm` files from `Test FSMs` as test data. Ignore the temporary Office lock files that start with `~$`.

Definition of done:

- Valid and invalid sample files are available to tests.
- Test code can load sample files using stable paths.

Suggested commit:

`test: add FSM sample files as fixtures`

## Milestone 1: Domain Model

### Step 1.1: Add core domain value types and enums

Implement:

- `StateType`
- `ActionType`
- `Guard`
- base exception or error type if needed

Definition of done:

- Types compile.
- Unit tests cover simple `Guard.IsEmpty` behavior.

Suggested commit:

`feat(domain): add FSM value types`

### Step 1.2: Add states using Composite

Implement:

- abstract `State`
- `InitialState`
- `SimpleState`
- `CompoundState`
- `FinalState`

Include parent references and child support for `CompoundState`.

Definition of done:

- A compound state can contain nested states.
- A state can report whether it is nested in another state.
- No rendering, parsing, or validation code is inside domain classes.

Suggested commit:

`feat(domain): model state hierarchy`

### Step 1.3: Add triggers, actions, and transitions

Implement:

- `Trigger`
- `FsmAction`
- `StateAction`
- `TransitionAction`
- `Transition`

Definition of done:

- Transitions support source, destination, optional trigger, guard, and optional effect.
- Self-transitions are supported.
- State actions and transition effects are represented as objects.

Suggested commit:

`feat(domain): model transitions and actions`

### Step 1.4: Add `FsmDiagram`

Implement the diagram as the root aggregate.

Definition of done:

- Diagram stores states, triggers, and transitions.
- Diagram can find states and triggers by id.
- Diagram exposes incoming and outgoing transition queries.
- Initial state lookup is available.

Suggested commit:

`feat(domain): add FSM diagram aggregate`

### Step 1.5: Add Visitor entry points

Implement:

- `IFsmElementVisitor`
- `Accept` methods on diagram, states, and transitions

Definition of done:

- Domain elements accept a visitor.
- Visitor methods do not return console-specific output.
- Renderer behavior is still not implemented.

Suggested commit:

`feat(domain): add visitor traversal hooks`

## Milestone 2: Model Construction

### Step 2.1: Add `StateFactory`

Implement factory creation for state subtypes.

Definition of done:

- Factory maps every assignment state type to the correct class.
- Unknown or unsupported state types produce clear construction errors.

Suggested commit:

`feat(building): add state factory`

### Step 2.2: Add `FsmModelBuilder`

Implement builder methods for:

- states
- triggers
- transitions
- actions

Definition of done:

- Builder enforces unique ids.
- Builder resolves parent, state, trigger, and action owner references.
- Parser code will not need to wire relationships manually.

Suggested commit:

`feat(building): add FSM model builder`

## Milestone 3: Parser

### Step 3.1: Add tokenizer

Implement input normalization:

- ignore blank lines
- ignore lines that start with `#`
- split definitions by `;`
- preserve source line numbers
- enforce section order: `STATE`, `TRIGGER`, `ACTION`, `TRANSITION`

Important assignment detail:

- Comments are only valid on their own line. Avoid silently accepting inline comments after a definition.

Definition of done:

- Tokenizer tests cover comments, whitespace, CRLF input, and section ordering.

Suggested commit:

`feat(parser): tokenize FSM definitions`

### Step 3.2: Parse states

Implement `STATE <identifier> <parent> "<name>" : <state_type>;`.

Definition of done:

- Valid state definitions build states through the builder.
- Parent references must point to earlier states or `_`.
- Duplicate ids and malformed lines produce parse errors with line numbers.

Suggested commit:

`feat(parser): parse states`

### Step 3.3: Parse triggers

Implement `TRIGGER <identifier> "<description>";`.

Definition of done:

- Trigger ids are stored and duplicate ids are rejected.
- Malformed trigger definitions include line-numbered parse errors.

Suggested commit:

`feat(parser): parse triggers`

### Step 3.4: Parse transitions

Implement:

`TRANSITION <identifier> <source> -> <destination> [trigger] "<guard>";`

Support trigger-less automatic transitions. Some provided invalid examples omit the guard string, so decide explicitly whether to accept that as an empty guard for compatibility with Brightspace samples.

Definition of done:

- Existing states are resolved.
- Optional triggers are resolved when present.
- Self-transitions parse correctly.
- Unknown references and duplicate transition ids are rejected.

Suggested commit:

`feat(parser): parse transitions`

### Step 3.5: Parse actions

Implement:

`ACTION <identifier> "<description>" : <action_type>;`

Definition of done:

- State actions attach only to states.
- `TRANSITION_ACTION` attaches only to transitions.
- Invalid owner/type combinations produce clear errors.

Suggested commit:

`feat(parser): parse actions`

### Step 3.6: Add parser integration tests

Use:

- `example_lamp.fsm`
- `example_user_account.fsm`
- `valid_compound.fsm`
- `valid_deterministic.fsm`

Definition of done:

- Valid examples parse into the expected number of states, triggers, transitions, and actions.
- Nested compound states from the user account example are represented correctly.

Suggested commit:

`test(parser): cover sample FSM files`

## Milestone 4: Validation

### Step 4.1: Add validation pipeline

Implement:

- `IFsmValidator`
- `ValidationError`
- `ValidationResult`
- `ValidationPipeline`

Definition of done:

- Multiple validators can run in sequence.
- Multiple errors can be returned together.

Suggested commit:

`feat(validation): add validation pipeline`

### Step 4.2: Add deterministic transition validator

Detect ambiguous outgoing transitions per source state:

- same trigger and same guard
- same trigger with guards that do not distinguish behavior
- automatic transition mixed with other outgoing transitions

Definition of done:

- `invalid_deterministic1.fsm`, `invalid_deterministic2.fsm`, and `invalid_deterministic3.fsm` fail.
- `valid_deterministic.fsm` passes.
- Errors identify the source state and conflicting transition ids.

Suggested commit:

`feat(validation): detect non deterministic transitions`

### Step 4.3: Add initial/final transition validator

Detect:

- incoming transitions to initial states
- outgoing transitions from final states

Definition of done:

- `invalid_initial.fsm` fails.
- `invalid_final.fsm` fails.
- Errors identify the offending state and transition.

Suggested commit:

`feat(validation): enforce initial and final transition rules`

### Step 4.4: Add unreachable state validator

Use graph traversal from the diagram initial state.

Definition of done:

- `invalid_unreachable.fsm` fails.
- Reachability works across nested compound states.
- The validator is independent from rendering.

Suggested commit:

`feat(validation): detect unreachable states`

### Step 4.5: Optional compound-target validator

This is not required if unreachable states is the chosen third validator, but the provided `invalid_compound.fsm` makes this a useful small extra if time allows.

Definition of done:

- Transitions may start from compound states.
- Transitions may not end at compound states.
- `invalid_compound.fsm` fails and `valid_compound.fsm` passes.

Suggested commit:

`feat(validation): reject transitions ending at compound states`

## Milestone 5: Text Rendering

### Step 5.1: Add renderer abstraction

Implement:

- `IFsmRenderer`
- `RenderContext`
- `TextOutputBuilder`

Definition of done:

- Rendering contract supports full diagram, single state, and single transition.
- The abstraction does not depend on `Console.WriteLine`.

Suggested commit:

`feat(presentation): add renderer abstraction`

### Step 5.2: Implement console text renderer using Visitor

Render:

- hierarchy of states
- state actions
- transitions
- trigger, guard, and effect

Definition of done:

- Full rendering of `example_lamp.fsm` is readable.
- Nested compound rendering of `example_user_account.fsm` is readable.
- Domain classes still contain no presentation logic.

Suggested commit:

`feat(presentation): render FSM diagrams as text`

### Step 5.3: Add partial rendering

Support:

- single state with relevant incoming/outgoing transitions
- compound state with child states and contained transitions
- single transition

Definition of done:

- Partial rendering reuses the same renderer and visitor logic where practical.
- Tests confirm selected states/transitions can render without starting from the diagram root.

Suggested commit:

`feat(presentation): render selected FSM elements`

### Step 5.4: Add renderer tests

Definition of done:

- Tests verify important output fragments instead of exact full strings.
- Tests cover full rendering and at least two partial render cases.

Suggested commit:

`test(presentation): cover text rendering`

## Milestone 6: Application Wiring

### Step 6.1: Add user interface abstraction

Implement:

- `IUserInterface`
- `ConsoleUserInterface`

Definition of done:

- User interaction is isolated from parser, model, validation, and rendering.
- Console output happens only in the console UI/application layer.

Suggested commit:

`feat(app): add console user interface`

### Step 6.2: Add application flow

Implement:

1. Get input file path.
2. Parse file.
3. Validate diagram.
4. Print validation errors or rendered diagram.
5. Return a meaningful exit code.

Definition of done:

- Valid sample file renders from the command line.
- Invalid sample file prints validation errors.
- Parse errors and validation errors are distinguishable.

Suggested commit:

`feat(app): wire parser validation and renderer`

### Step 6.3: Add end-to-end smoke tests

Definition of done:

- One valid file produces rendered output.
- One invalid file produces errors and non-success status.

Suggested commit:

`test(app): add end to end smoke tests`

## Milestone 7: Submission Cleanup

### Step 7.1: Update documentation

Update:

- class diagram if implementation differs
- design pattern notes
- test overview
- run instructions

Definition of done:

- Documentation matches the code.
- Pattern usage is easy to explain during grading.

Suggested commit:

`docs: document architecture and patterns`

### Step 7.2: Final quality pass

Check:

- all tests pass
- no unused code
- clear error messages
- no presentation logic in domain
- no parser logic in validators
- no temporary files committed

Definition of done:

- Baseline must-have scope is complete.
- The codebase is ready for either simulation or second UI later.

Suggested commit:

`chore: prepare baseline submission`

## Recommended Nice-To-Have Path After Baseline

Prefer simulation as the later extension. The planned model, validator pipeline, graph traversal, and logging-friendly application structure already support it. Do not start it until all must-have requirements and tests are complete.

First simulation steps later:

1. Add simulation state model and current-state tracking.
2. Add trigger selection and guard confirmation.
3. Add simulation log abstraction.
4. Reuse renderer to show the current active state.

