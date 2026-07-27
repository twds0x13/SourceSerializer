# Internals

## Interface Dispatch Algorithm

The scanner tries all registered concrete implementations for each interface and selects the one advancing farthest in the input:

```
Input: "3.5, -2"     → Try Vec2.Scan → reaches end ← selected
Input: "3.5, -2, 7.1" → Try Vec2.Scan → matches only first two fields
                       → Try Vec3D.Scan → reaches end ← selected
```

The emitter uses a C# `switch` pattern match for runtime type dispatch.

## Generic Resolution Roslyn Fallback

When `ParseGenericType` fails to find a type in `openGenerics`, it does not immediately give up. `TryResolveViaInterfaces` uses Roslyn `Compilation.GetTypeByMetadataName` to resolve BCL types, inspects their `AllInterfaces`, and finds matching default interface templates.

For multiple matches, Roslyn inheritance relationships filter to the most derived interface. If still tied, a fixed priority order is used: `IList > ISet > IReadOnlyList > IDictionary > IReadOnlyDictionary`.

## Collection Emit

Self-collection types (`List<T>`, `HashSet<T>`) use a `foreach` + `<first>`/`<body>` pattern:

```csharp
// First element: no separator
// Subsequent: foreach skipping first, with separator
foreach (var item in value) { ... }
```

## Whitespace Stripping (WhitespaceStripper)

`WhitespaceStripper` is the core runtime input preprocessing component, automatically invoked inside `Deserialize<T>()` and `TryScan<T>()`. It uses a two-pass zero-allocation algorithm:

**Pass 1 (count)**: traverses the input, skips all whitespace outside quoted strings, and computes the output length. An `inString` state flag tracks whether the current position is inside double-quote delimiters — whitespace inside quotes (and `\"` escapes) is fully preserved in the output length.

**Pass 2 (fill)**: creates the target-length string via `string.Create`, and in the callback re-traverses the input, writing non-whitespace characters (and all characters inside quotes) to the output buffer.

Early-return optimizations: if the output length equals the input length (no whitespace to strip), the original string is returned directly to avoid copying. If the output length is 0 (all whitespace), `string.Empty` is returned.

Design rationale: separating whitespace preprocessing from per-type `Scan_Xxx` methods centralizes it into a single zero-allocation utility. Generated scanner code does not need whitespace-skip branches before every literal text match — scanners assume the input is already compacted. This separation also allows the whitespace handling strategy to evolve independently (e.g., future comment support) without touching per-type generated code.

## Array Buffer Strategy

`T[]` array fields and `List<T>` collection fields use different assignment paths in scanner code generation:

| Field Type | Codegen Path | Final Assignment |
|-----------|-------------|------------------|
| `List<T>` | Declare `var __list = new List<T>()`, loop with `__list.Add(item)` | Direct assignment |
| `T[]` | Declare `var __buf = new T[16]`, loop writes to `__buf[__cnt++]`, doubling grow | `Array.Copy(__buf, value, __cnt)` |

The `IsArrayCollection` flag (in `SerializerGenerator.cs` line 1346) controls path selection. Since `T[]` cannot use `.Add()`, it uses a pre-allocated buffer + tracked count + final `Array.Copy` approach. The buffer starts at 16 elements and doubles on overflow (up to 1024, then ×2 growth).

Design rationale: both `List<T>`'s internal growth strategy and `T[]`'s explicit buffer management target the same goal — avoiding quadratic copies from one-at-a-time reallocation. The difference is that `List<T>` encapsulates the growth logic (`.Add()` handles it internally), while `T[]` requires the code generator to explicitly manage the buffer.

## EmitHelpers Shared Utilities

`EmitHelpers` unifies method name generation (`GetMethodName`), unique variable naming (`GetUniqueVar`), and counter management across ScanCodeEmitter and EmitCodeEmitter. Eliminates ~20 lines of duplicated code.

## See Also

- [SG Pipeline Overview](./pipeline/overview): Compile-time pipeline stages
- [Architecture Decisions](./architecture-decisions): Design trade-offs
- [Known Issues](./known-issues): Class type variable scoping issue
- [SerializerBlocks API](../api/serializer-blocks): Runtime registry
- [Hot Reload & Cross-Assembly](../guide/hot-reload): ChainBlock usage scenarios
