# Migration Guide

## v3.5.x → Latest

### WhitespaceStripper API change

The `WhitespaceStripper.Strip()` static method has been replaced with a `readonly ref struct` that completes stripping on construction.

```csharp
// v3.5.1 and earlier
string compact = WhitespaceStripper.Strip(text);
block.Scan(compact.AsSpan(), 0, out value);

// v3.5.2 and later
using var compact = new WhitespaceStripper(text);
block.Scan(compact.Span, 0, out value);
```

`Deserialize<T>()` and `TryScan<T>()` have been adapted internally — no manual changes required.

## v3.4.x → v3.5.0

### Built-in type expansion: 13 → 16

Three new built-in types added: `IntPtr`, `UIntPtr`, `Guid`. These have the same zero-allocation span scanners as `int`, `float`, etc., and work directly in templates without `[ExternalTemplate]`.

```csharp
[Template("Handle(<IntPtr Value>)")]
struct Handle { public IntPtr Value; }

[Template("Id(<Guid ID>)")]
struct Id { public Guid ID; }
```

## v3.3.x → v3.4.0

### Roslyn downgrade to 4.1

The SG project's `Microsoft.CodeAnalysis.CSharp` was downgraded from 4.8 to 4.1 for Unity 2022.3 compatibility. Template syntax is unaffected.

### Pre-compiled SG DLL

`packages/sourceserializer/Plugins/SourceSerializer.Generator.dll` is now shipped as a pre-compiled output. Unity projects no longer need to compile the Roslyn analyzer locally — the DLL under `Plugins/` is loaded directly.

### Unity .meta files

All `.cs`, `.csproj`, and `.asmdef` files under `packages/sourceserializer/` now include corresponding `.meta` files. Unity projects installed via npm correctly resolve GUIDs and import settings.

### Whitespace-tolerant parsing

`Deserialize<T>()` and `TryScan<T>()` now automatically preprocess input for whitespace. Templates `"Point2D(3.5, -2.1)"` and `"Point2D( 3.5 , -2.1 )"` are equivalent.

## v1.x → v2.0

### Repetition syntax change

The `<repetition>` tag is no longer a user-facing primitive. Use `<first>/<body>` pairs instead.

```csharp
// v1.x
[Template("Data(<repetition><float Items></repetition>)")]

// v2.0
[Template("Data(<first><float Items></first><body>, <float Items></body>)")]
```

Compact-format templates are auto-converted. XML-format templates must expand the outer `<repetition>` into `<first>` + `<body>` pairs.

### Generic type support

Open generic types marked with `[Template]` are automatically resolved when a concrete instantiation (e.g., `Wrapper<float>`) appears as a field type in another template. Unlimited type parameters are supported and resolved by position.

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

Built-in types: 16 total — float, double, int, uint, long, ulong, short, ushort, byte, sbyte, bool, char, string, IntPtr, UIntPtr, Guid.
