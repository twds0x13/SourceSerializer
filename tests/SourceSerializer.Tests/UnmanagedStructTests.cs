using System;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Unmanaged struct test types
// ═══════════════════════════════════════════════════════

[Template("<float X> <float Y>")]
public struct Point2D
{
    public float X;
    public float Y;
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

[ExternalTemplate(typeof(ExternalPoint), "<float A> <float B>")]
public struct ExternalPoint
{
    public float A;
    public float B;
}

[Template("<float Damage>|<optional>draw <int DrawsProvide>|</optional>idx:<int StartIndex>")]
public struct SpellCard
{
    public float Damage;
    public int DrawsProvide;
    public int StartIndex;
}

// ═══════════════════════════════════════════════════════
// Unmanaged struct tests
// ═══════════════════════════════════════════════════════

public class UnmanagedStructTests
{
    // ── Point2D ──

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
    public void FloatField_AsDouble()
    {
        Assert.That(SerializerScanners.TryGetScanner<Point2D>(out var scan), Is.True);
        int r = scan("1.5d -2.0".AsSpan(), 0, out Point2D v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(1.5f).Within(1e-5f));
    }

    // ── Vec3 ──

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
    public void Vec3_EmptyString_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<Vec3>(out var scan), Is.True);
        int r = scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Entity (nested struct) ──

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

    // ── XmlPoint2D ──

    [Test]
    public void XmlPoint2D_ParsesWithCommaSeparator()
    {
        Assert.That(SerializerScanners.TryGetScanner<XmlPoint2D>(out var scan), Is.True);
        int r = scan("1.5, -2.5".AsSpan(), 0, out XmlPoint2D v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(-2.5f).Within(1e-5f));
    }

    // ── ExternalPoint ──

    [Test]
    public void ExternalPoint_ParsesCorrectly()
    {
        Assert.That(SerializerScanners.TryGetScanner<ExternalPoint>(out var scan), Is.True);
        int r = scan("10 20".AsSpan(), 0, out ExternalPoint v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.A, Is.EqualTo(10f));
        Assert.That(v.B, Is.EqualTo(20f));
    }

    // ── SpellCard ──

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
    public void SpellCard_WithoutPipe_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<SpellCard>(out var scan), Is.True);
        int r = scan("10.5".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }
}
