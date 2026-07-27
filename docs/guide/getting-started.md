# 快速入门

SourceSerializer 用 attribute 声明结构布局，source generator 在编译期生成零分配 span 扫描器。

## 安装

Unity 项目在 `manifest.json` 中添加：

```json
"com.twds0x13.sourceserializer": "https://github.com/twds0x13/SourceSerializer.git#main"
```

.NET 项目在 `.csproj` 中引用 source generator：

```xml
<ItemGroup>
  <ProjectReference Include="..\SourceSerializer\packages\sourceserializer\SourceGenerator\SourceSerializer.Generator.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

## 声明模板

用 `[Template("...")]` 在 struct 上声明文本格式：

```csharp
using SourceSerializer;

[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D
{
    public float X;
    public float Y;
}
```

## 使用生成的解析器

编译后，source generator 生成 `GeneratedSerializers.Scan_Point2D` 并注册到 `SerializerBlocks`：

```csharp
SerializerBlocks.TryGet<Point2D>(out var scan);
int pos = scan.Scan("Point2D(3.5, -2.1)".AsSpan(), 0, out Point2D v);
// pos > 0, v.X == 3.5f, v.Y == -2.1f
```

## 使用生成的序列化器

编译后，source generator 同时生成 `Emit_Point2D` 方法并注册到 `SerializerBlocks`：

```csharp
SerializerBlocks.TryGet<Point2D>(out var emit);
var sb = new StringBuilder();
emit.Emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
Console.WriteLine(sb.ToString()); // "Point2D(3.5, -2.1)"
```

## 一行式便捷调用

对于简单场景，可以直接使用 `SerializerBlocks` 的三个便捷方法，消除 `TryGet` + `StringBuilder` 样板代码：

```csharp
// 序列化
string s = SerializerBlocks.Serialize(new Point2D { X = 3.5f, Y = -2.1f });

// 反序列化（自动空白符剔除）
Point2D v = SerializerBlocks.Deserialize<Point2D>("Point2D(3.5, -2.1)");

// 非抛出式反序列化
if (SerializerBlocks.TryScan<Point2D>(input, out var result))
    Console.WriteLine(result);
```

输入中的空白符被 `WhitespaceStripper` 自动剔除，以下写法均等价：

```csharp
SerializerBlocks.Deserialize<Point2D>("Point2D(3.5, -2.1)");
SerializerBlocks.Deserialize<Point2D>("  Point2D( 3.5 ,  -2.1 )  ");
```

## 下一步

- [模板语法](./template-syntax): compact 格式、XML 格式、四种原语、嵌套、泛型集合
- [Managed vs Unmanaged](./managed-vs-unmanaged): 双策略选择
- [编译期诊断](./diagnostics): 错误代码参考
- [API 参考](/api/): Template、ExternalTemplate、Tag、TypeAlias 属性
