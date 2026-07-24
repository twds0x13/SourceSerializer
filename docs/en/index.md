---
layout: home

hero:
  name: "SourceSerializer"
  text: Compile-time Serializer Generator
  tagline: Attribute-defined schema. Source-generated parser at compile time. Zero reflection, zero boxing.
  image:
    src: /logo.svg
    alt: SourceSerializer
  actions:
    - theme: brand
      text: Get Started
      link: /en/guide/getting-started
    - theme: alt
      text: API Reference
      link: /en/api/

features:
  - title: Zero Reflection, Zero Boxing
    details: Compile-time SG emits C# span scanners. Runtime zero heap allocation, no heap memory, Burst-compatible.
  - title: Managed / Unmanaged Dual Strategy
    details: unmanaged types use single-pass span scanning, zero heap allocation; managed type deserialization is implemented, determined at compile time via Roslyn IsUnmanagedType.
  - title: Four XML Primitives
    details: Literal text, field, optional block, repetition block. Compact syntax and XML syntax are equivalent and interchangeable.
  - title: 13 Built-in Type Scanners
    details: float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char, string. Hand-written zero-allocation span scanners.
  - title: Compile-time Error Diagnostics
    details: Circular dependency detection, readonly field rejection, missing type warnings, scalar-in-repetition errors. Errors surface at compile time, not at runtime.
  - title: Compile-time Emitter
    details: Simultaneously generates the SerializerBlocks pipeline. Struct-to-StringBuilder serialization with zero allocation, supporting built-in types, custom nested types, and enum tags.
---
