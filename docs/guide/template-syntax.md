# 模板语法

SourceSerializer 支持两种等价的模板书写格式：compact 格式和 XML 格式。

## Compact 格式

```csharp
[Template("<float Damage>|<optional>draw <int Cards></optional>")]
```

## XML 格式

```xml
<literal-template>
  <field type="float" name="Damage"/>
  <text>|</text>
  <optional>
    <text>draw </text>
    <field type="int" name="Cards"/>
  </optional>
</literal-template>
```

## 四种原语

| 原语 | Compact | XML | 语义 |
|------|---------|-----|------|
| 裸文字 | 直接书写 | `<text>...</text>` | 逐字符精确匹配 |
| 字段 | `<type name>` | `<field type="" name=""/>` | 调用对应类型扫描器 |
| 可选块 | `<optional>...</optional>` | `<optional>...</optional>` | 尝试匹配，失败回退 |
| 重复块 | `<repetition>...</repetition>` | `<repetition>...</repetition>` | 循环匹配，失败退出 |

## 内置类型

12 种 C# 内置类型无需额外配置：float、double、int、uint、long、ulong、short、ushort、byte、sbyte、bool、char。

## 自定义类型别名

```csharp
[assembly: TypeAlias("Distance", "float")]
```

## 枚举标签

```csharp
enum Element { [Tag("fire")] Fire, [Tag("ice")] Ice }
```

本文档正在编写中。
