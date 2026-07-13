# API 参考

## Attributes

| Attribute | 目标 | 说明 |
|-----------|------|------|
| `[Template("...")]` | struct, class | 声明类型的文本模板 |
| `[ExternalTemplate(typeof(T), "...")]` | assembly, class, struct | 为第三方类型声明模板 |
| `[Tag("label")]` | enum field | 为枚举成员声明字符串标签 |
| `[TypeAlias("Alias", "float")]` | assembly | 注册类型别名 |

## SerializerScanners

```csharp
partial class SerializerScanners
{
    static bool TryGetScanner<T>(out ScannerDelegate<T> scanner);
}
```

本文档正在编写中。
