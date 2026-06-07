---
title: Context Mode Copilot Guidance
description: Routing guidance for VS Code Copilot when context-mode is enabled in this repository
ms.date: 2026-05-10
ms.topic: reference
---

## Use context-mode for data-heavy work

Use the `context-mode` MCP tools when a task would otherwise pull large amounts of raw data into chat context.

Prefer `ctx_execute`, `ctx_execute_file`, or `ctx_batch_execute` over repeated shell, file, web, or search calls when the goal is to process or summarize data.

Prefer `ctx_fetch_and_index` and `ctx_search` when you need to inspect large web pages, logs, generated output, or documentation and only return the relevant excerpts.

## Keep normal repo workflows intact

Keep using the existing repository guidance in `AGENTS.md` for build commands, GitNexus usage, and repository conventions.

Keep using normal VS Code and MCP tools when the task is small, local, and does not risk flooding the context window.

## Verification commands

If you need to confirm the setup, ask Copilot to run `ctx stats` or `ctx doctor`.