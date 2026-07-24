using System;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

/// <summary>
/// 模拟热更新场景：手动实现 ISerializerBlock&lt;T&gt; + 通过非泛型 AddBlock(Type, block) 注册。
/// 不依赖 SG 生成代码——完全模拟 Assembly.Load 后的注册流程。
/// </summary>

// 测试用类型：模拟热更 DLL 中的 struct 定义
public struct HotSword
{
    public float Atk;
    public float Crit;

    public override readonly string ToString() => $"Sword({Atk}, {Crit})";
}

public struct HotShield
{
    public float Def;
    public float Weight;

    public override readonly string ToString() => $"Shield({Def}, {Weight})";
}

// 手动实现的 ISerializerBlock<T>：模拟热更 DLL 中的序列化器
public readonly struct Block_HotSword : ISerializerBlock<HotSword>
{
    // 格式: Sword(100, 0.15)
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

public readonly struct Block_HotShield : ISerializerBlock<HotShield>
{
    // 格式: Shield(50, 8)
    public int Scan(ReadOnlySpan<char> text, int pos, out HotShield value)
    {
        value = default;
        if (pos + 7 > text.Length) return pos;
        int start = pos;

        // "Shield("
        if (!text.Slice(pos, 7).SequenceEqual("Shield(".AsSpan())) return pos;
        pos += 7;

        int pre = pos;
        pos = SerializerRegistry.Scan_Float(text, pos, out float def);
        if (pos == pre) return start;
        value.Def = def;

        // ", "
        if (pos + 1 >= text.Length || text[pos] != ',' || text[pos + 1] != ' ') return start;
        pos += 2;

        pos = SerializerRegistry.Scan_Float(text, pos, out float weight);
        if (pos == pre) return start;
        value.Weight = weight;

        // ")"
        if (pos >= text.Length || text[pos] != ')') return start;
        pos++;

        return pos;
    }

    public void Emit(StringBuilder sb, HotShield value)
    {
        sb.Append("Shield(");
        SerializerRegistry.Emit_Float(sb, value.Def);
        sb.Append(", ");
        SerializerRegistry.Emit_Float(sb, value.Weight);
        sb.Append(')');
    }
}

public class HotReloadTests
{
    [SetUp]
    public void SetUp()
    {
        // 确保测试间隔离：移除可能残留的注册
        SerializerBlocks.RemoveBlock(typeof(HotSword));
        SerializerBlocks.RemoveBlock(typeof(HotShield));
    }

    [TearDown]
    public void TearDown()
    {
        SerializerBlocks.RemoveBlock(typeof(HotSword));
        SerializerBlocks.RemoveBlock(typeof(HotShield));
    }

    // ═══════════════════════════════════════════════════════
    // 非泛型 AddBlock(Type, ISerializerBlock)
    // ═══════════════════════════════════════════════════════

    [Test]
    public void AddBlock_NonGeneric_RegistersType()
    {
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
        Assert.That(SerializerBlocks.TryGet<HotSword>(out var block), Is.True);
        Assert.That(block, Is.Not.Null);
    }

    [Test]
    public void AddBlock_NonGeneric_Roundtrip()
    {
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());

        var original = new HotSword { Atk = 100f, Crit = 0.15f };
        var serialized = SerializerBlocks.Serialize(original);

        var deserialized = SerializerBlocks.Deserialize<HotSword>(serialized);
        Assert.That(deserialized.Atk, Is.EqualTo(100f).Within(1e-5f));
        Assert.That(deserialized.Crit, Is.EqualTo(0.15f).Within(1e-5f));
    }

    [Test]
    public void AddBlock_NonGeneric_TwoTypes()
    {
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
        SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield());

        var shield = new HotShield { Def = 50f, Weight = 8f };
        var serialized = SerializerBlocks.Serialize(shield);
        Assert.That(serialized, Is.EqualTo("Shield(50, 8)"));

        var deserialized = SerializerBlocks.Deserialize<HotShield>(serialized);
        Assert.That(deserialized.Def, Is.EqualTo(50f).Within(1e-5f));
        Assert.That(deserialized.Weight, Is.EqualTo(8f).Within(1e-5f));
    }

    [Test]
    public void AddBlock_NonGeneric_OverwritesExisting()
    {
        var first = new Block_HotSword();
        var second = new Block_HotSword();
        SerializerBlocks.AddBlock(typeof(HotSword), first);
        SerializerBlocks.AddBlock(typeof(HotSword), second);

        Assert.That(SerializerBlocks.TryGet<HotSword>(out var result), Is.True);
        // 两次注册后 TryGet 应返回最后一次注册的 block
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void AddBlock_NonGeneric_NullType_Throws()
    {
        Assert.That(() => SerializerBlocks.AddBlock(null!, new Block_HotSword()),
            Throws.ArgumentNullException);
    }

    [Test]
    public void AddBlock_NonGeneric_NullBlock_Throws()
    {
        Assert.That(() => SerializerBlocks.AddBlock(typeof(HotSword), null!),
            Throws.ArgumentNullException);
    }

    // ═══════════════════════════════════════════════════════
    // 非泛型 RemoveBlock(Type)
    // ═══════════════════════════════════════════════════════

    [Test]
    public void RemoveBlock_NonGeneric_RemovesType()
    {
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
        Assert.That(SerializerBlocks.TryGet<HotSword>(out _), Is.True);

        SerializerBlocks.RemoveBlock(typeof(HotSword));
        Assert.That(SerializerBlocks.TryGet<HotSword>(out _), Is.False);
    }

    [Test]
    public void RemoveBlock_NonGeneric_NotRegistered_NoOp()
    {
        Assert.That(() => SerializerBlocks.RemoveBlock(typeof(HotSword)), Throws.Nothing);
    }

    [Test]
    public void RemoveBlock_NonGeneric_NullType_Throws()
    {
        Assert.That(() => SerializerBlocks.RemoveBlock(null!),
            Throws.ArgumentNullException);
    }

    // ═══════════════════════════════════════════════════════
    // 类型版本管理（热更新场景的核心）
    // ═══════════════════════════════════════════════════════

    [Test]
    public void VersionManagement_OldAndNewCoexist()
    {
        // 注册旧版本
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
        // 注册新版本（覆盖旧版）
        var newBlock = new Block_HotSword();
        SerializerBlocks.AddBlock(typeof(HotSword), newBlock);

        // 最新注册的生效（TryGet 返回最后一次 AddBlock 的实例）
        Assert.That(SerializerBlocks.TryGet<HotSword>(out var result), Is.True);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void VersionManagement_RemoveOldType()
    {
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
        SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield());

        // 下线 HotSword
        SerializerBlocks.RemoveBlock(typeof(HotSword));

        Assert.That(SerializerBlocks.TryGet<HotSword>(out _), Is.False);
        Assert.That(SerializerBlocks.TryGet<HotShield>(out _), Is.True);
    }

    // ═══════════════════════════════════════════════════════
    // Builder 流式 API — 非泛型
    // ═══════════════════════════════════════════════════════

    [Test]
    public void Builder_AddBlock_NonGeneric_Chain()
    {
        SerializerBlocks
            .AddBlock(typeof(HotSword), new Block_HotSword())
            .AddBlock(typeof(HotShield), new Block_HotShield());

        Assert.That(SerializerBlocks.TryGet<HotSword>(out _), Is.True);
        Assert.That(SerializerBlocks.TryGet<HotShield>(out _), Is.True);
    }

    [Test]
    public void Builder_RemoveBlock_NonGeneric_Chain()
    {
        SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
        SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield())
                        .RemoveBlock(typeof(HotSword));

        Assert.That(SerializerBlocks.TryGet<HotSword>(out _), Is.False);
        Assert.That(SerializerBlocks.TryGet<HotShield>(out _), Is.True);
    }
}
