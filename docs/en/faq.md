# Troubleshooting Guide

A symptom-organized diagnostic guide. If you are not sure where to start, match your symptom first, then follow the steps.

## Deserialization Failures

### Symptom: `TryGet<T>` returns false

The type has no registered serializer block.

**Checklist:**

1. Does the type have a `[Template]` attribute? SSR003 catches missing template dependencies at compile time, but hot-reload DLLs can bypass SG compilation.
2. Are all fields marked with `[TemplateIgnore]`? An empty template still registers the type, but Scan consumes no input.
3. Was `EnsureInitialized()` called? The first `TryGet<T>` triggers it automatically. If `RemoveBlock<T>` was called manually beforehand, subsequent `TryGet<T>` will not re-trigger initialization.
4. Cross-assembly scenario: is the hot-reload DLL's `GeneratedSerializers.Init()` explicitly invoked? `EnsureInitialized()` only scans loaded assemblies once on the first `TryGet<T>`. Later-loaded DLLs require manual initialization.

### Symptom: `Scan` returns pos (no advance)

The input format does not match the template. Scan cannot recognize the first literal character or field type at the current position.

**Checklist:**

1. Compare field by field: are delimiters consistent? Does the template have a space after commas?
2. Are string fields quoted? `Scan_String` requires double quotes.
3. Are enum tags spelled correctly? The `[Tag("fire")]` scanner matches the tag string exactly.
4. Optional blocks: fields at default values can be omitted from input — this is normal behavior, not a parse failure.
5. Interface dispatch prefix ambiguity: if two concrete type templates are prefixes of each other (`Vec(x,y)` and `Vec(x,y,z)`), the scanner may stop early on the first type. Check SSR005.
6. If calling `block.Scan(span, pos, out _)` directly, input whitespace is **not** automatically stripped. Use `Deserialize<T>()` or call `WhitespaceStripper.Strip()` manually first.

### Symptom: `Deserialize<T>` throws an exception

- `InvalidOperationException: "No SerializerBlock registered for X"` — type unregistered, see the TryGet checklist above.
- `FormatException: "Failed to deserialize 'X' as Y"` — Scan failed but block exists, see the Scan checklist above.

### Symptom: Whitespace causes parse failures

**Only affects direct `block.Scan` calls.** `Deserialize<T>()` and `TryScan<T>()` automatically invoke `WhitespaceStripper.Strip()` in v3.4+.

When calling `block.Scan(text, 0, out _)` directly:
- Literal text in templates requires exact character-by-character match. `"Point2D(3.5, -2.1)"` and `"Point2D( 3.5 , -2.1 )"` are not equivalent.
- Solution: use `Deserialize<T>()` or manually call `WhitespaceStripper.Strip(text)` before Scan.

## Serialization Issues

### Symptom: Emit output does not match template definition

1. Does field order match the template declaration order? Emit outputs fields in template order.
2. Enum fields without `[Tag]` fall back to `value.ToString()`, outputting the C# member name rather than a custom string.
3. Optional blocks: when a field equals its default value, the entire optional block is skipped. This is by design, not a bug.
4. String fields are handled by `Emit_String`, which always outputs double quotes.

### Symptom: Output lacks indentation

Does the template use `<indent>` tags? `<indent>...</indent>` injects newlines and tab indentation on Emit. For indentation inside collection fields, nest `<indent>` inside `<first>`/`<body>`:

```csharp
[Template("Config(<indent><first><string K>: <float V></first><body>, <string K>: <float V></body></indent>)")]
```

## Compile-Time Errors

### SSR001-SSR006 Quick Reference

| Code | Title | Trigger | Fix |
|------|-------|---------|-----|
| SSR001 | Template Parse Error | Template string does not conform to compact or XML syntax | Check angle bracket closure and quote pairing |
| SSR002 | Readonly field | readonly field with no matching constructor | Provide a constructor with parameters matching fields by name and type |
| SSR003 | Missing template dependency | Field type has no `[Template]` and is not a built-in type | Add `[Template]`, `[ExternalTemplate]`, or `[TemplateIgnore]` |
| SSR004 | Scalar field in repetition | Non-collection field inside `<repetition>` | Use a collection type like `List<T>` |
| SSR005 | Template ambiguity | Two concrete types sharing an interface have prefix-ambiguous templates | Adjust templates so prefixes are distinguishable |
| SSR006 | Overriding built-in type | `[ExternalTemplate]` targets one of the 16 built-in types | Remove ExternalTemplate, wrap in a higher-level template |

## Performance

### Symptom: Excessive string allocations (GC pressure)

1. `Scan` accepts `ReadOnlySpan<char>` — do not create substrings; pass span slices directly.
2. `Deserialize<T>()` internally calls `WhitespaceStripper.Strip()` which produces a new string — for high-frequency use, bypass the allocation with `TryGet` + `Scan(span)`.
3. `Emit` uses `StringBuilder` — reuse the StringBuilder instance by calling `Clear()` instead of `new StringBuilder()`.
4. Enum tag switch-on-string scanners: tag length affects match performance; put high-frequency tags earlier in the switch.

### Symptom: Initialization delay

`EnsureInitialized()` reflectively scans all loaded assemblies. The first call latency depends on assembly count (typically milliseconds). Optimization: warm up early in startup by calling `TryGet<AnyKnownType>` once. Subsequent calls have zero overhead.

`SerializerBlocks.Serialize<T>()` and `Deserialize<T>()` internally call `TryGet<T>` on every invocation — a static field read of about 2ns; no additional caching needed.

## Cross-Assembly / Hot Reload

### Symptom: Hot-reload DLL types cannot be deserialized

1. Did the SG run during the DLL's compilation? Check for `.g.cs` files in the `obj/` directory.
2. Was `GeneratedSerializers.Init()` explicitly called after loading the DLL? `EnsureInitialized()` only scans once on first `TryGet<T>`.
3. Interface types: verify chain merge is correct — new types append to the tail of `ChainBlock<T>`, preserving the parse priority of existing types.

### Symptom: Type still usable after `RemoveBlock<T>`

`RemoveBlock<T>()` is not an idempotent inverse — if the same type was registered via `AddBlock` twice (e.g., once from the main assembly and once from a hot-reload DLL), `RemoveBlock` removes the entire chain, invalidating all registrations at once. Subsequent `AddBlock` is required to restore.

For interface types, `RemoveBlock` removes the entire `ChainBlock<T>` (all assemblies' contributions). Individual assembly contributions cannot be removed separately.

### Symptom: `AddBlock` has no effect

The first `AddBlock<T>` call triggers `EnsureInitialized()`. If hot-reload DLLs load after this point, they are not automatically discovered. Required ordering:

```csharp
// Correct order
DLL.Load("hotfix.dll");           // 1. Load DLL first
DLL.Invoke("GeneratedSerializers.Init");  // 2. Explicit init
// TryGet can now find the DLL's new types
```

## ExternalTemplate Pitfalls

### Symptom: ExternalTemplate override of built-in types does not work

`ExternalTemplate(typeof(float), ...)` triggers SSR006 at compile time. The 16 built-in types are handled by hand-written zero-allocation span scanners and cannot be overridden.

Solution: wrap the built-in type in a higher-level template:

```csharp
// Wrong
[assembly: ExternalTemplate(typeof(float), "Float(<float>)")]  // SSR006

// Correct
[Template("MyFloat(<float Value>)")]
struct MyFloat { float Value; }
```

### Symptom: ExternalTemplate override of default collection templates does not work

The parameter to `ExternalTemplate(typeof(List<>), ...)` must be an open generic (`typeof(List<>)`), not a concrete instance (`typeof(List<float>)`).

Class-level `ExternalTemplate` takes precedence over interface default templates — if the same type has both a class-level override and an interface-level default, the class-level override wins. Check for conflicting `ExternalTemplate` declarations.

## See Also

- [Diagnostics](/en/guide/diagnostics): complete SSR001-SSR006 error code reference
- [Internals](/en/technical/internals): interface dispatch, ChainBlock merge, WhitespaceStripper implementation
- [Indent & Whitespace Handling](/en/guide/indent-and-whitespace): `<indent>` syntax and three-tier whitespace strategy
- [Hot Reload & Cross-Assembly](/en/guide/hot-reload): ChainBlock usage scenarios
- [Migration Guide](/en/migration-guide): API changes between versions
