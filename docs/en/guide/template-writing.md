# SourceSerializer Template Writing Guide

This guide demonstrates complete template syntax through concrete data structures. Each example includes the C# type definition, template string, and sample inputs.

The recommended unified style: **`TypeName(<type field>, <type field>, ...)`** — function-call wrapping with comma+space separators. This mirrors the `List(...)`, `Dict(...)`, and `HashSet(...)` collection format for visual consistency.

---

## 1. Primitive Value Types

```csharp
[Template("Point(<float X>, <float Y>)")]
struct Point { float X; float Y; }

// Input: "Point(3.5, -2)"
// Input: "Point(100, 0.5)"
```

Multiple fields use comma separators. String values are always quoted:

```csharp
[Template("Player(<string Name>, <int Level>)")]
struct Player { string Name; int Level; }
// Input: "Player(\"warrior\", 5)"

[Template("Range(<float Min>, <float Max>)")]
struct Range { float Min; float Max; }
// Input: "Range(10, 100)"
```

**All built-in types**: `float` `double` `int` `uint` `long` `ulong` `short` `ushort` `byte` `sbyte` `bool` `char` `string`

---

## 2. Optional Blocks

```csharp
[Template("Damage(<float Base><optional>, <float Bonus></optional>)")]
struct Damage
{
    float Base;
    float Bonus;  // optional field, default = 0
}
// Input: "Damage(100)"          → Base=100, Bonus=0
// Input: "Damage(100, 25)"      → Base=100, Bonus=25
```

Optional blocks can wrap multiple fields:

```csharp
[Template("Player(<string Name><optional>, <int Level>, <float Exp></optional>)")]
struct Player
{
    string Name;
    int Level;
    float Exp;
}
// Input: "Player(\"warrior\")"                   → Name=warrior, Level=0, Exp=0
// Input: "Player(\"warrior\", 5, 1200.5)"        → Name=warrior, Level=5, Exp=1200.5
```

---

## 3. Enum Tags

```csharp
enum Element : byte { Physical = 0, [Tag("fire")] Fire, [Tag("ice")] Ice }

[Template("Spell(<Element Elem>, <float Dmg>)")]
struct Spell { Element Elem; float Dmg; }
// Input: "Spell(fire, 50)"
// Input: "Spell(ice, 30)"
```

`[Tag("...")]` declares the text representation of each enum member. Use the enum type name directly in the template — the SG auto-detects tags.

---

## 4. Nested Types

```csharp
[Template("Vec2(<float X>, <float Y>)")]
struct Vec2 { float X; float Y; }

[Template("Entity(<string Name>, <Vec2 Pos>)")]
struct Entity { string Name; Vec2 Pos; }
// Input: "Entity(\"treasure\", Vec2(10, 20))"
```

Recursive dependencies resolve automatically: the SG processes `Vec2` first, then `Entity` which references it. Nested `Prefix(...)` wrappers naturally form a tree structure.

---

## 5. Collections (List / Array / Dictionary / HashSet)

```csharp
[Template("Skill(<float Base><optional>, <List<float> Multipliers></optional>)")]
struct Skill
{
    float Base;
    List<float> Multipliers;
}
// Input: "Skill(100)"                           → Multipliers=List()
// Input: "Skill(100, List(1.5))"                → Multipliers=List(1.5)
// Input: "Skill(100, List(1.5, 2.0, 0.75))"    → Multipliers=List(1.5, 2.0, 0.75)
```

**Collection fields work directly in templates — no explicit template declaration needed.** The system provides default templates matched through implemented interfaces:

| Default Interface | Covered Types | Serialized Format |
|-------------------|--------------|-------------------|
| `IList<T>` | `List<T>`, `T[]`, `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, etc. | `List(...)` |
| `ISet<T>` | `HashSet<T>`, `SortedSet<T>`, `ISet<T>`, `IReadOnlySet<T>`, etc. | `HashSet(...)` |
| `IReadOnlyList<T>` | `IReadOnlyList<T>`, etc. | `List(...)` |
| `IDictionary<K,V>` | `Dictionary<K,V>`, `SortedDictionary<K,V>`, `IDictionary<K,V>`, etc. | `Dict(k: v, ...)` |
| `IReadOnlyDictionary<K,V>` | `IReadOnlyDictionary<K,V>`, etc. | `Dict(k: v, ...)` |

**Interface-first principle:** prefer defining an interface with a template for custom collections rather than annotating each class. Class-level `[ExternalTemplate]` takes precedence over interface templates.

```csharp
[Template("Payload(<int Id>, <float[] Data>)")]
struct Payload { int Id; float[] Data; }
// Input: "Payload(42, List(3.5, 7, 1))"

[Template("Stats(<Dictionary<string,int> Entries>)")]
struct Stats { Dictionary<string,int> Entries; }
// Input: "Stats(Dict(hp:100, atk:50))"
```

---

## 6. Custom Generic Types

Single-parameter generics:

```csharp
[Template("Wrapper(<T Value>)")]
struct Wrapper<T> where T : unmanaged { T Value; }

[Template("UsesWrapper(<Wrapper<float> W>)")]
struct UsesWrapper { Wrapper<float> W; }
// Input: "UsesWrapper(Wrapper(3.5))"
```

Multi-parameter generics:

```csharp
[Template("Pair(<T1 First>, <T2 Second>)")]
struct Pair<T1, T2> where T1 : unmanaged where T2 : unmanaged
{
    T1 First;
    T2 Second;
}

[Template("UsesPair(<Pair<float,int> P>)")]
struct UsesPair { Pair<float, int> P; }
// Input: "UsesPair(Pair(3.5, 42))"
```

Type parameter names are arbitrary (`T`, `TKey`, `TValue`, `TData` all work). The SG maps type parameters by **position**, with no limit on count. `Wrapper<float>` triggers automatic synthesis when referenced by `UsesWrapper`.

---

## 7. Interface Dispatch

```csharp
interface IVector { }

[Template("Vec2(<float X>, <float Y>)")]
struct Vec2 : IVector { float X; float Y; }

[Template("Vec3(<float X>, <float Y>, <float Z>)")]
struct Vec3 : IVector { float X; float Y; float Z; }

// Use the interface name directly in the template
[Template("VectorWrapper(<IVector V>)")]
struct VectorWrapper { IVector V; }
// Input: "VectorWrapper(Vec2(1.5, -2))"
// Input: "VectorWrapper(Vec3(3, 5, 7))"
```

Any number of interface implementations is supported (struct + class mixed). Interface blocks registered across assemblies via hot-reload DLLs are automatically chain-merged.

---

## 8. `[TemplateIgnore]`

```csharp
[Template("Container(<float Value>)")]
struct Container
{
    float Value;
    [TemplateIgnore] object RuntimeCache;  // excluded from serialization
}
```

Fields marked `[TemplateIgnore]` do not appear in generated code and do not trigger SSR004. Use for runtime state, caches, and other non-serializable fields.

---

## 9. Type Aliases

```csharp
[assembly: TypeAlias("HP", "float")]
[assembly: TypeAlias("MP", "float")]

[Template("Stats(<HP Health>, <MP Mana>)")]
struct Stats { float Health; float Mana; }
// Input: "Stats(100, 50)"
```

Aliases only change the type name in the template. Parsing behavior is identical to the original type.

---

## 10. External Type Templates

Register templates for types whose source code you don't own:

**Concrete types**:

```csharp
[assembly: ExternalTemplate(typeof(Vector3), "Vector3(<float x>, <float y>, <float z>)")]
```

**Overriding default collection templates**:

```csharp
// Semicolons instead of commas
[assembly: ExternalTemplate(typeof(List<>), "List(<first><T item></first><body>; <T item></body>)")]

// Equals instead of colons
[assembly: ExternalTemplate(typeof(Dictionary<,>), "Dict(<first><TKey key>=<TValue value></first><body>, <TKey key>=<TValue value></body>)")]
```

`typeof(List<>)` and `typeof(Dictionary<,>)` reference open generic types. Type parameter placeholders must match real names (`T`, `TKey`, `TValue`). The SG substitutes by name when synthesizing concrete instances.

---

## 11. Class Types

```csharp
[Template("NamedValue(<string Name>, <float Value>)")]
class NamedValue
{
    public string Name;
    public float Value;
}

// Classes can be used as field types in other templates
[Template("Modifiable(<float Base><optional>, <List<NamedValue> Mods></optional>)")]
class Modifiable
{
    public float Base;
    public List<NamedValue> Mods;
}
// Input: "Modifiable(100, List(NamedValue(\"sword\", 1.5), NamedValue(\"shield\", 2.5)))"
```

Class fields are initialized via `new T()` (structs use `default`). The SG determines strategy at compile time via Roslyn's `IsUnmanagedType`.

---

## 12. Composite Data Structures

### Item Table

```csharp
enum ItemType { [Tag("weapon")] Weapon, [Tag("potion")] Potion, [Tag("scroll")] Scroll }

[Template("Item(<int Id>, <string Name>, <ItemType Type>, <int Price><optional>, <string Description></optional>)")]
struct Item
{
    int Id;
    string Name;
    ItemType Type;
    int Price;
    string Description;
}
// Input: "Item(42, \"steel_sword\", weapon, 150)"
// Input: "Item(99, \"health_vial\", potion, 25, \"restores 50 HP\")"
```

### Deep Nesting

```csharp
[Template("Vec3(<float X>, <float Y>, <float Z>)")]
struct Vec3 { float X; float Y; float Z; }

[Template("Sphere(<Vec3 Center>, <float Radius>)")]
struct Sphere { Vec3 Center; float Radius; }

[Template("Zone(<string Name>, <Sphere Bounds>)")]
struct Zone { string Name; Sphere Bounds; }
// Input: "Zone(\"safe_zone\", Sphere(Vec3(10, 0, 5), 100))"
```

### Config with Optional Generic Collections

```csharp
[Template("BuffConfig(<string Name>, <float Duration><optional>, <List<string> Tags></optional>)")]
struct BuffConfig
{
    string Name;
    float Duration;
    List<string> Tags;
}
// Input: "BuffConfig(\"berserk\", 30)"
// Input: "BuffConfig(\"shield\", 10, List(\"fire\", \"reflect\", \"timed\"))"
```

### Complex Nesting with Interface Generics

```csharp
interface IEffect { }

[Template("DamageEffect(<float Amount>)")]
struct DamageEffect : IEffect { float Amount; }

[Template("RegenEffect(<float Duration>, <float Rate>)")]
struct RegenEffect : IEffect { float Duration; float Rate; }

[Template("ElementDescriptor(<int Id>, <IEffect Form>)")]
struct ElementDescriptor
{
    int Id;
    IEffect Form;
}

[Template("Ability(<string Name>, <float Base><optional>, <List<ElementDescriptor> Combo></optional>)")]
struct Ability
{
    string Name;
    float Base;
    List<ElementDescriptor> Combo;
}
// Input: "Ability(\"fireball\", 100, List(ElementDescriptor(1, DamageEffect(50)), ElementDescriptor(2, RegenEffect(5, 0.1))))"
```

---

## 13. Compact vs XML Syntax

Both syntaxes are equivalent — use either:

```csharp
// Compact syntax (recommended)
[Template("Point(<float X>, <float Y><optional>, <float Z></optional>)")]

// XML syntax
[Template(@"
  <literal-template>
    <text>Point(</text>
    <field type=""float"" name=""X""/>
    <text>, </text>
    <field type=""float"" name=""Y""/>
    <optional>
      <text>, </text>
      <field type=""float"" name=""Z""/>
    </optional>
    <text>)</text>
  </literal-template>")]
```

Use XML syntax when `<` or `>` characters need escaping in compact form.

---

## 14. Deprecated Patterns

| Old | Replacement |
|-----|-------------|
| Bare fields without type prefix | Wrap in `TypeName(...)` |
| `\|` or arbitrary separators | Unified comma `, ` separator |
| `<repetition>A</repetition>` | `<first>A</first><body>A</body>` |
| `<repetition><first>A</first><body>B</body></repetition>` | `<first>A</first><body>B</body>` |

Use `<first>/<body>` directly without wrapping in `<repetition>`. Collection type templates are built into the SG — no manual repetition logic is needed.

---

## 15. Diagnostic Quick Reference

| Code | Meaning |
|------|---------|
| SSR001 | Template syntax error |
| SSR002 | Circular template dependency |
| SSR003 | Field referenced in template is readonly with no matching constructor — add constructor or remove readonly |
| SSR004 | Field type missing `[Template]` — add `[Template]`, `[ExternalTemplate]`, or `[TemplateIgnore]` |
| SSR005 | Scalar field inside repetition block — use a collection type instead |
| SSR006 | Template ambiguity across interface implementations — adjust templates so each implementation is distinguishable |
