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
    details: Compile-time SG emits C# span scanners. Runtime stackalloc allocation, no heap memory, Burst-compatible.
  - title: Managed / Unmanaged Dual Strategy
    details: unmanaged types use single-pass span scanning; managed types use two-phase Walk-Serialize with native circular reference support.
  - title: Four XML Primitives
    details: Literal text, field, optional block, repetition block. Compact syntax and XML syntax are equivalent and interchangeable.
  - title: 12 Built-in Type Scanners
    details: float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char. Hand-written zero-allocation span scanners.
  - title: Compile-time Error Diagnostics
    details: Circular dependency detection, readonly struct rejection, missing type warnings. Errors surface at compile time, not at runtime.
---
