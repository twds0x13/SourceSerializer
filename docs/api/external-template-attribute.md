# `[ExternalTemplate]`

为无法直接修改源码的第三方类型声明序列化模板。

## 签名

```csharp
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class ExternalTemplateAttribute : Attribute
{
    public Type TargetType { get; }
    public string Template { get; }
    public ExternalTemplateAttribute(Type targetType, string template);
}
```

## 构造参数

| 参数 | 类型 | 说明 |
|------|------|------|
| `targetType` | `Type` | 目标类型 |
| `template` | `string` | 模板字符串 |

## 用法

可置于 assembly、class 或 struct 上。External 模板覆盖 struct 级别 `[Template]`（Priority B 语义）。

```csharp
// 为第三方库中的 struct 注册模板
[assembly: ExternalTemplate(typeof(UnityEngine.Vector3),
    "<float x> <float y> <float z>")]

// 或直接放在 struct 上方
[ExternalTemplate(typeof(ExternalPoint), "<float A> <float B>")]
public struct ExternalPoint { public float A; public float B; }
```
