# Example: Interface Auto-Dispatch

Use an interface name directly in a template when multiple implementations exist. The SG collects all implementations at compile time and auto-dispatches to the matching type at runtime.

## Basic Interface Dispatch

```csharp
public interface IVector { }

[Template("Vec2(<float X>, <float Y>)")]
public struct Vec2 : IVector
{
    public float X;
    public float Y;
}

[Template("Vec3(<float X>, <float Y>, <float Z>)")]
public struct Vec3 : IVector
{
    public float X;
    public float Y;
    public float Z;
}

[Template("<IVector V>")]
public struct VectorWrapper
{
    public IVector V;
}
```

```csharp
SerializerBlocks.TryGet<VectorWrapper>(out var block);

// Input matches Vec2
block.Scan("Vec2(1.5, -2)".AsSpan(), 0, out var v1);
// v1.V is Vec2 { X = 1.5f, Y = -2f }

// Input matches Vec3
block.Scan("Vec3(3, 5, 7)".AsSpan(), 0, out var v2);
// v2.V is Vec3 { X = 3f, Y = 5f, Z = 7f }
```

Scan tries each implementation in declaration order; the first one that advances `pos` wins. Emit uses switch pattern matching for runtime type dispatch.

## Interface Collections (Heterogeneous Lists)

```csharp
[Template("<float Base><optional>, <List<IVector> Targets></optional>")]
public struct Attack
{
    public float Base;
    public List<IVector> Targets;
}
```

```csharp
SerializerBlocks.TryGet<Attack>(out var block);

block.Scan("100, List(Vec2(3, 5), Vec3(1, 2, 3))".AsSpan(), 0, out var v);
// v.Base == 100f
// v.Targets[0] is Vec2 { X = 3f, Y = 5f }
// v.Targets[1] is Vec3 { X = 1f, Y = 2f, Z = 3f }
```

Different implementation types can coexist in the same collection. The SG-generated interface dispatch block independently tries all implementations for each element.

## Cross-Assembly Chain Merge

Interface blocks registered from different assemblies automatically merge into a `ChainBlock<T>` dispatch chain:

```csharp
// Main assembly: SG generates Block_IVector{Vec2, Vec3}
GeneratedSerializers.Init();

// After hot-reload DLL loads: SG generates Block_IVector{Vec6}
DLL.GeneratedSerializers.Init();
// Interface type auto chain-merge: ChainBlock{ Block_IVector{Vec2,Vec3}, Block_IVector{Vec6} }
// "Vec6(1,2,3,4,5,6)" → try Vec2/Vec3 first → no match → try Vec6 → succeeds
```

See `InterfaceDispatchTests.cs` and `ChainBlockTests.cs` for complete runnable test cases.

## See Also

- [Template Syntax](../guide/template-syntax): the five primitives and template declarations
- [Hot Reload & Cross-Assembly Registration](../guide/hot-reload): full ChainBlock chain merge mechanism
- [Internals](../technical/internals): source-level details on interface dispatch and ChainBlock
