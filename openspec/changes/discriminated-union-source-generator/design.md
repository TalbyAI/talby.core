# Design: Discriminated Union Source Generator

## Technical Approach

Replace the current no-op incremental generator with a symbol-driven discriminated-union generator that first emits the `GenerateDiscriminatedUnion` marker attribute as post-initialization source, then recognizes that marker on a root type, walks nested types in source order, and classifies union cases by direct or indirect inheritance from that root. The generated surface replaces the handwritten union region in `Talby.Core.Validation/ValidationPath.cs` while keeping `ValidationPath` as the canonical behavioral reference. `Match` dispatch remains leaf-only, so abstract or concrete grouping nodes surface through `IsXxx` but do not get `Match` arms. The source-generator project already has the correct analyzer-package shape, so `Talby.Core.SourceGenerators.csproj` stays unchanged.

## Architecture Decisions

### Decision: Marker-first discovery with semantic case classification

**Choice**: Emit the marker attribute from the generator project itself, then use semantic analysis to build the union tree and validate inheritance.
**Alternatives considered**: Adding a runtime attribute type; hard-coding only `ValidationPath`.
**Rationale**: Post-initialization source keeps the marker in the analyzer package, avoids a runtime dependency, and makes the annotation available to consumer projects without extra packaging.

### Decision: One generated file per union root with deterministic order

**Choice**: Emit exactly one `.g.cs` file per annotated root, ordered by source declaration order with ancestor/grouping types before descendant leaf cases.
**Alternatives considered**: A single combined generated file; alphabetical ordering.
**Rationale**: Per-root files keep review diffs and diagnostics small, and source order preserves author intent.

### Decision: Conservative invalid-nested policy

**Choice**: Ignore nested types that do not inherit from the annotated root; if no valid cases remain, report a diagnostic and emit no file for that root.
**Alternatives considered**: Failing the whole compilation; promoting any nested partial type.
**Rationale**: This matches the spec’s explicit safety rule and prevents accidental promotion of non-cases.

### Decision: Leaf-only Match dispatch

**Choice**: Generate `Match<TResult>` and `Match` only for leaf cases, while intermediate or grouping nodes expose `IsXxx` and stay out of dispatch.
**Alternatives considered**: Emitting `Match` arms for every node in the union tree.
**Rationale**: Leaf-only dispatch keeps the API aligned with the existing `ValidationPath` contract and avoids treating grouping nodes as terminal values.

## Data Flow

Root syntax with marker -> incremental pipeline -> semantic root/case discovery -> union tree classification -> generated source writer -> one root-specific file.

    Annotated root syntax
            │
            ▼
    Root symbol + nested types
            │
            ▼
    Inheritance filtering + ordering
            │
            ▼
    Generated union surface

## File Changes

| File                                                                             | Action | Description                                                                                                                    |
| -------------------------------------------------------------------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------ |
| `Talby.Core.SourceGenerators/GenerateDiscriminatedUnionAttribute.cs`             | Create | Emit the marker attribute source so consuming projects can annotate roots without a runtime dependency.                         |
| `Talby.Core.SourceGenerators/TalbyCoreIncrementalGenerator.cs`                   | Delete | Retire the placeholder no-op generator entry point.                                                                            |
| `Talby.Core.SourceGenerators/DiscriminatedUnionGenerator.cs`                     | Create | New incremental generator that discovers annotated roots, emits docs, and writes the union surface.                            |
| `Talby.Core.Validation/ValidationPath.cs`                                        | Modify | Remove the handwritten generated region and keep the root/cases as the canonical example for generator output.                 |
| `test/Talby.Core.SourceGenerators.UnitTests/DiscriminatedUnionGeneratorTests.cs` | Create | Add generator-output coverage for `ValidationPath`, indirect inheritance, deterministic ordering, and invalid-nested handling. |

## Interfaces / Contracts

The generated root partial type exposes `IsXxx` for every discovered nested type, `Match<TResult>` and `Match` for leaf cases only, plus `MatchXxx<TResult>(TResult defaultValue, Func<Xxx, TResult> matchFunc)` and `MatchXxx(Action<Xxx> matchAction, Action? defaultAction = null)` helpers for each leaf case. Intermediate or grouping nodes are observable through `IsXxx` but are intentionally excluded from `Match` dispatch. Every generated type and member includes XML documentation comments. Unknown runtime types still throw `InvalidOperationException` in the exhaustive matchers.

## Testing Strategy

| Layer         | What to Test           | Approach                                                                                                                                   |
| ------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Unit          | Generator output shape | Run the generator against a `ValidationPath`-style sample and assert the emitted members, docs, and one-file-per-root contract.            |
| Unit          | Invalid nested policy  | Verify non-derived nested types are ignored when valid cases exist, intermediate nodes stay out of Match dispatch, and zero valid cases produce a diagnostic with no generated file. |
| Project shape | Analyzer packaging     | Keep the existing project-shape guardrails so the generator remains analyzer-only with no build output.                                    |

## Migration / Rollout

No migration required. This is a compile-time replacement of the handwritten union surface with generated code, so the public API shape stays stable during the same change.

## Open Questions

- None. The remaining implementation details are mechanical once the generator pipeline and diagnostics are wired.
