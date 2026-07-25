# 示例: 自定义泛型类型

开放泛型模板（如 `Wrapper<T>`）在字段引用具体实例（如 `Wrapper<float>`）时自动合成。支持单参数、多参数、嵌套泛型集合。

## 单参数泛型

```csharp
[Template("<T Value>")]
public struct Wrapper<T> where T : unmanaged
{
    public T Value;
}

[Template("<Wrapper<float> W>")]
public struct UsesWrapper
{
    public Wrapper<float> W;
}
```

```csharp
SerializerBlocks.TryGet<UsesWrapper>(out var block);

block.Scan("Wrapper(3.5)".AsSpan(), 0, out var v);
// v.W.Value == 3.5f
```

`Wrapper<float>` 被 `UsesWrapper` 引用时，SG 自动基于 `Wrapper<T>` 模板合成具体实例。使用方无需为每个具体类型显式声明。

## 多参数泛型

```csharp
[Template("<T1 First>, <T2 Second>")]
public struct Pair<T1, T2>
    where T1 : unmanaged
    where T2 : unmanaged
{
    public T1 First;
    public T2 Second;
}

[Template("<Pair<float,int> P>")]
public struct UsesPair
{
    public Pair<float, int> P;
}
```

```csharp
SerializerBlocks.TryGet<UsesPair>(out var block);

block.Scan("Pair(3.5, 42)".AsSpan(), 0, out var v);
// v.P.First == 3.5f, v.P.Second == 42
```

类型参数名任意（`T`、`TKey`、`TValue`、`TData` 均可）：SG 按位置映射，不限数量。

## 嵌套泛型集合

```csharp
[Template("<List<Wrapper<float>> Items>")]
public struct WrapperList
{
    public List<Wrapper<float>> Items;
}
```

```csharp
SerializerBlocks.TryGet<WrapperList>(out var block);

block.Scan("List(Wrapper(1.5), Wrapper(2.0), Wrapper(3.5))".AsSpan(), 0, out var v);
// v.Items[0].Value == 1.5f, v.Items[1].Value == 2.0f, v.Items[2].Value == 3.5f
```

泛型合成递归处理：SG 在为 `WrapperList` 生成代码前，先合成 `Wrapper<float>` 的具体模板，再合成 `List<Wrapper<float>>`。递归深度不限。

## 与默认接口模板的关系

用户自定义泛型模板的优先级高于系统默认接口模板。例如：自定义 `MyList<T>` 带 `[Template("MyList(<T item>)")]` 不会走 `IList<T>` 默认模板。

运行测试参考 `GenericTemplateTests.cs` 中的完整用例。

## 参见

- [模板写作指南](../guide/template-writing): 泛型类型模板的完整语法
- [SG 管线全景](../technical/pipeline/overview): 泛型实例合成的管线位置
- [架构决策](../technical/architecture-decisions): ADR-10 泛型实例合成的方案对比
