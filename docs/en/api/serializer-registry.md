# `SerializerRegistry`

Built-in type registry. Provides zero-allocation span scanner methods for 16 C# built-in unmanaged types.

## Signature

```csharp
public static class SerializerRegistry
```

## Built-in Types

| Type | Scanner Method | Supported Format |
|------|---------------|------------------|
| `float` | `Scan_Float` | Optional sign, integer, optional decimal, optional f/F/d/D suffix |
| `double` | `Scan_Double` | Optional sign, integer, optional decimal, optional e/E exponent, optional d/D suffix |
| `int` | `Scan_Int` | Optional sign, integer |
| `uint` | `Scan_Uint` | Unsigned integer |
| `long` | `Scan_Long` | Optional sign, integer, optional L/l suffix |
| `ulong` | `Scan_Ulong` | Unsigned integer, optional U/u suffix, optional L/l suffix |
| `short` | `Scan_Short` | Delegates to Scan_Int, result truncated to short |
| `ushort` | `Scan_Ushort` | Delegates to Scan_Uint, result truncated to ushort |
| `byte` | `Scan_Byte` | Delegates to Scan_Uint, result truncated to byte |
| `sbyte` | `Scan_Sbyte` | Delegates to Scan_Int, result truncated to sbyte |
| `bool` | `Scan_Bool` | Exact match of `true` or `false` |
| `char` | `Scan_Char` | Reads a single character |
| `IntPtr` | `Scan_IntPtr` | Optional sign, integer (platform-dependent width: 4 or 8 bytes) |
| `UIntPtr` | `Scan_UIntPtr` | Unsigned integer (platform-dependent width) |
| `Guid` | `Scan_Guid` | Standard GUID format (36 characters with hyphens) |
| `string` | `Scan_String` | Quoted or unquoted character sequence; Emit always adds quotes |

## Scanner Method Convention

All scanner methods follow a uniform signature:

```csharp
public static int Scan_Xxx(ReadOnlySpan<char> src, int pos, out Xxx value)
```

Return value convention: `> pos` indicates successful match and returns the end position; `== pos` indicates no match (parse failure), value is `default`.

## Emit Methods

Each built-in type also provides a corresponding Emit method with a uniform signature:

```csharp
public static void Emit_Xxx(StringBuilder sb, Xxx value)
```

| Type | Emit Method | Output Format |
|------|------------|---------------|
| `float` | `Emit_Float` | G9 format, `CultureInfo.InvariantCulture` |
| `double` | `Emit_Double` | G17 format, `CultureInfo.InvariantCulture` |
| `int` | `Emit_Int` | Integer text |
| `uint` | `Emit_Uint` | Integer text |
| `long` | `Emit_Long` | Integer text |
| `ulong` | `Emit_Ulong` | Integer text |
| `short` | `Emit_Short` | Integer text (delegates to `Emit_Int`) |
| `ushort` | `Emit_Ushort` | Integer text (delegates to `Emit_Uint`) |
| `byte` | `Emit_Byte` | Integer text (delegates to `Emit_Uint`) |
| `sbyte` | `Emit_Sbyte` | Integer text (delegates to `Emit_Int`) |
| `bool` | `Emit_Bool` | `"true"` or `"false"` |
| `char` | `Emit_Char` | Single character (no quotes) |
| `string` | `Emit_String` | Double-quoted, null produces no output |

Design rationale: Emit methods use `StringBuilder` rather than returning `string` — this allows callers to compose multiple field outputs into a single `StringBuilder` instance, avoiding intermediate allocations from string concatenation. The `G9` (float) and `G17` (double) format specifiers guarantee round-trip fidelity: serializing then deserializing preserves the original value per IEEE 754.
