# SourceSerializer 模板写作指南

本文用具体数据结构演示模板的完整写法。每个例子给出 C# 类型定义、模板、可解析的输入示例。

模板推荐的统一风格：**`TypeName(<type field>, <type field>, ...)`**——函数调用式包裹，逗号+空格分隔。与 `List(...)`、`Dict(...)`、`HashSet(...)` 集合格式保持一致。

---

## 1. 基础值类型

```csharp
[Template("Point(<float X>, <float Y>)")]
struct Point { float X; float Y; }

// 输入: "Point(3.5, -2)"
// 输入: "Point(100, 0.5)"
```

多个字段一律逗号分隔，字符串值加引号：

```csharp
[Template("Player(<string Name>, <int Level>)")]
struct Player { string Name; int Level; }
// 输入: "Player(\"warrior\", 5)"

[Template("Range(<float Min>, <float Max>)")]
struct Range { float Min; float Max; }
// 输入: "Range(10, 100)"
```

**全部内置类型**：`float` `double` `int` `uint` `long` `ulong` `short` `ushort` `byte` `sbyte` `bool` `char` `string`

---

## 2. 可选块

```csharp
[Template("Damage(<float Base><optional>, <float Bonus></optional>)")]
struct Damage
{
    float Base;
    float Bonus;  // 可选字段，默认值 = 0
}
// 输入: "Damage(100)"          → Base=100, Bonus=0
// 输入: "Damage(100, 25)"      → Base=100, Bonus=25
```

可选块也可以包裹多个字段：

```csharp
[Template("Player(<string Name><optional>, <int Level>, <float Exp></optional>)")]
struct Player
{
    string Name;
    int Level;
    float Exp;
}
// 输入: "Player(\"warrior\")"                   → Name=warrior, Level=0, Exp=0
// 输入: "Player(\"warrior\", 5, 1200.5)"        → Name=warrior, Level=5, Exp=1200.5
```

---

## 3. 枚举标签

```csharp
enum Element : byte { Physical = 0, [Tag("fire")] Fire, [Tag("ice")] Ice }

[Template("Spell(<Element Elem>, <float Dmg>)")]
struct Spell { Element Elem; float Dmg; }
// 输入: "Spell(fire, 50)"
// 输入: "Spell(ice, 30)"
```

`[Tag("...")]` 声明枚举成员在文本中的表示。模板中直接用枚举类型名，SG 自动识别。

---

## 4. 嵌套类型

```csharp
[Template("Vec2(<float X>, <float Y>)")]
struct Vec2 { float X; float Y; }

[Template("Entity(<string Name>, <Vec2 Pos>)")]
struct Entity { string Name; Vec2 Pos; }
// 输入: "Entity(\"treasure\", Vec2(10, 20))"
```

递归依赖自动解析：SG 先处理 `Vec2`，再处理引用它的 `Entity`。嵌套类型的 `Prefix(...)` 自然形成树状结构。

---

## 5. 集合（List / Array / Dictionary / HashSet）

```csharp
[Template("Skill(<float Base><optional>, <List<float> Multipliers></optional>)")]
struct Skill
{
    float Base;
    List<float> Multipliers;
}
// 输入: "Skill(100)"                           → Multipliers=List()
// 输入: "Skill(100, List(1.5))"                → Multipliers=List(1.5)
// 输入: "Skill(100, List(1.5, 2.0, 0.75))"    → Multipliers=List(1.5, 2.0, 0.75)
```

**集合字段在模板中直接使用，无需显式声明模板。** 系统为以下接口提供了默认模板：

| 默认接口模板 | 覆盖的集合类型 | 序列化格式 |
|-------------|--------------|-----------|
| `IList<T>` | `List<T>`, `T[]`, `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>` 等 | `List(...)` |
| `ISet<T>` | `HashSet<T>`, `SortedSet<T>`, `ISet<T>`, `IReadOnlySet<T>` 等 | `HashSet(...)` |
| `IReadOnlyList<T>` | `IReadOnlyList<T>` 等 | `List(...)` |
| `IDictionary<K,V>` | `Dictionary<K,V>`, `SortedDictionary<K,V>`, `IDictionary<K,V>` 等 | `Dict(k: v, ...)` |
| `IReadOnlyDictionary<K,V>` | `IReadOnlyDictionary<K,V>` 等 | `Dict(k: v, ...)` |

**接口优先原则**：推荐为自定义集合定义接口并标注模板，而非逐个类标注。类级 `[ExternalTemplate]` 优先级高于接口模板。

```csharp
[Template("Payload(<int Id>, <float[] Data>)")]
struct Payload { int Id; float[] Data; }
// 输入: "Payload(42, List(3.5, 7, 1))"

[Template("Stats(<Dictionary<string,int> Entries>)")]
struct Stats { Dictionary<string,int> Entries; }
// 输入: "Stats(Dict(hp:100, atk:50))"
```

---

## 6. 自定义泛型类型

单参数泛型：

```csharp
[Template("Wrapper(<T Value>)")]
struct Wrapper<T> where T : unmanaged { T Value; }

[Template("UsesWrapper(<Wrapper<float> W>)")]
struct UsesWrapper { Wrapper<float> W; }
// 输入: "UsesWrapper(Wrapper(3.5))"
```

多参数泛型：

```csharp
[Template("Pair(<T1 First>, <T2 Second>)")]
struct Pair<T1, T2> where T1 : unmanaged where T2 : unmanaged
{
    T1 First;
    T2 Second;
}

[Template("UsesPair(<Pair<float,int> P>)")]
struct UsesPair { Pair<float, int> P; }
// 输入: "UsesPair(Pair(3.5, 42))"
```

类型参数名任意（`T`、`TKey`、`TValue`、`TData` 均可）：SG 按**位置**映射，不限数量。`Wrapper<float>` 被 `UsesWrapper` 引用时自动触发合成。

---

## 7. 接口自动分发

```csharp
interface IVector { }

[Template("Vec2(<float X>, <float Y>)")]
struct Vec2 : IVector { float X; float Y; }

[Template("Vec3(<float X>, <float Y>, <float Z>)")]
struct Vec3 : IVector { float X; float Y; float Z; }

// 模板中直接写接口名
[Template("VectorWrapper(<IVector V>)")]
struct VectorWrapper { IVector V; }
// 输入: "VectorWrapper(Vec2(1.5, -2))"
// 输入: "VectorWrapper(Vec3(3, 5, 7))"
```

接口实现可以有任意多个（struct + class 混合），SG 内部生成 dispatch。不同程序集通过热更 DLL 注册的接口块自动链合并，所有具现类型保持可达。

---

## 8. `[TemplateIgnore]`

```csharp
[Template("Container(<float Value>)")]
struct Container
{
    float Value;
    [TemplateIgnore] object RuntimeCache;  // 不参与序列化
}
```

标记了 `[TemplateIgnore]` 的字段不出现在生成的代码中，不触发依赖错误（SSR004）。用于运行时状态、缓存、循环引用等不可序列化字段。

---

## 9. 类型别名

```csharp
[assembly: TypeAlias("HP", "float")]
[assembly: TypeAlias("MP", "float")]

[Template("Stats(<HP Health>, <MP Mana>)")]
struct Stats { float Health; float Mana; }
// 输入: "Stats(100, 50)"
```

别名只改变模板中的类型名，解析行为与原始类型一致。

---

## 10. 外部类型模板

给非自有代码的类型注册模板（第三方库、系统类型、泛型集合）：

**具体类型**：

```csharp
[assembly: ExternalTemplate(typeof(Vector3), "Vector3(<float x>, <float y>, <float z>)")]
```

**覆盖泛型集合默认模板**：

```csharp
// 用分号替换逗号分隔
[assembly: ExternalTemplate(typeof(List<>), "List(<first><T item></first><body>; <T item></body>)")]

// 用等号替换冒号
[assembly: ExternalTemplate(typeof(Dictionary<,>), "Dict(<first><TKey key>=<TValue value></first><body>, <TKey key>=<TValue value></body>)")]
```

`typeof(List<>)` 和 `typeof(Dictionary<,>)` 引用开放泛型类型。模板中的类型参数占位符必须与真实类型参数名一致（`T`、`TKey`、`TValue`），SG 在合成具体实例时按名称替换。

---

## 11. class 类型

```csharp
[Template("NamedValue(<string Name>, <float Value>)")]
class NamedValue
{
    public string Name;
    public float Value;
}

// 类可以做为其他模板的字段类型
[Template("Modifiable(<float Base><optional>, <List<NamedValue> Mods></optional>)")]
class Modifiable
{
    public float Base;
    public List<NamedValue> Mods;
}
// 输入: "Modifiable(100, List(NamedValue(\"sword\", 1.5), NamedValue(\"shield\", 2.5)))"
```

class 字段自动 `new T()` 初始化（struct 用 `default`）。SG 在编译期通过 Roslyn 的 `IsUnmanagedType` 判定。

---

## 12. 组合数据结构

### 道具表

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
// 输入: "Item(42, \"steel_sword\", weapon, 150)"
// 输入: "Item(99, \"health_vial\", potion, 25, \"restores 50 HP\")"
```

### 深度嵌套

```csharp
[Template("Vec3(<float X>, <float Y>, <float Z>)")]
struct Vec3 { float X; float Y; float Z; }

[Template("Sphere(<Vec3 Center>, <float Radius>)")]
struct Sphere { Vec3 Center; float Radius; }

[Template("Zone(<string Name>, <Sphere Bounds>)")]
struct Zone { string Name; Sphere Bounds; }
// 输入: "Zone(\"safe_zone\", Sphere(Vec3(10, 0, 5), 100))"
```

### 带可选泛型集合的配置

```csharp
[Template("BuffConfig(<string Name>, <float Duration><optional>, <List<string> Tags></optional>)")]
struct BuffConfig
{
    string Name;
    float Duration;
    List<string> Tags;
}
// 输入: "BuffConfig(\"berserk\", 30)"
// 输入: "BuffConfig(\"shield\", 10, List(\"fire\", \"reflect\", \"timed\"))"
```

### 含接口泛型的复杂嵌套

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
// 输入: "Ability(\"fireball\", 100, List(ElementDescriptor(1, DamageEffect(50)), ElementDescriptor(2, RegenEffect(5, 0.1))))"
```

---

## 13. 紧凑语法 vs XML 语法

两种写法等价，选用任意一种：

```csharp
// 紧凑语法（推荐）
[Template("Point(<float X>, <float Y><optional>, <float Z></optional>)")]

// XML 语法
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

紧凑语法中的 `<` `>` 需要转义时用 XML 语法。

---

## 14. 模板中哪些写法已经废弃

| 旧写法 | 替代 |
|--------|------|
| 裸字段无类型前缀 | 使用 `TypeName(...)` 包裹 |
| `\|` 或随意分隔符 | 统一逗号 `, ` 分隔 |
| `<repetition>A</repetition>` | `<first>A</first><body>A</body>` |
| `<repetition><first>A</first><body>B</body></repetition>` | `<first>A</first><body>B</body>` |

`<first>/<body>` 直接使用，不需要包裹在 `<repetition>` 中。集合类型的模板由 SG 内置处理。

---

## 15. 诊断速查

| 代码 | 含义 |
|------|------|
| SSR001 | 模板语法错误 |
| SSR002 | 循环依赖 |
| SSR003 | 模板引用的字段是 readonly 且无匹配构造器 — 添加构造器或移除 readonly |
| SSR004 | 字段类型缺少 `[Template]` — 加 `[Template]`、`[ExternalTemplate]`、或 `[TemplateIgnore]` |
| SSR005 | 标量字段在重复块内 — 改用集合类型 |
| SSR006 | 接口实现间模板歧义 — 调整模板使各实现可区分 |
