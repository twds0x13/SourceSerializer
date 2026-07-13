using System;
using SourceSerializer;
using NUnit.Framework;

// ── Type alias registration ──
[assembly: TypeAlias("Distance", "float")]

// ═══════════════════════════════════════════════════════
// Test structs with [Template] attributes
// ═══════════════════════════════════════════════════════

[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
}

[Template("<float Damage>|<optional>draw <int DrawsProvide>|</optional>idx:<int StartIndex>")]
public struct SpellCard
{
    public float Damage;
    public int DrawsProvide;
    public int StartIndex;
}

[Template("<float X> <float Y> <float Z>")]
public struct Vec3
{
    public float X;
    public float Y;
    public float Z;
}

[Template("(<Vec3 Pos>)")]
public struct Entity
{
    public Vec3 Pos;
}

[Template(@"
  <literal-template>
    <field type=""float"" name=""X""/>
    <text>, </text>
    <field type=""float"" name=""Y""/>
  </literal-template>")]
public struct XmlPoint2D
{
    public float X;
    public float Y;
}

[Template("<float Damage><repetition>, <float Multipliers></repetition>")]
public struct DamageCompact
{
    public float Damage;
    public float Multipliers;
}

[Template(@"
  <literal-template>
    <field type=""float"" name=""Damage""/>
    <repetition>
      <text>, </text>
      <field type=""float"" name=""Multipliers""/>
    </repetition>
  </literal-template>")]
public struct DamageWithMultipliers
{
    public float Damage;
    public float Multipliers;
}

[ExternalTemplate(typeof(ExternalPoint), "<float A> <float B>")]
public struct ExternalPoint
{
    public float A;
    public float B;
}

// ── Enum tag types ──
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

// ── TypeAlias types ──
[Template("<Distance Range>")]
public struct DistanceWrapper
{
    public float Range;
}

// ── Numeric / primitive types ──
[Template("<long Val>")]
public struct LongField { public long Val; }

[Template("<ulong Val>")]
public struct UlongField { public ulong Val; }

[Template("<short Val>")]
public struct ShortField { public short Val; }

[Template("<char Val>")]
public struct CharField { public char Val; }

[Template("<bool Val>")]
public struct BoolField { public bool Val; }

[Template("<float X><repetition>, <float Rest></repetition>")]
public struct RepetitionMulti
{
    public float X;
    public float Rest;
}

// ═══════════════════════════════════════════════════════
// Tests
// ═══════════════════════════════════════════════════════

public class TemplateTests
{
    [Test]
    public void Point2D_ParsesTwoFloats()
    {
        Assert.That(SerializerScanners.TryGetScanner<Point2D>(out var scan), Is.True);
        int r = scan("3.5 -2.1".AsSpan(), 0, out Point2D v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(-2.1f).Within(1e-5f));
    }

    [Test]
    public void Point2D_NoMatch_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<Point2D>(out var scan), Is.True);
        int r = scan("hello".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void SpellCard_FullFormat_ParsesAllFields()
    {
        Assert.That(SerializerScanners.TryGetScanner<SpellCard>(out var scan), Is.True);
        int r = scan("10.5|draw 2|idx:1".AsSpan(), 0, out SpellCard v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(10.5f).Within(1e-5f));
        Assert.That(v.DrawsProvide, Is.EqualTo(2));
        Assert.That(v.StartIndex, Is.EqualTo(1));
    }

    [Test]
    public void SpellCard_WithoutDraw_ParsesDamageAndIndex()
    {
        Assert.That(SerializerScanners.TryGetScanner<SpellCard>(out var scan), Is.True);
        int r = scan("10.5|idx:0".AsSpan(), 0, out SpellCard v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(10.5f).Within(1e-5f));
        Assert.That(v.StartIndex, Is.EqualTo(0));
    }

    [Test]
    public void Vec3_ParsesThreeFloats()
    {
        Assert.That(SerializerScanners.TryGetScanner<Vec3>(out var scan), Is.True);
        int r = scan("1 2 3".AsSpan(), 0, out Vec3 v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(1f));
        Assert.That(v.Y, Is.EqualTo(2f));
        Assert.That(v.Z, Is.EqualTo(3f));
    }

    [Test]
    public void Entity_ParsesNestedVec3()
    {
        Assert.That(SerializerScanners.TryGetScanner<Entity>(out var scan), Is.True);
        int r = scan("(1 2 3)".AsSpan(), 0, out Entity v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Pos.X, Is.EqualTo(1f));
        Assert.That(v.Pos.Y, Is.EqualTo(2f));
        Assert.That(v.Pos.Z, Is.EqualTo(3f));
    }

    [Test]
    public void XmlPoint2D_ParsesWithCommaSeparator()
    {
        Assert.That(SerializerScanners.TryGetScanner<XmlPoint2D>(out var scan), Is.True);
        int r = scan("1.5, -2.5".AsSpan(), 0, out XmlPoint2D v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(-2.5f).Within(1e-5f));
    }

    [Test]
    public void ExternalPoint_ParsesCorrectly()
    {
        Assert.That(SerializerScanners.TryGetScanner<ExternalPoint>(out var scan), Is.True);
        int r = scan("10 20".AsSpan(), 0, out ExternalPoint v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.A, Is.EqualTo(10f));
        Assert.That(v.B, Is.EqualTo(20f));
    }

    [Test]
    public void Repetition_Compact_ZeroExtra()
    {
        Assert.That(SerializerScanners.TryGetScanner<DamageCompact>(out var scan), Is.True);
        int r = scan("42".AsSpan(), 0, out DamageCompact v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.EqualTo(0f));
    }

    [Test]
    public void Repetition_Compact_OneExtra()
    {
        Assert.That(SerializerScanners.TryGetScanner<DamageCompact>(out var scan), Is.True);
        int r = scan("42, 1.5".AsSpan(), 0, out DamageCompact v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Repetition_Xml_MultipleExtra()
    {
        Assert.That(SerializerScanners.TryGetScanner<DamageWithMultipliers>(out var scan), Is.True);
        int r = scan("42, 1.5, 2.0, 3.5".AsSpan(), 0, out DamageWithMultipliers v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.EqualTo(3.5f).Within(1e-5f));
    }

    // ── Enum tag ──

    [Test]
    public void TaggedSpell_ParsesFire()
    {
        Assert.That(SerializerScanners.TryGetScanner<TaggedSpell>(out var scan), Is.True);
        int r = scan("fire|10".AsSpan(), 0, out TaggedSpell v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Type, Is.EqualTo(Element.Fire));
        Assert.That(v.Power, Is.EqualTo(10f));
    }

    [Test]
    public void TaggedSpell_ParsesIce()
    {
        Assert.That(SerializerScanners.TryGetScanner<TaggedSpell>(out var scan), Is.True);
        int r = scan("ice|5".AsSpan(), 0, out TaggedSpell v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Type, Is.EqualTo(Element.Ice));
        Assert.That(v.Power, Is.EqualTo(5f));
    }

    [Test]
    public void TaggedSpell_UnknownTag_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<TaggedSpell>(out var scan), Is.True);
        int r = scan("water|10".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── TypeAlias ──

    [Test]
    public void DistanceWrapper_ParsesWithAlias()
    {
        Assert.That(SerializerScanners.TryGetScanner<DistanceWrapper>(out var scan), Is.True);
        int r = scan("42.5".AsSpan(), 0, out DistanceWrapper v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Range, Is.EqualTo(42.5f).Within(1e-5f));
    }

    // ── Long / Ulong ──

    [Test]
    public void LongField_ParsesWithLSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<LongField>(out var scan), Is.True);
        int r = scan("123L".AsSpan(), 0, out LongField v);
        Assert.That(r, Is.EqualTo(4)); // includes 'L' suffix
        Assert.That(v.Val, Is.EqualTo(123L));
    }

    [Test]
    public void LongField_ParsesNegative()
    {
        Assert.That(SerializerScanners.TryGetScanner<LongField>(out var scan), Is.True);
        int r = scan("-456".AsSpan(), 0, out LongField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(-456L));
    }

    [Test]
    public void UlongField_ParsesWithUlSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<UlongField>(out var scan), Is.True);
        int r = scan("123UL".AsSpan(), 0, out UlongField v);
        Assert.That(r, Is.EqualTo(5)); // includes 'UL' suffix
        Assert.That(v.Val, Is.EqualTo(123UL));
    }

    [Test]
    public void UlongField_ParsesWithLowercaseSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<UlongField>(out var scan), Is.True);
        int r = scan("456ul".AsSpan(), 0, out UlongField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo(456UL));
    }

    // ── Bool / Char ──

    [Test]
    public void BoolField_ParsesTrue()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("true".AsSpan(), 0, out BoolField v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.True);
    }

    [Test]
    public void BoolField_ParsesFalse()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("false".AsSpan(), 0, out BoolField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.False);
    }

    [Test]
    public void BoolField_Invalid_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("maybe".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void CharField_ParsesSingle()
    {
        Assert.That(SerializerScanners.TryGetScanner<CharField>(out var scan), Is.True);
        int r = scan("A".AsSpan(), 0, out CharField v);
        Assert.That(r, Is.EqualTo(1));
        Assert.That(v.Val, Is.EqualTo('A'));
    }

    [Test]
    public void CharField_EmptyString_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<CharField>(out var scan), Is.True);
        int r = scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Negative / edge cases ──

    [Test]
    public void Point2D_WrongLiteral_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<Point2D>(out var scan), Is.True);
        int r = scan("abc 1.0".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Point2D_TruncatedInput_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<Point2D>(out var scan), Is.True);
        int r = scan("1.0".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Vec3_EmptyString_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<Vec3>(out var scan), Is.True);
        int r = scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Repetition_MultipleMatches_KeepsLast()
    {
        Assert.That(SerializerScanners.TryGetScanner<RepetitionMulti>(out var scan), Is.True);
        int r = scan("10, 20, 30".AsSpan(), 0, out RepetitionMulti v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(10f));
        Assert.That(v.Rest, Is.EqualTo(30f).Within(1e-5f));
    }
}
