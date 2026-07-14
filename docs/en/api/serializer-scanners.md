# `SerializerScanners`

Partial class. The source generator injects generated scanner methods and registers delegates.

## Signature

```csharp
internal static partial class SerializerScanners
{
    public static bool TryGetScanner<T>(out ScannerDelegate<T> scanner);
}

public delegate int ScannerDelegate<T>(ReadOnlySpan<char> src, int pos, out T value);
```

## TryGetScanner

Checks whether type `T` has a scanner registered via `[Template]`.

| Parameter | Type | Description |
|-----------|------|-------------|
| `scanner` | `out ScannerDelegate<T>` | Scanner delegate, `null` if not registered |
| Returns | `bool` | Whether a scanner was successfully obtained |

## ScannerDelegate

Source-generated scanner methods match the signature of built-in type scanners:

| Parameter | Type | Description |
|-----------|------|-------------|
| `src` | `ReadOnlySpan<char>` | Input character span |
| `pos` | `int` | Start parse position |
| `value` | `out T` | Parse result, `default` on failure |
| Returns | `int` | End position after parsing, `== pos` on failure |

## Internal Registration Mechanism

The source generator registers scanners via a static constructor in the generated `SerializerScanners.g.cs`:

```csharp
static SerializerScanners()
{
    ScannerRegistry<Point2D>.Scanner = (src, pos, out Point2D v) => {
        int r = Scan_Point2D(src, pos, out v);
        return r;
    };
}
```

`ScannerRegistry<T>` is an internal generic static field container holding one `ScannerDelegate<T>` instance per `T`.
