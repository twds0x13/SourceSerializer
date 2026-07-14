# SourceSerializer

[English](./README.en.md)

[![.NET Test](https://github.com/twds0x13/SourceSerializer/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/twds0x13/SourceSerializer/actions/workflows/dotnet-test.yml)
[![Codecov](https://codecov.io/gh/twds0x13/SourceSerializer/branch/main/graph/badge.svg)](https://app.codecov.io/gh/twds0x13/SourceSerializer)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

编译期序列化：用 attribute 声明结构，source generator 在编译期输出零分配解析器。

## JSON vs SourceSerializer

| 维度 | JSON（反射驱动） | SourceSerializer（编译期生成） |
|------|----------------|------------------------------|
| Schema 在哪里 | 运行时从类型反射 | 编译期 attribute 声明 |
| 解析器何时产生 | 运行时解释 Schema | 编译期 SG 输出 C# |
| 内存分配 | 堆分配 + 装箱 | 零堆分配，out 输出 |
| 类型安全 | `object` 中转 | `out TData value` 强类型 |
| 错误发现时机 | 运行时 NRE | 编译期诊断 |

## 特性

- `[Template("...")]` 声明结构体布局：字段、分隔符、可选块、可重复序列
- Unmanaged 路径：`stackalloc` span 扫描器，零堆分配，Burst 兼容
- 序列化方向：编译期生成 `SerializerEmitters`，struct 到 StringBuilder 零分配
- Managed 路径：两步走，无需 `$ref` 即可处理循环引用（规划中）
- 12 种 C# 内置类型的内置扫描器与发射器（float、int、bool、char 等）

## 安装

```json
"com.twds0x13.sourceserializer": "https://github.com/twds0x13/SourceSerializer.git#main"
```

## 快速开始

```csharp
using SourceSerializer;

[Template("<float damage><repetition>, <float multipliers></repetition>")]
public struct DamageData
{
    public float damage;
    public float multipliers;
}

SerializerScanners.TryGetScanner<DamageData>(out var scan);
scan("42, 1.5, 2.0".AsSpan(), 0, out DamageData v);
// v.damage == 42, v.multipliers == 2.0

// 序列化
SerializerEmitters.TryGetEmitter<DamageData>(out var emit);
var sb = new StringBuilder();
emit(sb, v);  // sb.ToString() == "42, 1.5, 2.0"
```

## 文档

[twds0x13.github.io/SourceSerializer](https://twds0x13.github.io/SourceSerializer/)
