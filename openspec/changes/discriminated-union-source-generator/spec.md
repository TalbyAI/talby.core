---
title: Discriminated Union Source Generator Spec
description: Specification for generating discriminated union APIs from annotated root types in Talby.Core
---

## Overview

Add a source generator that turns an annotated root type into the discriminated-union surface currently shown in [Talby.Core.Validation/ValidationPath.cs](../../../Talby.Core.Validation/ValidationPath.cs). The generated surface must stay aligned with the canonical example while replacing the placeholder generator in the source-generator project.

## Scope

### In scope

* `GenerateDiscriminatedUnion` marker discovery.
* Nested case discovery through direct or indirect inheritance from the annotated root.
* Generated `IsXxx` members and exactly two root `Match` overloads.
* `on...` handler parameter names on the generated root `Match` overloads.
* Leaf cases first, then parent or grouping cases in declaration order.
* Branch validation that allows either a grouping handler or the full descendant set on the same branch, but not both.
* One generated file per union root.
* Deterministic ordering and case selection rules.
* A conservative rule for invalid nested partials.

### Out of scope

* New union modeling patterns outside nested partial inheritance.
* Broad validation-model refactors.
* Supporting unrelated or top-level case discovery.
* Adding any root `Match` overloads beyond the two confirmed shapes.

## Requirements

### Marker and discovery

* The generator MUST provide `GenerateDiscriminatedUnion` as generated source from the source-generator project.
* The generator MUST treat `GenerateDiscriminatedUnion` as a marker with no parameters.
* The generator MUST discover annotated root types and inspect their nested types.
* Annotated root types MUST be top-level declarations. A nested annotated root MUST report a diagnostic and MUST NOT emit a generated union file.
* A nested type MUST be treated as a case only when it inherits, directly or indirectly, from the annotated root.
* Abstract or intermediate nested types MUST be recognized in the union tree and MUST expose `IsXxx` members.
* The root partial type MUST expose exactly two `Match` overloads.
* The generated `Match` overloads MUST use `on...` parameter names.
* The generated `Match` overloads MUST order leaf cases first, then parent or grouping cases in declaration order.
* The generated `Match` overloads MUST validate branch coverage so a grouping handler excludes descendant handlers on the same branch.

### Generated surface

* The generated source MUST include XML documentation comments for every generated type and member.
* The root partial type MUST expose `IsXxx` for every discovered nested type in the union tree.
* The root partial type MUST expose exactly two `Match` overloads for the complete branch-aware surface.
* The generated `Match` overloads MUST include all discovered cases, ordered with leaf cases first and parent or grouping cases afterward in declaration order.
* The root partial type MUST use `on...` parameter names for generated `Match` handlers.
* The generated matchers MUST preserve the defensive unknown-type behavior shown by ValidationPath and throw `InvalidOperationException` for unmatched runtime types.
* Generated ordering MUST be deterministic and MUST follow source declaration order within the leaf group and within the parent or grouping group.
* Each annotated union root MUST produce exactly one generated source file.

### Example contract

* `ValidationPath` remains the canonical behavioral reference for this slice.
* The generated surface MUST preserve the current example semantics: `RootPath` as the root case, `ChildPath` as the intermediate grouping case, and `PropertyPath` and `IndexPath` as leaf cases.
* The generated surface MUST preserve `IsRootPath`, `IsChildPath`, `IsPropertyPath`, and `IsIndexPath` while moving the root `Match` handlers to the branch-aware `on...` contract.

## Invalid Nested Partials

This change uses a conservative policy for nested types that are present under an annotated root but do not qualify as cases.

* Such nested types MUST NOT be promoted to union cases.
* Nested annotated roots MUST be rejected with a diagnostic and MUST NOT emit a generated union file.
* If at least one valid case exists, generation MUST continue for the valid cases and ignore the invalid ones for the generated surface.
* If an annotated root resolves to zero valid cases, the generator MUST report a diagnostic and MUST NOT emit a union file for that root.

## Acceptance Scenarios

1. Given a root annotated with `GenerateDiscriminatedUnion` and nested partial derived cases, when the generator runs, then the root receives `IsXxx` members for the union tree and exactly two `Match` overloads for the complete branch-aware surface.
2. Given `ValidationPath` with `RootPath`, `ChildPath`, `PropertyPath`, and `IndexPath`, when the generator runs, then the generated surface matches the branch-aware contract, with `on...` root handlers, leaf cases first, parent or grouping cases afterward, and branch exclusivity enforced.
3. Given a nested partial that inherits indirectly from the annotated root, when the generator runs, then that type is included as a case.
4. Given a nested partial under the annotated root that does not inherit from the root, when the generator runs, then it is ignored for matching; valid cases still generate, and zero valid cases produce a diagnostic instead of a file.
5. Given a nested annotated root, when the generator runs, then it reports a diagnostic and does not emit a generated union file for that root.
6. Given two annotated roots, when the generator runs, then each root gets its own generated file and independent union surface.
7. Given any generated type or member, when the generator runs, then the emitted source includes XML documentation comments for that element.

## Notes

* The placeholder generator must be replaced by the real discriminated-union generator during implementation.
* This spec does not require changing the public validation model beyond the generated union surface.
