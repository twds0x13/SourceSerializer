# 架构决策记录

## ADR-1: 接口优先默认模板

将默认模板从具体类（`List<T>`、`Dictionary<K,V>`）迁移到接口（`IList<T>`、`ISet<T>`、`IDictionary<K,V>` 等）。具体类型通过 Roslyn `AllInterfaces` 自动匹配。消除 `GenericInterfaceAliases` 间接映射层。

优先级链：类级显式模板 > 接口级显式模板 > 默认接口模板。

## ADR-2: SerializerBlocks 统一 API

删除独立的 `SerializerScanners` 和 `SerializerEmitters` 类，统一为 `SerializerBlocks`。`ISerializerBlock<T>` 接口同时提供 `Scan` 和 `Emit` 能力。消除两套独立的注册表和委托类型。

## ADR-3: 删除 Walk 阶段引用

代码注释和文档长期引用一个不存在的"managed Walk 阶段"。集合 emit 实际为 `foreach` 单趟实现。删除所有相关注释、移除 `NeedsWalkPhase`、更新文档。

## ADR-4: 合并 Scanner/Emitter 共享工具

提取 `EmitHelpers` 静态类，统一方法名生成、计数器管理、`EmitEntry` 字段复制。消除 CodeEmitter 和 EmitCodeEmitter 之间的重复代码和手工程式。

## ADR-5: CollectionKind 重命名

`CollectionKind.List` 含义不准确（覆盖 `List`、`ISet`、`IReadOnlyList` 等六种不同契约）。重命名为 `CollectionKind.Sequential`。代码生成根据字段实际类型选择 `List<T>` 或 `HashSet<T>` 构造器。
