# Internals

## Interface Dispatch Algorithm

The scanner tries all registered concrete implementations for each interface and selects the one advancing farthest in the input:

```
Input: "3.5, -2"     → Try Vec2.Scan → reaches end ← selected
Input: "3.5, -2, 7.1" → Try Vec2.Scan → matches only first two fields
                       → Try Vec3D.Scan → reaches end ← selected
```

The emitter uses a C# `switch` pattern match for runtime type dispatch.

## Generic Resolution Roslyn Fallback

When `ParseGenericType` fails to find a type in `openGenerics`, it does not immediately give up. `TryResolveViaInterfaces` uses Roslyn `Compilation.GetTypeByMetadataName` to resolve BCL types, inspects their `AllInterfaces`, and finds matching default interface templates.

For multiple matches, Roslyn inheritance relationships filter to the most derived interface. If still tied, a fixed priority order is used: `IList > ISet > IReadOnlyList > IDictionary > IReadOnlyDictionary`.

## Collection Emit

Self-collection types (`List<T>`, `HashSet<T>`) use a `foreach` + `<first>`/`<body>` pattern:

```csharp
// First element: no separator
// Subsequent: foreach skipping first, with separator
foreach (var item in value) { ... }
```

## EmitHelpers Shared Utilities

`EmitHelpers` unifies method name generation (`GetMethodName`), unique variable naming (`GetUniqueVar`), and counter management across CodeEmitter and EmitCodeEmitter. Eliminates ~20 lines of duplicated code.
