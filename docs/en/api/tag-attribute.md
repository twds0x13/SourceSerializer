# `[Tag]`

Declares a string tag for an enum member. The source generator automatically produces a switch-on-string scanner.

## Signature

```csharp
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class TagAttribute : Attribute
{
    public string Tag { get; }
    public TagAttribute(string tag);
}
```

## Constructor Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `tag` | `string` | Tag string |

## Usage

Annotate enum members with `[Tag]`. Use the enum type name as the field type in templates:

```csharp
enum Element : byte
{
    Physical = 0,
    [Tag("fire")]  Fire,
    [Tag("ice")]   Ice,
    [Tag("magic")] Magic,
}

[Template("<Element Type>")]
public struct Spell
{
    public Element Type;
}
```

After compilation, the source generator emits a `Scan_Enum_Element` method that performs a switch match on the tag string. Matching tags return their corresponding enum value; non-matching input returns parse failure.

## Note

Enum members without `[Tag]` do not get a matching case in the generated scanner. In the example above, `Physical = 0` has no tag and cannot be parsed from a string.
