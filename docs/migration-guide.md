# 迁移指南

## v3.5.x → 最新

### WhitespaceStripper API 变更

`WhitespaceStripper.Strip()` 静态方法已替换为 `readonly ref struct`，构造即完成剥离。

```csharp
// v3.5.1 及之前
string compact = WhitespaceStripper.Strip(text);
block.Scan(compact.AsSpan(), 0, out value);

// v3.5.2 及之后
using var compact = new WhitespaceStripper(text);
block.Scan(compact.Span, 0, out value);
```

`Deserialize<T>()` 和 `TryScan<T>()` 内部已自动适配，无需手动修改。

## v3.4.x → v3.5.0

### 内置类型扩展：13 → 16

新增三种内置类型：`IntPtr`、`UIntPtr`、`Guid`。这些类型拥有与 `int`、`float` 等相同的零分配 span 扫描器，无需 `[ExternalTemplate]` 即可直接在模板中使用。

```csharp
[Template("Handle(<IntPtr Value>)")]
struct Handle { public IntPtr Value; }

[Template("Id(<Guid ID>)")]
struct Id { public Guid ID; }
```

## v3.3.x → v3.4.0

### Roslyn 降级至 4.1

SG 项目的 `Microsoft.CodeAnalysis.CSharp` 从 4.8 降级至 4.1，以兼容 Unity 2022.3 内置的 Roslyn 版本。不影响模板语法，但如项目中手动引用了新版 Roslyn API 需降级。

### 预编译 SG DLL

`packages/sourceserializer/Plugins/SourceSerializer.Generator.dll` 作为预编译 SG 输出随包发布。Unity 项目无需在本地编译 Roslyn analyzer——`Plugins/` 下的 DLL 直接被 Unity 加载。

### Unity .meta 文件

`packages/sourceserializer/` 目录下所有 `.cs`、`.csproj`、`.asmdef` 现均包含对应的 `.meta` 文件。从 npm 安装的 Unity 项目可正确识别 GUID 和导入设置。

### 空白符容错解析

`Deserialize<T>()` 和 `TryScan<T>()` 现自动对输入做空白符预处理。模板 `"Point2D(3.5, -2.1)"` 和 `"Point2D( 3.5 , -2.1 )"` 等价。

## v1.x → v2.0

### Repetition 语法变更

`<repetition>` 标签不再是用户面原语，改为 `<first>/<body>` 组合。

```csharp
// v1.x
[Template("Data(<repetition><float Items></repetition>)")]

// v2.0
[Template("Data(<first><float Items></first><body>, <float Items></body>)")]
```

compact 格式模板自动转换，无需手动修改。XML 格式需将外层 `<repetition>` 展开为 `<first>` + `<body>` 对。

### 泛型类型支持

任意开放泛型类型标记 `[Template]` 后，特化实例（如 `Wrapper<float>`）在其他模板的字段类型中自动解析。支持无限类型参数，按位置解析。

### SerializerScanners / SerializerEmitters 已删除

`SerializerScanners` 和 `SerializerEmitters` 类已移除，统一为 `SerializerBlocks`。

```csharp
// v2.x
SerializerScanners.TryGetScanner<T>(out var scan);
scan(text, pos, out var value);

SerializerEmitters.TryGetEmitter<T>(out var emit);
emit(sb, value);

// v3.0
SerializerBlocks.TryGet<T>(out var block);
block.Scan(text, pos, out value);
block.Emit(sb, value);
```

### 委托类型已删除

`ScannerDelegate<T>` 和 `EmitterDelegate<T>` 不再存在。替代为 `ISerializerBlock<T>` 接口。

### 静态构造器注册已移除

注册逻辑从 `SerializerScanners.g.cs` 和 `SerializerEmitters.g.cs` 集中到 `SerializerBlocks.g.cs` 的 `Init()` 方法。三个 `.g.cs` 文件均仍在生成，分别贡献 `Scan_Xxx`、`Emit_Xxx`、`Init + Block_Xxx` 到 `GeneratedSerializers` 类。

### 内置类型计数

内置类型共 16 种：`float`、`double`、`int`、`uint`、`long`、`ulong`、`short`、`ushort`、`byte`、`sbyte`、`bool`、`char`、`string`、`IntPtr`、`UIntPtr`、`Guid`。

## 参见

- [快速入门](/guide/getting-started)：最新版本的安装与首次使用
- [常见问题](/faq)：常见使用问题
- [SerializerBlocks API](/api/serializer-blocks)：最新的运行时 API
