# ADR 001: Backend Language and Framework

## Status
Accepted

## Context
We need a backend that handles deterministic business logic: nutrition calculations, budget math, validation, and data persistence. This backend must be the source of truth and must not rely on AI for calculations.

## Decision
Use **C# 14** with **ASP.NET Core 10** for the backend.

## Consequences

### Positive
- Strong typing prevents an entire class of runtime errors in financial and nutritional calculations.
- EF Core provides robust ORM with PostgreSQL support.
- ASP.NET Core has excellent performance and middleware ecosystem.
- Native JSON serialization and async/await patterns.
- MassTransit has first-class .NET support.
- C# 14 introduces enhanced pattern matching, extension types, and improved performance features.

### Negative
- More verbose than Python or Node.js for rapid prototyping.
- Requires .NET 10 SDK in development environment (mitigated by Nix shell).
- AI agent code (OpenClaw) may use Python, creating a polyglot environment.

## Alternatives Considered
- **Python (FastAPI):** Faster prototyping, but weak typing for deterministic calculations. Rejected.
- **Node.js (NestJS):** Good ecosystem, but JavaScript's floating-point behavior is risky for money and nutrition math. Rejected.
- **Go:** Excellent performance, but less mature ORM ecosystem and more boilerplate. Rejected for v1.
