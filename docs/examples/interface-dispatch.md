# 示例: 接口自动分派

同一接口的多种实现类型在模板中直接使用接口名。SG 在编译期收集所有实现，运行时自动分派到匹配的类型。

## 基础接口分派

```csharp
public interface IVector { }

[Template("Vec2(<float X>, <float Y>)")]
public struct Vec2 : IVector
{
    public float X;
    public float Y;
}

[Template("Vec3(<float X>, <float Y>, <float Z>)")]
public struct Vec3 : IVector
{
    public float X;
    public float Y;
    public float Z;
}

[Template("<IVector V>")]
public struct VectorWrapper
{
    public IVector V;
}
```

```csharp
SerializerBlocks.TryGet<VectorWrapper>(out var block);

// 输入匹配 Vec2
block.Scan("Vec2(1.5, -2)".AsSpan(), 0, out var v1);
// v1.V is Vec2 { X = 1.5f, Y = -2f }

// 输入匹配 Vec3
block.Scan("Vec3(3, 5, 7)".AsSpan(), 0, out var v2);
// v2.V is Vec3 { X = 3f, Y = 5f, Z = 7f }
```

Scan 按声明顺序尝试所有具现类型，首个推进 `pos` 的类型胜出。Emit 用 switch 模式匹配进行运行时类型分派。

## 接口集合（异质列表）

```csharp
[Template("<float Base><optional>, <List<IVector> Targets></optional>")]
public struct Attack
{
    public float Base;
    public List<IVector> Targets;
}
```

```csharp
SerializerBlocks.TryGet<Attack>(out var block);

block.Scan("100, List(Vec2(3, 5), Vec3(1, 2, 3))".AsSpan(), 0, out var v);
// v.Base == 100f
// v.Targets[0] is Vec2 { X = 3f, Y = 5f }
// v.Targets[1] is Vec3 { X = 1f, Y = 2f, Z = 3f }
```

同一集合中可以混入不同实现类型。SG 生成的接口分发块对每个元素独立尝试所有实现。

## 跨程序集链合并

不同程序集注册的同一接口块自动合并为 `ChainBlock<T>` 分发链：

```csharp
// 主程序集: SG 生成 Block_IVector{Vec2, Vec3}
GeneratedSerializers.Init();

// 热更 DLL 加载后: SG 生成 Block_IVector{Vec6}
DLL.GeneratedSerializers.Init();
// 接口类型自动链合并: ChainBlock{ Block_IVector{Vec2,Vec3}, Block_IVector{Vec6} }
// "Vec6(1,2,3,4,5,6)" 先试 Vec2/Vec3 不匹配 → 试 Vec6 成功
```

运行测试参考 `InterfaceDispatchTests.cs`、`ChainBlockTests.cs` 中的完整用例。

## 参见

- [模板语法](../guide/template-syntax): 四种原语与模板声明
- [热更新与跨程序集注册](../guide/hot-reload): ChainBlock 链合并的完整机制
- [内部机制](../technical/internals): 接口分派与 ChainBlock 的源码级细节
