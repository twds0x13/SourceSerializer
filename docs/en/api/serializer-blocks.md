# `SerializerBlocks`

Serializer block registry. The central cross-assembly registration point — both the SG and hot-reload DLLs register or remove `ISerializerBlock<TData>` implementations via `AddBlock<T>` / `AddBlocks` / `RemoveBlock<T>`.

## Core Interface

```csharp
public interface ISerializerBlock<TData>
{
    int Scan(ReadOnlySpan<char> text, int pos, out TData value);
    void Emit(StringBuilder sb, TData value);
}

public static class SerializerBlocks
{
    public static bool TryGet<TData>(out ISerializerBlock<TData>? block);
    public static string Serialize<TData>(TData value);
    public static TData Deserialize<TData>(string text);
}
```

## TryGet

Checks whether type `TData` has a registered serializer block. The first call triggers `EnsureInitialized()`, which scans all loaded assemblies for `GeneratedSerializers.Init()` and registers built-in types.

| Parameter | Type | Description |
|-----------|------|-------------|
| `block` | `out ISerializerBlock<TData>?` | Serializer block; `null` if not registered |
| Return | `bool` | Whether the block was found |

## AddBlock

```csharp
public static Builder AddBlock<T>(ISerializerBlock<T> block);
public static Builder AddBlock(Type dataType, ISerializerBlock block);
```

Registers a serializer block. The generic overload is called directly; the non-generic overload is for hot-reload scenarios where the caller does not hold the type at compile time.

**Interface chain merge**: when `typeof(T).IsInterface`, multiple registrations are **appended** to a dispatch chain rather than overwriting. This allows different assemblies to each generate their own interface dispatch block, with runtime auto-merge into a `ChainBlock<T>`. Non-interface types retain standard overwrite semantics.

**Example**:

```csharp
// Server assembly registers IVector dispatch block (knows Vec2 and Vec3)
GeneratedSerializers.Init();
// → AddBlock<IVector>(Block_IVector{Vec2, Vec3})

// Hot-reload DLL registers its own IVector dispatch block (knows Vec6)
DLL.GeneratedSerializers.Init();
// → AddBlock<IVector>(Block_IVector{Vec6})
// → Chain merged: ChainBlock{ Block_IVector{Vec2,Vec3}, Block_IVector{Vec6} }
// Deserializing "Vec6(1,2,3,4,5,6)": try Vec2/Vec3 → no match → try Vec6 → match
```

## RemoveBlock

```csharp
public static void RemoveBlock<T>();
public static void RemoveBlock(Type dataType);
```

Removes the registration for a type. For interface types, removes the entire dispatch chain. Silently succeeds when not registered.

## AddBlocks

```csharp
public static void AddBlocks(params ISerializerBlock[] blocks);
```

Batch-registers heterogeneous blocks. Each block's generic parameter is derived via reflection, delegating to `RegisterBlock<T>` to reuse chain merge logic.

## GeneratedSerializers Initialization

The SG generates `public static partial class GeneratedSerializers` at compile time, containing `Scan_Xxx` / `Emit_Xxx` methods for all user types plus a registration entry point:

```csharp
public static partial class GeneratedSerializers
{
    public static void Init()
    {
        // Idempotent: second call is a no-op
        SerializerBlocks.AddBlock<Point2D>(new Block_Point2D());
        SerializerBlocks.AddBlock<IVector>(new Block_IVector());
        // ... all types
    }
}
```

`Init()` is called automatically by `EnsureInitialized()` on first `TryGet<T>` via AppDomain reflection scan. Hot-reload DLL entry points should explicitly call their own `GeneratedSerializers.Init()`.

## Built-in Type Registration

After scanning all `GeneratedSerializers.Init()` methods, `EnsureInitialized()` registers `BuiltinBlock_*` implementations for all 13 built-in types (float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char, string), ensuring built-in serialization is always available.

## Internal Implementation

Each type receives a SG-generated `public readonly struct` implementing `ISerializerBlock<T>`:

```csharp
public readonly struct Block_Point2D : ISerializerBlock<Point2D>
{
    public int Scan(ReadOnlySpan<char> t, int p, out Point2D v) =>
        GeneratedSerializers.Scan_Point2D(t, p, out v);

    public void Emit(StringBuilder sb, Point2D v) =>
        GeneratedSerializers.Emit_Point2D(sb, v);
}
```

## See Also

- [Template Attribute](./template-attribute)
- [ExternalTemplate Attribute](./external-template-attribute)
- [SerializerRegistry](./serializer-registry)
