---
title: Talby.Core Source Generator Tasks
version: 1.0
date_created: 2026-06-07
last_updated: 2026-06-07
tags:
  - tasks
  - design
  - source-generators
  - incremental-generator
  - packaging
  - talby.core
---

# Tasks: Talby.Core Source Generators

## Review Workload Forecast

| Field                   | Value       |
| ----------------------- | ----------- |
| Estimated changed lines | 180-260     |
| 400-line budget risk    | Low         |
| Chained PRs recommended | No          |
| Suggested split         | Single PR   |
| Delivery strategy       | ask-on-risk |
| Chain strategy          | pending     |

Decision needed before apply: No
Chained PRs recommended: No
Chain strategy: pending
400-line budget risk: Low

### Suggested Work Units

| Unit | Goal                                                   | Likely PR | Notes                                                                      |
| ---- | ------------------------------------------------------ | --------- | -------------------------------------------------------------------------- |
| 1    | Stand up the generator project boundary and validation | PR 1      | Keep the slice infrastructure-only and one-way from contracts to generator |

## Phase 1: Project Foundation

- [x] 1.1 Create `Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj` with `net10.0`, nullable, implicit usings, packable analyzer/source-generator metadata, and Roslyn generator references.
- [x] 1.2 Add the minimal incremental generator entry point in `Talby.Core.SourceGenerators/` that implements `IIncrementalGenerator` and only scaffolds the pipeline.

## Phase 2: Solution and Dependency Wiring

- [x] 2.1 Add `Talby.Core.Types/Talby.Core.Types.csproj` and `Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj` to `Talby.Core.slnx`.
- [x] 2.2 Add the one-way project reference from `Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj` to `Talby.Core.Types/Talby.Core.Types.csproj`; keep `Talby.Core.Types` free of any generator reference.

## Phase 3: Packaging and Boundary Validation

- [x] 3.1 Verify the generator project packs as an analyzer/source-generator package shape with `dotnet pack Talby.Core.SourceGenerators/Talby.Core.SourceGenerators.csproj`.
- [x] 3.2 Verify `dotnet build Talby.Core.slnx` succeeds and confirms the contracts project builds without a reverse dependency on the generator project.
- [x] 3.3 Inspect the produced package and project graph to confirm the first slice stops before any emitted feature behavior.
