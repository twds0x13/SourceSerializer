# SourceSerializer

[中文](./README.md)

[![.NET Test](https://github.com/twds0x13/SourceSerializer/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/twds0x13/SourceSerializer/actions/workflows/dotnet-test.yml)
[![Codecov](https://codecov.io/gh/twds0x13/SourceSerializer/branch/main/graph/badge.svg)](https://app.codecov.io/gh/twds0x13/SourceSerializer)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Compile-time serialization: declare schema with attributes, source generator emits a zero-allocation parser at build time.

## JSON vs SourceSerializer

| Dimension | JSON (reflection-driven) | SourceSerializer (compile-time) |
|-----------|--------------------------|--------------------------------|
| Schema location | Reflected from types at runtime | Compile-time attribute declarations |
| Parser generation | Runtime schema interpretation | Compile-time SG emits C# |
| Memory allocation | Heap allocation + boxing | zero heap allocation, zero GC |
| Type safety | `object` passthrough | `out TData value` strongly typed |
| Error discovery | Runtime NRE | Compile-time diagnostics |

## Features

- `[Template("...")]` declares struct layout: fields, separators, optional blocks, repeatable sequences
- Unmanaged path: span scanner, zero heap allocation, Burst-compatible
- Serialization direction: compile-time generated `SerializerBlocks`, struct to StringBuilder with zero allocation
- Built-in scanners and emitters for 17 C# primitive types (float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char, string, decimal, nint, nuint, Half)

## Installation

```json
"com.twds0x13.sourceserializer": "https://github.com/twds0x13/SourceSerializer.git#main"
```

## Quick Start

```csharp
using SourceSerializer;

[Template("<float damage><repetition>, <float multipliers></repetition>")]
public struct DamageData
{
    public float damage;
    public float multipliers;
}

SerializerBlocks.TryGet<DamageData>(out var block);
block.Scan("42, 1.5, 2.0".AsSpan(), 0, out DamageData v);
// v.damage == 42, v.multipliers == 2.0

// Serialization
var sb = new StringBuilder();
block.Emit(sb, v);  // sb.ToString() == "42, 1.5, 2.0"
```

## Documentation

[twds0x13.github.io/SourceSerializer](https://twds0x13.github.io/SourceSerializer/)
