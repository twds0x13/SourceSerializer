# 基础示例

SourceSerializer 的核心用法：声明类型、解析文本、序列化回文本。

## Point2D

```csharp
[Template("Point2D(<float X>, <float Y>)")]
struct Point2D { float X; float Y; }

SerializerBlocks.TryGet<Point2D>(out var block);
block.Scan("Point2D(3.5, -2.1)".AsSpan(), 0, out var p);
```

## 嵌套结构体

```csharp
[Template("Vec2(<float X>, <float Y>)")]
struct Vec2 { float X; float Y; }

[Template("Entity(<string Name>, <Vec2 Pos>)")]
struct Entity { string Name; Vec2 Pos; }

// 输入: "Entity(\"treasure\", Vec2(10, 20))"
```

## 可选块

```csharp
[Template("Damage(<float Base><optional>, <float Bonus></optional>)")]
struct Damage { float Base; float Bonus; }

// "Damage(100)" → Base=100, Bonus=0
// "Damage(100, 25)" → Base=100, Bonus=25
```

## 序列化

```csharp
SerializerBlocks.TryGet<Point2D>(out var block);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "Point2D(3.5, -2.1)"
```

运行测试参考 `PrimitiveScannerTests.cs`、`UnmanagedStructTests.cs` 中的完整用例。
