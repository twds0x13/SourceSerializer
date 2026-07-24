# 内部机制

## 接口分派算法

扫描器对接口按声明顺序尝试所有具现类型，**首个推进者胜出**（非最长匹配）：

```
输入: "Vec2(3.5, -2)"       → 尝试 Vec2.Scan → 推进 → 选中
输入: "Vec3(3.5, -2, 7.1)"  → 尝试 Vec2.Scan → 不匹配
                             → 尝试 Vec3.Scan → 推进 → 选中
```

发射器对接口用 C# `switch` 模式匹配进行运行时类型分派。

不同程序集通过热更 DLL 注册的接口块自动链合并（`ChainBlock<T>`）——后注册追加到分发链，保持所有具现类型可达。

## 泛型解析 Roslyn 回退

当 `ParseGenericType` 在 `openGenerics` 中找不到类型时，`TryResolveViaInterfaces` 通过 Roslyn `Compilation.GetTypeByMetadataName` 解析 BCL 类型，检查其 `AllInterfaces`，找到匹配的默认接口模板。

多重匹配时，用 Roslyn 继承关系筛选最派生接口；若仍平级，按固定优先级：`IList > ISet > IReadOnlyList > IDictionary > IReadOnlyDictionary`。

## 泛型实例合成

`ResolveGenericTypeInstances` 从字段引用中发现具体泛型实例（如 `List<float>`），基于开放泛型默认模板自动合成完整的 struct 模板定义。支持递归嵌套（如 `List<Wrapper<float>>`）和多类型参数（如 `Dictionary<string, int>`）。

## 校验阶段

| 校验 | 诊断码 | 说明 |
|------|--------|------|
| 重复字段检测 | SSR005 | 标量字段在 repetition 块内 → 改用集合类型 |
| 字段可变性 | SSR003 | readonly 字段且无匹配构造器 → 加构造器或移除 readonly |
| 模板歧义 | SSR006 | 同接口的两个具现类型模板互为前缀 → 调整模板使可区分 |

## 集合 emit

集合类型（`List<T>`、`HashSet<T>`）的 emit 采用统一 `foreach` + `<first>`/`<body>` 模式，使用布尔首标志避免空集合异常：

```csharp
bool __first_X = true;
foreach (var __item in value)
{
    if (__first_X) { /* 首元素：无前导分隔符 */ __first_X = false; }
    else { /* 后续元素：输出分隔符 */ }
    // 逐字段 emit
}
```

数组类型内部走 `CollectionKind.Array` 路径——使用 buffer + `Array.Copy` 替代 `.Add()`，文本格式与 List 相同（`List(...)` 包裹）。

## GeneratedSerializers 独立类

SG 生成的全部 Scan/Emit 方法和 Block 结构体位于 `public static partial class GeneratedSerializers`。编译期三文件各贡献一部分：

- `SerializerScanners.g.cs` → `Scan_Xxx` 方法
- `SerializerEmitters.g.cs` → `Emit_Xxx` 方法
- `SerializerBlocks.g.cs` → `Init()` 注册入口 + `Block_Xxx` 包装结构体

`Init()` 由 `SerializerBlocks.EnsureInitialized()` 通过 AppDomain 反射扫描自动发现和调用。幂等——二次调用直接返回。

## EmitHelpers 共享工具

`EmitHelpers` 统一了 CodeEmitter 和 EmitCodeEmitter 的方法名生成（`GetMethodName`）、唯一变量名生成（`GetUniqueVar`）、名称消毒（sanitize `[]`）和计数器管理。
