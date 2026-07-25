# 示例: 枚举标签与类型别名

枚举成员用 `[Tag]` 声明文本标签，类型别名用 `[TypeAlias]` 给内置类型起别名。

## 枚举标签

```csharp
enum Element : byte
{
    [Tag("fire")]  Fire,
    [Tag("ice")]   Ice,
    [Tag("magic")] Magic,
}

[Template("Spell(<Element Type>, <float Power>)")]
public struct Spell
{
    public Element Type;
    public float Power;
}
```

```csharp
SerializerBlocks.TryGet<Spell>(out var block);

block.Scan("Spell(fire, 50)".AsSpan(), 0, out Spell v);
// v.Type == Element.Fire, v.Power == 50f

block.Scan("Spell(ice, 30)".AsSpan(), 0, out Spell v);
// v.Type == Element.Ice, v.Power == 30f
```

枚举类型名（`Element`）直接在模板中使用，SG 自动识别 `[Tag]` 注解并生成 switch-on-string 扫描器。无 `[Tag]` 的枚举成员在 Emit 时回退到 `value.ToString()`。

## 类型别名

```csharp
[assembly: TypeAlias("HP", "float")]
[assembly: TypeAlias("MP", "float")]

[Template("Stats(<HP Health>, <MP Mana>)")]
public struct Stats
{
    public float Health;
    public float Mana;
}
```

```csharp
SerializerBlocks.TryGet<Stats>(out var block);

block.Scan("Stats(100, 50)".AsSpan(), 0, out Stats v);
// v.Health == 100f, v.Mana == 50f
```

别名只改变模板中的类型名，解析行为与原始类型一致。别名可映射到任何已注册类型（内置和自定义均可）。

## 组合使用

枚举标签和类型别名可以在同一个 struct 中组合：

```csharp
[assembly: TypeAlias("Elem", "Element")]

[Template("AdvancedSpell(<Elem Type>, <float Cost>)")]
public struct AdvancedSpell
{
    public Element Type;
    public float Cost;
}
```

```csharp
SerializerBlocks.TryGet<AdvancedSpell>(out var block);

block.Scan("AdvancedSpell(magic, 120)".AsSpan(), 0, out AdvancedSpell v);
// v.Type == Element.Magic, v.Cost == 120f
```

运行测试参考 `EnumTagTests.cs`、`TypeAliasTests.cs` 中的完整用例。

## 参见

- [Tag API](../api/tag-attribute): 枚举标签的完整 API 签名
- [TypeAlias API](../api/type-alias-attribute): 类型别名的完整 API 签名
- [模板语法](../guide/template-syntax): 四种原语、内置类型、集合格式
