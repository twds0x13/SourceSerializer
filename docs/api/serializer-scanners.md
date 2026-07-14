# `SerializerScanners`

partial class，source generator 注入生成的扫描方法并注册委托。

## 签名

```csharp
internal static partial class SerializerScanners
{
    public static bool TryGetScanner<T>(out ScannerDelegate<T> scanner);
}

public delegate int ScannerDelegate<T>(ReadOnlySpan<char> src, int pos, out T value);
```

## TryGetScanner

检查类型 `T` 是否已通过 `[Template]` 注册了扫描器。

| 参数 | 类型 | 说明 |
|------|------|------|
| `scanner` | `out ScannerDelegate<T>` | 扫描器委托，未注册时为 `null` |
| 返回值 | `bool` | 是否成功获取 |

## ScannerDelegate

source generator 生成的扫描方法签名与内置类型扫描方法一致：

| 参数 | 类型 | 说明 |
|------|------|------|
| `src` | `ReadOnlySpan<char>` | 输入字符 span |
| `pos` | `int` | 起始解析位置 |
| `value` | `out T` | 解析结果，失败时为 `default` |
| 返回值 | `int` | 解析后的结束位置，`== pos` 表示失败 |

## 内部注册机制

source generator 在生成的 `SerializerScanners.g.cs` 中通过静态构造函数注册扫描器：

```csharp
static SerializerScanners()
{
    ScannerRegistry<Point2D>.Scanner = (src, pos, out Point2D v) => {
        int r = Scan_Point2D(src, pos, out v);
        return r;
    };
}
```

`ScannerRegistry<T>` 是内部的泛型静态字段容器，每个 `T` 持有一个 `ScannerDelegate<T>` 实例。
