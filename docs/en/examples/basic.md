# Basic Examples

Core usage: declare types, parse text, serialize back.

## Point2D

```csharp
[Template("<float X> <float Y>")]
struct Point2D { float X; float Y; }

SerializerBlocks.TryGet<Point2D>(out var block);
block.Scan("3.5 -2.1".AsSpan(), 0, out var p);
```

## Nested Structs

```csharp
[Template("<float X>, <float Y>")]
struct Vec2 { float X; float Y; }

[Template("<string Name> at <Vec2 Pos>")]
struct Entity { string Name; Vec2 Pos; }

// Input: "treasure at 10, 20"
```

## Optional Blocks

```csharp
[Template("<float Base><optional>, <float Bonus></optional>")]
struct Damage { float Base; float Bonus; }

// "100" → Base=100, Bonus=0
// "100, 25" → Base=100, Bonus=25
```

## Serialization

```csharp
SerializerBlocks.TryGet<Point2D>(out var block);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "3.5 -2.1"
```

See `PrimitiveScannerTests.cs` and `UnmanagedStructTests.cs` for complete runnable test cases.
