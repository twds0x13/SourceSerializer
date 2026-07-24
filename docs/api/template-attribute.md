# `[Template]`

为 struct 或 class 声明序列化模板。source generator 在编译期生成 span scanner。

## 签名

```csharp
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TemplateAttribute : Attribute
{
    public string Template { get; }
    public TemplateAttribute(string template);
}
```

## 构造参数

| 参数 | 类型 | 说明 |
|------|------|------|
| `template` | `string` | 模板字符串（compact 语法或 XML 格式） |

## 模板语法

`<type fieldname>` 字段指令映射到内置类型或已注册类型的扫描器。

Compact 示例：

```csharp
[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D { public float X; public float Y; }
```

带 optional 和 repetition：

```csharp
[Template("SpellCard(<float Damage><optional>, draw <int Cards></optional>)")]
public struct SpellCard { public float Damage; public int Cards; }

[Template("DamageData(<float Damage><repetition>, <float Multipliers></repetition>)")]
public struct DamageData { public float Damage; public float Multipliers; }
```

也可用于 class：

```csharp
[Template("NamedItem(<string Name>)")]
public class NamedItem { public string Name; }
```

等价 XML 格式：

```xml
<literal-template>
  <field type="float" name="Damage"/>
  <repetition>
    <text>, </text>
    <field type="float" name="Multipliers"/>
  </repetition>
</literal-template>
```

## 运行时使用

反序列化（scan）：

```csharp
SerializerBlocks.TryGet<Point2D>(out var scan);
scan.Scan("Point2D(3.5, -2.1)".AsSpan(), 0, out Point2D v);
```

序列化（emit）：

```csharp
SerializerBlocks.TryGet<Point2D>(out var emit);
var sb = new StringBuilder();
emit.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
Console.WriteLine(sb.ToString()); // "Point2D(3.5, -2.1)"
```
