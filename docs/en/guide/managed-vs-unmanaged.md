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

The serialization direction is also available. The source generator simultaneously emits an `Emit_Point2D` method that writes the instance to a `StringBuilder`:

```csharp
SerializerEmitters.TryGetEmitter<Point2D>(out var emit);
var sb = new StringBuilder();
emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "3.5 -2.1"
```

Both scanning and emitting are implemented for the unmanaged path.

## Managed Path

When `T : class` or managed struct (containing string, List, object references, etc.), different handling applies.

### Deserialization (Implemented)

The Source Generator uses Roslyn's `ITypeSymbol.IsUnmanagedType` as the authoritative source of truth, splitting into two orthogonal dimensions:

| Dimension | Meaning | Roslyn Source | Codegen Impact |
|-----------|---------|---------------|----------------|
| `NeedsHeapAlloc` | Is the type a class? | `TypeKind == Class` | `new T()` vs `default` |

Roslyn is the single source of truth. `IsUnmanagedType` covers the full definition of C# 7.3+ `unmanaged` constraints (including generic specialization), requiring zero lines of manual rules in the SG.

### Serialization Direction (Partially Implemented)

Serialization for the unmanaged path is fully implemented. The source generator uses `EmitCodeEmitter` to produce `SerializerEmitters.g.cs`, supporting:

- Literal text output
- Field serialization (built-in types, custom nested types, enum tags)
- Optional blocks (output when field is non-default)

Two-phase serialization for the managed path is still planned:

1. **Walk**: traverse the object graph, assign sequential int IDs to each object
2. **Serialize**: replace reference fields with int IDs

Circular references are handled natively without `$ref` annotations or graph analysis. Type strategy is determined at compile time via Roslyn `IsUnmanagedType`.

`<repetition>` block serialization uses `foreach` iteration over collection elements, symmetric with the scanning direction.

## Selection Guide

| Scenario | Strategy |
|----------|----------|
| Pure numeric structs (Vector3, StatBlock, Point2D) | Unmanaged: both scan and emit complete |
| Types with string, List, object references | Managed: deserialization available, serialization partially available |
