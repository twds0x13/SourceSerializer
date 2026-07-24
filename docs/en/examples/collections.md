# Collection Examples

Collection types work directly in templates — no explicit template declaration needed.

## List

```csharp
[Template("Skill(<float Base><optional>, <List<float> Multipliers></optional>)")]
struct Skill { float Base; List<float> Multipliers; }

// Input: "Skill(100)" → Multipliers=List()
// Input: "Skill(100, List(1.5, 2.0))" → Multipliers=List(1.5, 2.0)

// Serialization
SerializerBlocks.TryGet<Skill>(out var block);
block.Emit(sb, new Skill { Base = 100, Multipliers = new() { 1.5f, 2.0f } });
// → "Skill(100, List(1.5, 2.0))"
```

## HashSet

```csharp
[Template("UsesHashSet(<HashSet<float> Items>)")]
struct UsesHashSet { HashSet<float> Items; }

// Serialized format: "UsesHashSet(HashSet(1.5, 2.0))"
```

## Dictionary

```csharp
// Resolved via IDictionary<K,V> default interface template
// Serialized format: "Dict(k: v, k2: v2)"
```

## Nested Generic Collections

```csharp
[Template("HasNestedHashSet(<List<HashSet<float>> Nested>)")]
struct HasNestedHashSet { List<HashSet<float>> Nested; }
// Serialized: "HasNestedHashSet(List(HashSet(1.5, 2.0), HashSet(3.0)))"
```

See `CollectionRepetitionTests.cs` and `GenericTemplateTests.cs` for complete runnable test cases.
