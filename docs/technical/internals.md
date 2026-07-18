# 内部机制

## 接口分派算法

扫描器对每个接口尝试所有已注册的具体实现，选推进输入最远的：

```
输入: "3.5, -2"     → 尝试 Vec2.Scan → 推进到末尾 ← 选它
输入: "3.5, -2, 7.1" → 尝试 Vec2.Scan → 只匹配前两个字段
                      → 尝试 Vec3D.Scan → 推进到末尾 ← 选它
```

发射器对接口用 C# `switch` 模式匹配进行运行时类型分派。

## 泛型解析 Roslyn 回退

当 `ParseGenericType` 在 `openGenerics` 中找不到类型时，不立即放弃。`TryResolveViaInterfaces` 通过 Roslyn `Compilation.GetTypeByMetadataName` 解析 BCL 类型，检查其 `AllInterfaces`，找到匹配的默认接口模板。

多重匹配时，用 Roslyn 继承关系筛选最派生接口；若仍平级，按固定优先级：`IList > ISet > IReadOnlyList > IDictionary > IReadOnlyDictionary`。

## 集合 emit

自集合类型（`List<T>`、`HashSet<T>`）的 emit 采用 `foreach` + `<first>`/`<body>` 模式：

```csharp
// 首元素：无分隔符
var __elem = Enumerable.First(value);
var __elem = __elem;   // 遮蔽变量名
// 后续：foreach 跳过首元素，含分隔符
foreach (var __elem in value) { ... }
```

## EmitHelpers 共享工具

`EmitHelpers` 统一了 CodeEmitter 和 EmitCodeEmitter 的方法名生成（`GetMethodName`）、唯一变量名生成（`GetUniqueVar`）和计数器管理。消除两个发射器之间约 20 行重复代码。
