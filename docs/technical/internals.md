# 内部机制

## 接口分派算法

扫描器对接口按声明顺序尝试所有具现类型，**首个推进者胜出**（非最长匹配）：

```
输入: "Vec2(3.5, -2)"       → 尝试 Vec2.Scan → 推进 → 选中
输入: "Vec3(3.5, -2, 7.1)"  → 尝试 Vec2.Scan → 不匹配
                             → 尝试 Vec3.Scan → 推进 → 选中
```

发射器对接口用 C# `switch` 模式匹配进行运行时类型分派。

不同程序集通过热更 DLL 注册的接口块自动链合并（`ChainBlock<T>`）：后注册追加到分发链，保持所有具现类型可达。

## 接口链合并 (ChainBlock\<T\>)

`RegisterBlock<T>` 是 `AddBlock` 的核心分发逻辑，对接口类型做合并而非覆盖：

```
if (typeof(T).IsInterface && _blocks 已有注册):
    if (已有是 ChainBlock<T>):
        chain.AddLink(newBlock)           // 追加到已有链表尾
    else:
        newChain = new ChainBlock<T>()
        newChain.AddLink(旧block)         // 旧注册前置
        newChain.AddLink(newBlock)        // 新注册后置
else:
    _blocks[key] = newBlock              // 非接口类型，直接覆盖
```

**ChainBlock.Scan**：按 `_links` 顺序遍历，首个推进 `pos` 的 link 胜出并立即返回。如果所有 link 都无法识别输入（返回 `pos`），整体返回 `pos` 表示失败。

**ChainBlock.Emit**：按 `_links` 顺序逐一切换匹配。记录 `sb.Length` 调用前值，link 调用后 `sb.Length > before` 表示该 link 的 switch 命中并输出了内容，立即返回。依赖 SG 生成的 switch dispatch：未匹配类型的 case 不写入 sb。

**非泛型 AddBlock(Type, ISerializerBlock)**：从 block 的运行时类型反射提取 `ISerializerBlock<T>` 接口的泛型参数 T，通过 `MakeGenericMethod` 委托到 `RegisterBlock<T>`，复用同一合并逻辑。

线程安全：`_links` 的写入在 `lock(_syncRoot)` 内，读取在锁外。调用方约定所有 `AddBlock` 在首次 `Serialize`/`Deserialize` 前完成。

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

数组类型内部走 `CollectionKind.Array` 路径：使用 buffer + `Array.Copy` 替代 `.Add()`，文本格式与 List 相同（`List(...)` 包裹）。

## GeneratedSerializers 独立类

SG 生成的全部 Scan/Emit 方法和 Block 结构体位于 `public static partial class GeneratedSerializers`。编译期三文件各贡献一部分：

- `SerializerScanners.g.cs` → `Scan_Xxx` 方法
- `SerializerEmitters.g.cs` → `Emit_Xxx` 方法
- `SerializerBlocks.g.cs` → `Init()` 注册入口 + `Block_Xxx` 包装结构体

`Init()` 由 `SerializerBlocks.EnsureInitialized()` 通过 AppDomain 反射扫描自动发现和调用。幂等：二次调用直接返回。

## 空白符剔除（WhitespaceStripper）

`WhitespaceStripper` 是运行时输入预处理的核心组件，在 `Deserialize<T>()` 和 `TryScan<T>()` 中自动调用。采用两阶段零分配算法：

**第一阶段（计数）**：遍历输入，跳过引号外部的所有空白符，计算输出长度。维护 `inString` 状态标志追踪是否处于双引号内部——引号内的空白符（及 `\"` 转义）完整计入输出长度。

**第二阶段（填充）**：通过 `string.Create` 创建目标长度的字符串，在回调中重新遍历输入，将非空白字符（及引号内全部字符）逐字写入输出缓冲区。

早返优化：如果输出长度等于输入长度（无空白符需剔除），直接返回原字符串避免复制；如果输出长度为 0（全空白），返回 `string.Empty`。

设计原理：将空白符预处理从各类型的 `Scan_Xxx` 方法中分离，集中为单一零分配工具。生成的扫描器代码无需在每个 literal text 匹配前插入空白符跳过分支——扫描器假设输入已紧凑化。这一分离也使得空白符处理策略可独立演进（如未来增加注释支持），不波及每个类型的生成代码。

## Array 缓冲区策略

`T[]` 数组字段和 `List<T>` 集合字段在扫描器代码生成中使用不同的赋值路径：

| 字段类型 | 代码生成路径 | 最终赋值 |
|---------|------------|---------|
| `List<T>` | 声明 `var __list = new List<T>()`，循环内 `__list.Add(item)` | 直接赋值 |
| `T[]` | 声明 `var __buf = new T[16]`，循环内写入 `__buf[__cnt++]`，倍增扩容 | `Array.Copy(__buf, value, __cnt)` |

`IsArrayCollection` 标志（`SerializerGenerator.cs` 第 1346 行）控制路径选择。`T[]` 无法使用 `.Add()`，采用预分配缓冲区 + 跟踪计数 + 最终 `Array.Copy` 的方案。缓冲区初始 16 元素，超出时倍增（上限 1024 后按 ×2 增长）。

设计原理：`List<T>` 内部的增长策略和 `T[]` 的显式缓冲区管理目标相同——避免逐元素扩容导致的二次复制。区别在于 `List<T>` 封装了增长逻辑（`.Add()` 内部处理），而 `T[]` 需要代码生成器显式管理缓冲区。

## EmitHelpers 共享工具

`EmitHelpers` 统一了 ScanCodeEmitter 和 EmitCodeEmitter 的方法名生成（`GetMethodName`）、唯一变量名生成（`GetUniqueVar`）、名称消毒（sanitize `[]`）和计数器管理。

## 参见

- [SG 管线全景](./pipeline/overview)：编译期管线各阶段
- [架构决策](./architecture-decisions)：关键设计的方案对比
- [已知限制](./known-issues)：class 类型变量作用域问题
- [SerializerBlocks API](../api/serializer-blocks)：运行时注册表
- [热更新与跨程序集注册](../guide/hot-reload)：ChainBlock 使用场景
