# `[Template]`

Declares a serialization template for a struct or class. The source generator emits a span scanner at compile time.

## Signature

```csharp
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TemplateAttribute : Attribute
{
    public string Template { get; }
    public TemplateAttribute(string template);
}
```

## Constructor Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `template` | `string` | Template string (compact syntax or XML format) |

## Template Syntax

`<type fieldname>` directives map to built-in or registered type scanners.

Compact examples:

```csharp
[Template("<float X> <float Y>")]
public struct Point2D { public float X; public float Y; }

[Template("<float Damage>|<optional>draw <int Cards></optional>")]
public struct SpellCard { public float Damage; public int Cards; }

[Template("<float Damage><repetition>, <float Multipliers></repetition>")]
public struct DamageData { public float Damage; public float Multipliers; }
```

Equivalent XML format:

```xml
<literal-template>
  <field type="float" name="Damage"/>
  <repetition>
    <text>, </text>
    <field type="float" name="Multipliers"/>
  </repetition>
</literal-template>
```

## Runtime Usage

```csharp
SerializerScanners.TryGetScanner<Point2D>(out var scan);
scan("3.5 -2.1".AsSpan(), 0, out Point2D v);
```
