# 迁移指南

## v2.x → v3.0

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

内置类型共 13 种：`float`、`double`、`int`、`uint`、`long`、`ulong`、`short`、`ushort`、`byte`、`sbyte`、`bool`、`char`、`string`。
