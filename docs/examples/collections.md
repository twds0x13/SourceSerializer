# 集合示例

集合类型在模板中直接使用，无需显式声明模板。

## List

```csharp
[Template("Skill(<float Base><optional>, <List<float> Multipliers></optional>)")]
struct Skill { float Base; List<float> Multipliers; }

// 输入: "Skill(100)" → Multipliers=List()
// 输入: "Skill(100, List(1.5, 2.0))" → Multipliers=List(1.5, 2.0)

// 序列化
SerializerBlocks.TryGet<Skill>(out var block);
block.Emit(sb, new Skill { Base = 100, Multipliers = new() { 1.5f, 2.0f } });
// → "Skill(100, List(1.5, 2.0))"
```

## HashSet

```csharp
[Template("UsesHashSet(<HashSet<float> Items>)")]
struct UsesHashSet { HashSet<float> Items; }

// 序列化格式: "UsesHashSet(HashSet(1.5, 2.0))"
```

## Dictionary

```csharp
// 通过 IDictionary<K,V> 默认接口模板自动解析
// 格式: "key: value, key2: value2"
```

## 嵌套泛型集合

```csharp
[Template("HasNestedHashSet(<List<HashSet<float>> Nested>)")]
struct HasNestedHashSet { List<HashSet<float>> Nested; }
// 序列化格式: "HasNestedHashSet(List(HashSet(1.5, 2.0), HashSet(3.0)))"
```

运行测试参考 `CollectionRepetitionTests.cs`、`GenericTemplateTests.cs` 中的完整用例。
