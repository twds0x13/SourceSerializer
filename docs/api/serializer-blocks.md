# `SerializerBlocks`

序列化器块注册表。每个 `[Template]` 类型在编译期由 SG 生成一个 `ISerializerBlock<T>` 实现并自动注册，提供 scan（反序列化）和 emit（序列化）双向能力。

## 签名

```csharp
public interface ISerializerBlock<TData>
{
    int Scan(ReadOnlySpan<char> text, int pos, out TData value);
    void Emit(StringBuilder sb, TData value);
}

internal static partial class SerializerBlocks
{
    public static bool TryGet<TData>(out ISerializerBlock<TData> block);
    public static string Serialize<TData>(TData value);
    public static TData Deserialize<TData>(string text);
}
```

## TryGet

检查类型 `TData` 是否已通过 `[Template]` 注册了序列化器块。

| 参数 | 类型 | 说明 |
|------|------|------|
| `block` | `out ISerializerBlock<TData>` | 序列化器块，未注册时为 `null` |
| 返回值 | `bool` | 是否成功获取 |

## ISerializerBlock

### Scan

| 参数 | 类型 | 说明 |
|------|------|------|
| `text` | `ReadOnlySpan<char>` | 输入字符 span |
| `pos` | `int` | 起始解析位置 |
| `value` | `out TData` | 解析结果，失败时为 `default` |
| 返回值 | `int` | 解析后的结束位置，`== pos` 表示失败 |

### Emit

| 参数 | 类型 | 说明 |
|------|------|------|
| `sb` | `StringBuilder` | 输出目标 |
| `value` | `TData` | 要序列化的值 |

## 内部实现

SG 为每个类型生成一个 `readonly struct` 并注册到 `SerializerBlocks`：

```csharp
readonly struct Block_Point2D : ISerializerBlock<Point2D>
{
    public int Scan(ReadOnlySpan<char> t, int p, out Point2D v) =>
        SerializerBlocks.Scan_Point2D(t, p, out v);

    public void Emit(StringBuilder sb, Point2D v) =>
        SerializerBlocks.Emit_Point2D(sb, v);
}
```

`BlockRegistry<TData>` 是内部泛型静态字段容器，`TryGet<T>` 读取此字段，零字典查找。

## 参见

- [Template 属性](./template-attribute)
- [ExternalTemplate 属性](./external-template-attribute)
- [SerializerRegistry](./serializer-registry)
