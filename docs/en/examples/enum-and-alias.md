# Example: Enum Tags & Type Aliases

Declare text labels for enum members with `[Tag]`, and give friendly names to built-in types with `[TypeAlias]`.

## Enum Tags

```csharp
enum Element : byte
{
    [Tag("fire")]  Fire,
    [Tag("ice")]   Ice,
    [Tag("magic")] Magic,
}

[Template("Spell(<Element Type>, <float Power>)")]
public struct Spell
{
    public Element Type;
    public float Power;
}
```

```csharp
SerializerBlocks.TryGet<Spell>(out var block);

block.Scan("Spell(fire, 50)".AsSpan(), 0, out Spell v);
// v.Type == Element.Fire, v.Power == 50f

block.Scan("Spell(ice, 30)".AsSpan(), 0, out Spell v);
// v.Type == Element.Ice, v.Power == 30f
```

Use the enum type name directly in the template: the SG auto-detects `[Tag]` annotations and generates a switch-on-string scanner. Untagged enum members fall back to `value.ToString()` during Emit.

## Type Aliases

```csharp
[assembly: TypeAlias("HP", "float")]
[assembly: TypeAlias("MP", "float")]

[Template("Stats(<HP Health>, <MP Mana>)")]
public struct Stats
{
    public float Health;
    public float Mana;
}
```

```csharp
SerializerBlocks.TryGet<Stats>(out var block);

block.Scan("Stats(100, 50)".AsSpan(), 0, out Stats v);
// v.Health == 100f, v.Mana == 50f
```

Aliases only change the type name used in templates; parsing behavior is identical to the original type. Aliases can map to any registered type (built-in or custom).

## Combined Use

Enum tags and type aliases can be combined in the same struct:

```csharp
[assembly: TypeAlias("Elem", "Element")]

[Template("AdvancedSpell(<Elem Type>, <float Cost>)")]
public struct AdvancedSpell
{
    public Element Type;
    public float Cost;
}
```

```csharp
SerializerBlocks.TryGet<AdvancedSpell>(out var block);

block.Scan("AdvancedSpell(magic, 120)".AsSpan(), 0, out AdvancedSpell v);
// v.Type == Element.Magic, v.Cost == 120f
```

See `EnumTagTests.cs` and `TypeAliasTests.cs` for complete runnable test cases.

## See Also

- [Tag API](../api/tag-attribute): full API signature for enum tags
- [TypeAlias API](../api/type-alias-attribute): full API signature for type aliases
- [Template Syntax](../guide/template-syntax): the four primitives, built-in types, collection formats
