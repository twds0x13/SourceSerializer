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

[Template("<Element Type>|<float Power>")]
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
        Assert.That(SerializerBlocks.TryGet<TaggedSpell>(out var block), Is.True);
        int r = block.Scan("fire|10".AsSpan(), 0, out TaggedSpell v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Type, Is.EqualTo(Element.Fire));
        Assert.That(v.Power, Is.EqualTo(10f));
    }

    [Test]
    public void TaggedSpell_ParsesIce()
    {
        Assert.That(SerializerBlocks.TryGet<TaggedSpell>(out var block), Is.True);
        int r = block.Scan("ice|5".AsSpan(), 0, out TaggedSpell v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Type, Is.EqualTo(Element.Ice));
        Assert.That(v.Power, Is.EqualTo(5f));
    }

    [Test]
    public void TaggedSpell_ParsesMagic()
    {
        Assert.That(SerializerBlocks.TryGet<TaggedSpell>(out var block), Is.True);
        int r = block.Scan("magic|100".AsSpan(), 0, out TaggedSpell v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Type, Is.EqualTo(Element.Magic));
    }

    [Test]
    public void TaggedSpell_UnknownTag_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<TaggedSpell>(out var block), Is.True);
        int r = block.Scan("water|10".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }
}
