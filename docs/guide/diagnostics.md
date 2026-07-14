# 编译期诊断

SourceSerializer 的所有错误和警告在编译期通过 Roslyn 诊断报告。不会等到运行时 NRE 才发现问题。

## 诊断代码

| 代码 | 级别 | 标题 | 触发条件 |
|------|------|------|---------|
| SSR001 | Error | Template Parse Error | 模板字符串无法解析为有效的 compact 或 XML 格式 |
| SSR002 | Error | Circular template dependency | 两个或多个模板互相引用，形成循环依赖 |
| SSR003 | Error | Readonly struct cannot use `[Template]` | `readonly struct` 标注了 `[Template]`，其字段不可赋值 |
| SSR004 | Warning | Missing template dependency | 模板引用了无 `[Template]` 且非内置类型的字段类型 |
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

## SSR003：只读结构体

`readonly struct` 的字段不可赋值，`[Template]` 的反序列化需要写入字段。

触发示例：

```csharp
[Template("<float X>")]
public readonly struct Point { public float X; }  // SSR003
```

修复：移除 `readonly` 修饰符。

## SSR004：缺失模板依赖

字段类型既不是 12 种内置类型，也没有 `[Template]` 标注。source generator 跳过该字段并报告警告，编译继续。

触发示例：

```csharp
public struct Unregistered { public float X; }

[Template("<Unregistered Data>")]  // SSR004
public struct Container { public Unregistered Data; }
```

修复：为被引用类型添加 `[Template]`，或改用内置类型。

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

## 参见

- [模板语法](./template-syntax): compact 与 XML 格式
- [Managed vs Unmanaged](./managed-vs-unmanaged): 类型策略选择
