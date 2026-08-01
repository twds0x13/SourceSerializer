# 缩进块与空白符处理

SourceSerializer 通过三层策略实现输入空白符的完全透明和输出格式的声明式控制。用户无需在输入预处理上编写任何代码——`Deserialize<T>()` 和 `TryScan<T>()` 自动处理。

## 三层空白符策略

```
编译期                   运行时                      输出时
compactWhitespace  →  WhitespaceStripper 构造  →  <indent> 换行+缩进
(缩小生成代码)         (用户输入透明化)               (格式化输出)
```

**第一层（编译期）**：`ScanCodeEmitter` 在生成 `Scan_Xxx` 方法前，将模板中 literal text 节点的空白符提前剥离（`compactWhitespace: true`）。这意味着生成的 C# 源码中用于精确匹配的字符串常量已不含空白符。

**第二层（运行时）**：`Deserialize<T>()` 和 `TryScan<T>()` 在调用 `block.Scan` 之前，自动构造 `WhitespaceStripper` 对输入做预处理。这一层对用户完全透明。

**第三层（输出时）**：`<indent>` 标签在 Emit 时注入换行和缩进制表符，用于生成可读的层级化输出。这一层对 Scan 无影响。

## `<indent>` 缩进块

### 语法

Compact 格式和 XML 格式均支持：

```csharp
// Compact 格式
[Template("Zone(<string Name><indent>, <float X>, <float Y></indent>)")]

// XML 格式
<indent>
  <text>, </text>
  <field type="float" name="X"/>
  <text>, </text>
  <field type="float" name="Y"/>
</indent>
```

### Scan 行为：no-op

`<indent>` 是纯 Emit 期指令。Scan 时它等于不存在——body 节点照常解析，`<indent>` 和 `</indent>` 标签被完全跳过。具体实现中，`ScanCodeEmitter.EmitNode` 遇到 `IndentNode` 直接递归进入 body，不做任何额外操作。

设计原理：将格式化关注点从解析逻辑中分离。解析行为不因缩进而改变——无论模板中是否有 `<indent>`，Scan 的行为完全一致。输出格式的改变不波及解析正确性。

### Emit 行为：换行 + 缩进

Emit 时，`<indent>` 开口标签注入 `\n` + `(indentLevel+1)` 个 `\t`，闭合标签注入 `\n` + `indentLevel` 个 `\t`。`indentLevel` 从 0 开始，每嵌套一层 `<indent>` 递增：

```csharp
// 模板
[Template("Config(<indent><first><string Key>: <float Value></first><body>, <string Key>: <float Value></body></indent>)")]

// 输入: Config(hp: 100, atk: 50, def: 30)
// Emit 输出:
// Config(
//   hp: 100,
//   atk: 50,
//   def: 30
// )
```

### 与 `<repetition>` 嵌套

`<indent>` 可以包裹 `<repetition>` 以实现树状层级输出。由于 Scan 时 `<indent>` 被跳过，`<first>/<body>` 的语义不受影响：

```csharp
[Template("Zone(<string Name><indent>, <float X>, <float Y><repetition><first><List<Item> Items></first><body>, <List<Item> Items></body></repetition></indent>)")]

// Emit 输出:
// Zone("safe_zone",
//   100, 200,
//   List(Item("sword", 10), Item("shield", 5))
// )
```

### 与 `<optional>` 嵌套

`<indent>` 包裹 `<optional>` 时，如果可选块的所有字段均为 default 值，整个可选块（连同 `<indent>` 注入的缩进）被跳过——不输出换行，不输出缩进：

```csharp
[Template("Player(<string Name><indent><optional>, <float HP>, <float MP></optional></indent>)")]

// 输入1: Player("warrior", 100, 50)
// Emit: Player("warrior",
//         100, 50)

// 输入2: Player("warrior")  (HP=0, MP=0 → default, optional skip)
// Emit: Player("warrior")
```

## WhitespaceStripper 运行时预处理

### 算法概述

`WhitespaceStripper` 是 `readonly ref struct`，构造即完成剥离。使用两阶段算法：

```csharp
public readonly unsafe ref struct WhitespaceStripper
{
    public ReadOnlySpan<char> Span { get; }
    public WhitespaceStripper(string text) { /* ... */ }
    public void Dispose() { /* ... */ }  // duck-typed using 模式
}
```

**第一遍（计数）**：遍历输入，计算剔除引号外部空白符后的输出长度。维护 `inString` 布尔标志——当遇到 `"` 时切换状态。在 `inString` 内部，所有字符（包括空白符和 `\"` 转义序列）计入输出长度。在 `inString` 外部，`char.IsWhiteSpace(c)` 返回 true 的字符被跳过。

**第二遍（填充）**：仅当输出长度不等于输入长度（有空白符需剔除）时执行。通过 `Marshal.AllocHGlobal` 分配 native memory 缓冲区，重新遍历输入将保留字符写入缓冲区。完成后 `Span` 属性指向该 native buffer，`Dispose()` 时通过 `Marshal.FreeHGlobal` 释放。

**三态决策**：
- `outputLen == input.Length`：无空白符。`Span` 直接回指原串，零分配。
- `outputLen == 0`：全空白。`Span = ReadOnlySpan<char>.Empty`，零分配。
- 其他：分配 native memory，`Dispose()` 时释放。

### 调用位置

`WhitespaceStripper` 在两个位置被自动构造和使用：

1. `SerializerBlocks.Deserialize<T>(string text)` —— 调用 Scan 前
2. `SerializerBlocks.TryScan<T>(string text, out TData value)` —— 调用 Scan 前

```csharp
using var compact = new WhitespaceStripper(text);
block.Scan(compact.Span, 0, out value);
```

直接调用 `block.Scan(span, pos, out _)` **不会**触发空白符预处理——用户需自行保证输入已紧凑化，或手动构造 `new WhitespaceStripper(text)`。

### 设计原理

将空白符预处理从各类型的 `Scan_Xxx` 方法中分离，带来三个收益：

1. **扫描器简化**：生成的 `Scan_Xxx` 方法中的每个 literal text 匹配不需要插入空白符跳过分支。生成的代码体积更小，运行时分支更少。
2. **策略独立演进**：空白符处理策略（如未来增加注释支持）可以修改 `WhitespaceStripper` 实现而不触及任何类型的生成代码。
3. **集中式优化**：两阶段 native memory 方案将内存分配集中在单点。如果分散到每个类型的扫描器中，每个 scanner 都需要自行管理空白符跳过逻辑和分配策略。

## compactWhitespace 编译期优化

`ScanCodeEmitter` 在生成代码前，对模板 AST 中的 literal text 节点执行空白符剥离。实现中，`EmitAll` 方法接收 `compactWhitespace: true` 参数（由 `SerializerGenerator` 在第 635 行传入）。

效果示例：对于模板 `"Point2D(<float X>, <float Y>)"`，literal text 节点包含 `"Point2D("`、`", "`、`")"` 三段文字。compactWhitespace 模式会将这三段文字中的空白符去除后写入生成的代码——虽然这个例子中文字不含空白符，但对于多行 XML 模板中嵌入的空格和换行，此优化减少了生成代码中的字符串常量体积。

注意：此选项非用户可配置——它由 SG 在编译期内置启用，与运行时的 `WhitespaceStripper` 形成互补。编译期剥离的是**模板文字中的空白符**（生成代码体积优化），运行时剥离的是**用户输入中的空白符**（解析容错）。

## 完整示例：从模板到格式化输出

```csharp
// 1. 声明模板（含 <indent> 缩进）
[Template("Config(<string Name><indent><first><string K>: <float V></first><body>, <string K>: <float V></body></indent>)")]
struct Config
{
    string Name;
    List<ConfigEntry> Entries;
}

// 2. 反序列化（自动空白符剔除）
var config = SerializerBlocks.Deserialize<Config>(
    "  Config( server ,  host : 8080 ,  port : 443 ,  timeout : 30 )  ");
// 空白符被 WhitespaceStripper 自动处理

// 3. 序列化（<indent> 注入格式化输出）
string output = SerializerBlocks.Serialize(config);
// Config(server,
//   host: 8080,
//   port: 443,
//   timeout: 30
// )
```

此示例展示了三层策略的协作：输入中的空白符被 `WhitespaceStripper` 剔除（步骤 2），输出中的格式化由 `<indent>` 注入（步骤 3），生成的扫描器代码不含空白符匹配逻辑（步骤 1 对应的编译期优化）。

## 参见

- [模板语法](/guide/template-syntax)：五种原语的完整语法参考
- [内部机制](/technical/internals)：WhitespaceStripper 与 Array 缓冲区的源码级实现细节
- [SG 管线全景](/technical/pipeline/overview)：compactWhitespace 在管线中的位置
- [故障排除指南](/faq)：空白符导致解析失败的诊断步骤
