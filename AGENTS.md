---
title: Talby.Core agent instructions
description: Concise workspace guidance for AI coding agents working in the Talby.Core repository
ms.date: 2026-05-10
ms.topic: reference
---

## Start here

* Build the solution with `dotnet build Talby.Core.slnx`.
* Run the Aspire app host with `dotnet run --project Talby.Core.AppHost`.
* Do not run `dotnet run` from the repository root. The root has a solution file, not a runnable project.
* Restore the repo-local dotnet tools with `dotnet tool restore`.
* Run the C# formatter with `dotnet csharpier format .` and validate formatting with `dotnet csharpier check .`.
* Check for outdated NuGet dependencies with `dotnet outdated`.
* Install `context-mode` globally with `npm i -g context-mode` before using the repo-local Copilot MCP and hook setup.
* No test projects exist yet. If you add tests, create a dedicated test project instead of placing test code in the existing projects.
* Always use the caveman skill for concise and brief explanations.

## Copilot context-mode

* The shared VS Code Copilot MCP config lives in [.vscode/mcp.json](.vscode/mcp.json). It registers both `gitnexus` and `context-mode`.
* The shared VS Code Copilot hook config lives in [.github/hooks/context-mode.json](.github/hooks/context-mode.json).
* Optional routing guidance for Copilot lives in [.github/copilot-instructions.md](.github/copilot-instructions.md).
* After cloning the repo and installing `context-mode`, restart VS Code, run `MCP: List Servers`, and confirm both servers appear.
* In Copilot Chat, type `ctx stats` or `ctx doctor` to verify the `context-mode` tools are reachable.

## Project map

* [Talby.Core.AppHost/AppHost.cs](Talby.Core.AppHost/AppHost.cs) is the Aspire entry point. It currently only builds and runs the distributed application host.
* [Talby.Core.ServiceDefaults/Extensions.cs](Talby.Core.ServiceDefaults/Extensions.cs) contains the reusable defaults for service discovery, HTTP resilience, OpenTelemetry, and development health endpoints.
* [Talby.Core.AppHost/Properties/launchSettings.json](Talby.Core.AppHost/Properties/launchSettings.json) defines the local Aspire dashboard and resource service endpoints used during development.

## Working conventions

* This repository targets `net10.0` with nullable reference types and implicit usings enabled in both projects.
* When you add an application or API project, reference `Talby.Core.ServiceDefaults` and call `AddServiceDefaults()` in that service's startup path.
* If the service is an ASP.NET Core app, use `MapDefaultEndpoints()` for the development health endpoints defined in [Talby.Core.ServiceDefaults/Extensions.cs](Talby.Core.ServiceDefaults/Extensions.cs).
* OpenTelemetry OTLP export is conditional. It only turns on when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured.
* Health endpoints are intentionally excluded from tracing. Keep `/health` and `/alive` out of normal request telemetry unless you have a strong reason to change that behavior.
* When you refresh the GitNexus index manually, run `gitnexus analyze --no-stats` to avoid churn in generated instruction files.

## Environment notes

* [Talby.Core.AppHost/Talby.Core.AppHost.csproj](Talby.Core.AppHost/Talby.Core.AppHost.csproj) uses `Aspire.AppHost.Sdk` and stores local secrets through the configured `UserSecretsId`.
* The launch profiles in [Talby.Core.AppHost/Properties/launchSettings.json](Talby.Core.AppHost/Properties/launchSettings.json) assume a local Aspire development environment. Check those endpoints first if the app host starts but the dashboard integration is missing.

## First files to inspect

* [Talby.Core.slnx](Talby.Core.slnx)
* [Talby.Core.AppHost/AppHost.cs](Talby.Core.AppHost/AppHost.cs)
* [Talby.Core.ServiceDefaults/Extensions.cs](Talby.Core.ServiceDefaults/Extensions.cs)
* [Talby.Core.AppHost/Properties/launchSettings.json](Talby.Core.AppHost/Properties/launchSettings.json)

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **Talby.Core**. Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/Talby.Core/context` | Codebase overview, check index freshness |
| `gitnexus://repo/Talby.Core/clusters` | All functional areas |
| `gitnexus://repo/Talby.Core/processes` | All execution flows |
| `gitnexus://repo/Talby.Core/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
