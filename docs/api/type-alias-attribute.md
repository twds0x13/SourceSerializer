# `[TypeAlias]`

声明模板中的类型别名，映射到内置 C# 值类型。

## 签名

```csharp
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class TypeAliasAttribute : Attribute
{
    public string Alias { get; }
    public string CSharpType { get; }
    public TypeAliasAttribute(string alias, string csharpType);
}
```

## 构造参数

| 参数 | 类型 | 说明 |
|------|------|------|
| `alias` | `string` | 模板中使用的别名 |
| `csharpType` | `string` | 映射到的 C# 值类型名 |

## 用法

assembly 级 attribute，注册后模板中可使用别名替代内置类型名：

```csharp
[assembly: TypeAlias("Distance", "float")]
[assembly: TypeAlias("Count", "int")]
```

模板中使用：

```csharp
[Template("<Distance X> <Distance Y>")]
public struct Point2D { public float X; public float Y; }
```

## 参见

- [示例: 枚举标签与别名](/examples/enum-and-alias)：Tag + TypeAlias 组合示例
- [Tag 属性](./tag-attribute)：枚举成员标签的完整签名
- [模板语法](/guide/template-syntax)：类型别名在模板中的使用
