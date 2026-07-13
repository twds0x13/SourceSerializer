# `[TypeAlias]`

Declares a type alias for use in templates, mapping to a built-in C# value type.

## Signature

```csharp
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class TypeAliasAttribute : Attribute
{
    public string Alias { get; }
    public string CSharpType { get; }
    public TypeAliasAttribute(string alias, string csharpType);
}
```

## Constructor Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `alias` | `string` | Alias used in templates |
| `csharpType` | `string` | Target C# value type name |

## Usage

Assembly-level attribute. Once registered, the alias can be used in templates in place of the built-in type name:

```csharp
[assembly: TypeAlias("Distance", "float")]
[assembly: TypeAlias("Count", "int")]
```

Template usage:

```csharp
[Template("<Distance X> <Distance Y>")]
public struct Point2D { public float X; public float Y; }
```
