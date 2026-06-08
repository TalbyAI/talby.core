---
title: Discriminated Union Source Generator Tasks
description: Implementation tasks for the discriminated union source generator change
---

## Overview

This plan turns the spec and design into an ordered implementation slice for the source generator.

## Tasks

1. [x] Add `GenerateDiscriminatedUnionAttribute.cs` to `Talby.Core.SourceGenerators` and emit the attribute through post-initialization source so consuming projects can use the marker without a runtime dependency.
2. [ ] Replace `TalbyCoreIncrementalGenerator` with a real incremental pipeline in `DiscriminatedUnionGenerator.cs` that discovers annotated roots, walks nested types, and classifies valid cases by direct or indirect inheritance.
3. [ ] Emit the generated union surface with deterministic source-order output, XML documentation for every generated type and member, `IsXxx` members for the full union tree, leaf-only `Match` helpers, and defensive `InvalidOperationException` fallbacks.
4. [ ] Update `Talby.Core.Validation/ValidationPath.cs` to annotate the root with `GenerateDiscriminatedUnion`, remove the handwritten generated region, and keep the root/case model as the canonical example input.
5. [ ] Add `test/Talby.Core.SourceGenerators.UnitTests/DiscriminatedUnionGeneratorTests.cs` covering marker emission, `ValidationPath` output parity, indirect inheritance, invalid nested types, one-file-per-root behavior, deterministic ordering, and XML docs.
6. [ ] Run the focused generator test project, the validation test project, and the source-generator shape checks to confirm the analyzer package still behaves as expected.

## Validation

* `dotnet test test/Talby.Core.SourceGenerators.UnitTests`
* `dotnet test test/Talby.Core.Validation.UnitTests`
* `dotnet build Talby.Core.slnx`
