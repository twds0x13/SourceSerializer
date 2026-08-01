using System;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Enum tag test types
// ═══════════════════════════════════════════════════════

public enum Element : byte
{
    [Tag("fire")] Fire,
    [Tag("ice")]  Ice,
    [Tag("magic")] Magic,
}

[Template("TaggedSpell(Type:<Element Type>, Power:<float Power>)")]
public struct TaggedSpell
{
    public Element Type;
    public float Power;
}

// ═══════════════════════════════════════════════════════
// Enum tag tests
// ═══════════════════════════════════════════════════════

public class EnumTagTests
{
    [Test]
    public void TaggedSpell_ParsesFire()
    {
        Assert.That(SerializerBlocks.TryScan<TaggedSpell>("TaggedSpell(Type:fire, Power:10)", out TaggedSpell v), Is.True);
        Assert.That(v.Type, Is.EqualTo(Element.Fire));
        Assert.That(v.Power, Is.EqualTo(10f));
    }

    [Test]
    public void TaggedSpell_ParsesIce()
    {
        Assert.That(SerializerBlocks.TryScan<TaggedSpell>("TaggedSpell(Type:ice, Power:5)", out TaggedSpell v), Is.True);
        Assert.That(v.Type, Is.EqualTo(Element.Ice));
        Assert.That(v.Power, Is.EqualTo(5f));
    }

    [Test]
    public void TaggedSpell_ParsesMagic()
    {
        Assert.That(SerializerBlocks.TryScan<TaggedSpell>("TaggedSpell(Type:magic, Power:100)", out TaggedSpell v), Is.True);
        Assert.That(v.Type, Is.EqualTo(Element.Magic));
    }

    [Test]
    public void TaggedSpell_UnknownTag_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<TaggedSpell>("TaggedSpell(Type:water, Power:10)", out _), Is.False);
    }

    // ── : int 后端枚举 ──

    [Test]
    public void IntBackedEnum_ScanAndEmit()
    {
        Assert.That(SerializerBlocks.TryScan<IntSpell>("IntSpell(Fire, 10)", out var v), Is.True);
        Assert.That(v.Elem, Is.EqualTo(IntElement.Fire));
        Assert.That(v.Power, Is.EqualTo(10));

        Assert.That(SerializerBlocks.TryGet<IntSpell>(out var block), Is.True);
        var sb = new System.Text.StringBuilder();
        block.Emit(sb, v);
        Assert.That(sb.ToString(), Is.EqualTo("IntSpell(Fire, 10)"));
    }

    // ── 无 [Tag] 枚举 — 当前 SG 不支持（无 [Tag] 枚举触发 SSR004）
    // 待 SG 添加 enum 值名 fallback 后补充测试。
}

public enum IntElement : int
{
    [Tag("Fire")] Fire = 0,
    [Tag("Ice")]  Ice  = 1,
}

[Template("IntSpell(<IntElement Elem>, <int Power>)")]
public struct IntSpell
{
    public IntElement Elem;
    public int Power;
}
