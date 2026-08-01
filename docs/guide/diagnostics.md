# 编译期诊断

SourceSerializer 的所有错误和警告在编译期通过 Roslyn 诊断报告。不会等到运行时 NRE 才发现问题。

## 诊断代码

| 代码 | 级别 | 标题 | 触发条件 |
|------|------|------|---------|
| SSR001 | Error | Template Parse Error | 模板字符串无法解析为有效的 compact 或 XML 格式 |
| SSR002 | Error | Readonly field cannot be assigned | 模板引用的字段是 `readonly`，且类型没有匹配的构造器 |
| SSR003 | Error | Missing template dependency | 模板引用了无 `[Template]` 且非内置类型的字段类型，且字段未标记 `[TemplateIgnore]` |
| SSR004 | Error | Scalar field inside `<repetition>` | 非集合字段出现在 `<repetition>` 块内 |
| SSR005 | Error | Template ambiguity | 同接口的两种具现类型模板互为前缀，接口分派无法可靠区分 |
| SSR006 | Error | Cannot override built-in type | `[ExternalTemplate]` 覆盖了 16 种内置类型之一 |

## SSR001：模板解析错误

模板字符串不符合 compact 或 XML 语法规则时触发。

触发示例：

```csharp
[Template("<float X")]  // 缺少闭合 '>'
public struct Bad { public float X; }
```

修复：确保模板字符串符合 [模板语法](./template-syntax) 规范。

## SSR002：只读字段

字段声明为 `readonly`，无法被反序列化代码赋值。`readonly struct` 的所有字段均为 readonly（C# CS8340），需要提供匹配构造器。

触发示例：

```csharp
[Template("<float Attack> <float CritRate>")]
public readonly struct Damage
{
    public readonly float Attack;   // SSR002（无匹配构造器）
    public readonly float CritRate; // SSR002
}
```

修复：添加一个参数与所有字段按名称和类型匹配的构造器。SourceSerializer 通过贪心构造自动发现并使用此构造器：

```csharp
[Template("<float Attack> <float CritRate>")]
public readonly struct Damage
{
    public readonly float Attack;
    public readonly float CritRate;
    public Damage(float attack, float critRate) { Attack = attack; CritRate = critRate; }
}
```

编译期生成 `new Damage(__f_Attack, __f_CritRate)` 替代逐字段赋值。

## SSR003：缺失模板依赖

字段类型既不是 16 种内置类型，也没有 `[Template]` 标注，且字段未标记 `[TemplateIgnore]`。编译将停止。

触发示例：

```csharp
public struct Unregistered { public float X; }

[Template("<Unregistered Data>")]  // SSR003
public struct Container { public Unregistered Data; }
```

修复方案：
- 为被引用类型添加 `[Template]` 或 `[ExternalTemplate]`
- 改用内置类型
- 如果该字段不参与序列化，为其添加 `[TemplateIgnore]` 并从模板字符串中移除引用

## SSR004：重复块内的标量字段

`<repetition>` 块内的标量字段每次迭代覆盖前一轮的值，中间结果丢失。应使用集合类型。

触发示例：

```csharp
[Template("<repetition><first><float Items></first><body>, <float Items></body></repetition>")]  // SSR004
public struct Bad { public float Items; }
```

修复：将字段改为集合类型：

```csharp
[Template("<repetition><first><float Items></first><body>, <float Items></body></repetition>")]
public struct Good { public List<float> Items; }
```

## 使用 `[TemplateIgnore]` 忽略字段

当结构体包含不应参与序列化的字段（缓存值、运行时常量、内部状态），且该字段类型没有 `[Template]` 时，使用 `[TemplateIgnore]` 标记。被忽略的字段不出现在 scanner 和 emitter 代码中。

```csharp
public struct CacheData { public float[] Cache; }

[Template("<float Value>")]
public struct Stats
{
    public float Value;
    [TemplateIgnore] public CacheData InternalCache;
}
```

注意：被标记的字段不应出现在模板字符串中。若模板字符串仍引用该字段的类型，source generator 仍会报告 SSR003 错误。

## SSR005：模板歧义

同一接口的两种具现类型的模板互为前缀，导致接口分派无法可靠区分。编译将停止。

触发示例：

```csharp
interface IVector { }

[Template("Vec(<float X>, <float Y>)")]
struct Vec2 : IVector { float X; float Y; }

[Template("Vec(<float X>, <float Y>, <float Z>)")]
struct Vec3 : IVector { float X; float Y; float Z; }
// Vec2 的模板 "Vec(<float X>, <float Y>)" 是 Vec3 模板的前缀
// 扫描时无法确定何时停止 → SSR005
```

修复方法：调整模板使各具现类型的前缀可区分，例如 `Vec2(...)` 和 `Vec3(...)` 使用不同前缀。

## SSR006：覆盖内置类型

尝试用 `[ExternalTemplate]` 覆盖 16 种内置类型之一时触发。

触发示例：

```csharp
[assembly: ExternalTemplate(typeof(float), "Float(<float>)")]
// → SSR006: Cannot override built-in type 'float'
```

根因：内置类型由 `SerializerRegistry` 中的手写零分配 span 扫描器处理。`[ExternalTemplate]` 无法生成与手写扫描器性能等价的代码——手写扫描器的 `readonly ref struct` 布局和 SIMD 友好的分支结构无法通过模板编译表达。同时，允许覆盖内置类型会导致跨程序集的类型解析行为不一致：同一类型在不同程序集的模板中可能被解析为不同的格式。

修复：移除对内置类型的 `[ExternalTemplate]`。如需对内置类型应用自定义格式，在更上层模板中包装：

```csharp
// 正确：在更上层包装
[Template("MyFloat(<float Value>)")]
struct MyFloat { float Value; }
```

## 参见

- [模板语法](./template-syntax): compact 与 XML 格式，五种原语
- [Managed vs Unmanaged](./managed-vs-unmanaged): 类型策略选择
- [SerializerRegistry API](../api/serializer-registry): 16 种内置类型扫描与发射方法
