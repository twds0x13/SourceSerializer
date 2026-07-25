# Example: Custom Generic Types

Open generic templates (e.g. `Wrapper<T>`) are auto-synthesized into concrete instances when a field reference appears (e.g. `Wrapper<float>`). Single-parameter, multi-parameter, and nested generic collections are all supported.

## Single-Parameter Generics

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

When `Wrapper<float>` is referenced by `UsesWrapper`, the SG auto-synthesizes a concrete instance from the `Wrapper<T>` template. No per-instance declarations are needed.

## Multi-Parameter Generics

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

Type parameter names are arbitrary (`T`, `TKey`, `TValue`, `TData` all work): the SG maps by position, with no limit on the number of parameters.

## Nested Generic Collections

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

Generic synthesis is recursive: before generating code for `WrapperList`, the SG first synthesizes a concrete template for `Wrapper<float>`, then synthesizes `List<Wrapper<float>>`. Recursion depth is unlimited.

## Interaction with Default Interface Templates

User-defined generic templates take priority over system default interface templates. For example, a custom `MyList<T>` with `[Template("MyList(<T item>)")]` will not use the `IList<T>` default template.

See `GenericTemplateTests.cs` for complete runnable test cases.

## See Also

- [Template Writing Guide](../guide/template-writing): complete generic template syntax
- [SG Pipeline Overview](../technical/pipeline/overview): where generic synthesis sits in the pipeline
- [Architecture Decisions](../technical/architecture-decisions): ADR-10 on generic instance synthesis trade-offs
