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

[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}
```

## 使用生成的解析器

编译后，source generator 生成 `Scan_Point2D` 方法并注册到 `SerializerScanners`：

```csharp
SerializerScanners.TryGetScanner<Point2D>(out var scan);
int pos = scan("3.5 -2.1".AsSpan(), 0, out Point2D v);
// pos > 0, v.X == 3.5f, v.Y == -2.1f
```

## 使用生成的序列化器

编译后，source generator 同时生成 `Emit_Point2D` 方法并注册到 `SerializerEmitters`：

```csharp
SerializerEmitters.TryGetEmitter<Point2D>(out var emit);
var sb = new StringBuilder();
emit(sb, new Point2D { X = 3.5f, Y = -2.1f });
Console.WriteLine(sb.ToString()); // "3.5 -2.1"
```

## 下一步

- [模板语法](./template-syntax): compact 格式、XML 格式、四种原语、嵌套、泛型集合
- [Managed vs Unmanaged](./managed-vs-unmanaged): 双策略选择
- [编译期诊断](./diagnostics): 错误代码参考
- [API 参考](/api/): Template、ExternalTemplate、Tag、TypeAlias 属性
