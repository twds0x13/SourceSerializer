# Compile-time Diagnostics

All SourceSerializer errors and warnings are reported at compile time via Roslyn diagnostics. Problems never wait until runtime to surface.

## Diagnostic Codes

| Code | Severity | Title | Trigger |
|------|----------|-------|---------|
| SSR001 | Error | Template Parse Error | The template string cannot be parsed as valid compact or XML format |
| SSR002 | Error | Readonly field cannot be assigned | A field referenced in the template is `readonly` and no matching constructor exists |
| SSR003 | Error | Missing template dependency | Template references a type without `[Template]` and the field is not marked `[TemplateIgnore]` |
| SSR004 | Error | Scalar field inside `<repetition>` | A non-collection field appears inside a `<repetition>` block |
| SSR005 | Error | Template ambiguity | Two concrete types sharing an interface have templates that are prefixes of each other |
| SSR006 | Error | Cannot override built-in type | `[ExternalTemplate]` targets one of the 16 built-in types |

## SSR001: Template Parse Error

Triggered when the template string does not conform to compact or XML syntax rules.

Example trigger:

```csharp
[Template("<float X")]  // Missing closing '>'
public struct Bad { public float X; }
```

Fix: ensure the template string follows the [Template Syntax](./template-syntax) specification.

## SSR002: Readonly Field

A field is declared `readonly` and cannot be assigned by deserialization code. All fields of a `readonly struct` are implicitly readonly (C# CS8340), requiring a matching constructor.

Example trigger:

```csharp
[Template("<float Attack> <float CritRate>")]
public readonly struct Damage
{
    public readonly float Attack;   // SSR002 (no matching constructor)
    public readonly float CritRate; // SSR002
}
```

Fix: add a constructor whose parameters match all fields by name and type. SourceSerializer discovers it automatically via greedy constructor matching:

```csharp
[Template("<float Attack> <float CritRate>")]
public readonly struct Damage
{
    public readonly float Attack;
    public readonly float CritRate;
    public Damage(float attack, float critRate) { Attack = attack; CritRate = critRate; }
}
```

The generated code uses `new Damage(__f_Attack, __f_CritRate)` instead of field-by-field assignment.

## SSR003: Missing Template Dependency

A field type is neither one of the 16 built-in types nor annotated with `[Template]`, and the field is not marked `[TemplateIgnore]`. Compilation will stop.

Example trigger:

```csharp
public struct Unregistered { public float X; }

[Template("<Unregistered Data>")]  // SSR003
public struct Container { public Unregistered Data; }
```

Fix options:
- Add `[Template]` or `[ExternalTemplate]` to the referenced type
- Use a built-in type instead
- If the field should not participate in serialization, mark it with `[TemplateIgnore]` and remove the reference from the template string

## SSR004: Scalar Field Inside Repetition

A scalar field inside a `<repetition>` block gets overwritten on each iteration, losing intermediate values. Use a collection type instead.

Example trigger:

```csharp
[Template("<repetition><first><float Items></first><body>, <float Items></body></repetition>")]  // SSR004
public struct Bad { public float Items; }
```

Fix: change the field to a collection type:

```csharp
[Template("<repetition><first><float Items></first><body>, <float Items></body></repetition>")]
public struct Good { public List<float> Items; }
```

## Using `[TemplateIgnore]` to Skip Fields

When a struct contains fields that should not participate in serialization (cache values, runtime constants, internal state), and the field type has no `[Template]`, mark it with `[TemplateIgnore]`. Ignored fields are excluded from scanner and emitter code.

```csharp
public struct CacheData { public float[] Cache; }

[Template("<float Value>")]
public struct Stats
{
    public float Value;
    [TemplateIgnore] public CacheData InternalCache;
}
```

Note: marked fields should not appear in the template string. If the template string still references the field's type, the source generator will still report SSR003.

## SSR005: Template Ambiguity

Two concrete types implementing the same interface have templates that are prefixes of each other, making interface dispatch unable to reliably distinguish them. Compilation will stop.

Trigger example:

```csharp
interface IVector { }

[Template("Vec(<float X>, <float Y>)")]
struct Vec2 : IVector { float X; float Y; }

[Template("Vec(<float X>, <float Y>, <float Z>)")]
struct Vec3 : IVector { float X; float Y; float Z; }
// Vec2's template "Vec(<float X>, <float Y>)" is a prefix of Vec3's template
// The scanner cannot determine when to stop → SSR005
```

Fix: adjust templates so each concrete type's prefix is distinguishable, e.g., `Vec2(...)` and `Vec3(...)` with different prefixes.

## SSR006: Overriding Built-in Types

Triggered when `[ExternalTemplate]` attempts to override one of the 16 built-in types.

Trigger example:

```csharp
[assembly: ExternalTemplate(typeof(float), "Float(<float>)")]
// → SSR006: Cannot override built-in type 'float'
```

Root cause: built-in types are handled by hand-written zero-allocation span scanners in `SerializerRegistry`. `[ExternalTemplate]` cannot generate code that matches the performance characteristics of these hand-written scanners — their `readonly ref struct` layout and SIMD-friendly branch structure cannot be expressed via template compilation. Additionally, allowing built-in type overrides would cause inconsistent type resolution across assemblies: the same type could be parsed in different formats depending on which assembly's template is active.

Fix: remove `[ExternalTemplate]` for built-in types. To apply custom formatting to a built-in type, wrap it in a higher-level template:

```csharp
// Correct: wrap at a higher level
[Template("MyFloat(<float Value>)")]
struct MyFloat { float Value; }
```

## See Also

- [Template Syntax](./template-syntax): Compact and XML formats
- [Managed vs Unmanaged](./managed-vs-unmanaged): Type strategy selection
