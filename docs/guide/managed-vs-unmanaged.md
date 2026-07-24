# Managed vs Unmanaged

SourceSerializer 在编译期根据 `typeof(T)` 判别策略。

## Unmanaged 路径

约束 `T : unmanaged` 时走单次解析策略。

source generator 生成一个 `Scan_TypeName` 方法，接收 `ReadOnlySpan<char>`，逐字段填充 struct 实例。全程零堆分配，Burst 兼容。

```csharp
[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D
{
    public float X;
    public float Y;
}
// 编译期生成:
// internal static int Scan_Point2D(ReadOnlySpan<char> src, int pos, out Point2D value)
```

序列化方向同样可用。source generator 同时生成 `Emit_Point2D` 方法，将实例写入 `StringBuilder`：

```csharp
SerializerBlocks.TryGet<Point2D>(out var block);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "3.5 -2.1"
```

Unmanaged 路径的扫描和发射均已实现。

## Managed 路径

`T : class` 或 managed struct（含 string、List、对象引用等字段）时处理方案不同。

### 反序列化（已实现）

Source Generator 根据 Roslyn 的 `ITypeSymbol.IsUnmanagedType` 判定分叉，拆为两个正交维度：

| 维度 | 含义 | Roslyn 来源 | 代码生成影响 |
|------|------|------------|-------------|
| `NeedsHeapAlloc` | 类型是否为 class | `TypeKind == Class` | `new T()` vs `default` |

Roslyn 是编译期唯一真理来源。`IsUnmanagedType` 涵盖 C# 7.3+ `unmanaged` 约束的完整定义（含泛型特化），SG 零行手动规则。

### 序列化方向（部分实现）

Unmanaged 路径的序列化已完整实现。source generator 通过 `EmitCodeEmitter` 生成 `SerializerBlocks.g.cs`，支持：

- 裸文字（literal text）输出
- 字段序列化（内置类型、自定义嵌套类型、枚举标签）
- 可选块（字段非默认值时输出）

Managed 路径的两步走序列化仍在规划中：

1. **Walk**：遍历对象图，依次为每个对象分配 int 编号
2. **Serialize**：引用字段替换为 int 编号

天然支持循环引用，无需 `$ref` 标注或图分析。编译期通过 Roslyn `IsUnmanagedType` 判定类型策略。

`<repetition>` 块的序列化通过 `foreach` 迭代集合元素实现，与扫描方向对称。

## 选择指南

| 场景 | 策略 |
|------|------|
| 纯数值 struct（Vector3、StatBlock、Point2D） | Unmanaged：扫描和发射均完整 |
| 含 string、List、对象引用的类型 | Managed：反序列化可用，序列化部分可用 |
