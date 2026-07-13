# 快速入门

用 attribute 声明结构，source generator 在编译期生成零分配解析器。

## 安装

在 `manifest.json` 中添加：

```json
"com.twds0x13.sourceserializer": "https://github.com/twds0x13/SourceSerializer.git#main"
```

## 声明第一个模板

```csharp
using SourceSerializer;

[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}
```

## 使用生成的解析器

```csharp
SerializerScanners.TryGetScanner<Point2D>(out var scan);
scan("3.5 -2.1".AsSpan(), 0, out Point2D v);
// v.X == 3.5f, v.Y == -2.1f
```

## 下一步

- [模板语法](./template-syntax) — compact 格式、XML 格式、四种原语
- [Managed vs Unmanaged](./managed-vs-unmanaged) — 双策略选择指南
