# Basic Examples

Core usage: declare types, parse text, serialize back.

## Point2D

```csharp
[Template("Point2D(<float X>, <float Y>)")]
struct Point2D { float X; float Y; }

SerializerBlocks.TryGet<Point2D>(out var block);
block.Scan("Point2D(3.5, -2.1)".AsSpan(), 0, out var p);
```

## Nested Structs

```csharp
[Template("Vec2(<float X>, <float Y>)")]
struct Vec2 { float X; float Y; }

[Template("Entity(<string Name>, <Vec2 Pos>)")]
struct Entity { string Name; Vec2 Pos; }

// Input: "Entity(\"treasure\", Vec2(10, 20))"
```

## Optional Blocks

```csharp
[Template("Damage(<float Base><optional>, <float Bonus></optional>)")]
struct Damage { float Base; float Bonus; }

// "Damage(100)" → Base=100, Bonus=0
// "Damage(100, 25)" → Base=100, Bonus=25
```

## Serialization

```csharp
SerializerBlocks.TryGet<Point2D>(out var block);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "Point2D(3.5, -2.1)"
```

See `PrimitiveScannerTests.cs` and `UnmanagedStructTests.cs` for complete runnable test cases.
