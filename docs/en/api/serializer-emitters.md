# `SerializerEmitters`

Partial class. The source generator injects generated serialization methods and registers delegates. It mirrors `SerializerScanners` (deserialization direction) and shares the same `[Template]` declarations.

## Signature

```csharp
internal static partial class SerializerEmitters
{
    public static bool TryGetEmitter<T>(out EmitterDelegate<T> emitter);
}

public delegate void EmitterDelegate<T>(StringBuilder sb, T value);
```

`EmitterDelegate<T>` is defined in `SerializerScanners.cs`, alongside `ScannerDelegate<T>`.

## TryGetEmitter

Checks whether type `T` has a serializer registered via `[Template]`.

| Parameter | Type | Description |
|-----------|------|-------------|
| `emitter` | `out EmitterDelegate<T>` | The serializer delegate, or `null` if not registered |
| Return | `bool` | Whether the emitter was successfully obtained |

## EmitterDelegate

Unlike `ScannerDelegate<T>`, `EmitterDelegate<T>` does not return position information. Serialization has no concept of "match failure": given a valid instance, output always succeeds.

| Parameter | Type | Description |
|-----------|------|-------------|
| `sb` | `StringBuilder` | Output target buffer |
| `value` | `T` | The data instance to serialize |

## Internal Registration Mechanism

The source generator registers serializers via a static constructor in the generated `SerializerEmitters.g.cs`:

```csharp
static SerializerEmitters()
{
    EmitterRegistry<Point2D>.Emitter = (StringBuilder s, Point2D v) =>
    {
        Emit_Point2D(s, v);
    };
}
```

`EmitterRegistry<T>` is an internal generic static field container. Each `T` holds a single `EmitterDelegate<T>` instance.

## Built-in Type Emit Methods

`SerializerRegistry` provides hand-written zero-allocation serialization methods for 12 built-in types, corresponding to the scanner methods:

| Type | Emit Method |
|------|------------|
| `float` | `Emit_Float` |
| `double` | `Emit_Double` |
| `int` | `Emit_Int` |
| `uint` | `Emit_Uint` |
| `long` | `Emit_Long` |
| `ulong` | `Emit_Ulong` |
| `short` | `Emit_Short` |
| `ushort` | `Emit_Ushort` |
| `byte` | `Emit_Byte` |
| `sbyte` | `Emit_Sbyte` |
| `bool` | `Emit_Bool` |
| `char` | `Emit_Char` |
| `string` | `Emit_String` |

All Emit methods follow a uniform signature:

```csharp
internal static void Emit_Xxx(StringBuilder sb, Xxx value)
```

## Enum Tag Emit

For enums annotated with `[Tag]`, the source generator also produces switch-on-member Emit methods:

```csharp
internal static void Emit_Enum_Element(StringBuilder sb, Element value)
{
    switch (value)
    {
        case Element.Fire:  sb.Append("fire"); break;
        case Element.Ice:   sb.Append("ice"); break;
        case Element.Magic: sb.Append("magic"); break;
        default: sb.Append(value.ToString()); break;
    }
}
```

## Usage Example

```csharp
[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}

SerializerEmitters.TryGetEmitter<Point2D>(out var emit);
var sb = new StringBuilder();
emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
Console.WriteLine(sb.ToString()); // "3.5 -2.1"
```

## Limitations

`<repetition>` blocks use `foreach` iteration: first element uses separator-free pattern, subsequent elements use separator-included pattern. Both scanning and emitting are implemented for collection types.

## See Also

- [`SerializerScanners`](./serializer-scanners): Deserialization direction
- [`SerializerRegistry`](./serializer-registry): Built-in type scanners and emitters
- [`[Template]`](./template-attribute): Template declaration
