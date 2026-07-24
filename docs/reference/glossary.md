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

## 运行时

| 术语 | 说明 |
|------|------|
| 扫描器 (Scanner) | 从 `ReadOnlySpan<char>` 解析出 `TData` 的过程 |
| 发射器 (Emitter) | 将 `TData` 序列化到 `StringBuilder` 的过程 |
| 序列化器块 (Serializer Block) | `ISerializerBlock<T>` 实例，同时持有扫描和发射能力 |

## 编译期

| 术语 | 说明 |
|------|------|
| Source Generator (SG) | Roslyn `IIncrementalGenerator`，编译期生成 C# 源码 |
| 接口分派 (Interface Dispatch) | 扫描器按声明顺序尝试所有具现类型，首个推进者胜出 |
| 默认接口模板 | 系统内置的接口模板（IList、ISet、IReadOnlyList、IDictionary、IReadOnlyDictionary、Array） |
| Roslyn 回退 | 不在 openGenerics 中时，通过 AllInterfaces 查找匹配接口 |

## 泛型

| 术语 | 说明 |
|------|------|
| 开放泛型 (Open Generic) | `Wrapper<T>`，类型参数未关闭 |
| 闭合泛型 (Closed Generic) | `Wrapper<float>`，类型参数已关闭 |
| 泛型传递闭包 | `List<Wrapper<float>>` 中的递归合成过程 |
