# Compile-time Diagnostics

All SourceSerializer errors and warnings are reported at compile time via Roslyn diagnostics. Problems never wait until runtime to surface.

## Diagnostic Codes

| Code | Severity | Title | Trigger |
|------|----------|-------|---------|
| SSR001 | Error | Template Parse Error | The template string cannot be parsed as valid compact or XML format |
| SSR002 | Error | Circular template dependency | Two or more templates reference each other, forming a cycle |
| SSR003 | Error | Readonly struct cannot use `[Template]` | A `readonly struct` has `[Template]` applied, but its fields cannot be assigned |
| SSR004 | Warning | Missing template dependency | Template references a field type that has no `[Template]` and is not a built-in type |
| SSR005 | Error | Scalar field inside `<repetition>` | A non-collection field appears inside a `<repetition>` block |

## SSR001: Template Parse Error

Triggered when the template string does not conform to compact or XML syntax rules.

Example trigger:

```csharp
[Template("<float X")]  // Missing closing '>'
public struct Bad { public float X; }
```

Fix: ensure the template string follows the [Template Syntax](./template-syntax) specification.

## SSR002: Circular Template Dependency

Type A's template references type B, and B's template references A, forming a cycle. The source generator detects cycles via topological sort.

Example trigger:

```csharp
[Template("<B Other>")]
public struct A { public B Other; }

[Template("<A Other>")]
public struct B { public A Other; }
```

Fix: break the cycle by converting one reference to a built-in type or removing the outer reference.

## SSR003: Readonly Struct

A `readonly struct` has `[Template]` applied. Deserialization requires writing to fields, which readonly structs forbid.

Example trigger:

```csharp
[Template("<float X>")]
public readonly struct Point { public float X; }  // SSR003
```

Fix: remove the `readonly` modifier.

## SSR004: Missing Template Dependency

A field type is neither one of the 12 built-in types nor annotated with `[Template]`. The source generator skips the field and reports a warning. Compilation continues.

Example trigger:

```csharp
public struct Unregistered { public float X; }

[Template("<Unregistered Data>")]  // SSR004
public struct Container { public Unregistered Data; }
```

Fix: add `[Template]` to the referenced type, or use a built-in type instead.

## SSR005: Scalar Field Inside Repetition

A scalar field inside a `<repetition>` block gets overwritten on each iteration, losing intermediate values. Use a collection type instead.

Example trigger:

```csharp
[Template("<repetition>, <float Items></repetition>")]  // SSR005
public struct Bad { public float Items; }
```

Fix: change the field to a collection type:

```csharp
[Template("<repetition>, <float Items></repetition>")]
public struct Good { public List<float> Items; }
```

## See Also

- [Template Syntax](./template-syntax): Compact and XML formats
- [Managed vs Unmanaged](./managed-vs-unmanaged): Type strategy selection
