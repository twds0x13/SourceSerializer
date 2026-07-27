# `SerializerBlocks`

Serializer block registry. The central cross-assembly registration point — both the SG and hot-reload DLLs register or remove `ISerializerBlock<TData>` implementations via `AddBlock<T>` / `AddBlocks` / `RemoveBlock<T>`.

## Core Interface

```csharp
public interface ISerializerBlock { }  // non-generic marker, enables params ISerializerBlock[]

public interface ISerializerBlock<TData> : ISerializerBlock
{
    int Scan(ReadOnlySpan<char> text, int pos, out TData value);
    void Emit(StringBuilder sb, TData value);
}

public static class SerializerBlocks
{
    public static bool TryGet<TData>(out ISerializerBlock<TData>? block);
    public static string Serialize<TData>(TData value);
    public static TData Deserialize<TData>(string text);
    public static bool TryScan<TData>(string text, out TData value);
}
```

## TryGet

Checks whether type `TData` has a registered serializer block. The first call triggers `EnsureInitialized()`, which scans all loaded assemblies for `GeneratedSerializers.Init()` and registers built-in types.

| Parameter | Type | Description |
|-----------|------|-------------|
| `block` | `out ISerializerBlock<TData>?` | Serializer block; `null` if not registered |
| Return | `bool` | Whether the block was found |

## Serialize / Deserialize / TryScan

Three convenience methods that encapsulate the `TryGet` + `Emit`/`Scan` boilerplate for simple one-liner scenarios.

### Serialize`<T>`

```csharp
public static string Serialize<TData>(TData value);
```

Calls `TryGet<TData>` to obtain the block, executes `Emit` via `StringBuilder`, and returns the resulting string. Throws `InvalidOperationException` for unregistered types.

### Deserialize`<T>`

```csharp
public static TData Deserialize<TData>(string text);
```

Calls `TryGet<TData>` to obtain the block, preprocesses input via `WhitespaceStripper.Strip()`, then executes `Scan`. Throws `FormatException` on scan failure, `InvalidOperationException` for unregistered types.

### TryScan`<T>`

```csharp
public static bool TryScan<TData>(string text, out TData value);
```

Same behavior as `Deserialize` but without exceptions: returns `false` on scan failure or unregistered type, with `value` set to `default`.

Usage:

```csharp
// One-liner serialize
string s = SerializerBlocks.Serialize(new Point2D { X = 3.5f, Y = -2.1f });

// One-liner deserialize (auto whitespace stripping)
Point2D v = SerializerBlocks.Deserialize<Point2D>("  Point2D( 3.5 ,  -2.1 )  ");

// Non-throwing deserialize
if (SerializerBlocks.TryScan<Point2D>(input, out var result))
    Console.WriteLine(result);
```

Design rationale: these three methods, added in v3.4, exist to eliminate the seven-line `TryGet` + null-check + `new StringBuilder` + `Emit` + `ToString` (or `WhitespaceStripper.Strip` + `Scan`) boilerplate. Each method implementation is under 10 lines, delegating directly to `TryGet` and the corresponding `ISerializerBlock<T>` methods.

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

## Builder Fluent API

`AddBlock<T>()` returns a `Builder` nested class instance, enabling fluent chaining:

```csharp
public sealed class Builder
{
    public Builder AddBlock<T>(ISerializerBlock<T> block);
    public Builder AddBlock(Type dataType, ISerializerBlock block);
    public Builder AddBlocks(params ISerializerBlock[] blocks);
    public Builder RemoveBlock<T>();
    public Builder RemoveBlock(Type dataType);
}
```

All `Builder` methods delegate directly to the corresponding `SerializerBlocks` static methods, returning `Builder` itself for further chaining. `Builder` is pure syntactic sugar — it holds no state and is not a standalone registry.

```csharp
SerializerBlocks
    .AddBlock(new Block_Point2D())
    .AddBlock(typeof(Vec3), new Block_Vec3())
    .AddBlocks(new Block_Player(), new Block_Enemy());
```

## ISerializerBlock (Non-Generic Marker)

```csharp
public interface ISerializerBlock { }
```

An empty marker interface whose sole purpose is to allow differently-typed `ISerializerBlock<T>` instances to be received as `params ISerializerBlock[]`. C# does not support `params ISerializerBlock<>[]` (arrays of different generic instantiations have no common base type), so a non-generic marker serves as the upper bound for `ISerializerBlock<T>`.

Hand-written `ISerializerBlock<T>` implementations must also inherit this marker (otherwise `AddBlocks` won't accept them as parameters). SG-generated `Block_Xxx` structs include it automatically.

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

After scanning all `GeneratedSerializers.Init()` methods, `EnsureInitialized()` registers `BuiltinBlock_*` implementations for all 16 built-in types (float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char, string, IntPtr, UIntPtr, Guid), ensuring built-in serialization is always available.

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
