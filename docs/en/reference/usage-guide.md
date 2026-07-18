# SourceSerializer 模板写作指南

本文用具体数据结构演示模板的完整写法。每个例子给出 C# 类型定义、模板、可解析的输入示例。

---

## 1. 基础值类型

```csharp
// float / int / double / bool / string / long / byte / char / short ...
[Template("<float X>, <float Y>")]
struct Point { float X; float Y; }

// 输入: "3.5, -2"
// 输入: "100, 0.5"
```

分隔符和裸文字自由选择：逗号、竖线、空格、括号均可用：

```csharp
[Template("<string Name>|<int Level>")]
struct Player { string Name; int Level; }
// 输入: "warrior|5"

[Template("<float Min> to <float Max>")]
struct Range { float Min; float Max; }
// 输入: "10 to 100"
```

**全部内置类型**：`float` `double` `int` `uint` `long` `ulong` `short` `ushort` `byte` `sbyte` `bool` `char` `string`

---

## 2. 可选块

```csharp
[Template("<float Base><optional>, <float Bonus></optional>")]
struct Damage
{
    float Base;
    float Bonus;  // 可选字段，默认值 = 0
}
// 输入: "100"          → Base=100, Bonus=0
// 输入: "100, 25"      → Base=100, Bonus=25
```

可选块也可以包裹多个字段：

```csharp
[Template("<string Name><optional>|<int Level><float Exp></optional>")]
struct Player
{
    string Name;
    int Level;
    float Exp;
}
// 输入: "warrior"              → Name=warrior, Level=0, Exp=0
// 输入: "warrior|5|1200.5"     → Name=warrior, Level=5, Exp=1200.5
```

---

## 3. 枚举标签

```csharp
enum Element : byte { Physical = 0, [Tag("fire")] Fire, [Tag("ice")] Ice }

[Template("<Element Elem><float Dmg>")]
struct Spell { Element Elem; float Dmg; }
// 输入: "fire|50"
// 输入: "ice|30"
```

`[Tag("...")]` 声明枚举成员在文本中的表示。模板中直接用枚举类型名，SG 自动识别。

---

## 4. 嵌套类型

```csharp
[Template("<float X>, <float Y>")]
struct Vec2 { float X; float Y; }

[Template("<string Name> at <Vec2 Pos>")]
struct Entity { string Name; Vec2 Pos; }
// 输入: "treasure at 10, 20"
```

递归依赖自动解析：SG 先处理 `Vec2`，再处理引用它的 `Entity`。

---

## 5. 集合（List / Array / Dictionary）

```csharp
[Template("<float Base><optional>, <List<float> Multipliers></optional>")]
struct Skill
{
    float Base;
    List<float> Multipliers;
}
// 输入: "100"                    → Multipliers=[]
// 输入: "100, 1.5"              → Multipliers=[1.5]
// 输入: "100, 1.5, 2.0, 0.75"  → Multipliers=[1.5, 2.0, 0.75]
```

**Collection fields work directly in templates — no explicit template declaration needed.** The system provides default templates for these interfaces, and concrete types are matched through their implemented interfaces:

| Default Interface | Covered Types | Separator |
|-------------------|--------------|-----------|
| `IList<T>` | `List<T>`, `T[]`, `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, etc. | Comma |
| `ISet<T>` | `HashSet<T>`, `SortedSet<T>`, `ISet<T>`, `IReadOnlySet<T>`, etc. | Comma |
| `IReadOnlyList<T>` | `IReadOnlyList<T>`, etc. | Comma |
| `IDictionary<K,V>` | `Dictionary<K,V>`, `SortedDictionary<K,V>`, `IDictionary<K,V>`, etc. | Colon |
| `IReadOnlyDictionary<K,V>` | `IReadOnlyDictionary<K,V>`, etc. | Colon |

**Interface-first principle:** for custom collection types, prefer defining an interface with a template rather than annotating each class. All types implementing that interface automatically inherit the template. Class-level `[ExternalTemplate]` takes precedence over interface templates.

```csharp
[Template("<int Id><float[] Data>")]
struct Payload { int Id; float[] Data; }
// 输入: "42|3.5, 7, 1"    → Data=[3.5, 7, 1]

[Template("<Dictionary<string,int> Stats>")]
struct Stats { Dictionary<string,int> Stats; }
// 输入: "hp:100, atk:50"   → Stats={"hp":100, "atk":50}
```

---

## 6. 自定义泛型类型

单参数泛型：

```csharp
[Template("<T Value>")]
struct Wrapper<T> where T : unmanaged { T Value; }

[Template("<Wrapper<float> W>")]
struct UsesWrapper { Wrapper<float> W; }
// 输入: "3.5"
```

多参数泛型：

```csharp
[Template("<T1 First>, <T2 Second>")]
struct Pair<T1, T2> where T1 : unmanaged where T2 : unmanaged
{
    T1 First;
    T2 Second;
}

[Template("<Pair<float,int> P>")]
struct UsesPair { Pair<float, int> P; }
// 输入: "3.5, 42"
```

类型参数名任意（`T`、`TKey`、`TValue`、`TData` 均可）：SG 按**位置**映射，不限数量。`Wrapper<float>` 被 `UsesWrapper` 引用时自动触发合成。

---

## 7. 接口自动分发

```csharp
interface IVector { }

[Template("<float X>, <float Y>")]
struct Vec2 : IVector { float X; float Y; }

[Template("<float X>, <float Y>, <float Z>")]
struct Vec3 : IVector { float X; float Y; float Z; }

// 模板中直接写接口名
[Template("<IVector V>")]
struct VectorWrapper { IVector V; }
// 输入: "1.5, -2"         → Vec2(1.5, -2)
// 输入: "3, 5, 7"         → Vec3(3, 5, 7)  (最长匹配胜出)
```

接口实现可以有任意多个（struct + class 混合），SG 内部生成 dispatch。**最长匹配胜出**：共享前缀的模板不会误匹配。

---

## 8. `[TemplateIgnore]`

```csharp
[Template("<float Value>")]
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

[Template("<HP Health>, <MP Mana>")]
struct Stats { float Health; float Mana; }
// 输入: "100, 50"
```

别名只改变模板中的类型名，解析行为与原始类型一致。

---

## 10. 外部类型模板

用于给第三方库的类型注册模板：

```csharp
[assembly: ExternalTemplate(typeof(Vector3), "<float x>, <float y>, <float z>")]

// SG 自动为 UnityEngine.Vector3 生成扫描/发射代码
```

---

## 11. class 类型

```csharp
[Template("<string Name>|<float Value>")]
class NamedValue
{
    public string Name;
    public float Value;
}

// 类可以做为其他模板的字段类型
[Template("<float Base><optional>, <List<NamedValue> Mods></optional>")]
class Modifiable
{
    public float Base;
    public List<NamedValue> Mods;
}
// 输入: "100, sword|1.5, shield|2.5"
```

class 字段自动 `new T()` 初始化（struct 用 `default`）。SG 在编译期通过 Roslyn 的 `IsUnmanagedType` 判定。

---

## 12. 组合数据结构

### 道具表

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
// 输入: "42|steel_sword|weapon|150"
// 输入: "99|health_vial|potion|25|restores 50 HP"
```

### 深度嵌套

```csharp
[Template("<float X>, <float Y>, <float Z>")]
struct Vec3 { float X; float Y; float Z; }

[Template("<Vec3 Center>|<float Radius>")]
struct Sphere { Vec3 Center; float Radius; }

[Template("<string Name>|<Sphere Bounds>")]
struct Zone { string Name; Sphere Bounds; }
// 输入: "safe_zone|10, 0, 5|100"
```

### 带可选泛型集合的配置

```csharp
[Template("<string Name>|<float Duration><optional>|<List<string> Tags></optional>")]
struct BuffConfig
{
    string Name;
    float Duration;
    List<string> Tags;
}
// 输入: "berserk|30"
// 输入: "shield|10|fire, reflect, timed"
```

### 含接口泛型的复杂嵌套

```csharp
interface IEffect { }

[Template("<float Amount>")]
struct DamageEffect : IEffect { float Amount; }

[Template("<float Duration>|<float Rate>")]
struct RegenEffect : IEffect { float Duration; float Rate; }

[Template("<int Id>|<IForm Form>")]
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
// 输入: "fireball|100, 1|50, 2|5|0.1"
// Descriptor[0] = Id=1, Form=DamageEffect(50)
// Descriptor[1] = Id=2, Form=RegenEffect(5, 0.1)
```

---

## 13. 紧凑语法 vs XML 语法

两种写法等价，选用任意一种：

```csharp
// 紧凑语法[Template("<float X>, <float Y><optional>, <float Z></optional>")]

// XML 语法
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

紧凑语法中的 `<` `>` 需要转义时用 XML 语法。

---

## 14. 模板中哪些写法已经废弃

| 旧写法 | 替代 |
|--------|------|
| `<repetition>A</repetition>` | `<first>A</first><body>A</body>` |
| `<repetition><first>A</first><body>B</body></repetition>` | `<first>A</first><body>B</body>` |

`<first>/<body>` 直接使用，不需要包裹在 `<repetition>` 中。集合类型（`List<T>`、`Dictionary<K,V>`）的模板由 SG 内置处理，不需要手动编写重复逻辑。

---

## 15. 诊断速查

| 代码 | 含义 |
|------|------|
| SSR001 | 模板语法错误 |
| SSR002 | 循环依赖 |
| SSR003 | Field referenced in template is readonly with no matching ctor — add ctor or remove readonly |
| SSR004 | 字段类型缺少 `[Template]` — 加 `[Template]`、`[ExternalTemplate]`、或 `[TemplateIgnore]` |
| SSR005 | 标量字段在重复块内 — 改用集合类型 |
