# Managed vs Unmanaged

SourceSerializer 在编译期根据 `typeof(T)` 判别策略。

## Unmanaged 路径

约束 `T : unmanaged` 时走单次解析策略。

source generator 生成一个 `Scan_TypeName` 方法，接收 `ReadOnlySpan<char>`，逐字段填充 stackalloc 的 struct 实例。全程零堆分配，Burst 兼容。

```csharp
[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}
// 编译期生成:
// internal static int Scan_Point2D(ReadOnlySpan<char> src, int pos, out Point2D value)
```

当前版本实现 unmanaged 路径。

## Managed 路径

`T : class` 或 managed struct（含 string、List、对象引用等字段）时走两步策略：

1. **Walk**：遍历对象图，依次为每个对象分配 int 编号
2. **Serialize**：引用字段替换为 int 编号

天然支持循环引用，无需 `$ref` 标注或图分析。

Managed 路径尚未实现，计划在后续版本提供。

## 选择指南

| 场景 | 策略 |
|------|------|
| 纯数值 struct（Vector3、StatBlock、Point2D） | Unmanaged |
| 含 string、List、对象引用的类型 | Managed（规划中） |
