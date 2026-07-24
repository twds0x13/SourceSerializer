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
    details: Compile-time SG emits C# span scanners. Runtime zero heap allocation, no GC, Burst-compatible.
  - title: Managed / Unmanaged Full Coverage
    details: Unmanaged types with zero heap allocation; class, string, List, Dictionary fully supported for both Scan and Emit. Roslyn IsUnmanagedType compile-time dispatch.
  - title: Cross-Assembly + Hot Reload
    details: GeneratedSerializers.Init() discovered via AppDomain reflection. Interface dispatch chain merging (ChainBlock) lets hot-reload DLLs extend existing interfaces dynamically.
  - title: Four XML Primitives + Collection Format
    details: Literal text, field, optional block, repetition block. List()/Dict()/HashSet() function-call collection format, strings always quoted.
  - title: 13 Built-in Type Scanners
    details: float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char, string. Hand-written zero-allocation span scanners, public API.
  - title: Complete Compile-time Diagnostics
    details: "6 diagnostic codes (SSR001-SSR006) — syntax errors, circular deps, readonly fields, missing types, scalar-in-repetition, template ambiguity. All caught at compile time."
  - title: Generic Synthesis + Interface Dispatch
    details: Open generic templates auto-synthesize concrete instances. Interface fields auto-dispatch to concrete types, first-match-wins.
  - title: Convenience API
    details: SerializerBlocks.Serialize&lt;T&gt;() / Deserialize&lt;T&gt;() one-liners. Builder fluent chain. AddBlock/RemoveBlock generic and non-generic dual API.
---
