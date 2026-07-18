# Getting Started

SourceSerializer uses attributes to declare struct layouts. The source generator emits zero-allocation span scanners at compile time.

## Installation

For Unity projects, add to `manifest.json`:

```json
"com.twds0x13.sourceserializer": "https://github.com/twds0x13/SourceSerializer.git#main"
```

For .NET projects, reference the source generator in `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\SourceSerializer\packages\sourceserializer\SourceGenerator\SourceSerializer.Generator.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Declare a Template

Use `[Template("...")]` on a struct to declare its text format:

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

After compilation, the source generator emits a `Scan_Point2D` method and registers it in `SerializerBlocks`:

```csharp
SerializerBlocks.TryGet<Point2D>(out var scan);
int pos = block.Scan("3.5 -2.1".AsSpan(), 0, out Point2D v);
// pos > 0, v.X == 3.5f, v.Y == -2.1f
```

## Using the Generated Serializer

After compilation, the source generator also emits an `Emit_Point2D` method and registers it in `SerializerBlocks`:

```csharp
SerializerBlocks.TryGet<Point2D>(out var emit);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
Console.WriteLine(sb.ToString()); // "3.5 -2.1"
```

## Next Steps

- [Template Syntax](./template-syntax): compact format, XML format, four primitives, nesting, generic collections
- [Managed vs Unmanaged](./managed-vs-unmanaged): dual strategy selection
- [Diagnostics](./diagnostics): error code reference
- [API Reference](/en/api/): Template, ExternalTemplate, Tag, TypeAlias attributes
