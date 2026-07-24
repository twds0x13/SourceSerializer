# Managed vs Unmanaged

SourceSerializer determines type strategy at compile time via Roslyn's `ITypeSymbol`, generating corresponding Scan/Emit code. Both paths fully support serialization and deserialization.

## Unmanaged Path

Types constrained to `T : unmanaged` (pure value structs): the SG generates field-by-field span scanners with `stackalloc`, zero heap allocation, zero GC, Burst-compatible.

```csharp
[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D
{
    public float X;
    public float Y;
}
// Compile-time: public static Scan/Emit methods generated into GeneratedSerializers
```

```csharp
SerializerBlocks.TryGet<Point2D>(out var block);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "Point2D(3.5, -2.1)"
```

## Managed Path

Types with `string`, `class`, `List<T>`, interface references, etc.: the SG generates complete Scan/Emit methods identically. The only difference is allocation strategy:

| Dimension | Meaning | Roslyn Source | Code Gen Impact |
|-----------|---------|--------------|-----------------|
| `NeedsHeapAlloc` | Type is a class | `TypeKind == Class` | `new T()` vs `default` |
| `IsReadonlyStruct` | Readonly struct | `IsReadOnly` | Constructor path vs field assignment |
| `MatchedCtorParams` | Constructor param match | `IMethodSymbol.Parameters` | Name-matched ctor arguments |

Roslyn is the sole compile-time source of truth. `IsUnmanagedType` covers the full C# 7.3+ `unmanaged` constraint definition (including generic specialization). Zero manual rules in the SG.

```csharp
[Template("NamedValue(<string Name>, <float Value>)")]
class NamedValue
{
    public string Name;
    public float Value;
}
// Deserialization: auto new NamedValue(), field-by-field fill
// Serialization: field-by-field Emit_String/Emit_Float

[Template("Inventory(<string Name>, <List<NamedValue> Items>)")]
class Inventory
{
    public string Name;
    public List<NamedValue> Items;
}
// Collection fields via foreach iteration, symmetric with scan direction
```

The managed path has complete Scan and Emit support for class, managed struct, string, List, Dictionary, HashSet, interface references, nested generics, and all field combinations.

## Readonly Structs

```csharp
[Template("ReadOnlyVec(<float X>, <float Y>)")]
readonly struct ReadOnlyVec(float x, float y)
{
    public readonly float X = x;
    public readonly float Y = y;
}
```

The SG auto-discovers constructors whose parameters match fields by name, generating constructor calls instead of field-by-field assignment. Both public and internal constructors are supported.

## Selection Guide

| Scenario | Strategy |
|----------|----------|
| Pure value structs (Vector3, Point2D) | Unmanaged: stack allocation, highest performance |
| Types with string, List, object references | Managed: full support, heap allocation only at field init |
| Readonly structs | Constructor path: auto-matched |
