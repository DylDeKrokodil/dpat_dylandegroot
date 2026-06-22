# AI Execution Plan for FSM Viewer/Simulator

This plan translates the assignment into small, executable steps that an AI coding agent can complete in order.

It is optimized for:

- minimal ambiguity,
- clear handoff between tasks,
- easy validation after each step,
- and a clean path from basic version to later extension.

## 1. Goal

Build the **must-have** FSM viewer first, while keeping the architecture ready for one later upgrade:

- either a second UI,
- or FSM simulation.

## 2. Execution Rules

Use these rules while implementing:

- Do not build the nice-to-have yet.
- Keep the **domain/model layer** independent from console output.
- Keep parsing, validation, and rendering in separate modules/classes.
- Prefer small commits / small changes per step.
- Do not move to the next step until the current step has a clear “done”.
- Add unit tests as soon as a module becomes stable.

## 3. Implementation Order

### Step 1. Create project skeleton

Objective:

- Create a clean solution/project structure for domain, parser, validators, renderer, and tests.

Expected output:

- Buildable project.
- Test project added.
- Basic folder/package structure in place.

Suggested structure:

- `domain`
- `parser`
- `validation`
- `presentation`
- `application`
- `tests`

Definition of done:

- The project builds successfully.
- The test project runs, even if only with a placeholder test.
- The folder/package structure reflects separation of concerns.

### Step 2. Define the domain model

Objective:

- Create the in-memory FSM model without any parsing or console code.

Required model concepts:

- `FsmDiagram`
- `State`
- `InitialState`
- `SimpleState`
- `CompoundState`
- `FinalState`
- `Transition`
- `Trigger`
- `Action`
- `ActionType`
- `IFsmElementVisitor`

Minimum relationships:

- States belong to the diagram.
- States can have a parent state.
- Compound states can contain child states.
- Transitions have source and destination states.
- Actions belong to a state or transition owner.
- Diagram, state, and transition elements expose an `Accept(visitor)` method for Visitor-based traversal.

Definition of done:

- The model can represent all assignment concepts.
- Nested compound states are supported.
- Self-transitions are supported.
- The Visitor entry points exist in the model, but no rendering logic is placed in the model classes.
- No rendering or parsing logic exists in domain classes.

### Step 3. Add model construction strategy

Objective:

- Decide how the parser creates model objects in a safe and consistent way.

Recommended approach:

- Use a builder/factory layer for creating states, transitions, triggers, and actions.

Why this step matters:

- It prevents the parser from directly owning too much logic.
- It keeps object creation consistent.
- It helps later if the input source changes.

Definition of done:

- There is one clear place responsible for creating valid model objects.
- Parser code can depend on this creation layer later.

### Step 4. Implement file reader and tokenizer/parser foundation

Objective:

- Read the FSM text file and split it into definitions safely.

Requirements to support:

- Ignore blank lines.
- Ignore comment lines starting with `#`.
- Respect `;` as definition terminator.
- Preserve section order:
  - states
  - triggers
  - actions
  - transitions

Definition of done:

- A valid input file can be broken into normalized definitions.
- Incorrectly structured input produces a parse error.

### Step 5. Parse states

Objective:

- Parse all `STATE` definitions and build the state hierarchy.

Requirements to support:

- Identifier
- Parent identifier or `_`
- Display name
- State type

Important behavior:

- Parent state must already exist.
- State identifiers must be unique.

Definition of done:

- All states from a valid file are present in the model.
- Parent-child relationships are correct.
- Nested compound states work.
- Invalid state definitions produce clear errors.

### Step 6. Parse triggers

Objective:

- Parse all `TRIGGER` definitions.

Definition of done:

- Triggers are stored by identifier.
- Invalid or duplicate trigger definitions produce clear errors.

### Step 7. Parse transitions

Objective:

- Parse all `TRANSITION` definitions.

Requirements to support:

- Transition identifier
- Source state
- Destination state
- Optional trigger
- Optional guard string

Definition of done:

- Transitions connect existing states only.
- Trigger-less transitions are supported.
- Self-transitions are supported.
- Duplicate identifiers or unknown references produce clear errors.

### Step 8. Parse actions

Objective:

- Parse `ACTION` definitions and attach them to the correct owner.

Requirements to support:

- State actions:
  - `ENTRY_ACTION`
  - `DO_ACTION`
  - `EXIT_ACTION`
- Transition actions:
  - `TRANSITION_ACTION`

Definition of done:

- Actions are attached to the correct state or transition.
- Unsupported combinations produce clear errors.

### Step 9. Create parser integration tests

Objective:

- Prove the parser can build a valid FSM from sample assignment files.

Minimum tests:

- Valid simple FSM file parses successfully.
- Valid nested compound FSM file parses successfully.
- Comments and whitespace are ignored correctly.
- Optional trigger and empty guard are handled correctly.

Definition of done:

- Parser tests pass.
- Parsed objects match expected structure.

### Step 10. Build validator interface/pipeline

Objective:

- Make validation pluggable so multiple validators can run in sequence.

Recommended shape:

- `IValidator` or equivalent interface
- `ValidationResult`
- `ValidationError`
- `ValidationPipeline`

Definition of done:

- Validators can be registered and run together.
- Multiple validation errors can be reported clearly.

### Step 11. Implement validator: non-deterministic transitions

Objective:

- Detect ambiguous outgoing transitions from the same state.

Must detect:

- Same trigger with no distinguishing guard behavior.
- Guard combinations that still leave ambiguity.
- Automatic transitions that conflict with triggered transitions.

Definition of done:

- Invalid examples from the assignment are rejected.
- Clear error messages identify the state and conflicting transitions.

### Step 12. Implement validator: illegal initial/final transitions

Objective:

- Detect:
  - incoming transitions to an initial state,
  - outgoing transitions from a final state.

Definition of done:

- Both cases are detected.
- Error messages include the offending state/transition.

### Step 13. Implement validator: unreachable states

Objective:

- Detect any state that cannot be reached from the initial state.

Recommended method:

- Graph traversal starting at the initial state.

Definition of done:

- Reachable vs unreachable states are identified correctly.
- Invalid sample files are rejected with clear messages.

### Step 14. Add validator tests

Objective:

- Test all validators against the provided invalid assignment files.

Definition of done:

- Each validator has at least one failing test.
- At least one valid FSM passes all validators.

### Step 15. Design renderer abstraction

Objective:

- Introduce a rendering contract and Visitor-based traversal that are not tied directly to console code.

Recommended shape:

- `DiagramRenderer`
- `RenderContext`
- `IFsmElementVisitor`
- `TextOutputBuilder` or a simple string-based renderer output

Definition of done:

- The application can ask for rendering without depending on a concrete console implementation.
- FSM elements can call `Accept(visitor)` and the renderer can implement the visitor methods.
- A future graphical renderer could implement the same contract.

### Step 16. Implement console renderer for full diagram

Objective:

- Render the full FSM textually in a readable console format using the Visitor pattern.

Must show:

- state hierarchy,
- simple state actions,
- transitions,
- trigger/guard/effect when present.

Definition of done:

- A valid FSM can be rendered as readable text.
- Nested compound states are visible through indentation or another clear format.
- The renderer implements `IFsmElementVisitor`; domain classes only call visitor methods and do not build output strings.

### Step 17. Implement partial rendering

Objective:

- Render individual parts of the diagram.

Must support:

- single state,
- compound state with nested contents,
- single transition.

Definition of done:

- The renderer can start from a selected element, not only the root diagram.
- Partial rendering reuses the same Visitor methods as full rendering.

### Step 18. Add renderer tests

Objective:

- Verify that rendering includes the expected structural information.

Definition of done:

- Tests confirm important lines/sections exist.
- Tests cover full rendering and at least one partial rendering case.

### Step 19. Build application flow

Objective:

- Connect parser, validators, and renderer in a simple app entry point.

Basic flow:

1. Read file path.
2. Parse file into model.
3. Run validators.
4. If valid, render diagram.
5. If invalid, show clear errors.

Definition of done:

- A user can run the application on an input file.
- Valid files render successfully.
- Invalid files show validation feedback.

### Step 20. Final cleanup and documentation

Objective:

- Prepare for submission and future extension.

Must include:

- brief architecture notes,
- chosen design patterns,
- class diagram update,
- test overview.

Chosen design patterns to document:

- Composite: nested `State` / `CompoundState` hierarchy.
- Visitor: `IFsmElementVisitor` with `Accept(visitor)` on FSM elements.
- Strategy: validator classes behind a shared validator interface.
- Builder: `FsmModelBuilder` for constructing the model from parser output.
- Factory: `StateFactory` for creating state subtypes.

Definition of done:

- Another developer or AI can understand the structure quickly.
- The project is ready for grading on the must-have scope.

## 4. AI-Friendly Task Breakdown

If an AI agent is executing this work, these are the best task boundaries:

### Task A. Domain model only

Focus:

- Create classes/enums/interfaces for the FSM model only.

Do not do:

- parsing,
- rendering,
- validation,
- UI.

Success signal:

- The model compiles and supports all required relationships.
- Visitor entry points are present without adding presentation logic to the model.

### Task B. Parser only

Focus:

- Read assignment files and create the domain model.

Do not do:

- console rendering,
- validators beyond parse/reference errors.

Success signal:

- Valid sample files parse into the model.

### Task C. Validators only

Focus:

- Implement validator pipeline and required validators.

Do not do:

- rendering changes,
- UI logic.

Success signal:

- Invalid sample files fail with expected messages.

### Task D. Renderer only

Focus:

- Implement renderer abstraction and console output.
- Implement the renderer as a Visitor over the FSM model.

Do not do:

- parsing logic,
- validation logic in renderer.

Success signal:

- Parsed valid models can be rendered fully and partially.
- The renderer implements the visitor interface and reuses it for full and partial rendering.

### Task E. Application wiring

Focus:

- Glue parser, validators, and renderer together.

Do not do:

- deep model redesign.

Success signal:

- End-to-end run works from file input to output/error.

## 5. Recommended File/Module Ownership

To avoid messy architecture, keep ownership clear:

- `domain`: only FSM objects and domain rules
- `domain`: includes Visitor entry points, but no concrete rendering/validation output logic
- `parser`: file reading and syntax parsing
- `validation`: validators and validation results
- `presentation`: textual rendering and concrete visitor implementation for console output
- `application`: startup flow / orchestration
- `tests`: separate tests for parser, validators, renderer, and end-to-end flow

## 6. Definition of Basic Success

The basic version is complete when all of the following are true:

- Valid assignment input files parse successfully.
- The FSM model supports all required state/transition concepts.
- Three validators are implemented and tested.
- The full diagram can be rendered in the console.
- Partial rendering works.
- Visitor is used for model traversal during rendering.
- Model and presentation are strictly separated.
- Unit tests exist for parser, validation, and rendering.

## 7. Upgrade Path After Basic Version

Only after the basic version is stable, choose **one** extension.

### Option 1. Simulation

Best next step if you want the smoothest technical continuation.

Implementation order:

1. Add current-state tracking.
2. Add trigger activation flow.
3. Add guard evaluation contract.
4. Add transition execution flow.
5. Add logging for state actions and transition effects.
6. Extend console UI to step through simulation.

### Option 2. Second user interface

Best next step if you want to show architecture quality and UI separation.

Implementation order:

1. Reuse existing renderer abstraction.
2. Create graphical rendering objects.
3. Add a second presentation implementation.
4. Keep the same domain and validation pipeline.

## 8. Best Prompt Style for an AI Agent

When asking an AI to implement this project, use prompts like:

- “Implement only the FSM domain model from the requirements. Do not add parser or renderer code.”
- “Add the Visitor entry points to the FSM domain model. Do not put rendering logic in the model.”
- “Implement the parser for `STATE`, `TRIGGER`, `ACTION`, and `TRANSITION` using the existing domain model.”
- “Implement the validation pipeline and the three required validators. Reuse the parsed model.”
- “Implement a console renderer as an `IFsmElementVisitor` for full and partial FSM rendering without adding validation logic.”

Avoid prompts like:

- “Build the whole project at once.”
- “Do the FSM app however you think is best.”

## 9. Recommended Next Action

Start with:

- **Step 1 + Step 2**

That gives the cleanest base for all remaining work and keeps future refactors small.
