# Hot Reload and Cross-Assembly Registration

A core design of SourceSerializer is **compile-time generation and runtime registration sharing the same code path**. The SG-generated `GeneratedSerializers.Init()` can be called both at main assembly startup and when a hot-reload DLL loads — both paths use identical registration logic.

## Scenario

A typical business scenario:

```
1. Server v3.4 has been running for years with StrikeDamage, SpellDamage,
   DotDamage — all implementing IDamage. SG-generated Block_IDamage
   dispatches all three types.
2. A new expansion requires ReflectDamage — also IDamage, with an extra
   reflection ratio field.
3. Can't wait for the next client major version (3 months away).
   Hot-reload DLL is the answer.
```

## Hand-Writing ISerializerBlock\<T\>

New types in a hot-reload DLL **rely on SG generation** — the DLL's own compilation triggers the SG to produce its own `GeneratedSerializers`. But if the DLL's types lack `[Template]` (e.g. third-party types or custom formats), you hand-write the implementation.

The recommended format mirrors the docs' template style — `TypeName(arg1, arg2)`:

```csharp
// Type definition
public struct HotSword
{
    public float Atk;
    public float Crit;
}

// Hand-written serializer block — Sword(100, 0.15)
public readonly struct Block_HotSword : ISerializerBlock<HotSword>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out HotSword value)
    {
        value = default;
        if (pos + 6 > text.Length) return pos;
        int start = pos;

        // "Sword("
        if (!text.Slice(pos, 6).SequenceEqual("Sword(".AsSpan())) return pos;
        pos += 6;

        int pre = pos;
        pos = SerializerRegistry.Scan_Float(text, pos, out float atk);
        if (pos == pre) return start;
        value.Atk = atk;

        // ", "
        if (pos + 1 >= text.Length || text[pos] != ',' || text[pos + 1] != ' ') return start;
        pos += 2;

        pos = SerializerRegistry.Scan_Float(text, pos, out float crit);
        if (pos == pre) return start;
        value.Crit = crit;

        // ")"
        if (pos >= text.Length || text[pos] != ')') return start;
        pos++;

        return pos;
    }

    public void Emit(StringBuilder sb, HotSword value)
    {
        sb.Append("Sword(");
        SerializerRegistry.Emit_Float(sb, value.Atk);
        sb.Append(", ");
        SerializerRegistry.Emit_Float(sb, value.Crit);
        sb.Append(')');
    }
}
```

Hand-written blocks can call all 13 built-in types' public static Scan/Emit methods on `SerializerRegistry`, plus any `GeneratedSerializers.Scan_Xxx/Emit_Xxx` methods for types in the same assembly.

## Registration and Initialization

The DLL's initialization entry point:

```csharp
// Path A: DLL has [Template] types, SG already generated GeneratedSerializers
GeneratedSerializers.Init();
// → registers Block_Xxx for all types in the DLL

// Path B: hand-written blocks, register one by one
SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield());

// Path C: hybrid — Init() first for SG-generated, then AddBlock for hand-written
```

The main assembly's `EnsureInitialized()` scans all loaded assemblies via AppDomain reflection on first `TryGet<T>`. But the hot-reload DLL's `Init()` must be called **explicitly** after loading — it wasn't present during that initial scan.

`Init()` is idempotent — the `_initCalled` guard field makes subsequent calls no-ops.

## Interface Extension (Chain Merge)

This is the critical mechanism for hot reload. When a new type implements an existing interface, `AddBlock` appends to the dispatch chain:

```csharp
// === Server (compile-time) ===
// SG-generated Init() registered:
//   AddBlock<IDamage>(Block_IDamage{StrikeDamage, SpellDamage, DotDamage})
// → _blocks[IDamage] = Block_IDamage{Strike, Spell, DoT}

// === Hot-reload DLL loaded ===
// DLL's Init() registered:
//   AddBlock<IDamage>(Block_IDamage{ReflectDamage})
// → typeof(IDamage).IsInterface → RegisterBlock chain-merges
// → _blocks[IDamage] = ChainBlock{ Block_IDamage{Strike,Spell,DoT}, Block_IDamage{Reflect} }

// Deserializing "Reflect(50, 0.3, Spell(200, Fire))":
// ChainBlock.Scan → link0 tries Strike/Spell/DoT → no match
//                 → link1 tries Reflect → success!
```

For non-interface types, `AddBlock` retains overwrite semantics — later registration replaces earlier. Only interface types use the chain merge path.

## Removal and Version Management

```csharp
// Retire an old type
SerializerBlocks.RemoveBlock(typeof(HotSword));
// Interface removal removes the entire chain
SerializerBlocks.RemoveBlock<IDamage>();
```

`RemoveBlock` removes the key from the dictionary. For interface types, this removes the entire `ChainBlock` — all links are removed together.

## See Also

- [ChainBlock Internals](../technical/internals#interface-chain-merge-chainblock-t)
- [SerializerBlocks API](../api/serializer-blocks)
- [HotReloadTests](https://github.com/twds0x13/SourceSerializer/blob/main/tests/SourceSerializer.Tests/HotReloadTests.cs) — complete runnable test cases
