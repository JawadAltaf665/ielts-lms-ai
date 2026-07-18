# Contributing to IELTS LMS AI

Thank you for your interest in contributing to **IELTS LMS AI**. This document explains how we collaborate so that contributions are consistent, reviewable, and safe to ship.

> **Note:** The project is currently in the Architecture & Planning phase. Until the codebase is scaffolded, most contributions will be to documentation under [`/docs`](./docs).

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Workflow](#development-workflow)
- [Branching Strategy](#branching-strategy)
- [Commit Conventions](#commit-conventions)
- [Pull Requests](#pull-requests)
- [Coding Standards](#coding-standards)
- [Documentation](#documentation)
- [Reporting Issues](#reporting-issues)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](./CODE_OF_CONDUCT.md). By participating, you are expected to uphold it.

## How to Contribute

1. Check existing [issues](../../issues) and discussions before starting work.
2. For significant changes, open an issue first to discuss the approach.
3. Fork the repository (or create a branch if you have write access).
4. Make your changes following the standards below.
5. Open a pull request using the provided template.

## Development Workflow

1. Sync with the `main` branch.
2. Create a feature branch (see [Branching Strategy](#branching-strategy)).
3. Make focused, incremental commits.
4. Ensure builds, tests, and linters pass locally (once tooling exists).
5. Open a pull request and request review.

## Branching Strategy

- `main` — always deployable; protected.
- `feature/<short-description>` — new features.
- `fix/<short-description>` — bug fixes.
- `docs/<short-description>` — documentation-only changes.
- `chore/<short-description>` — tooling, config, and maintenance.

Never commit directly to `main`.

## Commit Conventions

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<optional scope>): <description>

[optional body]

[optional footer(s)]
```

Common types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.

Example:

```
docs(architecture): add multi-tenancy strategy draft
```

## Pull Requests

- Keep PRs small and focused on a single concern.
- Fill out the [pull request template](./.github/PULL_REQUEST_TEMPLATE.md) completely.
- Link related issues (e.g., `Closes #123`).
- Ensure CI passes and at least one reviewer approves before merge.
- Squash-merge unless there is a reason to preserve individual commits.

## Coding Standards

- Respect the repository [`.editorconfig`](./.editorconfig).
- Frontend: TypeScript with the project ESLint/Prettier configuration (once added).
- Backend: C#/.NET with the project analyzer and formatting rules (once added).
- Write tests for new behavior and keep existing tests green.
- Prefer clear, self-documenting code with meaningful names.

## Documentation

- Architecture documents live in [`/docs`](./docs).
- Update relevant documentation alongside code changes.
- Use clear headings, tables, and diagrams where helpful.

## Reporting Issues

Use the [issue templates](./.github/ISSUE_TEMPLATE) to report bugs or request features. For security vulnerabilities, **do not** open a public issue — follow [SECURITY.md](./SECURITY.md).
