# Getting Started

Declare structs with attributes; the source generator produces zero-allocation parsers at compile time.

## Installation

Add to `manifest.json`:

```json
"com.twds0x13.sourceserializer": "https://github.com/twds0x13/SourceSerializer.git#main"
```

## Declare Your First Template

```csharp
using SourceSerializer;

[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}
```

## Use the Generated Parser

```csharp
SerializerScanners.TryGetScanner<Point2D>(out var scan);
scan("3.5 -2.1".AsSpan(), 0, out Point2D v);
// v.X == 3.5f, v.Y == -2.1f
```

## Next Steps

- [Template Syntax](./template-syntax) — compact format, XML format, four primitives
- [Managed vs Unmanaged](./managed-vs-unmanaged) — dual-strategy selection guide
