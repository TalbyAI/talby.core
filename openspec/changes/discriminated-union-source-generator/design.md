# Design: Discriminated Union Source Generator

## Technical Approach

Replace the current no-op incremental generator with a symbol-driven discriminated-union generator that first emits the `GenerateDiscriminatedUnion` marker attribute as post-initialization source, then recognizes that marker on a root type, walks nested types in source order, and classifies union cases by direct or indirect inheritance from that root. The generated surface replaces the handwritten union region in `Talby.Core.Validation/ValidationPath.cs` while keeping `ValidationPath` as the canonical behavioral reference. Root `Match` dispatch becomes branch-aware: the generated root surface exposes exactly two `Match` overloads, uses `on...` parameter names, keeps leaf cases first, appends parent or grouping cases afterward in declaration order, and rejects mixed grouping-plus-descendant coverage on the same branch. The source-generator project already has the correct analyzer-package shape, so `Talby.Core.SourceGenerators.csproj` stays unchanged.

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

**Choice**: Ignore nested types that do not inherit from the annotated root; if no valid cases remain, report a diagnostic and emit no file for that root. Annotated roots that are themselves nested inside another type are rejected with a diagnostic and do not generate output.
**Alternatives considered**: Failing the whole compilation; promoting any nested partial type; silently generating nested roots as top-level partials.
**Rationale**: This matches the spec’s explicit safety rule, prevents accidental promotion of non-cases, and avoids emitting invalid top-level partials for nested roots.

### Decision: Branch-aware two-root-Match dispatch

**Choice**: Generate exactly two root `Match` overloads that cover the complete union tree with branch-aware validation, while intermediate or grouping nodes remain visible through `IsXxx`.
**Alternatives considered**: Keeping the current dispatch model; adding separate branch-specific APIs alongside the current surface.
**Rationale**: The confirmed contract requires the compact two-overload root surface, keeps `ValidationPath` as the canonical example, and makes grouping semantics explicit instead of hiding them behind branch-oblivious dispatch.

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

The generated root partial type exposes `IsXxx` for every discovered nested type and exactly two root `Match` overloads for the complete branch-aware surface. The generated `Match` handlers use `on...` parameter names, order leaf cases first and parent or grouping cases afterward in declaration order, and validate that a branch uses either the grouping handler or the full descendant set, but not both. Every generated type and member includes XML documentation comments. Unknown runtime types still throw `InvalidOperationException` in the exhaustive matchers.

## Testing Strategy

| Layer         | What to Test           | Approach                                                                                                                                   |
| ------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Unit          | Generator output shape | Run the generator against a `ValidationPath`-style sample and assert the emitted members, docs, the two-root-Match contract, and one-file-per-root behavior.            |
| Unit          | Invalid nested policy  | Verify non-derived nested types are ignored when valid cases exist, mixed grouping-plus-descendant coverage is rejected, nested annotated roots produce an error, and zero valid cases produce a diagnostic with no generated file. |
| Project shape | Analyzer packaging     | Keep the existing project-shape guardrails so the generator remains analyzer-only with no build output.                                    |

## Migration / Rollout

No migration required. This is a compile-time replacement of the handwritten union surface with generated code, so the public API shape stays stable during the same change.

## Open Questions

- None. The remaining implementation details are mechanical once the generator pipeline and diagnostics are wired.
