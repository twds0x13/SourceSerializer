# `SerializerEmitters`

partial class，source generator 注入生成的序列化方法并注册委托。与 `SerializerScanners`（反序列化方向）镜像设计，共享同一套 `[Template]` 声明。

## 签名

```csharp
internal static partial class SerializerEmitters
{
    public static bool TryGetEmitter<T>(out EmitterDelegate<T> emitter);
}

public delegate void EmitterDelegate<T>(StringBuilder sb, T value);
```

`EmitterDelegate<T>` 定义在 `SerializerScanners.cs`，与 `ScannerDelegate<T>` 并列。

## TryGetEmitter

检查类型 `T` 是否已通过 `[Template]` 注册了序列化器。

| 参数 | 类型 | 说明 |
|------|------|------|
| `emitter` | `out EmitterDelegate<T>` | 序列化器委托，未注册时为 `null` |
| 返回值 | `bool` | 是否成功获取 |

## EmitterDelegate

与 `ScannerDelegate<T>` 不同，`EmitterDelegate<T>` 不返回位置信息。序列化方向没有"匹配失败"的概念：给定有效实例，输出总是成功。

| 参数 | 类型 | 说明 |
|------|------|------|
| `sb` | `StringBuilder` | 输出目标缓冲区 |
| `value` | `T` | 待序列化的数据实例 |

## 内部注册机制

source generator 在生成的 `SerializerEmitters.g.cs` 中通过静态构造函数注册序列化器：

```csharp
static SerializerEmitters()
{
    EmitterRegistry<Point2D>.Emitter = (StringBuilder s, Point2D v) =>
    {
        Emit_Point2D(s, v);
    };
}
```

`EmitterRegistry<T>` 是内部的泛型静态字段容器，每个 `T` 持有一个 `EmitterDelegate<T>` 实例。

## 内置类型 Emit 方法

`SerializerRegistry` 为 12 种内置类型提供手写的零分配序列化方法，与扫描方法对应：

| 类型 | Emit 方法 |
|------|----------|
| `float` | `Emit_Float` |
| `double` | `Emit_Double` |
| `int` | `Emit_Int` |
| `uint` | `Emit_Uint` |
| `long` | `Emit_Long` |
| `ulong` | `Emit_Ulong` |
| `short` | `Emit_Short` |
| `ushort` | `Emit_Ushort` |
| `byte` | `Emit_Byte` |
| `sbyte` | `Emit_Sbyte` |
| `bool` | `Emit_Bool` |
| `char` | `Emit_Char` |
| `string` | `Emit_String` |

所有 Emit 方法遵循统一签名：

```csharp
internal static void Emit_Xxx(StringBuilder sb, Xxx value)
```

## 枚举标签 Emit

`[Tag]` 标注的枚举类型，source generator 同时生成 switch-on-member 的 Emit 方法：

```csharp
internal static void Emit_Enum_Element(StringBuilder sb, Element value)
{
    switch (value)
    {
        case Element.Fire:  sb.Append("fire"); break;
        case Element.Ice:   sb.Append("ice"); break;
        case Element.Magic: sb.Append("magic"); break;
        default: sb.Append(value.ToString()); break;
    }
}
```

## 用法示例

```csharp
[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}

SerializerEmitters.TryGetEmitter<Point2D>(out var emit);
var sb = new StringBuilder();
emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
Console.WriteLine(sb.ToString()); // "3.5 -2.1"
```

## 限制

`<repetition>` 块通过 `foreach` 迭代集合元素：首元素使用无分隔符模式，后续元素使用含分隔符模式。集合类型的扫描和发射均已实现。

## 参见

- [`SerializerScanners`](./serializer-scanners): 反序列化方向
- [`SerializerRegistry`](./serializer-registry): 内置类型扫描器与发射器
- [`[Template]`](./template-attribute): 模板声明
