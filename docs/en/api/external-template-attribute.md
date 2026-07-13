# `[ExternalTemplate]`

Declares a serialization template for third-party types whose source cannot be directly modified.

## Signature

```csharp
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class ExternalTemplateAttribute : Attribute
{
    public Type TargetType { get; }
    public string Template { get; }
    public ExternalTemplateAttribute(Type targetType, string template);
}
```

## Constructor Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `targetType` | `Type` | Target type |
| `template` | `string` | Template string |

## Usage

Can be placed on assembly, class, or struct. External templates override struct-level `[Template]` (Priority B semantics).

```csharp
// Register a template for a third-party struct
[assembly: ExternalTemplate(typeof(UnityEngine.Vector3),
    "<float x> <float y> <float z>")]

// Or placed directly above the struct
[ExternalTemplate(typeof(ExternalPoint), "<float A> <float B>")]
public struct ExternalPoint { public float A; public float B; }
```
