# `[TemplateIgnore]`

标记字段不参与序列化和反序列化。

## 签名

```csharp
[AttributeUsage(AttributeTargets.Field)]
public class TemplateIgnoreAttribute : Attribute { }
```

## 用法

```csharp
[Template("Container(<float Value>)")]
struct Container
{
    public float Value;
    [TemplateIgnore] public UnregisteredType InternalData;
}
```

`InternalData` 不出现在模板中，SG 生成的扫描器和发射器跳过此字段。

## 参见

- [编译期诊断: SSR004](../guide/diagnostics#ssr004---missing-template-dependency)
