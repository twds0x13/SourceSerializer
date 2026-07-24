# Managed vs Unmanaged

SourceSerializer 在编译期通过 Roslyn 的 `ITypeSymbol` 判定类型策略，生成对应的 Scan/Emit 代码。两种路径均完整支持序列化和反序列化。

## Unmanaged 路径

约束 `T : unmanaged` 的类型（纯数值 struct）：SG 生成逐字段赋值的 span 扫描器，全程 `stackalloc`，零堆分配，零 GC，Burst 兼容。

```csharp
[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D
{
    public float X;
    public float Y;
}
// 编译期生成 public static Scan/Emit 方法到 GeneratedSerializers
```

```csharp
SerializerBlocks.TryGet<Point2D>(out var block);
var sb = new StringBuilder();
block.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
// sb.ToString() == "Point2D(3.5, -2.1)"
```

## Managed 路径

含 `string`、`class`、`List<T>`、接口引用等字段的类型：SG 同样生成完整的 Scan/Emit 方法。区别仅在于分配策略：

| 维度 | 含义 | Roslyn 来源 | 代码生成影响 |
|------|------|------------|-------------|
| `NeedsHeapAlloc` | 类型为 class | `TypeKind == Class` | `new T()` vs `default` |
| `IsReadonlyStruct` | 只读 struct | `IsReadOnly` | 走构造器路径而非逐字段赋值 |
| `MatchedCtorParams` | 构造器参数匹配 | `IMethodSymbol.Parameters` | 按名匹配字段到构造器参数 |

Roslyn 是编译期唯一真理来源。`IsUnmanagedType` 涵盖 C# 7.3+ `unmanaged` 约束的完整定义（含泛型特化），SG 零行手动规则。

```csharp
[Template("NamedValue(<string Name>, <float Value>)")]
class NamedValue
{
    public string Name;
    public float Value;
}
// 反序列化：自动 new NamedValue()，逐字段填入
// 序列化：逐字段 Emit_String/Emit_Float

[Template("Inventory(<string Name>, <List<NamedValue> Items>)")]
class Inventory
{
    public string Name;
    public List<NamedValue> Items;
}
// 集合字段通过 foreach 迭代，与扫描方向对称
```

Managed 路径的 Scan 和 Emit 均已完整实现。支持 class、managed struct、string、List、Dictionary、HashSet、接口引用、嵌套泛型等所有字段组合。

## 只读 struct

```csharp
[Template("ReadOnlyVec(<float X>, <float Y>)")]
readonly struct ReadOnlyVec(float x, float y)
{
    public readonly float X = x;
    public readonly float Y = y;
}
```

SG 自动发现构造器参数与字段的按名匹配，生成构造器调用而非逐字段赋值。支持 public 和 internal 构造器。

## 选择指南

| 场景 | 策略 |
|------|------|
| 纯数值 struct（Vector3、Point2D） | Unmanaged：栈分配，最高性能 |
| 含 string、List、对象引用的类型 | Managed：完整支持，堆分配仅在字段初始化时 |
| 只读 struct | 构造器路径：自动匹配 |
