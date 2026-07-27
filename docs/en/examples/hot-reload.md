# Example: Hand-Written Serializers & Hot-Reload Registration

SG-generated `ISerializerBlock<T>` is the standard path. For types without `[Template]` (third-party libraries, custom formats), hand-written implementations provide equivalent registration and querying.

## Hand-Written ISerializerBlock\<T\>

```csharp
public struct HotSword
{
    public float Atk;
    public float Crit;
}

// Format: Sword(100, 0.15)
public readonly struct Block_HotSword : ISerializerBlock<HotSword>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out HotSword value)
    {
        value = default;
        int start = pos;

        if (!text.Slice(pos, 6).SequenceEqual("Sword(".AsSpan())) return pos;
        pos += 6;

        pos = SerializerRegistry.Scan_Float(text, pos, out float atk);
        value.Atk = atk;

        if (text[pos] != ',' || text[pos + 1] != ' ') return start;
        pos += 2;

        pos = SerializerRegistry.Scan_Float(text, pos, out float crit);
        value.Crit = crit;

        if (text[pos] != ')') return start;
        return pos + 1;
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

Hand-written blocks can call all 16 built-in `public static` Scan/Emit methods on `SerializerRegistry`, as well as SG-generated `Scan_Xxx`/`Emit_Xxx` methods on `GeneratedSerializers`.

## Registration and Usage

```csharp
// Register (generic overload)
SerializerBlocks.AddBlock<HotSword>(new Block_HotSword());

// Register (non-generic overload, for hot-reload DLL reflective invocation)
SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());

// Retrieve and use
SerializerBlocks.TryGet<HotSword>(out var block);
block.Scan("Sword(100, 0.15)".AsSpan(), 0, out var v);
// v.Atk == 100f, v.Crit == 0.15f

var sb = new StringBuilder();
block.Emit(sb, v);
// sb.ToString() == "Sword(100, 0.15)"
```

## Hybrid Registration: SG + Hand-Written

```csharp
// Path A: SG-generated Init() registers all [Template] types
GeneratedSerializers.Init();

// Path B: supplement with hand-written blocks
SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield());

// Path C: extend interface dispatch chains
// AddBlock<IModifier> on an interface type uses ChainBlock append, not overwrite
```

## Interface Extension (Chain Merge)

A hot-reload DLL adds a new implementation to an existing interface:

```csharp
// Server at compile time: SG generates Block_IDamage{Strike, Spell, DoT}
// After hot-reload DLL loads: SG generates Block_IDamage{Reflect}
DLL.GeneratedSerializers.Init();
// typeof(IDamage).IsInterface → chain merge
// ChainBlock{ Block_IDamage{Strike,Spell,DoT}, Block_IDamage{Reflect} }
// "Reflect(50, 0.3)" → try first three → no match → try Reflect → succeeds
```

See `HotReloadTests.cs` for complete runnable test cases.

## See Also

- [Hot Reload & Cross-Assembly Registration](../guide/hot-reload): concept walkthrough and full scenario
- [SerializerBlocks API](../api/serializer-blocks): AddBlock, RemoveBlock, chain merge API signatures
- [SerializerRegistry API](../api/serializer-registry): Scan/Emit methods for 16 built-in types
