## [3.4.6](https://github.com/twds0x13/SourceSerializer/compare/v3.4.5...v3.4.6) (2026-07-26)

### Bug Fixes

* repurpose SSR007 to block ExternalTemplate on built-in types ([2dc37cd](https://github.com/twds0x13/SourceSerializer/commit/2dc37cd4acd2acaa853f0a989c7dde6a2fe81022))

## [3.4.5](https://github.com/twds0x13/SourceSerializer/compare/v3.4.4...v3.4.5) (2026-07-26)

### Bug Fixes

* relax SSR007 to require only one side delimiter ([3d52216](https://github.com/twds0x13/SourceSerializer/commit/3d522160564d1313ddaef65cb44b6f0d071bce79))

## [3.4.4](https://github.com/twds0x13/SourceSerializer/compare/v3.4.3...v3.4.4) (2026-07-26)

### Bug Fixes

* downgrade SG to Roslyn 4.1 for Unity 2022.3 compatibility ([adf8a43](https://github.com/twds0x13/SourceSerializer/commit/adf8a436fb140f134ffab35d39003e4cb8f46342))

## [3.4.3](https://github.com/twds0x13/SourceSerializer/compare/v3.4.2...v3.4.3) (2026-07-26)

### Bug Fixes

* remove SourceGenerator~.meta, add Plugins.meta ([7fc8ab6](https://github.com/twds0x13/SourceSerializer/commit/7fc8ab64873d378b46872b953b87a8f3ef42f0c8))

## [3.4.2](https://github.com/twds0x13/SourceSerializer/compare/v3.4.1...v3.4.2) (2026-07-26)

### Bug Fixes

* ship pre-compiled SG DLL for Unity compatibility ([aedcafb](https://github.com/twds0x13/SourceSerializer/commit/aedcafbf3dfec28041fbec8a3d6caac9e8a1f2d4))

## [3.4.1](https://github.com/twds0x13/SourceSerializer/compare/v3.4.0...v3.4.1) (2026-07-26)

### Bug Fixes

* add missing .meta files and asmdef for Unity compatibility ([602c2a8](https://github.com/twds0x13/SourceSerializer/commit/602c2a814cfd317790c1458b4d91e64e6da1960a))

## [3.4.0](https://github.com/twds0x13/SourceSerializer/compare/v3.3.1...v3.4.0) (2026-07-26)

### Features

* indent emission ([fa6af29](https://github.com/twds0x13/SourceSerializer/commit/fa6af29294b7d0271a7d57a18f58d5c64b9d3262))
* whitespace-tolerant parsing ([02dea07](https://github.com/twds0x13/SourceSerializer/commit/02dea070fad26e3492b572808952e9b359ef596e))

## [3.3.1](https://github.com/twds0x13/SourceSerializer/compare/v3.3.0...v3.3.1) (2026-07-25)

### Bug Fixes

* add missing .meta files for BuiltinBlocks, ChainBlock, ISerializerBlock ([e12df46](https://github.com/twds0x13/SourceSerializer/commit/e12df46dc82aa6c1be77c3b0523601f917741a96))

## [3.3.0](https://github.com/twds0x13/SourceSerializer/compare/v3.2.0...v3.3.0) (2026-07-24)

### Features

* SG standalone mode + interface chain merge ([c8746d1](https://github.com/twds0x13/SourceSerializer/commit/c8746d1fc3b00d196b3c8a6414918f0c4e63f1e2))

## [3.2.0](https://github.com/twds0x13/SourceSerializer/compare/v3.1.0...v3.2.0) (2026-07-24)

### Features

* public Runtime API + non-generic AddBlock/RemoveBlock for hot-reload type registration ([431a340](https://github.com/twds0x13/SourceSerializer/commit/431a340be218a2a98bb5d23b2a352a55109017a5))

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
