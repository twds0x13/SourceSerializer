# `SerializerRegistry`

内置类型注册表。提供 12 种 C# 内置 unmanaged 类型的零分配 span 扫描方法。

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

## 扫描方法约定

所有扫描方法遵循统一签名：

```csharp
internal static int Scan_Xxx(ReadOnlySpan<char> src, int pos, out Xxx value)
```

返回值约定：`> pos` 表示匹配成功并返回结束位置；`== pos` 表示未匹配（解析失败），value 为 `default`。
