# SourceSerializer

Attribute-defined schema. Source-generated parser at compile time. Zero reflection, zero boxing.

## Compared to JSON

JSON discovers type structure at runtime via reflection. SourceSerializer does this work at compile time.

| | JSON | SourceSerializer |
|------|------|------|
| Schema | Runtime reflection | Compile-time attribute |
| Parser | Runtime | Compile time |
| Memory | Heap + boxing | `stackalloc`, zero GC |
| Type safety | `object` cast | Strongly typed |
| Circular references | `$ref` patches | Two-phase, native support |

## Quick Start

```csharp
[Template("<float x> <float y>")]
public struct Vec2
{
    public float x;
    public float y;
}

// Scan_Vec2 generated at compile time
SerializerScanners.TryGetScanner<Vec2>(out var scan);
scan("3.5 -2.1".AsSpan(), 0, out Vec2 v);
// v.x == 3.5f, v.y == -2.1f
```
