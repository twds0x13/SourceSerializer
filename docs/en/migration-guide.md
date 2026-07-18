# Migration Guide

## v2.x → v3.0

### SerializerScanners / SerializerEmitters Removed

The `SerializerScanners` and `SerializerEmitters` classes have been removed, unified into `SerializerBlocks`.

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

### Delegate Types Removed

`ScannerDelegate<T>` and `EmitterDelegate<T>` no longer exist. Replaced by the `ISerializerBlock<T>` interface.

### Static Constructor Registration Removed

Generated `SerializerScanners.g.cs` and `SerializerEmitters.g.cs` no longer contain registration code. All registration is unified in `SerializerBlocks.g.cs`.

### Built-in Type Count

Built-in types increased from 12 to 17: added `decimal`, `nint`, `nuint`, `Half`, `string`.
