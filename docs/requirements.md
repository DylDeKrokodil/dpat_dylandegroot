# FSM Viewer/Simulator Requirements

## 1. Goal

Build a Finite State Machine (FSM) application that can:

- read an FSM definition from the assignment input format,
- represent the FSM as runtime objects,
- validate the FSM,
- render the FSM textually in the console,
- and be structured so it can later be extended toward a higher grade with minimal rework.

This document focuses on the basic scope first, while keeping the design open for one later nice-to-have extension.

## 2. Scope Choice

### Phase 1: basic grade-ready scope

Phase 1 targets the **must-have requirements** from the assignment and should be sufficient for a grade up to **8**, assuming clean implementation and proper use of design patterns.

### Phase 2: upgrade-ready direction

The codebase must be prepared so we can later add **one** of the higher-grade extensions without changing the core domain model:

- either a **second user interface** (graphical rendering),
- or **FSM simulation**.

## 3. Functional Requirements

### FR-01 Input file loading

The system shall load an FSM definition from a text file that follows the assignment format.

Acceptance criteria:

- The file contains the sections `STATE`, `TRIGGER`, `ACTION`, and `TRANSITION`.
- The sections are processed in the required order.
- Comments starting with `#` are ignored.
- Empty lines and extra whitespace between definitions are ignored.
- Each definition is terminated with `;`.

### FR-02 Runtime model

The system shall convert the input file into an in-memory object model of the FSM.

Acceptance criteria:

- States, triggers, actions, and transitions are represented as objects.
- Relationships between states and transitions are preserved.
- Parent-child relations between compound and nested states are preserved.

### FR-03 Supported state types

The system shall support the following state types:

- `INITIAL`
- `SIMPLE`
- `COMPOUND`
- `FINAL`

Acceptance criteria:

- Each parsed state is stored with its state type.
- A state can have a parent state or no parent (`_`).
- Compound states can contain nested states.

### FR-04 Nested compound states

The system shall support compound states nested deeper than one level.

Acceptance criteria:

- A compound state may contain sub-states.
- A sub-state may itself be a compound state.
- Rendering and validation work correctly for nested compound states.

### FR-05 State actions

The system shall support actions on simple states.

Acceptance criteria:

- The following action types are supported:
  - `ENTRY_ACTION`
  - `DO_ACTION`
  - `EXIT_ACTION`
- Actions are linked to their owning state.
- A simple state can expose its configured actions in the textual output.

### FR-06 Transitions

The system shall support transitions between states.

Acceptance criteria:

- A transition has an identifier.
- A transition has a source state and a destination state.
- A transition may optionally reference a trigger.
- A transition may optionally contain a guard condition string.
- A transition may have a transition effect through an `ACTION` of type `TRANSITION_ACTION`.

### FR-07 Self-transitions

The system shall support self-transitions.

Acceptance criteria:

- A transition may have the same source and destination state.
- Self-transitions can be parsed, stored, validated, and rendered.

### FR-08 Textual rendering of the full FSM

The system shall render the complete FSM in the console in a readable textual format.

Acceptance criteria:

- The rendering shows the hierarchy of states.
- The rendering shows actions of simple states.
- The rendering shows transitions, including trigger, guard, and effect when present.
- The rendering is understandable without inspecting the raw input file.

### FR-09 Partial rendering

The system shall be able to render separate parts of the FSM, not only the full diagram.

Acceptance criteria:

- The system can render a single state.
- The system can render a compound state with its contained sub-states and transitions.
- The system can render a single transition.
- The rendering mechanism does not depend on always starting at the root diagram.

### FR-10 Validation of invalid diagrams

The system shall report meaningful errors for invalid FSM definitions.

Acceptance criteria:

- Validation runs after parsing and before normal rendering/use.
- Validation errors are shown as clear messages.
- Invalid input from the Brightspace examples can be used to verify validator behavior.

### FR-11 Validator 1: non-deterministic transitions

The system shall detect states with outgoing transitions that are not deterministic.

Acceptance criteria:

- Conflicting outgoing transitions from the same state are detected.
- Cases with overlapping trigger/guard combinations are rejected.
- Automatic transitions that make other transitions ambiguous or unreachable are rejected.

### FR-12 Validator 2: illegal initial/final transitions

The system shall detect the following invalid transition cases:

- an initial state with incoming transitions,
- a final state with outgoing transitions.

Acceptance criteria:

- Both cases produce a clear validation error.

### FR-13 Validator 3: recommended baseline choice

The system shall implement at least one of the assignment’s third validator options.

Recommended choice for Phase 1:

- **Unreachable states**

Reason:

- It supports future simulation work,
- it encourages a proper graph traversal model,
- and it gives more functional value than a purely syntactic check.

Acceptance criteria:

- Any state that cannot be reached from the initial state is reported.

Alternative accepted choice:

- reject transitions that target a compound state instead of a simple state inside it.

## 4. Non-Functional Requirements

### NFR-01 Language

The application shall be implemented in **Java** or **C#**, unless another OO language is explicitly approved by the instructor.

### NFR-02 Separation of concerns

The presentation layer and model layer shall be strictly separated.

Acceptance criteria:

- Domain classes contain no console-specific code.
- The presentation layer depends on the model layer, not the other way around.
- Replacing the console renderer with another UI requires minimal change to domain logic.

### NFR-03 Extensible architecture

The design shall be extensible for one future nice-to-have without replacing the core model.

Acceptance criteria:

- Parsing, validation, and rendering are separate responsibilities.
- Adding a second renderer or simulation engine can reuse the same FSM model.
- Validation logic can be extended with additional validators.

### NFR-04 Design patterns

The solution shall apply multiple relevant design patterns and the team shall be able to explain them.

Recommended pattern candidates:

- Composite for nested states
- Visitor for traversing/rendering FSM elements without putting presentation logic in the domain classes
- Strategy for validation behavior through separate validator classes
- Builder for constructing the model from parsed definitions
- Factory or Abstract Factory for state object creation
- State-related behavior helpers where useful

Chosen baseline pattern set for this project:

- **Composite**: `State` and `CompoundState` represent nested state hierarchies.
- **Visitor**: FSM elements expose `Accept(visitor)` so rendering and future traversal-based behavior can walk the model cleanly.
- **Strategy**: validators implement a shared validator interface and can be swapped or extended.
- **Builder**: `FsmModelBuilder` centralizes model construction from parser output.
- **Factory**: `StateFactory` creates the correct state subtype.

### NFR-05 Code quality

The code shall be clean, readable, and maintainable.

Acceptance criteria:

- Classes have clear responsibilities.
- Naming is consistent.
- Public APIs are small and purposeful.
- Error handling is explicit.

### NFR-06 Testability

The solution shall be covered by unit tests.

Acceptance criteria:

- Parser tests exist for valid input.
- Validator tests exist for the provided invalid input files.
- Rendering tests verify key textual output or rendering structure.

## 5. Recommended Upgrade-Ready Constraints

These are not extra features yet, but they make the future higher-grade extension much easier.

### UPG-01 Renderer abstraction

The application should define a rendering abstraction so that console rendering is just one implementation.

Why:

- This keeps the door open for the “second user interface” option.
- The console renderer can be implemented as a Visitor over the FSM model, so a later renderer can reuse the same traversal structure.

### UPG-02 Explicit domain objects

Triggers, guards, effects, states, and transitions should be stored as explicit model elements rather than being mixed into raw strings.

Why:

- This makes both validation and simulation easier later.

### UPG-03 Validator pipeline

Validators should be pluggable and executable as a sequence.

Why:

- This keeps new validation rules easy to add.

### UPG-04 Navigation from initial state

The model should make it easy to walk from the initial state through outgoing transitions.

Why:

- This is the foundation for future simulation.

### UPG-05 Logging abstraction

Any user-visible status or event reporting should go through a small abstraction instead of being written directly from domain classes.

Why:

- This helps later when adding simulation logs or a graphical UI.

## 6. Recommended Phase 1 Deliverable

The first version should include:

- parser for the assignment file format,
- domain model for states, transitions, triggers, and actions,
- console renderer for full and partial output,
- three validators,
- unit tests,
- documentation of chosen design patterns.

## 7. Suggested Build Order

1. Build the core FSM domain model.
2. Build the parser for states, triggers, actions, and transitions.
3. Add the validator pipeline and implement the required validators.
4. Add console rendering for full diagrams.
5. Add partial rendering support.
6. Add tests and clean up the architecture for extension.

## 8. Recommended Nice-to-Have Direction Later

If the team wants the easiest path to a higher grade after Phase 1, the most natural next step is:

- **FSM simulation**

Reason:

- the runtime model, graph traversal, validation, and logging already move in that direction,
- and it gives a strong functional extension without requiring layout work for graphics.

If the team prefers UI-focused work, then the alternative is:

- **second user interface** using the same model and renderer abstraction.
