# `[TemplateIgnore]`

Marks a field to be skipped during serialization and deserialization.

## Signature

```csharp
[AttributeUsage(AttributeTargets.Field)]
public class TemplateIgnoreAttribute : Attribute { }
```

## Usage

```csharp
[Template("Container(<float Value>)")]
struct Container
{
    public float Value;
    [TemplateIgnore] public UnregisteredType InternalData;
}
```

`InternalData` does not appear in the template. The SG-generated scanner and emitter skip this field.

## See Also

- [Diagnostics: SSR004](../guide/diagnostics#ssr004---missing-template-dependency)
