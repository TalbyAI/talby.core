---
title: Discriminated Union Source Generator Tasks
description: Implementation tasks for the discriminated union source generator change
---

## Overview

Phase 1 documentation updates are complete. These tasks describe the remaining implementation for the confirmed branch-aware discriminated-union contract.

## Tasks

1. [x] Rework `Talby.Core.SourceGenerators/DiscriminatedUnionGenerator.cs` so the root model carries ordered leaf and parent or grouping cases in declaration order.
   - [x] Discover annotated roots and project nested derived cases into a branch-aware root model.
   - [x] Preserve the zero-valid-case diagnostic when no valid cases remain.
2. [x] Rework `Talby.Core.SourceGenerators/DiscriminatedUnionSourceWriter.cs` to emit `on...` parameters, exactly two root `Match` overloads, and branch-exclusivity validation.
   - [x] Keep `IsXxx` members and defensive unknown-type handling intact.
   - [x] Order leaf cases first, then parent or grouping cases in declaration order.
3. [x] Update `Talby.Core.Validation/ValidationPath.cs` to remain the canonical sample while relying on the generated branch-aware contract instead of the handwritten dispatch surface.
4. [x] Update generator-output tests so they assert the two-root-Match model, `on...` naming, and leaf-first / parent-last ordering.
   - [x] Add negative coverage for mixed parent-plus-child handlers on the same branch.
   - [x] Add coverage for zero-valid-case handling.
5. [x] Keep the source-generator project-shape checks aligned with analyzer-only packaging expectations.
6. [x] Run the focused generator, validation, and solution builds once the implementation lands.

## Validation

* `dotnet test test/Talby.Core.SourceGenerators.UnitTests`
* `dotnet test test/Talby.Core.Validation.UnitTests`
* `dotnet build Talby.Core.slnx`
