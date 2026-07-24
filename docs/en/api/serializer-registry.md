# `SerializerRegistry`

Built-in type registry. Provides zero-allocation span scanner methods for 13 C# built-in unmanaged types.

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
| `string` | `Scan_String` | Quoted or unquoted character sequence; Emit always adds quotes |

## Scanner Method Convention

All scanner methods follow a uniform signature:

```csharp
public static int Scan_Xxx(ReadOnlySpan<char> src, int pos, out Xxx value)
```

Return value convention: `> pos` indicates successful match and returns the end position; `== pos` indicates no match (parse failure), value is `default`.
