using System;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

public class EmitterTests
{
    [Test]
    public void TryGetEmitter_UnregisteredType_ReturnsFalse()
    {
        Assert.That(SerializerBlocks.TryGet<DateTime>(out var block), Is.False);
        Assert.That(block, Is.Null);
    }

    [Test]
    public void Emit_Float_AppendsFormatted()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Float(sb, 3.5f);
        Assert.That(sb.ToString(), Is.EqualTo("3.5"));
    }

    [Test]
    public void Emit_Float_Negative()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Float(sb, -1.5f);
        Assert.That(sb.ToString(), Is.EqualTo("-1.5"));
    }

    [Test]
    public void Emit_Float_Integer()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Float(sb, 42f);
        Assert.That(sb.ToString(), Is.EqualTo("42"));
    }

    [Test]
    public void Emit_Double_AppendsRoundtrippable()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Double(sb, 3.5d);
        Assert.That(sb.ToString(), Is.EqualTo("3.5"));
    }

    [Test]
    public void Emit_Double_WithExponent()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Double(sb, 1.5e10);
        Assert.That(sb.ToString(), Is.EqualTo("15000000000"));
    }

    [Test]
    public void Emit_Int_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Int(sb, 42);
        Assert.That(sb.ToString(), Is.EqualTo("42"));
    }

    [Test]
    public void Emit_Int_Negative()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Int(sb, -42);
        Assert.That(sb.ToString(), Is.EqualTo("-42"));
    }

    [Test]
    public void Emit_Uint_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Uint(sb, 42u);
        Assert.That(sb.ToString(), Is.EqualTo("42"));
    }

    [Test]
    public void Emit_Long_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Long(sb, 123L);
        Assert.That(sb.ToString(), Is.EqualTo("123"));
    }

    [Test]
    public void Emit_Ulong_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Ulong(sb, 123UL);
        Assert.That(sb.ToString(), Is.EqualTo("123"));
    }

    [Test]
    public void Emit_Short_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Short(sb, (short)7);
        Assert.That(sb.ToString(), Is.EqualTo("7"));
    }

    [Test]
    public void Emit_Ushort_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Ushort(sb, (ushort)7);
        Assert.That(sb.ToString(), Is.EqualTo("7"));
    }

    [Test]
    public void Emit_Byte_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Byte(sb, (byte)255);
        Assert.That(sb.ToString(), Is.EqualTo("255"));
    }

    [Test]
    public void Emit_Sbyte_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Sbyte(sb, (sbyte)-128);
        Assert.That(sb.ToString(), Is.EqualTo("-128"));
    }

    [Test]
    public void Emit_Bool_True()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Bool(sb, true);
        Assert.That(sb.ToString(), Is.EqualTo("true"));
    }

    [Test]
    public void Emit_Bool_False()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Bool(sb, false);
        Assert.That(sb.ToString(), Is.EqualTo("false"));
    }

    [Test]
    public void Emit_Char_AppendsValue()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_Char(sb, 'A');
        Assert.That(sb.ToString(), Is.EqualTo("A"));
    }

    [Test]
    public void Emit_String_AlwaysQuoted()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_String(sb, "hello");
        Assert.That(sb.ToString(), Is.EqualTo("\"hello\""));
    }

    [Test]
    public void Emit_String_WithSpace_Quoted()
    {
        var sb = new StringBuilder();
        SerializerRegistry.Emit_String(sb, "hello world");
        Assert.That(sb.ToString(), Is.EqualTo("\"hello world\""));
    }

    // ═══════════════════════════════════════════════════════
    // Struct Emit + Roundtrip tests
    // ═══════════════════════════════════════════════════════

    [Test]
    public void Point2D_Emit_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<Point2D>(out var block), Is.True);

        var original = new Point2D { X = 3.5f, Y = -2.1f };
        var sb = new StringBuilder();
        block.Emit(sb, original);

        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.X, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(parsed.Y, Is.EqualTo(-2.1f).Within(1e-5f));
    }

    [Test]
    public void Vec3_Emit_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<Vec3>(out var block), Is.True);

        var original = new Vec3 { X = 1f, Y = 2f, Z = 3f };
        var sb = new StringBuilder();
        block.Emit(sb, original);

        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.X, Is.EqualTo(1f));
        Assert.That(parsed.Y, Is.EqualTo(2f));
        Assert.That(parsed.Z, Is.EqualTo(3f));
    }

    [Test]
    public void Entity_Emit_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<Entity>(out var block), Is.True);

        var original = new Entity { Pos = new Vec3 { X = 1f, Y = 2f, Z = 3f } };
        var sb = new StringBuilder();
        block.Emit(sb, original);

        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.Pos.X, Is.EqualTo(1f));
        Assert.That(parsed.Pos.Y, Is.EqualTo(2f));
        Assert.That(parsed.Pos.Z, Is.EqualTo(3f));
    }

    // ── Optional block tests ──

    [Test]
    public void SpellCard_Full_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<SpellCard>(out var block), Is.True);

        const string input = "10.5|draw 2|idx:1";
        int r = block.Scan(input.AsSpan(), 0, out var card);
        Assert.That(r, Is.GreaterThan(0));

        var sb = new StringBuilder();
        block.Emit(sb, card);

        r = block.Scan(sb.ToString().AsSpan(), 0, out var card2);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(card2.Damage, Is.EqualTo(card.Damage).Within(1e-5f));
        Assert.That(card2.DrawsProvide, Is.EqualTo(card.DrawsProvide));
        Assert.That(card2.StartIndex, Is.EqualTo(card.StartIndex));
    }

    [Test]
    public void SpellCard_WithoutOptional_Omitted()
    {
        Assert.That(SerializerBlocks.TryGet<SpellCard>(out var block), Is.True);

        var card = new SpellCard { Damage = 10f, DrawsProvide = 0, StartIndex = 1 };
        var sb = new StringBuilder();
        block.Emit(sb, card);

        string result = sb.ToString();
        Assert.That(result, Does.Not.Contain("draw"));
        Assert.That(result, Does.Contain("idx:1"));
    }

    // ── Enum tag tests ──

    [Test]
    public void TaggedSpell_Emit_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<TaggedSpell>(out var block), Is.True);

        const string input = "fire|10";
        int r = block.Scan(input.AsSpan(), 0, out var spell);
        Assert.That(r, Is.GreaterThan(0));

        var sb = new StringBuilder();
        block.Emit(sb, spell);

        Assert.That(sb.ToString(), Is.EqualTo("fire|10"));
    }

    // ── Managed type tests ──

    [Test]
    public void NamedValue_Emit_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<NamedValue>(out var block), Is.True);

        var original = new NamedValue { Name = "sword", Value = 3.5f };
        var sb = new StringBuilder();
        block.Emit(sb, original);

        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.Name, Is.EqualTo("sword"));
        Assert.That(parsed.Value, Is.EqualTo(3.5f).Within(1e-5f));
    }
}
