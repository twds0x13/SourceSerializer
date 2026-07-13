# Managed vs Unmanaged

SourceSerializer selects the strategy at compile time based on `typeof(T)`.

## Unmanaged Path

When `T : unmanaged`, uses single-pass parsing. Span scanner fills fields directly. Zero allocation, Burst-compatible.

Only the unmanaged path is implemented in the current version.

## Managed Path (Planned)

When `T : class` or managed `struct`, uses a two-phase approach:

1. **Walk** — traverse the object graph, assign sequential int IDs
2. **Serialize** — replace reference fields with int IDs

Circular references are handled naturally without graph analysis.

## Selection Guide

| Scenario | Recommendation |
|----------|---------------|
| Numeric structs (Vector3, StatBlock) | Unmanaged |
| Types with strings/lists/object refs | Managed (planned) |

This document is a work in progress.
