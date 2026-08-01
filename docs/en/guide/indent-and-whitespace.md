# Indent Blocks & Whitespace Handling

SourceSerializer uses a three-tier strategy for fully transparent input whitespace handling and declarative output formatting. Users write zero code for input preprocessing — `Deserialize<T>()` and `TryScan<T>()` handle it automatically.

## Three-Tier Whitespace Strategy

```
Compile-time              Runtime                         Emit-time
compactWhitespace  →  WhitespaceStripper construction  →  <indent> newline+indent
(reduce generated code)   (transparent user input)        (formatted output)
```

**Tier 1 (Compile-time)**: `ScanCodeEmitter` strips whitespace from literal text nodes in templates before generating `Scan_Xxx` methods (`compactWhitespace: true`). The string constants used for exact matching in generated C# source are whitespace-free.

**Tier 2 (Runtime)**: `Deserialize<T>()` and `TryScan<T>()` automatically construct `WhitespaceStripper` on input before calling `block.Scan`. This tier is fully transparent to callers.

**Tier 3 (Emit-time)**: `<indent>` tags inject newlines and tab indentation during Emit for readable hierarchical output. This tier has no effect on Scan.

## `<indent>` Indent Block

### Syntax

Both compact and XML formats are supported:

```csharp
// Compact format
[Template("Zone(<string Name><indent>, <float X>, <float Y></indent>)")]

// XML format
<indent>
  <text>, </text>
  <field type="float" name="X"/>
  <text>, </text>
  <field type="float" name="Y"/>
</indent>
```

### Scan behavior: no-op

`<indent>` is a pure emit-time directive. During Scan it is identity — body nodes parse as usual, and the `<indent>`/`</indent>` tags are completely skipped. In implementation, `ScanCodeEmitter.EmitNode` directly recurses into the body when encountering an `IndentNode`, with no additional operations.

Design rationale: separating formatting concerns from parsing logic. Parsing behavior is unaffected by indentation — whether `<indent>` is present in a template or not, Scan behaves identically. Output formatting changes never propagate to parsing correctness.

### Emit behavior: newlines + indentation

During Emit, the `<indent>` opening tag injects `\n` + `(indentLevel+1)` tabs, and the closing tag injects `\n` + `indentLevel` tabs. `indentLevel` starts at 0 and increments by 1 for each nested `<indent>`:

```csharp
// Template
[Template("Config(<indent><first><string Key>: <float Value></first><body>, <string Key>: <float Value></body></indent>)")]

// Input: Config(hp: 100, atk: 50, def: 30)
// Emit output:
// Config(
//   hp: 100,
//   atk: 50,
//   def: 30
// )
```

### Nesting with `<repetition>`

`<indent>` can wrap `<repetition>` for tree-like hierarchical output. Since `<indent>` is skipped during Scan, `<first>/<body>` semantics are unaffected:

```csharp
[Template("Zone(<string Name><indent>, <float X>, <float Y><repetition><first><List<Item> Items></first><body>, <List<Item> Items></body></repetition></indent>)")]

// Emit output:
// Zone("safe_zone",
//   100, 200,
//   List(Item("sword", 10), Item("shield", 5))
// )
```

### Nesting with `<optional>`

When `<indent>` wraps `<optional>` and all optional fields are at their default values, the entire optional block (including the indentation injected by `<indent>`) is skipped — no newlines, no tabs:

```csharp
[Template("Player(<string Name><indent><optional>, <float HP>, <float MP></optional></indent>)")]

// Input 1: Player("warrior", 100, 50)
// Emit: Player("warrior",
//         100, 50)

// Input 2: Player("warrior")  (HP=0, MP=0 → default, optional skipped)
// Emit: Player("warrior")
```

## WhitespaceStripper Runtime Preprocessing

### Algorithm Overview

`WhitespaceStripper` is a `readonly ref struct` that completes stripping on construction. It uses a two-pass algorithm:

```csharp
public readonly unsafe ref struct WhitespaceStripper
{
    public ReadOnlySpan<char> Span { get; }
    public WhitespaceStripper(string text) { /* ... */ }
    public void Dispose() { /* ... */ }  // duck-typed using pattern
}
```

**Pass 1 (count)**: traverses the input, computing the output length after stripping whitespace outside quoted strings. An `inString` boolean flag toggles on encountering `"`. Inside `inString`, all characters (including whitespace and `\"` escape sequences) count toward the output length. Outside `inString`, characters where `char.IsWhiteSpace(c)` returns true are skipped.

**Pass 2 (fill)**: executed only when whitespace needs stripping. Allocates a native memory buffer via `Marshal.AllocHGlobal`, then re-traverses the input writing preserved characters to the buffer. The `Span` property points to this native buffer; `Dispose()` releases it via `Marshal.FreeHGlobal`.

**Three-state dispatch**:
- `outputLen == input.Length`: no whitespace. `Span` points back to the original string — zero allocation.
- `outputLen == 0`: all whitespace. `Span = ReadOnlySpan<char>.Empty` — zero allocation.
- Otherwise: native memory allocation, released on `Dispose()`.

### Call Sites

`WhitespaceStripper` is automatically constructed and used in two places:

1. `SerializerBlocks.Deserialize<T>(string text)` — before calling Scan
2. `SerializerBlocks.TryScan<T>(string text, out TData value)` — before calling Scan

```csharp
using var compact = new WhitespaceStripper(text);
block.Scan(compact.Span, 0, out value);
```

Direct calls to `block.Scan(span, pos, out _)` do **not** trigger whitespace preprocessing — callers must ensure their input is already compacted, or manually construct `new WhitespaceStripper(text)`.

### Design Rationale

Separating whitespace preprocessing from per-type `Scan_Xxx` methods yields three benefits:

1. **Simpler scanners**: generated `Scan_Xxx` methods don't need whitespace-skip branches before every literal text match. Smaller generated code, fewer runtime branches.
2. **Independent strategy evolution**: whitespace handling strategy (e.g., future comment support) can be modified in `WhitespaceStripper` alone without touching per-type generated code.
3. **Centralized optimization**: the two-pass native memory approach concentrates allocation in a single point. If distributed across per-type scanners, each would need its own whitespace-skip logic and allocation strategy.

## compactWhitespace Compile-Time Optimization

`ScanCodeEmitter` strips whitespace from literal text nodes in the template AST before generating code. The `EmitAll` method receives `compactWhitespace: true` (passed from `SerializerGenerator` line 635).

Effect example: for the template `"Point2D(<float X>, <float Y>)"`, the literal text nodes are `"Point2D("`, `", "`, `")"`. compactWhitespace mode strips whitespace from these strings before writing them into generated code — while this specific example's text contains no whitespace, for multi-line XML templates with embedded spaces and newlines, this optimization reduces the string constant size in generated code.

Note: this option is not user-configurable — it is hardcoded as enabled by the SG at compile time, complementing the runtime `WhitespaceStripper`. Compile-time stripping targets **whitespace in template text** (generated code size optimization); runtime stripping targets **whitespace in user input** (parse tolerance).

## Complete Example: From Template to Formatted Output

```csharp
// 1. Declare template (with <indent> for formatting)
[Template("Config(<string Name><indent><first><string K>: <float V></first><body>, <string K>: <float V></body></indent>)")]
struct Config
{
    string Name;
    List<ConfigEntry> Entries;
}

// 2. Deserialize (auto whitespace stripping)
var config = SerializerBlocks.Deserialize<Config>(
    "  Config( server ,  host : 8080 ,  port : 443 ,  timeout : 30 )  ");
// Whitespace handled automatically by WhitespaceStripper

// 3. Serialize (<indent> injects formatted output)
string output = SerializerBlocks.Serialize(config);
// Config(server,
//   host: 8080,
//   port: 443,
//   timeout: 30
// )
```

This example demonstrates all three tiers cooperating: input whitespace is stripped by `WhitespaceStripper` (step 2), output formatting is injected by `<indent>` (step 3), and generated scanner code contains no whitespace matching logic (compile-time optimization from step 1).

## See Also

- [Template Syntax](/en/guide/template-syntax): complete syntax reference for all five primitives
- [Internals](/en/technical/internals): source-level implementation details of WhitespaceStripper and Array buffer strategy
- [SG Pipeline Overview](/en/technical/pipeline/overview): compactWhitespace's position in the pipeline
- [Troubleshooting Guide](/en/faq): diagnostic steps for whitespace-related parse failures
