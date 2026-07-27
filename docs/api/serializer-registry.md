# `SerializerRegistry`

内置类型注册表。提供 13 种 C# 内置 unmanaged 类型的零分配 span 扫描方法。

## 签名

```csharp
public static class SerializerRegistry
```

## 内置类型

| 类型 | 扫描方法 | 支持格式 |
|------|---------|---------|
| `float` | `Scan_Float` | 可选符号、整数、可选小数、可选 f/F/d/D 后缀 |
| `double` | `Scan_Double` | 可选符号、整数、可选小数、可选 e/E 指数、可选 d/D 后缀 |
| `int` | `Scan_Int` | 可选符号、整数 |
| `uint` | `Scan_Uint` | 无符号整数 |
| `long` | `Scan_Long` | 可选符号、整数、可选 L/l 后缀 |
| `ulong` | `Scan_Ulong` | 无符号整数、可选 U/u 后缀、可选 L/l 后缀 |
| `short` | `Scan_Short` | 委托到 Scan_Int，结果截断为 short |
| `ushort` | `Scan_Ushort` | 委托到 Scan_Uint，结果截断为 ushort |
| `byte` | `Scan_Byte` | 委托到 Scan_Uint，结果截断为 byte |
| `sbyte` | `Scan_Sbyte` | 委托到 Scan_Int，结果截断为 sbyte |
| `bool` | `Scan_Bool` | 精确匹配 `true` 或 `false` |
| `char` | `Scan_Char` | 读取单个字符 |
| `string` | `Scan_String` | 引号包裹或非空白字符序列，Emit 始终加引号 |

## 扫描方法约定

所有扫描方法遵循统一签名：

```csharp
public static int Scan_Xxx(ReadOnlySpan<char> src, int pos, out Xxx value)
```

返回值约定：`> pos` 表示匹配成功并返回结束位置；`== pos` 表示未匹配（解析失败），value 为 `default`。

## Emit 方法

每种内置类型同时提供对应的 Emit 方法，签名统一：

```csharp
public static void Emit_Xxx(StringBuilder sb, Xxx value)
```

| 类型 | Emit 方法 | 输出格式 |
|------|----------|---------|
| `float` | `Emit_Float` | G9 格式，`CultureInfo.InvariantCulture` |
| `double` | `Emit_Double` | G17 格式，`CultureInfo.InvariantCulture` |
| `int` | `Emit_Int` | 整数文本 |
| `uint` | `Emit_Uint` | 整数文本 |
| `long` | `Emit_Long` | 整数文本 |
| `ulong` | `Emit_Ulong` | 整数文本 |
| `short` | `Emit_Short` | 整数文本（委托到 `Emit_Int`） |
| `ushort` | `Emit_Ushort` | 整数文本（委托到 `Emit_Uint`） |
| `byte` | `Emit_Byte` | 整数文本（委托到 `Emit_Uint`） |
| `sbyte` | `Emit_Sbyte` | 整数文本（委托到 `Emit_Int`） |
| `bool` | `Emit_Bool` | `"true"` 或 `"false"` |
| `char` | `Emit_Char` | 单字符（无引号） |
| `string` | `Emit_String` | 双引号包裹，null 不输出 |

设计原理：Emit 方法使用 `StringBuilder` 而非返回 `string`——允许调用方在单个 `StringBuilder` 实例上拼接多个字段的输出，避免字符串连接产生的中间分配。`G9`（float）和 `G17`（double）格式保证 round-trip：序列化再反序列化后值不变，符合 IEEE 754 规范。

## 参见

- [SerializerBlocks API](./serializer-blocks)：用户类型注册表
- [核心概念](/guide/core-concepts)：两个注册表的分工
- [模板语法](/guide/template-syntax)：内置类型在模板中的使用
