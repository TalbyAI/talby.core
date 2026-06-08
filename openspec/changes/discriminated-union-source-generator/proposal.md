---
title: Discriminated Union Source Generator Proposal
description: Proposal for generating discriminated union APIs from annotated root types
---

## Intent

Add a `DiscriminatedUnionGenerator` for `ValidationPath`-style discriminated unions so the repetitive union API is produced from one annotated root instead of being hand-maintained. This reduces drift between the handwritten cases and the generated matching surface, and it gives the repo a clear pattern for future union types.

## Scope

### In Scope

- Introduce `DiscriminatedUnionGenerator` as the generator entry point
- Remove the current placeholder generator in the same change
- Generate union members from a root type marked with `GenerateDiscriminatedUnion`
- Discover nested partial case types by inheritance from the annotated root
- Emit `IsXxx`, `Match<TResult>`, `Match`, and `MatchXxx` helpers per union type

### Out of Scope

- Broad refactors of the validation model
- Changing public semantics outside the union surface
- Supporting non-nested or unrelated case discovery

## Capabilities

### New Capabilities

- `discriminated-union-generation`: generate union APIs and case dispatch from an annotated root type

### Modified Capabilities

- None

## Approach

Replace the current no-op incremental generator with a symbol-driven `DiscriminatedUnionGenerator` that identifies annotated roots, walks nested partial types, filters cases by inheritance from the root, and writes one generated file per union. Remove the placeholder generator in the same slice, keep the generated surface aligned with the existing handwritten example in `ValidationPath.cs`, and add tests around project shape and generator output.

## Affected Areas

| Area                                                           | Impact   | Description                                                 |
| -------------------------------------------------------------- | -------- | ----------------------------------------------------------- |
| `Talby.Core.SourceGenerators/DiscriminatedUnionGenerator.cs`   | New      | Implement the real generator pipeline                       |
| `Talby.Core.SourceGenerators/TalbyCoreIncrementalGenerator.cs` | Removed  | Retire the placeholder generator                            |
| `Talby.Core.Validation/ValidationPath.cs`                      | Modified | Use the generator contract as the reference union example   |
| `test/Talby.Core.SourceGenerators.UnitTests/`                  | Modified | Add coverage for generated union behavior and project shape |

## Risks

| Risk                                                                                | Likelihood | Mitigation                                                             |
| ----------------------------------------------------------------------------------- | ---------- | ---------------------------------------------------------------------- |
| Invalid nested partials create ambiguous input                                      | Medium     | Define a crisp diagnostic rule for non-case nested partials            |
| Generated API drifts from the handwritten example                                   | Medium     | Use the example file as the behavioral reference and add focused tests |
| Union surface grows faster than the generator can stay stable                       | Low        | Keep the first slice narrow and per-root file generation deterministic |
| Removing the placeholder and adding the real generator in one slice increases churn | Low        | Keep the rename/replacement mechanical and test the output shape       |

## Rollback Plan

If the generator proves unstable, restore the placeholder generator file and keep the handwritten union surface in place until the spec and diagnostics are tightened.

## Dependencies

- Agreement on the exact validation rule for nested partials that do not qualify as cases

## Success Criteria

- [ ] `DiscriminatedUnionGenerator` replaces the placeholder generator in the same change
- [ ] `ValidationPath` is generated from the marker instead of duplicated manually
- [ ] The generated API matches the current union surface and case names
- [ ] Tests cover both the generator project shape and the discriminated union output
