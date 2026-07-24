using System;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// 接口链式合并 + standalone 模式 — 测试类型定义
// ═══════════════════════════════════════════════════════

/// <summary>武器接口 — 模拟跨程序集的接口分发。</summary>
public interface IWeapon { }

public struct Sword : IWeapon
{
    public float Atk;
    public override string ToString() => $"Sword({Atk})";
}

public struct Bow : IWeapon
{
    public float Range;
    public override string ToString() => $"Bow({Range})";
}

/// <summary>第三个武器类型 — 模拟热更 DLL 第三次注册的追加链节。</summary>
public struct Staff : IWeapon
{
    public int Mana;
    public override string ToString() => $"Staff({Mana})";
}

// ── 手写 Block：模拟不同程序集各自 SG 生成的接口分发块 ──

/// <summary>模拟服务端 SG 生成的 IWeapon 分发块 — 只认识 Sword。</summary>
readonly struct Block_IWeapon_Sword : ISerializerBlock<IWeapon>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out IWeapon value)
    {
        value = default!;
        if (pos + 6 > text.Length) return pos;
        int start = pos;

        // "Sword("
        if (!text.Slice(pos, 6).SequenceEqual("Sword(".AsSpan())) return pos;
        pos += 6;

        int pre = pos;
        pos = SerializerRegistry.Scan_Float(text, pos, out float atk);
        if (pos == pre) return start;

        if (pos >= text.Length || text[pos] != ')') return start;
        pos++;

        value = new Sword { Atk = atk };
        return pos;
    }

    public void Emit(StringBuilder sb, IWeapon value)
    {
        if (value is Sword s)
        {
            sb.Append("Sword(");
            SerializerRegistry.Emit_Float(sb, s.Atk);
            sb.Append(')');
        }
    }
}

/// <summary>模拟热更 DLL SG 生成的 IWeapon 分发块 — 只认识 Bow。</summary>
readonly struct Block_IWeapon_Bow : ISerializerBlock<IWeapon>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out IWeapon value)
    {
        value = default!;
        if (pos + 4 > text.Length) return pos;
        int start = pos;

        // "Bow("
        if (!text.Slice(pos, 4).SequenceEqual("Bow(".AsSpan())) return pos;
        pos += 4;

        int pre = pos;
        pos = SerializerRegistry.Scan_Float(text, pos, out float range);
        if (pos == pre) return start;

        if (pos >= text.Length || text[pos] != ')') return start;
        pos++;

        value = new Bow { Range = range };
        return pos;
    }

    public void Emit(StringBuilder sb, IWeapon value)
    {
        if (value is Bow b)
        {
            sb.Append("Bow(");
            SerializerRegistry.Emit_Float(sb, b.Range);
            sb.Append(')');
        }
    }
}

/// <summary>模拟第二个热更 DLL SG 生成的 IWeapon 分发块 — 只认识 Staff。</summary>
readonly struct Block_IWeapon_Staff : ISerializerBlock<IWeapon>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out IWeapon value)
    {
        value = default!;
        if (pos + 6 > text.Length) return pos;
        int start = pos;

        // "Staff("
        if (!text.Slice(pos, 6).SequenceEqual("Staff(".AsSpan())) return pos;
        pos += 6;

        int pre = pos;
        pos = SerializerRegistry.Scan_Int(text, pos, out int mana);
        if (pos == pre) return start;

        if (pos >= text.Length || text[pos] != ')') return start;
        pos++;

        value = new Staff { Mana = mana };
        return pos;
    }

    public void Emit(StringBuilder sb, IWeapon value)
    {
        if (value is Staff s)
        {
            sb.Append("Staff(");
            SerializerRegistry.Emit_Int(sb, s.Mana);
            sb.Append(')');
        }
    }
}

// ═══════════════════════════════════════════════════════
// 接口链式合并 — 测试
// ═══════════════════════════════════════════════════════

public class ChainBlockTests
{
    [SetUp]
    public void SetUp()
    {
        SerializerBlocks.RemoveBlock(typeof(IWeapon));
    }

    [TearDown]
    public void TearDown()
    {
        SerializerBlocks.RemoveBlock(typeof(IWeapon));
    }

    // ── RegisterBlock: 接口首次注册（直接存入） ──

    [Test]
    public void Interface_FirstRegistration_StoredDirectly()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);
        Assert.That(block, Is.InstanceOf<Block_IWeapon_Sword>());
    }

    [Test]
    public void Interface_FirstRegistration_ScanWorks()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);
        int r = block.Scan("Sword(100)".AsSpan(), 0, out IWeapon v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<Sword>());
        Assert.That(((Sword)v).Atk, Is.EqualTo(100f).Within(1e-5f));
    }

    // ── RegisterBlock: 接口第二次注册（创建 ChainBlock） ──

    [Test]
    public void Interface_SecondRegistration_CreatesChain()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);
        // 第二次注册接口类型 → RegisterBlock 创建 ChainBlock
        Assert.That(block, Is.InstanceOf<ChainBlock<IWeapon>>());
    }

    [Test]
    public void Interface_SecondRegistration_ChainScansBoth()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);

        // 链节 0 匹配 Sword
        int r1 = block.Scan("Sword(100)".AsSpan(), 0, out IWeapon v1);
        Assert.That(r1, Is.GreaterThan(0));
        Assert.That(v1, Is.InstanceOf<Sword>());

        // 链节 0 不匹配 → 链节 1 匹配 Bow
        int r2 = block.Scan("Bow(50)".AsSpan(), 0, out IWeapon v2);
        Assert.That(r2, Is.GreaterThan(0));
        Assert.That(v2, Is.InstanceOf<Bow>());
        Assert.That(((Bow)v2).Range, Is.EqualTo(50f).Within(1e-5f));
    }

    [Test]
    public void Interface_SecondRegistration_ChainEmitsBoth()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);

        // Emit Sword: link0 命中
        var sb1 = new StringBuilder();
        block.Emit(sb1, new Sword { Atk = 100f });
        Assert.That(sb1.ToString(), Is.EqualTo("Sword(100)"));

        // Emit Bow: link0 不匹配 → link1 命中
        var sb2 = new StringBuilder();
        block.Emit(sb2, new Bow { Range = 50f });
        Assert.That(sb2.ToString(), Is.EqualTo("Bow(50)"));
    }

    // ── RegisterBlock: 接口第三次注册（追加到已有 ChainBlock） ──

    [Test]
    public void Interface_ThirdRegistration_AppendsToChain()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Staff());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);

        // Sword 仍在链中
        int r1 = block.Scan("Sword(100)".AsSpan(), 0, out IWeapon v1);
        Assert.That(r1, Is.GreaterThan(0));
        Assert.That(v1, Is.InstanceOf<Sword>());

        // Bow 仍在链中
        int r2 = block.Scan("Bow(50)".AsSpan(), 0, out IWeapon v2);
        Assert.That(r2, Is.GreaterThan(0));
        Assert.That(v2, Is.InstanceOf<Bow>());

        // 新增 Staff 可用
        int r3 = block.Scan("Staff(42)".AsSpan(), 0, out IWeapon v3);
        Assert.That(r3, Is.GreaterThan(0));
        Assert.That(v3, Is.InstanceOf<Staff>());
        Assert.That(((Staff)v3).Mana, Is.EqualTo(42));
    }

    // ── ChainBlock 失败路径 ──

    [Test]
    public void Interface_Chain_NoLinkMatches_ReturnsStart()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);
        int r = block.Scan("NotAWeapon".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Interface_Chain_Emit_NoLinkMatches_ProducesNothing()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);

        // Craft a value that is IWeapon but neither Sword nor Bow
        var unknown = new Staff { Mana = 99 };
        var sb = new StringBuilder();
        block.Emit(sb, unknown);
        Assert.That(sb.Length, Is.EqualTo(0));
    }

    // ── 非泛型 AddBlock(Type, ISerializerBlock) 接口链合并 ──

    [Test]
    public void NonGeneric_Interface_CreatesChain()
    {
        SerializerBlocks.AddBlock(typeof(IWeapon), new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock(typeof(IWeapon), new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out var block), Is.True);
        Assert.That(block, Is.InstanceOf<ChainBlock<IWeapon>>());

        int r = block.Scan("Bow(50)".AsSpan(), 0, out IWeapon v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<Bow>());
    }

    // ── RemoveBlock: 接口类型移除整条链 ──

    [Test]
    public void RemoveBlock_Interface_RemovesEntireChain()
    {
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Sword());
        SerializerBlocks.AddBlock<IWeapon>(new Block_IWeapon_Bow());

        Assert.That(SerializerBlocks.TryGet<IWeapon>(out _), Is.True);

        SerializerBlocks.RemoveBlock<IWeapon>();
        Assert.That(SerializerBlocks.TryGet<IWeapon>(out _), Is.False);
    }

    // ── 非接口类型覆盖语义不变 ──
    // 已在 HotReloadTests 中充分覆盖（AddBlock_NonGeneric_OverwritesExisting、
    // VersionManagement_OldAndNewCoexist 等测试）。此处仅做接口链合并的覆盖。

    // ── Init() 幂等性 ──

    [Test]
    public void Init_Idempotent_DoubleCall()
    {
        // GeneratedSerializers.Init() 已在 EnsureInitialized 中调用过一次。
        // 手动再调用一次不应出错或重复注册。
        Assert.That(() => GeneratedSerializers.Init(), Throws.Nothing);

        // 所有 SG 类型仍然可用
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out _), Is.True);
        Assert.That(SerializerBlocks.TryGet<IVector>(out _), Is.True);
        Assert.That(SerializerBlocks.TryGet<Vec2>(out _), Is.True);
    }
}
