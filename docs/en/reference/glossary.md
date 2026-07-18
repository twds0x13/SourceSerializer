# Glossary

## Template System

| Term | Description |
|------|-------------|
| Template | String declared via `[Template("...")]`, describing the data format |
| Compact Syntax | `<float X>` format, converted to XML via CompactToXml |
| XML Syntax | `<literal-template>` format, equivalent to compact syntax |
| Literal Text | Fixed characters in the template, matched character-by-character |
| Field | The type and name pair in `<float X>` |
| Optional Block | Template fragment wrapped in `<optional>...</optional>`, backtracked on failure |
| Repetition Block | Collection fragment wrapped in `<first>...</first><body>...</body>` |

## Runtime

| Term | Description |
|------|-------------|
| Scanner | Process of parsing `TData` from `ReadOnlySpan<char>` |
| Emitter | Process of serializing `TData` to `StringBuilder` |
| Serializer Block | `ISerializerBlock<T>` instance, holds both scan and emit capability |

## Compile Time

| Term | Description |
|------|-------------|
| Source Generator (SG) | Roslyn `IIncrementalGenerator`, generates C# source at compile time |
| Interface Dispatch | Scanner tries all interface implementations, picks the one advancing farthest |
| Default Interface Template | Built-in interface templates (IList, ISet, IDictionary, etc.) |
| Roslyn Fallback | When a type is not in openGenerics, resolve via AllInterfaces |

## Generics

| Term | Description |
|------|-------------|
| Open Generic | `Wrapper<T>`, type parameters not yet closed |
| Closed Generic | `Wrapper<float>`, type parameters closed |
| Generic Transitive Closure | Recursive synthesis process in `List<Wrapper<float>>` |
