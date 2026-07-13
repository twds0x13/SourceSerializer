# Managed vs Unmanaged

SourceSerializer selects the strategy at compile time based on `typeof(T)`.

## Unmanaged Path

When `T : unmanaged`, uses single-pass parsing.

The source generator emits a `Scan_TypeName` method that receives a `ReadOnlySpan<char>`, fills a stack-allocated struct instance field by field. Zero heap allocation, Burst-compatible.

```csharp
[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}
// Compile-time generation:
// internal static int Scan_Point2D(ReadOnlySpan<char> src, int pos, out Point2D value)
```

The unmanaged path is implemented in the current version.

## Managed Path

When `T : class` or managed struct (containing string, List, object references, etc.), uses a two-phase strategy:

1. **Walk**: traverse the object graph, assign sequential int IDs to each object
2. **Serialize**: replace reference fields with int IDs

Circular references are handled natively without `$ref` annotations or graph analysis.

The managed path is planned for a future version.

## Selection Guide

| Scenario | Strategy |
|----------|----------|
| Pure numeric structs (Vector3, StatBlock, Point2D) | Unmanaged |
| Types with string, List, object references | Managed (planned) |
