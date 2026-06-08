---
title: Discriminated Union Source Generator Proposal
description: Proposal for generating discriminated union APIs from annotated root types
---

## Intent

Add a `DiscriminatedUnionGenerator` for `ValidationPath`-style discriminated unions so one annotated root produces the matching API instead of a handwritten region. The generated surface keeps `IsXxx` members, exposes exactly two root `Match` overloads, and enforces branch-aware coverage so a grouping handler and its descendant handlers cannot be mixed on the same branch.

## Scope

### In Scope

- Introduce `DiscriminatedUnionGenerator` as the generator entry point
- Remove the current placeholder generator in the same change
- Generate union members from a root type marked with `GenerateDiscriminatedUnion`
- Discover nested partial case types by inheritance from the annotated root
- Emit `IsXxx` members and exactly two root `Match` overloads per union type
- Use `on...` parameter names for the generated root handlers
- Order `Match` parameters with leaf cases first, then parent or grouping cases in declaration order
- Validate branch coverage so grouping handlers exclude descendant handlers on the same branch
- Keep `ValidationPath` as the canonical example and acceptance reference

### Out of Scope

- Broad refactors of the validation model
- Changing public semantics outside the union surface
- Supporting non-nested or unrelated case discovery
- Adding extra root `Match` families beyond the two confirmed overloads

## Capabilities

### New Capabilities

- `discriminated-union-generation`: generate union APIs and case dispatch from an annotated root type
- `branch-aware-match-dispatch`: validate either a grouping handler or the full descendant set on each branch

### Modified Capabilities

- None

## Approach

Replace the current no-op incremental generator with a symbol-driven `DiscriminatedUnionGenerator` that identifies annotated roots, walks nested partial types, filters cases by inheritance from the root, and writes one generated file per union. The generated root surface should keep `IsXxx` members, emit exactly two `Match` overloads with `on...` handler names, order leaf cases before parent or grouping cases, and validate branch exclusivity before dispatching. Keep the generated surface aligned with `ValidationPath.cs`, which remains the canonical example for the contract.

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
- [ ] `ValidationPath` remains the canonical example for the generated contract
- [ ] The generated API uses `on...` handler names and exactly two root `Match` overloads
- [ ] Tests cover both the generator project shape and the discriminated union output
