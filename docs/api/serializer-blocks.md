# `SerializerBlocks`

序列化器块注册表。跨程序集的中心注册点：SG 和热更 DLL 均可通过 `AddBlock<T>` / `AddBlocks` / `RemoveBlock<T>` 注册或移除 `ISerializerBlock<TData>` 实现。

## 核心接口

```csharp
public interface ISerializerBlock { }  // 非泛型标记接口，使 params ISerializerBlock[] 成为可能

public interface ISerializerBlock<TData> : ISerializerBlock
{
    int Scan(ReadOnlySpan<char> text, int pos, out TData value);
    void Emit(StringBuilder sb, TData value);
}

public static class SerializerBlocks
{
    public static bool TryGet<TData>(out ISerializerBlock<TData>? block);
    public static string Serialize<TData>(TData value);
    public static TData Deserialize<TData>(string text);
    public static bool TryScan<TData>(string text, out TData value);
}
```

## TryGet

检查类型 `TData` 是否已注册序列化器块。首次调用触发 `EnsureInitialized()`，自动扫描所有已加载程序集中的 `GeneratedSerializers.Init()` 并注册内置类型。

| 参数 | 类型 | 说明 |
|------|------|------|
| `block` | `out ISerializerBlock<TData>?` | 序列化器块，未注册时为 `null` |
| 返回值 | `bool` | 是否成功获取 |

## Serialize / Deserialize / TryScan

三个便捷方法封装了 `TryGet` + `Emit`/`Scan` 的样板代码，适合简单场景下一行式调用。

### Serialize`<T>`

```csharp
public static string Serialize<TData>(TData value);
```

调用 `TryGet<TData>` 获取 block，通过 `StringBuilder` 执行 `Emit` 后返回字符串。未注册类型抛出 `InvalidOperationException`。

### Deserialize`<T>`

```csharp
public static TData Deserialize<TData>(string text);
```

调用 `TryGet<TData>` 获取 block，通过 `WhitespaceStripper.Strip()` 预处理输入后执行 `Scan`。Scan 失败抛出 `FormatException`，未注册类型抛出 `InvalidOperationException`。

### TryScan`<T>`

```csharp
public static bool TryScan<TData>(string text, out TData value);
```

与 `Deserialize` 行为相同但不抛异常：Scan 失败或类型未注册时返回 `false`，`value` 为 `default`。

使用示例：

```csharp
// 一行式序列化
string s = SerializerBlocks.Serialize(new Point2D { X = 3.5f, Y = -2.1f });

// 一行式反序列化（自动空白符剔除）
Point2D v = SerializerBlocks.Deserialize<Point2D>("  Point2D( 3.5 ,  -2.1 )  ");

// 非抛出式反序列化
if (SerializerBlocks.TryScan<Point2D>(input, out var result))
    Console.WriteLine(result);
```

设计原理：这三个方法在 v3.4 新增，目标是消除七行 `TryGet` + 判 null + `new StringBuilder` + `Emit` + `ToString`（或 `WhitespaceStripper.Strip` + `Scan`）的重复样板。每个方法的实现不超过 10 行，直接委托到 `TryGet` 和 `ISerializerBlock<T>` 的对应方法。

## AddBlock

```csharp
public static Builder AddBlock<T>(ISerializerBlock<T> block);
public static Builder AddBlock(Type dataType, ISerializerBlock block);
```

注册一个序列化器块。泛型版本直接调用；非泛型版本用于热更 DLL：调用方在编译期不持有类型。

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

## Builder 流式 API

`AddBlock<T>()` 返回 `Builder` 嵌套类实例，支持链式注册：

```csharp
public sealed class Builder
{
    public Builder AddBlock<T>(ISerializerBlock<T> block);
    public Builder AddBlock(Type dataType, ISerializerBlock block);
    public Builder AddBlocks(params ISerializerBlock[] blocks);
    public Builder RemoveBlock<T>();
    public Builder RemoveBlock(Type dataType);
}
```

所有 `Builder` 方法直接委托到 `SerializerBlocks` 的对应静态方法，返回值均为 `Builder` 自身以支持进一步链式调用。`Builder` 是纯语法糖——不作为独立注册表，不持有状态。

```csharp
SerializerBlocks
    .AddBlock(new Block_Point2D())
    .AddBlock(typeof(Vec3), new Block_Vec3())
    .AddBlocks(new Block_Player(), new Block_Enemy());
```

## ISerializerBlock（非泛型标记接口）

```csharp
public interface ISerializerBlock { }
```

空标记接口，唯一目的是使 `ISerializerBlock<T>` 的不同泛型实例可被统一接收为 `params ISerializerBlock[]` 参数类型。C# 不支持 `params ISerializerBlock<>[]`（不同泛型实参的数组无公共基类型），因此需要一个非泛型标记接口作为 `ISerializerBlock<T>` 的上界。

手写 `ISerializerBlock<T>` 实现时需同时继承此标记接口（否则 `AddBlocks` 不接受其参数），SG 生成的 `Block_Xxx` 结构体自动包含。

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
