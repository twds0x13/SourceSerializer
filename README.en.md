# SourceSerializer

[![.NET Test](https://github.com/twds0x13/SourceSerializer/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/twds0x13/SourceSerializer/actions/workflows/dotnet-test.yml)
[![Codecov](https://codecov.io/gh/twds0x13/SourceSerializer/branch/main/graph/badge.svg)](https://app.codecov.io/gh/twds0x13/SourceSerializer)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Compile-time serialization: declare schema with attributes, source generator emits a zero-allocation parser at build time.

## JSON vs SourceSerializer

| Dimension | JSON (reflection-driven) | SourceSerializer (compile-time) |
|-----------|--------------------------|--------------------------------|
| Schema location | Reflected from types at runtime | Compile-time attribute declarations |
| Parser generation | Runtime schema interpretation | Compile-time SG emits C# |
| Memory allocation | Heap allocation + boxing | stackalloc, zero GC |
| Type safety | `object` passthrough | `out TData value` strongly typed |
| Error discovery | Runtime NRE | Compile-time diagnostics |

## Features

- `[Template("...")]` declares struct layout: fields, separators, optional blocks, repeatable sequences
- Unmanaged path: `stackalloc` span scanner, zero heap allocation, Burst-compatible
- Managed path: two-phase walk-then-serialize, circular references without `$ref` (planned)
- Built-in scanners for 12 C# primitive types (float, int, bool, char, etc.)

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

SerializerScanners.TryGetScanner<DamageData>(out var scan);
scan("42, 1.5, 2.0".AsSpan(), 0, out DamageData v);
// v.damage == 42, v.multipliers == 2.0
```

## Documentation

[twds0x13.github.io/SourceSerializer](https://twds0x13.github.io/SourceSerializer/)
