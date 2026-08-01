# Architecture Decision Records

## ADR-1: Interface-First Default Templates

Migrated default templates from concrete classes (`List<T>`, `Dictionary<K,V>`) to interfaces (`IList<T>`, `ISet<T>`, `IDictionary<K,V>`, etc.). Concrete types matched automatically via Roslyn `AllInterfaces`. Eliminated the `GenericInterfaceAliases` indirection layer.

Priority chain: class-level explicit > interface-level explicit > default interface template.

## ADR-2: SerializerBlocks Unified API

Removed the separate `SerializerScanners` and `SerializerEmitters` classes, unified into `SerializerBlocks`. The `ISerializerBlock<T>` interface provides both `Scan` and `Emit` capabilities. Eliminated two separate registries and delegate types.

## ADR-3: Removed Walk Phase References

Code comments and documentation long referenced a non-existent "managed Walk phase." Collection emit is actually a `foreach` single-pass implementation. Removed all related comments, `NeedsWalkPhase`, and updated documentation.

## ADR-4: Merged Scanner/Emitter Shared Utilities

Extracted `EmitHelpers` static class, unifying method name generation, counter management, and `EmitEntry` field copying. Eliminated duplicated code and manual boilerplate between ScanCodeEmitter and EmitCodeEmitter.

## ADR-5: CollectionKind Rename

`CollectionKind.List` was semantically inaccurate (covering six different contracts: `List`, `ISet`, `IReadOnlyList`, etc.). Renamed to `CollectionKind.Sequential`. Code generation now selects `List<T>` or `HashSet<T>` constructor based on actual field type.

## ADR-6: GeneratedSerializers Standalone Class

All SG-generated code belongs to `public static partial class GeneratedSerializers`, fully decoupled from `SerializerRegistry` (non-partial, pure library code). Three files merge into one class via `partial`, with `public` ensuring cross-assembly visibility. Rejected alternative: writing generated code into a `partial` extension of `SerializerBlocks`, which would cause dictionary sharing issues between the package assembly and user assemblies.

## ADR-7: Reflection-Based Init Discovery

`EnsureInitialized()` reflectively scans all loaded assemblies via AppDomain, automatically discovering and invoking `GeneratedSerializers.Init()`. `Init()` is guarded by `_initCalled` for idempotency. Rejected alternative: an explicit configuration file listing all template-containing assemblies. The reflective scan's startup cost (milliseconds) is acceptable and eliminates the error surface of manually maintained assembly manifests.

## ADR-8: ChainBlock Interface Chain Merge

Interface-type `AddBlock` appends to a `ChainBlock<T>` dispatch chain rather than overwriting. Scan returns on the first link that advances `pos` (first-match-wins); Emit matches via a switch over the first link that produces output. This allows different assemblies (main assembly + hot-reload DLLs) to each generate their own interface dispatch blocks, automatically merged at runtime. Non-interface types retain overwrite semantics.

## ADR-9: Readonly Struct Constructor Matching

Readonly struct fields cannot be assigned field-by-field (C# CS8340). The SG discovers constructors via Roslyn `IMethodSymbol.Parameters`, performing greedy matching by name and type. On match, the SG emits a constructor call; on failure, it reports SSR002. Rejected alternative: requiring a constructor with a specific signature, but name-based matching reduces adoption friction.

## ADR-10: Generic Instance Synthesis

Open generic templates (e.g. `Wrapper<T>`) are auto-synthesized into concrete instances when a field reference appears (e.g. `Wrapper<float>`). Synthesis recurses through nested generics (e.g. `List<Wrapper<float>>`) and multi-parameter types (e.g. `Dictionary<string, int>`). `TryResolveViaInterfaces` provides a Roslyn fallback for BCL types not found in `openGenerics`. Rejected alternative: requiring explicit `[ExternalTemplate]` declarations for every concrete instance.
