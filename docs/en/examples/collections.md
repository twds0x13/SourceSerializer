# Collection Examples

Collection types work directly in templates without explicit template declarations.

## List

```csharp
[Template("<float Base><optional>, <List<float> Multipliers></optional>")]
struct Skill { float Base; List<float> Multipliers; }

// Input: "100" → Multipliers=[]
// Input: "100, 1.5, 2.0" → Multipliers=[1.5, 2.0]

// Serialization
SerializerBlocks.TryGet<Skill>(out var block);
block.Emit(sb, new Skill { Base = 100, Multipliers = new() { 1.5f, 2.0f } });
// → "100, 1.5, 2.0"
```

## HashSet

```csharp
[Template("<HashSet<float> Items>")]
struct UsesHashSet { HashSet<float> Items; }

// Automatically resolved via ISet<T> default interface template
```

## Dictionary

```csharp
// Automatically resolved via IDictionary<K,V> default interface template
// Format: "key: value, key2: value2"
```

## Nested Generic Collections

```csharp
[Template("<List<HashSet<float>> Nested>")]
struct HasNestedHashSet { List<HashSet<float>> Nested; }
```

See `CollectionRepetitionTests.cs` and `GenericTemplateTests.cs` for complete runnable test cases.
