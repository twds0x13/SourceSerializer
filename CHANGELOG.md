## [3.1.0](https://github.com/twds0x13/SourceSerializer/compare/v3.0.3...v3.1.0) (2026-07-24)

### Features

* array emit, collection List() format, string always-quoted, T[] synthesis ([23fbdf5](https://github.com/twds0x13/SourceSerializer/commit/23fbdf50c4083493314c953acbe25344aec05dd3))

## [3.0.3](https://github.com/twds0x13/SourceSerializer/compare/v3.0.2...v3.0.3) (2026-07-23)

### Bug Fixes

* make SerializerBlocks class and TryGet method public ([aada471](https://github.com/twds0x13/SourceSerializer/commit/aada471cb89f707feb28045c2f1b16ca96696371))

## [3.0.2](https://github.com/twds0x13/SourceSerializer/compare/v3.0.1...v3.0.2) (2026-07-23)

### Bug Fixes

* add .meta files and correct README installation URL ([3a9304d](https://github.com/twds0x13/SourceSerializer/commit/3a9304dc165adbd4d9a1149ae25164bbff82d9fc))

## [3.0.1](https://github.com/twds0x13/SourceSerializer/compare/v3.0.0...v3.0.1) (2026-07-18)

### Bug Fixes

* internal refactor for performance and maintainability ([659bade](https://github.com/twds0x13/SourceSerializer/commit/659bade42faf78e892a2f2c643fc2b4a8fb6799f))

## [3.0.0](https://github.com/twds0x13/SourceSerializer/compare/v2.1.0...v3.0.0) (2026-07-18)

### ⚠ BREAKING CHANGES

* SerializerScanners and SerializerEmitters removed.
SerializerBlocks is now the single API for scan+emit.

- Default templates: interfaces (IList, ISet, IReadOnlyList, IDictionary,
  IReadOnlyDictionary), resolved via Roslyn AllInterfaces.
- Delete GenericInterfaceAliases, TypeKind field, Scanner/Emitter delegates
  and registries. Merge all generation into SerializerBlocks partial class.
- Add TryResolveViaInterfaces for Roslyn-based BCL type resolution.
- Unify BuiltinTypes into BuiltinTypeNames; add decimal/nint/nuint/Half.
- Rename CollectionKind.List to Sequential; unify ClassifyFieldType/ByName.
- Fix string.Replace parameter safety (length-descending).
- Fix C# keyword field names with @ prefix.
- Implement collection emit (foreach + first/body) replacing stub.
- Add SerializerBlocks: ISerializerBlock<T> + per-type struct + Serialize/
  Deserialize convenience methods.

### Features

* interface-first templates, SerializerBlocks, architecture cleanup ([31a72fd](https://github.com/twds0x13/SourceSerializer/commit/31a72fdfefd757726e2f5b3720ff3dedce0c54a8))

## [2.1.0](https://github.com/twds0x13/SourceSerializer/compare/v2.0.0...v2.1.0) (2026-07-15)

### Features

* automatic interface dispatch via Roslyn Interface->Implementation resolution ([f4ac67a](https://github.com/twds0x13/SourceSerializer/commit/f4ac67a5ba146fe94d00994abbce23fc65bb928e))

## [2.0.0](https://github.com/twds0x13/SourceSerializer/compare/v1.2.2...v2.0.0) (2026-07-15)

### ⚠ BREAKING CHANGES

* <repetition> in compact template syntax is now
automatically converted to equivalent <first>/<body> pairs. The
<repetition> tag is no longer a user-facing primitive — use
<first>+<body> directly instead. Template authors with existing
<repetition> usage should migrate to <first>/<body>; compact
syntax templates are auto-converted and continue to compile.

Features:
- Any type with [Template] that is an open generic is automatically
  resolved when a concrete instance (e.g. Wrapper<float>) appears
  as a field type in another template
- Supports unlimited type parameters (T, TKey, TValue, or any
  user-defined names), resolved by position
- Synthesized generic instances inherit NeedsHeapAlloc/NeedsWalkPhase
  from their open generic definition
- Recursive discovery handles nested generics (List<Wrapper<float>>)
- Arity-suffixed StructName (Pair^2) avoids conflicts with concrete
  types of the same name
- Comma characters in generic type parameter lists are escaped in
  generated method names (Pair<float,int> → Scan_Pair_float_int)

### Features

* support user-defined generic types and remove <repetition> from public API ([a84af5c](https://github.com/twds0x13/SourceSerializer/commit/a84af5cf30d3d0669c50f8ae30a0185bfdfc3966))

## [1.2.2](https://github.com/twds0x13/SourceSerializer/compare/v1.2.1...v1.2.2) (2026-07-14)

### Bug Fixes

* **docs:** upgrade mermaid from 10.9.6 to 11.15.0 ([c0774cc](https://github.com/twds0x13/SourceSerializer/commit/c0774cc28b1b33b316e16303719d64a293fca0e4))

## [1.2.1](https://github.com/twds0x13/SourceSerializer/compare/v1.2.0...v1.2.1) (2026-07-14)

### Bug Fixes

* **docs:** logo, sidebar labels, Mermaid parity, and API navigation ([0a0871a](https://github.com/twds0x13/SourceSerializer/commit/0a0871a1a61f27abe8db0a5e9d461a5f938215d2))

## [1.2.0](https://github.com/twds0x13/SourceSerializer/compare/v1.1.0...v1.2.0) (2026-07-14)

### Features

* add [TemplateIgnore] attribute and upgrade SSR004 to Error ([2e2c400](https://github.com/twds0x13/SourceSerializer/commit/2e2c4003e018911132784c4e681af6b0c2088765))

## [1.1.0](https://github.com/twds0x13/SourceSerializer/compare/v1.0.1...v1.1.0) (2026-07-14)

### Features

* add serialization direction, class support, and generic collection auto-resolution ([2f8933b](https://github.com/twds0x13/SourceSerializer/commit/2f8933bb0fcfca382d67b5400377eeb77ceffc3c))

## [1.0.1](https://github.com/twds0x13/SourceSerializer/compare/v1.0.0...v1.0.1) (2026-07-13)

### Bug Fixes

* **docs:** use array sidebar like FluxFormula — show all entries on every page ([9ddf22a](https://github.com/twds0x13/SourceSerializer/commit/9ddf22a5010171427dd57a8bdd497dda5c3a0fce))

## 1.0.0 (2026-07-13)

### Features

* initial release — compile-time serializer generator ([ad759db](https://github.com/twds0x13/SourceSerializer/commit/ad759db64fe771df1988006ce07fd43e4c9eff50))
