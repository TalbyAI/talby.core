---
title: Talby.Core Source Generator Design
version: 1.0
date_created: 2026-06-07
last_updated: 2026-06-07
tags:
  - design
  - source-generators
  - incremental-generator
  - packaging
  - talby.core
---

# Talby.Core Source Generator Design

## 1. Purpose

This document defines the smallest viable implementation design for the first source-generator slice in Talby.Core. The design follows [spec/spec-architecture-talby-core-source-generators.md](spec/spec-architecture-talby-core-source-generators.md) as the source of truth and intentionally stops short of any production emitted source.

## 2. First-Slice Boundary

The first slice is infrastructure-only.

In scope:

- Add a dedicated generator project beside `Talby.Core.Types`.
- Make the generator project an incremental generator by implementing `IIncrementalGenerator`.
- Make the generator project packable as an analyzer/source-generator NuGet package.
- Add both `Talby.Core.Types` and the generator project to the solution.
- Prove the contracts-to-generator dependency direction is one way only.
- Validate build and package shape.

Out of scope:

- Any domain-specific generated APIs.
- Any emitted source files beyond a minimal generator scaffold.
- Any runtime dependency from contracts back to generator infrastructure.
- Any consumer integration beyond package shape validation.

## 3. Proposed Project Layout

Use two stable projects for the generator boundary:

- `Talby.Core.Types` for shared contracts only.
- `Talby.Core.SourceGenerators` for all generator implementation and package metadata.

The solution should include both projects so the relationship is validated in one build. `Talby.Core.Validation` remains as-is and is not part of this slice except where it already participates in the solution.

Recommended repository shape:

- `Talby.Core.Types/`
- `Talby.Core.SourceGenerators/`
- `Talby.Core.Validation/`
- `test/...` for future generator tests when the first emitted behavior exists

## 4. Dependency Direction

The dependency model is intentionally one-way:

- `Talby.Core.Types` depends on only shared libraries and other contract-level code.
- `Talby.Core.SourceGenerators` may reference `Talby.Core.Types` if future generation logic needs shared contract types.
- `Talby.Core.Types` must not reference `Talby.Core.SourceGenerators`.
- Consumer projects reference `Talby.Core.Types` for contracts and the generator package for compile-time generation, as separate dependencies.

This preserves contracts as the stable surface and keeps generator infrastructure reusable outside the repository.

## 5. Generator Architecture

The generator project should use the Roslyn incremental model and expose a single incremental generator entry point:

- Implement `IIncrementalGenerator`.
- Keep the initializer intentionally minimal in the first slice.
- Avoid classic full-rewrite generator patterns.
- Do not emit domain source until the packaging and build boundary are verified.

The design goal is to lock in the generator model early so later slices can extend an incremental pipeline instead of migrating from a different Roslyn model.

## 6. Packaging Shape

The generator project should be packaged as an analyzer/source-generator NuGet package, not as a runtime library.

Minimal package shape expectations:

- Pack the generator project as a reusable NuGet package.
- Mark it as analyzer/source-generator oriented so downstream builds load it at compile time.
- Keep the package publishable outside this repository.
- Keep `Talby.Core.Types` as a normal contracts package/project, not as part of the generator package.

Likely project metadata needs for the generator project are:

- `IsPackable=true`
- analyzer/source-generator package metadata such as `PackageType=Analyzer` or equivalent packaging configuration
- build output packaged for compiler consumption, not runtime execution
- a package identity that is stable enough for downstream reuse

The exact packaging implementation can stay minimal in the first slice as long as the produced package restores as a compiler-time analyzer package.

## 7. Solution Membership

Current state verification from the repository shows:

- `Talby.Core.Validation` is already in `Talby.Core.slnx`.
- `Talby.Core.Types` exists as a project but is not yet in the solution.
- No generator project exists yet.

The solution update for this slice should therefore:

1. Add `Talby.Core.Types` to `Talby.Core.slnx`.
2. Add `Talby.Core.SourceGenerators` to `Talby.Core.slnx`.
3. Leave `Talby.Core.Validation` intact.

That keeps the new boundary explicit and avoids hidden project drift.

## 8. Implementation Order

Build the slice in this order:

1. Add the generator project with the repository baseline: `net10.0`, nullable enabled, implicit usings enabled.
2. Add the minimal incremental generator entry point implementing `IIncrementalGenerator`.
3. Configure packing so the project can be consumed as an analyzer/source-generator package.
4. Add solution membership for `Talby.Core.Types` and the generator project.
5. Verify that `Talby.Core.Types` has no reference to the generator project.
6. Validate solution build and package output.

This order keeps the first slice reversible and makes the project boundary visible before any feature behavior exists.

## 9. Validation Strategy

First-slice validation should focus on infrastructure rather than generated output.

Primary checks:

- Build the full solution successfully.
- Confirm the generator project compiles as an incremental generator.
- Confirm the generator package can be produced as an analyzer/source-generator package.
- Confirm `Talby.Core.Types` stays free of generator references.

Useful commands for the first slice are:

- `dotnet build Talby.Core.slnx`
- `dotnet pack` for the generator project once packaging metadata is in place

No emitted-source snapshot tests are required yet because there is no production generator output in scope.

## 10. Risks and Tradeoffs

The main tradeoff is compatibility versus minimality. Keeping the generator project aligned with the repository baseline is the smallest change, but a future compatibility review may decide to target a broader analyzer-friendly TFM if downstream reuse demands it.

Other risks:

- Packaging settings can look correct in the project file but still fail at restore or pack time if the analyzer shape is incomplete.
- A later feature slice could accidentally introduce a reverse dependency from `Talby.Core.Types` back to the generator project if the boundary is not enforced now.
- A no-op first slice can feel thin, but that is intentional: it validates the hard infrastructure edges before any emitted source exists.

## 11. Decision Summary

- The generator lives in its own project: `Talby.Core.SourceGenerators`.
- The generator is incremental and implements `IIncrementalGenerator`.
- The generator is packable as an analyzer/source-generator package.
- `Talby.Core.Types` remains contracts-only and never references the generator.
- The solution should include both projects before feature generation begins.
- The first slice ends at build-and-pack validation, with no production emitted source.
