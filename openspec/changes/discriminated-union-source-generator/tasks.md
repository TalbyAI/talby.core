---
title: Discriminated Union Source Generator Tasks
description: Implementation tasks for the discriminated union source generator change
---

## Overview

This plan turns the spec and design into an ordered implementation slice for the source generator.

## Tasks

1. [x] Add `GenerateDiscriminatedUnionAttribute.cs` to `Talby.Core.SourceGenerators` and emit the attribute through post-initialization source so consuming projects can use the marker without a runtime dependency.
2. [x] Consolidate `Talby.Core.SourceGenerators/DiscriminatedUnionGenerator.cs` into a real incremental pipeline.
   - [x] [sequential] Wire the syntax provider so it discovers only annotated root type declarations and projects each one into a per-root model. Spec coverage: marker and discovery, one generated file per union root.
   - [x] [sequential] Traverse nested type declarations in source order and classify each candidate as direct or indirect inheritance from the annotated root while keeping invalid nested types out of the valid-case set. Spec coverage: marker and discovery, invalid nested partials, deterministic ordering.
   - [x] [sequential] Feed the classified root model into the source-output stage so later generation can emit one file per root and report the zero-valid-cases diagnostic only when no valid cases remain. Spec coverage: invalid nested partials, one generated file per union root.
3. [x] Emit the generated union surface with deterministic source-order output, XML documentation for every generated type and member, `IsXxx` members for the full union tree, leaf-only `Match` helpers, and defensive `InvalidOperationException` fallbacks.
   - [x] [sequential] Keep abstract and concrete grouping nodes out of `Match` dispatch while still emitting `IsXxx` for them. Spec coverage: marker and discovery, generated surface, example contract.
4. [x] Update `Talby.Core.Validation/ValidationPath.cs` to annotate the root with `GenerateDiscriminatedUnion`, remove the handwritten generated region, and keep the root/case model as the canonical example input.
5. [x] Add `test/Talby.Core.SourceGenerators.UnitTests/DiscriminatedUnionGeneratorTests.cs` covering marker emission, `ValidationPath` output parity, indirect inheritance, invalid nested types, one-file-per-root behavior, deterministic ordering, and XML docs.
   - [x] [sequential] Add coverage that concrete grouping nodes do not receive `Match` arms and remain observable only through `IsXxx`. Spec coverage: generated surface, example contract.
6. [x] Run the focused generator test project, the validation test project, and the source-generator shape checks to confirm the analyzer package still behaves as expected.

## Validation

* `dotnet test test/Talby.Core.SourceGenerators.UnitTests`
* `dotnet test test/Talby.Core.Validation.UnitTests`
* `dotnet build Talby.Core.slnx`
