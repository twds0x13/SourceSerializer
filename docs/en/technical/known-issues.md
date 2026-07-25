# Known Issues

## Variable scope issue with class types

`[Template]` on `class` types has a variable scoping issue. For class types such as `NamedPoint`, the SG-generated `Scan` method may reference non-existent variables (e.g. `_Y_42`).

Root cause: When `CodeEmitter` processes the `NeedsHeapAlloc` path (class types), the variable declaration and scoping strategy differs from the struct path. The code generation logic for this path has not yet been fully investigated.

Scope: `class` types only. `struct` types (including `readonly struct`) are unaffected.

Workarounds:
- Use a struct wrapper: extract the class's serializable fields into a struct, then convert between the class and struct
- Hand-write an `ISerializerBlock<T>` implementation: bypass SG generation and implement `Scan` and `Emit` directly. See [Hot Reload & Cross-Assembly Registration](/en/guide/hot-reload) for a hand-written example
