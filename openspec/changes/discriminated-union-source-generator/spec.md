---
title: Discriminated Union Source Generator Spec
description: Specification for generating discriminated union APIs from annotated root types in Talby.Core
---

## Overview

Add a source generator that turns an annotated root type into the same discriminated-union surface currently shown in [Talby.Core.Validation/ValidationPath.cs](../../../Talby.Core.Validation/ValidationPath.cs). The generated surface must stay aligned with the existing manual example while replacing the placeholder generator in the source-generator project.

## Scope

### In scope

* `GenerateDiscriminatedUnion` marker discovery.
* Nested case discovery through direct or indirect inheritance from the annotated root.
* Generated `IsXxx`, `Match<TResult>`, `Match`, and `MatchXxx` helpers.
* One generated file per union root.
* Deterministic ordering and case selection rules.
* A conservative rule for invalid nested partials.

### Out of scope

* New union modeling patterns outside nested partial inheritance.
* Broad validation-model refactors.
* Supporting unrelated or top-level case discovery.

## Requirements

### Marker and discovery

* The generator MUST treat `GenerateDiscriminatedUnion` as a marker with no parameters.
* The generator MUST discover annotated root types and inspect their nested types.
* A nested type MUST be treated as a case only when it inherits, directly or indirectly, from the annotated root.
* Abstract or intermediate nested types MUST be recognized in the union tree and MUST expose `IsXxx` members.
* Abstract or intermediate nested types MUST NOT appear in `Match<TResult>` or `Match` unless they are leaf cases.

### Generated surface

* The generated source MUST include XML documentation comments for every generated type and member.
* The root partial type MUST expose `IsXxx` for every discovered nested type in the union tree.
* The root partial type MUST expose `Match<TResult>` and `Match` overloads for the leaf cases only.
* The root partial type MUST expose `MatchXxx<TResult>(TResult defaultValue, Func<Xxx, TResult> matchFunc)` and `MatchXxx(Action<Xxx> matchAction, Action? defaultAction = null)` helpers for each leaf case.
* The generated matchers MUST preserve the defensive unknown-type behavior shown by ValidationPath and throw `InvalidOperationException` for unmatched runtime types.
* Generated ordering MUST be deterministic and MUST follow the source declaration order of the nested type tree, with ancestor or grouping types before their descendant leaf cases.
* Each annotated union root MUST produce exactly one generated source file.

### Example contract

* `ValidationPath` remains the canonical behavioral reference for this slice.
* The generated surface MUST preserve the current example semantics: `RootPath` as the root case, `ChildPath` as the intermediate grouping case, and `PropertyPath` and `IndexPath` as leaf cases.
* The generated surface MUST preserve the current naming pattern for `IsRootPath`, `IsChildPath`, `IsPropertyPath`, `IsIndexPath`, `MatchRootPath`, `MatchPropertyPath`, and `MatchIndexPath`.

## Invalid Nested Partials

This change uses a conservative policy for nested types that are present under an annotated root but do not qualify as cases.

* Such nested types MUST NOT be promoted to union cases.
* If at least one valid case exists, generation MUST continue for the valid cases and ignore the invalid ones for the generated surface.
* If an annotated root resolves to zero valid cases, the generator MUST report a diagnostic and MUST NOT emit a union file for that root.

## Acceptance Scenarios

1. Given a root annotated with `GenerateDiscriminatedUnion` and nested partial derived cases, when the generator runs, then the root receives `IsXxx` members for the union tree and match helpers for leaf cases only.
2. Given `ValidationPath` with `RootPath`, `ChildPath`, `PropertyPath`, and `IndexPath`, when the generator runs, then the generated surface matches the current manual region's behavior and naming.
3. Given a nested partial that inherits indirectly from the annotated root, when the generator runs, then that type is included as a case.
4. Given a nested partial under the annotated root that does not inherit from the root, when the generator runs, then it is ignored for matching; valid cases still generate, and zero valid cases produce a diagnostic instead of a file.
5. Given two annotated roots, when the generator runs, then each root gets its own generated file and independent union surface.
6. Given any generated type or member, when the generator runs, then the emitted source includes XML documentation comments for that element.

## Notes

* The placeholder generator must be replaced by the real discriminated-union generator during implementation.
* This spec does not require changing the public validation model beyond the generated union surface.
