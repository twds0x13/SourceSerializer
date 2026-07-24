# SourceSerializer Template Writing Guide

This guide demonstrates complete template syntax through concrete data structures. Each example includes the C# type definition, template string, and sample inputs.

---

## 1. Primitive Value Types

```csharp
// float / int / double / bool / string / long / byte / char / short ...
[Template("<float X>, <float Y>")]
struct Point { float X; float Y; }

// Input: "3.5, -2"
// Input: "100, 0.5"
```

Separators and literal text are freely chosen: commas, pipes, spaces, and parentheses all work:

```csharp
[Template("<string Name>|<int Level>")]
struct Player { string Name; int Level; }
// Input: "warrior|5"

[Template("<float Min> to <float Max>")]
struct Range { float Min; float Max; }
// Input: "10 to 100"
```

**All built-in types**: `float` `double` `int` `uint` `long` `ulong` `short` `ushort` `byte` `sbyte` `bool` `char` `string`

---

## 2. Optional Blocks

```csharp
[Template("<float Base><optional>, <float Bonus></optional>")]
struct Damage
{
    float Base;
    float Bonus;  // optional field, default = 0
}
// Input: "100"          → Base=100, Bonus=0
// Input: "100, 25"      → Base=100, Bonus=25
```

Optional blocks can wrap multiple fields:

```csharp
[Template("<string Name><optional>|<int Level><float Exp></optional>")]
struct Player
{
    string Name;
    int Level;
    float Exp;
}
// Input: "warrior"              → Name=warrior, Level=0, Exp=0
// Input: "warrior|5|1200.5"     → Name=warrior, Level=5, Exp=1200.5
```

---

## 3. Enum Tags

```csharp
enum Element : byte { Physical = 0, [Tag("fire")] Fire, [Tag("ice")] Ice }

[Template("<Element Elem><float Dmg>")]
struct Spell { Element Elem; float Dmg; }
// Input: "fire|50"
// Input: "ice|30"
```

`[Tag("...")]` declares the text representation of each enum member. Use the enum type name directly in the template — the SG auto-detects tags.

---

## 4. Nested Types

```csharp
[Template("<float X>, <float Y>")]
struct Vec2 { float X; float Y; }

[Template("<string Name> at <Vec2 Pos>")]
struct Entity { string Name; Vec2 Pos; }
// Input: "treasure at 10, 20"
```

Recursive dependencies resolve automatically: the SG processes `Vec2` first, then `Entity` which references it.

---

## 5. Collections (List / Array / Dictionary)

```csharp
[Template("<float Base><optional>, <List<float> Multipliers></optional>")]
struct Skill
{
    float Base;
    List<float> Multipliers;
}
// Input: "100"                    → Multipliers=[]
// Input: "100, 1.5"              → Multipliers=[1.5]
// Input: "100, 1.5, 2.0, 0.75"  → Multipliers=[1.5, 2.0, 0.75]
```

**Collection fields work directly in templates — no explicit template declaration needed.** The system provides default templates matched through implemented interfaces:

| Default Interface | Covered Types | Separator |
|-------------------|--------------|-----------|
| `IList<T>` | `List<T>`, `T[]`, `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, etc. | Comma |
| `ISet<T>` | `HashSet<T>`, `SortedSet<T>`, `ISet<T>`, `IReadOnlySet<T>`, etc. | Comma |
| `IReadOnlyList<T>` | `IReadOnlyList<T>`, etc. | Comma |
| `IDictionary<K,V>` | `Dictionary<K,V>`, `SortedDictionary<K,V>`, `IDictionary<K,V>`, etc. | Colon |
| `IReadOnlyDictionary<K,V>` | `IReadOnlyDictionary<K,V>`, etc. | Colon |

**Serialized format**: `List(1.5, 2.0, 3.5)` for lists, `HashSet(100, 200)` for sets, `Dict(hp:100, atk:50)` for dictionaries. Strings are always emitted with quotes.

**Interface-first principle:** for custom collection types, prefer defining an interface with a template rather than annotating each class. All types implementing that interface automatically inherit the template. Class-level `[ExternalTemplate]` takes precedence over interface templates.

Users can override any default template via `[ExternalTemplate]` (see Section 10).

```csharp
[Template("<int Id><float[] Data>")]
struct Payload { int Id; float[] Data; }
// Input: "42|3.5, 7, 1"    → Data=[3.5, 7, 1]

[Template("<Dictionary<string,int> Stats>")]
struct Stats { Dictionary<string,int> Stats; }
// Input: "hp:100, atk:50"   → Stats={"hp":100, "atk":50}
```

---

## 6. Custom Generic Types

Single-parameter generics:

```csharp
[Template("<T Value>")]
struct Wrapper<T> where T : unmanaged { T Value; }

[Template("<Wrapper<float> W>")]
struct UsesWrapper { Wrapper<float> W; }
// Input: "3.5"
```

Multi-parameter generics:

```csharp
[Template("<T1 First>, <T2 Second>")]
struct Pair<T1, T2> where T1 : unmanaged where T2 : unmanaged
{
    T1 First;
    T2 Second;
}

[Template("<Pair<float,int> P>")]
struct UsesPair { Pair<float, int> P; }
// Input: "3.5, 42"
```

Type parameter names are arbitrary (`T`, `TKey`, `TValue`, `TData` all work). The SG maps type parameters by **position**, with no limit on parameter count. `Wrapper<float>` triggers automatic synthesis when referenced by `UsesWrapper`.

---

## 7. Interface Dispatch

```csharp
interface IVector { }

[Template("<float X>, <float Y>")]
struct Vec2 : IVector { float X; float Y; }

[Template("<float X>, <float Y>, <float Z>")]
struct Vec3 : IVector { float X; float Y; float Z; }

// Use the interface name directly in the template
[Template("<IVector V>")]
struct VectorWrapper { IVector V; }
// Input: "1.5, -2"         → Vec2(1.5, -2)
// Input: "3, 5, 7"         → Vec3(3, 5, 7)  (longest match wins)
```

Any number of interface implementations is supported (struct + class mixed). The SG generates dispatch internally. **Longest match wins**: templates with shared prefixes will not mis-match.

Interfaces registered across multiple assemblies via hot-reload DLLs are merged into a dispatch chain — later registrations append to the chain rather than overwriting, so all concrete types remain reachable.

---

## 8. `[TemplateIgnore]`

```csharp
[Template("<float Value>")]
struct Container
{
    float Value;
    [TemplateIgnore] object RuntimeCache;  // excluded from serialization
}
```

Fields marked `[TemplateIgnore]` do not appear in generated code and do not trigger dependency errors (SSR004). Use for runtime state, caches, circular references, and other non-serializable fields.

---

## 9. Type Aliases

```csharp
[assembly: TypeAlias("HP", "float")]
[assembly: TypeAlias("MP", "float")]

[Template("<HP Health>, <MP Mana>")]
struct Stats { float Health; float Mana; }
// Input: "100, 50"
```

Aliases only change the type name in the template. Parsing behavior is identical to the original type.

---

## 10. External Type Templates

Register templates for types whose source code you don't own (third-party libraries, system types, generic collections):

**Concrete types**:

```csharp
[assembly: ExternalTemplate(typeof(Vector3), "<float x>, <float y>, <float z>")]
```

**Overriding default collection templates**:

```csharp
// Use semicolons instead of commas as separators
[assembly: ExternalTemplate(typeof(List<>), "<first><T item></first><body>; <T item></body>")]

// Use equals instead of colons
[assembly: ExternalTemplate(typeof(Dictionary<,>), "<first><TKey key>=<TValue value></first><body>, <TKey key>=<TValue value></body>")]
```

`typeof(List<>)` and `typeof(Dictionary<,>)` reference open generic types. Type parameter placeholders in the template must match real type parameter names (`T`, `TKey`, `TValue`). The SG substitutes by name when synthesizing concrete instances.

---

## 11. Class Types

```csharp
[Template("<string Name>|<float Value>")]
class NamedValue
{
    public string Name;
    public float Value;
}

// Classes can be used as field types in other templates
[Template("<float Base><optional>, <List<NamedValue> Mods></optional>")]
class Modifiable
{
    public float Base;
    public List<NamedValue> Mods;
}
// Input: "100, sword|1.5, shield|2.5"
```

Class fields are initialized via `new T()` (structs use `default`). The SG determines allocation strategy at compile time via Roslyn's `IsUnmanagedType`.

---

## 12. Composite Data Structures

### Item Table

```csharp
enum ItemType { [Tag("weapon")] Weapon, [Tag("potion")] Potion, [Tag("scroll")] Scroll }

[Template("<int Id>|<string Name>|<ItemType Type>|<int Price><optional>|<string Description></optional>")]
struct Item
{
    int Id;
    string Name;
    ItemType Type;
    int Price;
    string Description;
}
// Input: "42|steel_sword|weapon|150"
// Input: "99|health_vial|potion|25|restores 50 HP"
```

### Deep Nesting

```csharp
[Template("<float X>, <float Y>, <float Z>")]
struct Vec3 { float X; float Y; float Z; }

[Template("<Vec3 Center>|<float Radius>")]
struct Sphere { Vec3 Center; float Radius; }

[Template("<string Name>|<Sphere Bounds>")]
struct Zone { string Name; Sphere Bounds; }
// Input: "safe_zone|10, 0, 5|100"
```

### Config with Optional Generic Collections

```csharp
[Template("<string Name>|<float Duration><optional>|<List<string> Tags></optional>")]
struct BuffConfig
{
    string Name;
    float Duration;
    List<string> Tags;
}
// Input: "berserk|30"
// Input: "shield|10|fire, reflect, timed"
```

### Complex Nesting with Interface Generics

```csharp
interface IEffect { }

[Template("<float Amount>")]
struct DamageEffect : IEffect { float Amount; }

[Template("<float Duration>|<float Rate>")]
struct RegenEffect : IEffect { float Duration; float Rate; }

[Template("<int Id>|<IEffect Form>")]
struct ElementDescriptor
{
    int Id;
    IEffect Form;
}

[Template("<string Name>|<float Base><optional>|<List<ElementDescriptor> Combo></optional>")]
struct Ability
{
    string Name;
    float Base;
    List<ElementDescriptor> Combo;
}
// Input: "fireball|100, 1|50, 2|5|0.1"
// Descriptor[0] = Id=1, Form=DamageEffect(50)
// Descriptor[1] = Id=2, Form=RegenEffect(5, 0.1)
```

---

## 13. Compact vs XML Syntax

Both syntaxes are equivalent — use either:

```csharp
// Compact syntax
[Template("<float X>, <float Y><optional>, <float Z></optional>")]

// XML syntax
[Template(@"
  <literal-template>
    <field type=""float"" name=""X""/>
    <text>, </text>
    <field type=""float"" name=""Y""/>
    <optional>
      <text>, </text>
      <field type=""float"" name=""Z""/>
    </optional>
  </literal-template>")]
```

Use XML syntax when `<` or `>` characters need escaping in compact form.

---

## 14. Deprecated Patterns

| Old | Replacement |
|-----|-------------|
| `<repetition>A</repetition>` | `<first>A</first><body>A</body>` |
| `<repetition><first>A</first><body>B</body></repetition>` | `<first>A</first><body>B</body>` |

Use `<first>/<body>` directly without wrapping in `<repetition>`. Collection type templates (`List<T>`, `Dictionary<K,V>`) are built into the SG — no manual repetition logic is needed.

---

## 15. Diagnostic Quick Reference

| Code | Meaning |
|------|---------|
| SSR001 | Template syntax error |
| SSR002 | Circular template dependency |
| SSR003 | Field referenced in template is readonly with no matching constructor — add constructor or remove readonly |
| SSR004 | Field type missing `[Template]` — add `[Template]`, `[ExternalTemplate]`, or `[TemplateIgnore]` |
| SSR005 | Scalar field inside repetition block — use a collection type instead |
| SSR006 | Template ambiguity across interface implementations |
