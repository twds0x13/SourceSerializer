# `SerializerBlocks`

Serializer block registry. Each `[Template]` type receives a compile-time generated `ISerializerBlock<T>` implementation, registered automatically. Provides bidirectional scan (deserialization) and emit (serialization) from a single lookup.

## Signature

```csharp
public interface ISerializerBlock<TData>
{
    int Scan(ReadOnlySpan<char> text, int pos, out TData value);
    void Emit(StringBuilder sb, TData value);
}

internal static partial class SerializerBlocks
{
    public static bool TryGet<TData>(out ISerializerBlock<TData> block);
    public static string Serialize<TData>(TData value);
    public static TData Deserialize<TData>(string text);
}
```

## TryGet

Checks whether type `TData` has a registered serializer block via `[Template]`.

| Parameter | Type | Description |
|-----------|------|-------------|
| `block` | `out ISerializerBlock<TData>` | Serializer block, `null` if unregistered |
| Returns | `bool` | Whether the block was found |

## ISerializerBlock

### Scan

| Parameter | Type | Description |
|-----------|------|-------------|
| `text` | `ReadOnlySpan<char>` | Input character span |
| `pos` | `int` | Start position for parsing |
| `value` | `out TData` | Parsed result, `default` on failure |
| Returns | `int` | Position after parsing, `== pos` on failure |

### Emit

| Parameter | Type | Description |
|-----------|------|-------------|
| `sb` | `StringBuilder` | Output target |
| `value` | `TData` | Value to serialize |

## Internal Implementation

The SG generates a `readonly struct` per type and registers it with `SerializerBlocks`:

```csharp
readonly struct Block_Point2D : ISerializerBlock<Point2D>
{
    public int Scan(ReadOnlySpan<char> t, int p, out Point2D v) =>
        SerializerBlocks.Scan_Point2D(t, p, out v);

    public void Emit(StringBuilder sb, Point2D v) =>
        SerializerBlocks.Emit_Point2D(sb, v);
}
```

`BlockRegistry<TData>` is an internal generic static field container. `TryGet<T>` reads this field — zero dictionary lookup overhead.

## See Also

- [Template Attribute](./template-attribute)
- [ExternalTemplate Attribute](./external-template-attribute)
- [SerializerRegistry](./serializer-registry)
