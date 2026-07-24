# Frequently Asked Questions

## Template Declaration

**Q: How do I declare a template for a readonly struct?**

A: Readonly structs need a matching constructor. The SG automatically discovers constructors whose parameters match field names and types.

**Q: Can I use custom types in templates?**

A: Yes. Add `[Template]` to the custom type, then reference it in nested templates. The SG resolves dependency order automatically.

**Q: How do I apply templates to BCL types?**

A: Use `[ExternalTemplate(typeof(List<>), "...")]` to override defaults. Class-level overrides take priority over default interface templates.

## Runtime

**Q: What if TryGet returns false?**

A: Check that the type has a `[Template]` attribute. Check for `[TemplateIgnore]` on all fields resulting in an empty template.

**Q: Scan returns 0?**

A: Input doesn't match the template format. Check separators, field types, optional block positions. Compare template against input.

**Q: Do I need to cache ISerializerBlock?**

A: Not required. `TryGet<T>` is a static field read with negligible overhead (~2ns). Caching in a `static readonly` field is friendlier.

**Q: How to serialize collections like List?**

A: `List<T>` and similar have default interface templates. Use `ISerializerBlock<List<T>>` directly. Format: first element no separator, subsequent comma-space separated.

## Generics

**Q: How to write templates for custom generic types?**

A: Use type parameter names as placeholders: `[Template("<T Value>")]`. Concrete instances like `Wrapper<float>` are synthesized automatically.

**Q: Why isn't my custom collection type working?**

A: Define an interface for the collection and annotate it with a template rather than each concrete class. All implementing types automatically inherit the template.

**Q: How do I use enum names instead of integers?**

A: Add `[Tag("fire")]` on enum members, then use the enum type name directly in the template (e.g., `<Element Elem>`). The SG auto-generates bidirectional tag-to-value mapping. Enum values without `[Tag]` fall back to `value.ToString()` on emit.

**Q: How do I alias field type names?**

A: Use `[assembly: TypeAlias("HP", "float")]` at assembly level. Write `<HP Health>` in templates. Parsing behavior is identical to the original type. Aliases can map to any registered type.
