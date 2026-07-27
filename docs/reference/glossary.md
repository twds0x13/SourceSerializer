# 术语表

## 模板系统

| 术语 | 说明 |
|------|------|
| 模板 (Template) | `[Template("...")]` 声明的字符串，描述数据格式 |
| 紧凑语法 | `<float X>` 格式，通过 CompactToXml 转换为 XML |
| XML 语法 | `<literal-template>` 格式，与紧凑语法等价 |
| 裸文字 (Literal Text) | 模板中的固定字符，扫描时逐字匹配 |
| 字段 (Field) | `<float X>` 中的 `float` 类型和 `X` 字段名 |
| 可选块 (Optional Block) | `<optional>...</optional>` 包裹的模板片段，匹配失败回退 |
| 重复块 (Repetition Block) | `<repetition><first>...</first><body>...</body></repetition>` 包裹的集合片段 |
| 缩进块 (Indent Block) | `<indent>...</indent>` 包裹的模板片段。Emit 时输出层级缩进，Scan 时为 no-op |
| compactWhitespace | `ScanCodeEmitter` 编译期选项：生成代码前去除 literal text 中的空白符，减小生成代码体积 |
| WhitespaceStripper | 运行时输入预处理：两阶段零分配 `string.Create` 实现，剔除引号外部空白符 |

## 运行时

| 术语 | 说明 |
|------|------|
| 扫描器 (Scanner) | 从 `ReadOnlySpan<char>` 解析出 `TData` 的过程 |
| 发射器 (Emitter) | 将 `TData` 序列化到 `StringBuilder` 的过程 |
| 序列化器块 (Serializer Block) | `ISerializerBlock<T>` 实例，同时持有扫描和发射能力 |
| 非泛型标记接口 | `ISerializerBlock`（无泛型参数），使 `ISerializerBlock<T>` 实例可被 `params ISerializerBlock[]` 接收 |
| 一行式 API (Convenience API) | `SerializerBlocks.Serialize<T>()` / `Deserialize<T>()` / `TryScan<T>()` 单方法调用封装，消除 TryGet + StringBuilder 样板 |
| Builder | `SerializerBlocks.Builder` 嵌套类，`AddBlock<T>()` 返回的流式链式注册构建器 |

## 编译期

| 术语 | 说明 |
|------|------|
| Source Generator (SG) | Roslyn `IIncrementalGenerator`，编译期生成 C# 源码 |
| 接口分派 (Interface Dispatch) | 扫描器按声明顺序尝试所有具现类型，首个推进者胜出 |
| 默认接口模板 | 系统内置的接口模板（IList、ISet、IReadOnlyList、IDictionary、IReadOnlyDictionary、Array） |
| Roslyn 回退 | 不在 openGenerics 中时，通过 AllInterfaces 查找匹配接口 |
| SSR007 | 编译期错误：尝试用 `[ExternalTemplate]` 覆盖 16 种内置类型之一 |
| Array 缓冲区 | `T[]` 集合字段使用的代码生成路径：预分配缓冲区 + 跟踪计数 + 最终 `Array.Copy`，与 `List<T>` 的 `.Add()` 路径相对 |

## 泛型

| 术语 | 说明 |
|------|------|
| 开放泛型 (Open Generic) | `Wrapper<T>`，类型参数未关闭 |
| 闭合泛型 (Closed Generic) | `Wrapper<float>`，类型参数已关闭 |
| 泛型传递闭包 | `List<Wrapper<float>>` 中的递归合成过程 |

## 参见

- [核心概念](/guide/core-concepts)：端到端架构全景
- [模板语法](/guide/template-syntax)：五种原语与术语使用
- [内部机制](/technical/internals)：术语对应的源码细节
