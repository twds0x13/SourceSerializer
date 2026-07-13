# SourceSerializer

用 attribute 声明 Schema，source generator 在编译期生成专用解析器。零反射，零装箱。

## 与 JSON 的区别

JSON 在运行时通过反射发现类型结构。SourceSerializer 在编译期完成这项工作。

| | JSON | SourceSerializer |
|------|------|------|
| Schema 定义 | 运行时反射 | 编译期 attribute |
| 解析器生成 | 运行时 | 编译期 |
| 内存分配 | 堆分配 + 装箱 | `stackalloc`，零 GC |
| 类型安全 | `object` 中转 | 强类型 |
| 循环引用 | `$ref` 补丁 | 两步走，原生支持 |

## 快速开始

```csharp
[Template("<float x> <float y>")]
public struct Vec2
{
    public float x;
    public float y;
}

// 编译期生成 Scan_Vec2 方法
SerializerScanners.TryGetScanner<Vec2>(out var scan);
scan("3.5 -2.1".AsSpan(), 0, out Vec2 v);
// v.x == 3.5f, v.y == -2.1f
```
