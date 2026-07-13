# Template Syntax

SourceSerializer supports two equivalent template formats: compact and XML.

## Compact Format

```csharp
[Template("<float Damage>|<optional>draw <int Cards></optional>")]
```

## XML Format

```xml
<literal-template>
  <field type="float" name="Damage"/>
  <text>|</text>
  <optional>
    <text>draw </text>
    <field type="int" name="Cards"/>
  </optional>
</literal-template>
```

## Four Primitives

| Primitive | Compact | XML | Semantics |
|-----------|---------|-----|-----------|
| Literal text | Write directly | `<text>...</text>` | Exact char-by-char match |
| Field | `<type name>` | `<field type="" name=""/>` | Invokes corresponding type scanner |
| Optional block | `<optional>...</optional>` | `<optional>...</optional>` | Attempt match, rewind on failure |
| Repetition block | `<repetition>...</repetition>` | `<repetition>...</repetition>` | Loop match, exit on failure |

## Built-in Types

12 C# built-in types require no extra configuration: float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char.

## Custom Type Aliases

```csharp
[assembly: TypeAlias("Distance", "float")]
```

## Enum Tags

```csharp
enum Element { [Tag("fire")] Fire, [Tag("ice")] Ice }
```

This document is a work in progress.
