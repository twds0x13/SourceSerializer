# 故障排除指南

按症状组织的诊断指南。如果你不知道从哪开始，先看症状，再按步骤排查。

## 反序列化失败

### 症状：`TryGet<T>` 返回 false

类型没有注册序列化器块。

**检查流程：**

1. 类型是否标注了 `[Template]`？SSR004 在编译期拦截缺失的模板依赖，但热更 DLL 可能跳过 SG 编译。
2. 所有字段是否被 `[TemplateIgnore]` 标记导致空模板？空模板的类型仍然注册，但 Scan 不消费任何输入。
3. `EnsureInitialized()` 是否被调用？首次 `TryGet<T>` 自动触发，但如果在此之前手动调用了 `RemoveBlock<T>`，再调用 `TryGet<T>` 不会重新触发初始化。
4. 跨程序集场景：热更 DLL 的 `GeneratedSerializers.Init()` 是否被显式调用？`EnsureInitialized()` 只在首次 `TryGet<T>` 时扫描已加载程序集，后续加载的 DLL 需手动初始化。

### 症状：`Scan` 返回 pos（未推进）

输入格式与模板不匹配。Scan 在当前位置无法识别模板的第一个字面字符或字段类型。

**检查流程：**

1. 逐字段对比模板和输入：分隔符是否一致？模板中的逗号后是否有空格？
2. 字符串字段是否用引号包裹？`Scan_String` 要求双引号。
3. 枚举标签是否拼写正确？`[Tag("fire")]` 的扫描器精确匹配标签字符串。
4. 可选块：字段为 default 时输入可以省略该字段，这是正常行为而非解析失败。
5. 接口分派的前缀歧义：如果两个具现类型模板互为前缀（`Vec(x,y)` 和 `Vec(x,y,z)`），扫描器可能在第一个类型处提前停止。检查 SSR006。
6. 如果直接调用 `block.Scan(span, pos, out _)`，输入中的空白符**不会**被自动剔除。应使用 `Deserialize<T>()` 或先手动 `WhitespaceStripper.Strip()`。

### 症状：`Deserialize<T>` 抛出异常

- `InvalidOperationException: "No SerializerBlock registered for X"` — 类型未注册，见上方 TryGet 流程。
- `FormatException: "Failed to deserialize 'X' as Y"` — Scan 失败但 block 存在，见上方 Scan 流程。

### 症状：空白符导致解析失败

**仅影响直接调用 `block.Scan` 的场景。** `Deserialize<T>()` 和 `TryScan<T>()` 在 v3.4+ 自动调用 `WhitespaceStripper.Strip()` 预处理输入。

如果直接调用 `block.Scan(text, 0, out _)`：
- 模板中的字面文本需逐字符精确匹配，`"Point2D(3.5, -2.1)"` 和 `"Point2D( 3.5 , -2.1 )"` 不等价。
- 解决方案：使用 `Deserialize<T>()` 或在调用 Scan 前手动 `WhitespaceStripper.Strip(text)`。

## 序列化问题

### 症状：Emit 输出与模板定义不一致

1. 字段顺序与模板声明顺序是否一致？Emit 按模板中字段出现顺序输出。
2. 枚举字段没有 `[Tag]` 标签时，Emit 回退到 `value.ToString()`，输出为枚举成员的 C# 名称而非自定义字符串。
3. 可选块：字段值为 `default` 时，整个可选块被跳过不输出。这是设计行为，不是 bug。
4. 字符串字段由 `Emit_String` 处理，始终输出双引号包裹。

### 症状：输出格式缺少缩进

模板是否使用了 `<indent>` 标签？`<indent>...</indent>` 在 Emit 时注入换行+缩进制表符。集合内字段的缩进需要在 `<first>`/`<body>` 中嵌套 `<indent>`：

```csharp
[Template("Config(<indent><first><string K>: <float V></first><body>, <string K>: <float V></body></indent>)")]
```

## 编译期错误

### SSR001-SSR007 速查表

| 代码 | 标题 | 触发条件 | 修复 |
|------|------|---------|------|
| SSR001 | 模板解析错误 | 模板字符串不符合 compact 或 XML 语法 | 检查尖括号闭合、引号配对 |
| SSR002 | 循环模板依赖 | A 引用 B，B 引用 A | 打破循环，将一环改为内置类型 |
| SSR003 | 只读字段 | readonly 字段且无匹配构造器 | 提供参数与字段按名称类型匹配的构造器 |
| SSR004 | 缺失模板依赖 | 字段类型无 `[Template]` 且不是内置类型 | 加 `[Template]`、`[ExternalTemplate]` 或 `[TemplateIgnore]` |
| SSR005 | 重复块内的标量字段 | 非集合字段在 `<repetition>` 内 | 改用 `List<T>` 等集合类型 |
| SSR006 | 模板歧义 | 同接口的两个具现类型模板互为前缀 | 调整模板使前缀可区分 |
| SSR007 | 覆盖内置类型 | `[ExternalTemplate]` 目标为 16 种内置类型之一 | 移除 ExternalTemplate，在上层模板包装 |

## 性能

### 症状：大量字符串分配（GC 压力）

1. `Scan` 接受 `ReadOnlySpan<char>`——不要创建 substring，直接传 span 切片。
2. `Deserialize<T>()` 内部调用 `WhitespaceStripper.Strip()` 产生新字符串——高频场景用 `TryGet` + `Scan(span)` 绕过字符串分配。
3. `Emit` 使用 `StringBuilder`——复用 `StringBuilder` 实例，调用 `Clear()` 而非 `new StringBuilder()`。
4. 枚举标签的 switch-on-string 扫描器：标签长度影响匹配性能，高频标签放在 switch 前面。

### 症状：初始化延迟

`EnsureInitialized()` 通过 AppDomain 反射扫描所有已加载程序集。首次调用耗时取决于程序集数量（通常毫秒级）。优化：在启动早期调用一次 `TryGet<任意已知类型>` 预热，后续调用零开销。

`SerializerBlocks.Serialize<T>()` 和 `Deserialize<T>()` 每次调用都走 `TryGet<T>`——静态字段查找，约 2ns，无需额外缓存。

## 跨程序集 / 热更

### 症状：热更 DLL 的新类型无法反序列化

1. DLL 编译时 SG 是否生成？检查 `obj/` 目录中的 `.g.cs` 文件。
2. DLL 加载后是否显式调用了 `GeneratedSerializers.Init()`？`EnsureInitialized()` 仅在首次 `TryGet<T>` 时扫描一次。
3. 接口类型：确认链合并逻辑正确——新类型追加到 `ChainBlock<T>` 尾部，不影响已有类型的解析优先级。

### 症状：`RemoveBlock<T>` 后类型仍然可用

`RemoveBlock<T>()` 不是幂等逆操作——如果同一个类型被 `AddBlock` 注册了两次（例如主程序集和热更 DLL 各注册一次），`RemoveBlock` 移除整条链，所有注册同时失效。后续需要重新 `AddBlock` 恢复。

接口类型 `RemoveBlock` 移除整条 `ChainBlock<T>`（所有程序集的注册），无法单独移除某个程序集的贡献。

### 症状：`AddBlock` 后不生效

`AddBlock<T>` 首次调用触发 `EnsureInitialized()`，如果此时热更 DLL 尚未加载，后续加载的 DLL 不会自动被发现。时序要求：

```csharp
// 正确顺序
DLL.Load("hotfix.dll");           // 1. 先加载 DLL
DLL.Invoke("GeneratedSerializers.Init");  // 2. 显式初始化
// TryGet 现在可以找到 DLL 中的新类型
```

## ExternalTemplate 使用陷阱

### 症状：`ExternalTemplate` 覆盖内置类型不生效

`ExternalTemplate(typeof(float), ...)` 触发 SSR007 编译错误。16 种内置类型由手写零分配 span 扫描器处理，不可覆盖。

解决方案：在更上层模板包装内置类型：

```csharp
// 错误
[assembly: ExternalTemplate(typeof(float), "Float(<float>)")]  // SSR007

// 正确
[Template("MyFloat(<float Value>)")]
struct MyFloat { float Value; }
```

### 症状：`ExternalTemplate` 覆盖默认集合模板不生效

`ExternalTemplate(typeof(List<>), ...)` 的参数必须是开放泛型（`typeof(List<>)`），不能是具体实例（`typeof(List<float>)`）。

类级 `ExternalTemplate` 优先级高于接口默认模板——如果同一类型同时有类级覆盖和接口级默认模板，类级覆盖生效。确认是否有其他 `ExternalTemplate` 干扰。

## 参见

- [编译期诊断](/guide/diagnostics)：SSR001-SSR007 完整错误代码
- [内部机制](/technical/internals)：接口分派、ChainBlock 链合并、WhitespaceStripper 实现
- [缩进与空白符处理](/guide/indent-and-whitespace)：`<indent>` 语法与三层空白符策略
- [热更新与跨程序集注册](/guide/hot-reload)：ChainBlock 使用场景
- [迁移指南](/migration-guide)：版本间 API 变更
