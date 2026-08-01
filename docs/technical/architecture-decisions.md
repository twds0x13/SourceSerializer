# 架构决策记录

## ADR-1: 接口优先默认模板

将默认模板从具体类（`List<T>`、`Dictionary<K,V>`）迁移到接口（`IList<T>`、`ISet<T>`、`IDictionary<K,V>` 等）。具体类型通过 Roslyn `AllInterfaces` 自动匹配。消除 `GenericInterfaceAliases` 间接映射层。

优先级链：类级显式模板 > 接口级显式模板 > 默认接口模板。

## ADR-2: SerializerBlocks 统一 API

删除独立的 `SerializerScanners` 和 `SerializerEmitters` 类，统一为 `SerializerBlocks`。`ISerializerBlock<T>` 接口同时提供 `Scan` 和 `Emit` 能力。消除两套独立的注册表和委托类型。

## ADR-3: 删除 Walk 阶段引用

代码注释和文档长期引用一个不存在的"managed Walk 阶段"。集合 emit 实际为 `foreach` 单趟实现。删除所有相关注释、移除 `NeedsWalkPhase`、更新文档。

## ADR-4: 合并 Scanner/Emitter 共享工具

提取 `EmitHelpers` 静态类，统一方法名生成、计数器管理、`EmitEntry` 字段复制。消除 ScanCodeEmitter 和 EmitCodeEmitter 之间的重复代码和手工程式。

## ADR-5: CollectionKind 重命名

`CollectionKind.List` 含义不准确（覆盖 `List`、`ISet`、`IReadOnlyList` 等六种不同契约）。重命名为 `CollectionKind.Sequential`。代码生成根据字段实际类型选择 `List<T>` 或 `HashSet<T>` 构造器。

## ADR-6: GeneratedSerializers 独立类

SG 生成的全部代码归属 `public static partial class GeneratedSerializers`，与 `SerializerRegistry`（非 partial，纯库代码）完全解耦。三文件通过 `partial` 合并为一个类，`public` 修饰符保证跨程序集可见。替代方案：将生成代码直接写入 `SerializerBlocks` 的 `partial` 扩展，但会在包程序集和用户程序集之间产生字典不共享的问题。

## ADR-7: 反射 Init 发现

`EnsureInitialized()` 通过 AppDomain 反射扫描所有已加载程序集，自动发现并调用 `GeneratedSerializers.Init()`。`Init()` 由 `_initCalled` 守卫保证幂等。替代方案：显式配置文件列出所有含模板的程序集。反射扫描的启动开销（毫秒级）可接受，且消除了手动维护程序集清单的出错面。

## ADR-8: ChainBlock 接口链合并

接口类型的 `AddBlock` 追加到 `ChainBlock<T>` 分发链而非覆盖。Scan 按 link 顺序首个推进者胜出，Emit 按 switch 匹配首个命中。这使得不同程序集（主程序集 + 热更 DLL）可以各自生成接口分发块，运行时自动合并。非接口类型保持覆盖语义。

## ADR-9: 只读 struct 构造器匹配

只读 struct 的字段无法逐字段赋值（C# CS8340）。SG 通过 Roslyn `IMethodSymbol.Parameters` 发现构造器，按名称和类型做贪心匹配。匹配成功时生成构造器调用，失败时报告 SSR002。替代方案：要求用户始终提供特定签名的构造器，但按名匹配降低了接入成本。

## ADR-10: 泛型实例合成

开放泛型模板（如 `Wrapper<T>`）在字段引用出现具体实例（如 `Wrapper<float>`）时自动合成。合成过程递归处理嵌套泛型（如 `List<Wrapper<float>>`）和多类型参数（如 `Dictionary<string, int>`）。`TryResolveViaInterfaces` 通过 Roslyn 回退解析不在 `openGenerics` 中的 BCL 类型。替代方案：要求用户为每个具体实例显式声明 `[ExternalTemplate]`。

## 参见

- [SG 管线全景](./pipeline/overview)：ADR 对应的管线阶段
- [内部机制](./internals)：ADR 对应的源码细节
- [已知限制](./known-issues)：当前未解决的设计限制
