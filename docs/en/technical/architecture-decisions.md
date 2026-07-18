# Architecture Decision Records

## ADR-1: Interface-First Default Templates

Migrated default templates from concrete classes (`List<T>`, `Dictionary<K,V>`) to interfaces (`IList<T>`, `ISet<T>`, `IDictionary<K,V>`, etc.). Concrete types matched automatically via Roslyn `AllInterfaces`. Eliminated the `GenericInterfaceAliases` indirection layer.

Priority chain: class-level explicit > interface-level explicit > default interface template.

## ADR-2: SerializerBlocks Unified API

Removed the separate `SerializerScanners` and `SerializerEmitters` classes, unified into `SerializerBlocks`. The `ISerializerBlock<T>` interface provides both `Scan` and `Emit` capabilities. Eliminated two separate registries and delegate types.

## ADR-3: Removed Walk Phase References

Code comments and documentation long referenced a non-existent "managed Walk phase." Collection emit is actually a `foreach` single-pass implementation. Removed all related comments, `NeedsWalkPhase`, and updated documentation.

## ADR-4: Merged Scanner/Emitter Shared Utilities

Extracted `EmitHelpers` static class, unifying method name generation, counter management, and `EmitEntry` field copying. Eliminated duplicated code and manual boilerplate between CodeEmitter and EmitCodeEmitter.

## ADR-5: CollectionKind Rename

`CollectionKind.List` was semantically inaccurate (covering six different contracts: `List`, `ISet`, `IReadOnlyList`, etc.). Renamed to `CollectionKind.Sequential`. Code generation now selects `List<T>` or `HashSet<T>` constructor based on actual field type.
