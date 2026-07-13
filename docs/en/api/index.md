# API Reference

## Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[Template("...")]` | struct, class | Declares a text template for the type |
| `[ExternalTemplate(typeof(T), "...")]` | assembly, class, struct | Declares a template for a third-party type |
| `[Tag("label")]` | enum field | Declares a string tag for an enum member |
| `[TypeAlias("Alias", "float")]` | assembly | Registers a type alias |

## SerializerScanners

```csharp
partial class SerializerScanners
{
    static bool TryGetScanner<T>(out ScannerDelegate<T> scanner);
}
```

This document is a work in progress.
