# `SerializerBlocks`

序列化器块注册表。跨程序集的中心注册点——SG 和热更 DLL 均可通过 `AddBlock<T>` / `AddBlocks` / `RemoveBlock<T>` 注册或移除 `ISerializerBlock<TData>` 实现。

## 核心接口

```csharp
public interface ISerializerBlock<TData>
{
    int Scan(ReadOnlySpan<char> text, int pos, out TData value);
    void Emit(StringBuilder sb, TData value);
}

public static class SerializerBlocks
{
    public static bool TryGet<TData>(out ISerializerBlock<TData>? block);
    public static string Serialize<TData>(TData value);
    public static TData Deserialize<TData>(string text);
}
```

## TryGet

检查类型 `TData` 是否已注册序列化器块。首次调用触发 `EnsureInitialized()`，自动扫描所有已加载程序集中的 `GeneratedSerializers.Init()` 并注册内置类型。

| 参数 | 类型 | 说明 |
|------|------|------|
| `block` | `out ISerializerBlock<TData>?` | 序列化器块，未注册时为 `null` |
| 返回值 | `bool` | 是否成功获取 |

## AddBlock

```csharp
public static Builder AddBlock<T>(ISerializerBlock<T> block);
public static Builder AddBlock(Type dataType, ISerializerBlock block);
```

注册一个序列化器块。泛型版本直接调用；非泛型版本用于热更 DLL——调用方在编译期不持有类型。

**接口类型的链合并**：对于 `typeof(T).IsInterface`，多次注册做链式追加而非覆盖。这使得不同程序集可以各自生成接口分发块，运行时自动合并为 `ChainBlock<T>`。非接口类型的后注册覆盖先注册（标准行为）。

**示例**：

```csharp
// 服务端程序集注册 IVector 分发块（只认识 Vec2 和 Vec3）
GeneratedSerializers.Init();
// → AddBlock<IVector>(Block_IVector{Vec2, Vec3})

// 热更 DLL 加载后注册自己的 IVector 分发块（只认识 Vec6）  
DLL.GeneratedSerializers.Init();
// → AddBlock<IVector>(Block_IVector{Vec6})
// → 链合并：ChainBlock{ Block_IVector{Vec2,Vec3}, Block_IVector{Vec6} }
// 反序列化 "Vec6(1,2,3,4,5,6)" 时：先试 Vec2/Vec3 不匹配 → 试 Vec6 匹配
```

## RemoveBlock

```csharp
public static void RemoveBlock<T>();
public static void RemoveBlock(Type dataType);
```

移除指定类型的注册。接口类型移除整条分发链。未注册时静默成功。

## AddBlocks

```csharp
public static void AddBlocks(params ISerializerBlock[] blocks);
```

批量注册异构块。每个 block 的泛型参数通过反射推导，委托到 `RegisterBlock<T>` 以复用链合并逻辑。

## GeneratedSerializers 初始化

SG 编译期生成 `public static partial class GeneratedSerializers`，包含所有用户类型的 `Scan_Xxx` / `Emit_Xxx` 方法和注册入口：

```csharp
public static partial class GeneratedSerializers
{
    public static void Init()
    {
        // 幂等：二次调用直接返回
        SerializerBlocks.AddBlock<Point2D>(new Block_Point2D());
        SerializerBlocks.AddBlock<IVector>(new Block_IVector());
        // ... 所有类型
    }
}
```

`Init()` 由 `EnsureInitialized()` 在首次 `TryGet<T>` 时通过 AppDomain 反射扫描自动调用。热更 DLL 的入口点应显式调用自身的 `GeneratedSerializers.Init()`。

## 内置类型注册

`EnsureInitialized()` 在扫描完所有 `GeneratedSerializers.Init()` 后注册 13 种内置类型（float、double、int、uint、long、ulong、short、ushort、byte、sbyte、bool、char、string）的 `BuiltinBlock_*`，确保内置类型始终可用。

## 内部实现

每个类型由 SG 生成一个 `public readonly struct` 实现 `ISerializerBlock<T>`：

```csharp
public readonly struct Block_Point2D : ISerializerBlock<Point2D>
{
    public int Scan(ReadOnlySpan<char> t, int p, out Point2D v) =>
        GeneratedSerializers.Scan_Point2D(t, p, out v);

    public void Emit(StringBuilder sb, Point2D v) =>
        GeneratedSerializers.Emit_Point2D(sb, v);
}
```

## 参见

- [Template 属性](./template-attribute)
- [ExternalTemplate 属性](./external-template-attribute)
- [SerializerRegistry](./serializer-registry)
