# `[Tag]`

为枚举成员声明字面量标签。source generator 自动生成 switch-on-string 扫描器。

## 签名

```csharp
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class TagAttribute : Attribute
{
    public string Tag { get; }
    public TagAttribute(string tag);
}
```

## 构造参数

| 参数 | 类型 | 说明 |
|------|------|------|
| `tag` | `string` | 标签字符串 |

## 用法

在枚举成员上标注 `[Tag]`，模板中使用枚举类型名作为字段类型：

```csharp
enum Element : byte
{
    Physical = 0,
    [Tag("fire")]  Fire,
    [Tag("ice")]   Ice,
    [Tag("magic")] Magic,
}

[Template("<Element Type>")]
public struct Spell
{
    public Element Type;
}
```

编译后，source generator 生成 `Scan_Enum_Element` 方法，对标签字符串执行 switch 匹配。匹配到的标签返回对应枚举值，未匹配返回解析失败。

## 注意

如果枚举成员未标注 `[Tag]`，source generator 不会为其生成匹配 case。上例中 `Physical = 0` 没有标签，无法通过字符串匹配解析。
