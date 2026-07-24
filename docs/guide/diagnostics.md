# 编译期诊断

SourceSerializer 的所有错误和警告在编译期通过 Roslyn 诊断报告。不会等到运行时 NRE 才发现问题。

## 诊断代码

| 代码 | 级别 | 标题 | 触发条件 |
|------|------|------|---------|
| SSR001 | Error | Template Parse Error | 模板字符串无法解析为有效的 compact 或 XML 格式 |
| SSR002 | Error | Circular template dependency | 两个或多个模板互相引用，形成循环依赖 |
| SSR003 | Error | Readonly field cannot be assigned | 模板引用的字段是 `readonly`，且类型没有匹配的构造器 |
| SSR004 | Error | Missing template dependency | 模板引用了无 `[Template]` 且非内置类型的字段类型，且字段未标记 `[TemplateIgnore]` |
| SSR005 | Error | Scalar field inside `<repetition>` | 非集合字段出现在 `<repetition>` 块内 |

## SSR001：模板解析错误

模板字符串不符合 compact 或 XML 语法规则时触发。

触发示例：

```csharp
[Template("<float X")]  // 缺少闭合 '>'
public struct Bad { public float X; }
```

修复：确保模板字符串符合 [模板语法](./template-syntax) 规范。

## SSR002：循环模板依赖

类型 A 的模板引用类型 B，B 的模板又引用 A，形成环路。source generator 通过拓扑排序检测循环。

触发示例：

```csharp
[Template("<B Other>")]
public struct A { public B Other; }

[Template("<A Other>")]
public struct B { public A Other; }
```

修复：打破循环，将其中一环改为内置类型或移除外层引用。

## SSR003：只读字段

字段声明为 `readonly`，无法被反序列化代码赋值。`readonly struct` 的所有字段均为 readonly（C# CS8340），需要提供匹配构造器。

触发示例：

```csharp
[Template("<float Attack> <float CritRate>")]
public readonly struct Damage
{
    public readonly float Attack;   // SSR003（无匹配构造器）
    public readonly float CritRate; // SSR003
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

## SSR004：缺失模板依赖

字段类型既不是 13 种内置类型，也没有 `[Template]` 标注，且字段未标记 `[TemplateIgnore]`。编译将停止。

触发示例：

```csharp
public struct Unregistered { public float X; }

[Template("<Unregistered Data>")]  // SSR004
public struct Container { public Unregistered Data; }
```

修复方案：
- 为被引用类型添加 `[Template]` 或 `[ExternalTemplate]`
- 改用内置类型
- 如果该字段不参与序列化，为其添加 `[TemplateIgnore]` 并从模板字符串中移除引用

## SSR005：重复块内的标量字段

`<repetition>` 块内的标量字段每次迭代覆盖前一轮的值，中间结果丢失。应使用集合类型。

触发示例：

```csharp
[Template("<repetition>, <float Items></repetition>")]  // SSR005
public struct Bad { public float Items; }
```

修复：将字段改为集合类型：

```csharp
[Template("<repetition>, <float Items></repetition>")]
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

注意：被标记的字段不应出现在模板字符串中。若模板字符串仍引用该字段的类型，source generator 仍会报告 SSR004 错误。

## 参见

- [模板语法](./template-syntax): compact 与 XML 格式
- [Managed vs Unmanaged](./managed-vs-unmanaged): 类型策略选择
