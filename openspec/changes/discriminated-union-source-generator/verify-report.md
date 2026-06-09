---
title: Discriminated Union Source Generator Verify Report
description: Verification report for the discriminated-union-source-generator change
---

## Verification Report

**Change**: discriminated-union-source-generator
**Mode**: Standard verify
**Status**: PASS

### Completeness

| Item             | Result   | Evidence                                                                                                                       |
| ---------------- | -------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Tasks            | Complete | `tasks.md` marks top-level tasks 1-6 complete, matching the branch-aware source-generator, validation, tests, project-shape checks, and build tasks |
| Spec coverage    | Complete | Generator, invalid nested policy, deterministic ordering, one file per root, XML docs, and `ValidationPath` parity are covered |
| Build            | Passed   | `dotnet build Talby.Core.slnx --nologo`                                                                                        |
| Generator tests  | Passed   | `dotnet test test/Talby.Core.SourceGenerators.UnitTests/Talby.Core.SourceGenerators.UnitTests.csproj --nologo`                 |
| Validation tests | Passed   | `dotnet test test/Talby.Core.Validation.UnitTests/Talby.Core.Validation.UnitTests.csproj --nologo`                             |
| Coverage         | Not run  | No repo-specific coverage command was required for this verify pass                                                            |

### Spec Compliance Matrix

| Requirement             | Result | Evidence                                                                                                                                                            |
| ----------------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Marker emission         | PASS   | `GenerateDiscriminatedUnionAttribute` is emitted as post-initialization source                                                                                      |
| Nested discovery        | PASS   | `DiscriminatedUnionGenerator` walks nested types and accepts direct and indirect inheritance from the annotated root                                                |
| Invalid nested partials | PASS   | Non-derived nested types are ignored when valid cases exist; roots with zero valid cases report `TCSG001` and emit no file                                          |
| Generated surface       | PASS   | `DiscriminatedUnionSourceWriter` emits `IsXxx`, root `Match<TResult>`/`Match`, leaf `MatchXxx<TResult>`, leaf `MatchXxx`, and `InvalidOperationException` fallbacks |
| Ordering                | PASS   | Tests assert source-order output for `ValidationPath` and indirect inheritance shapes                                                                               |
| XML docs                | PASS   | Tests assert summary and doc content on generated members                                                                                                           |
| One file per root       | PASS   | Tests confirm separate generated outputs for two annotated roots                                                                                                    |

### Correctness Table

| Area             | Result  | Notes                                                                  |
| ---------------- | ------- | ---------------------------------------------------------------------- |
| Runtime behavior | Correct | Generator output and validation model both build and test successfully |
| Diagnostics      | Correct | `TCSG001` is covered for zero-valid-case roots                         |
| Determinism      | Correct | Ordering is stable in tests and implementation                         |
| API shape        | Correct | The generated surface matches the manual `ValidationPath` contract     |

### Design Coherence

| Decision                            | Result  | Notes                                                                      |
| ----------------------------------- | ------- | -------------------------------------------------------------------------- |
| Marker-first incremental pipeline   | Aligned | The generator emits the attribute and uses syntax-provider discovery       |
| One file per root                   | Aligned | Each annotated root produces its own generated source file                 |
| Conservative invalid-nested policy  | Aligned | Invalid nested types are ignored unless no valid cases remain              |
| ValidationPath as canonical example | Aligned | `ValidationPath` is annotated and still serves as the behavioral reference |

### Issues

None.

### Verdict

PASS
