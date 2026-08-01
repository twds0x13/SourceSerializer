using System;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Unmanaged struct test types
// ═══════════════════════════════════════════════════════

[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D
{
    public float X;
    public float Y;
}

[Template("Vec3(<float X>, <float Y>, <float Z>)")]
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

[ExternalTemplate(typeof(ExternalPoint), "ExternalPoint(<float A>, <float B>)")]
public struct ExternalPoint
{
    public float A;
    public float B;
}

[Template("SpellCard(Damage:<float Damage><optional>, DrawsProvide:<int DrawsProvide></optional>, StartIndex:<int StartIndex>)")]
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
        Assert.That(SerializerBlocks.TryScan<Point2D>("Point2D(3.5, -2.1)", out Point2D v), Is.True);
        Assert.That(v.X, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(-2.1f).Within(1e-5f));
    }

    [Test]
    public void Point2D_NoMatch_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<Point2D>("hello", out _), Is.False);
    }

    [Test]
    public void Point2D_WrongLiteral_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<Point2D>("abc 1.0", out _), Is.False);
    }

    [Test]
    public void Point2D_TruncatedInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<Point2D>("1.0", out _), Is.False);
    }

    [Test]
    public void FloatField_AsDouble()
    {
        Assert.That(SerializerBlocks.TryScan<Point2D>("Point2D(1.5d, -2.0)", out Point2D v), Is.True);
        Assert.That(v.X, Is.EqualTo(1.5f).Within(1e-5f));
    }

    // ── Vec3 ──

    [Test]
    public void Vec3_ParsesThreeFloats()
    {
        Assert.That(SerializerBlocks.TryScan<Vec3>("Vec3(1, 2, 3)", out Vec3 v), Is.True);
        Assert.That(v.X, Is.EqualTo(1f));
        Assert.That(v.Y, Is.EqualTo(2f));
        Assert.That(v.Z, Is.EqualTo(3f));
    }

    [Test]
    public void Vec3_EmptyString_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<Vec3>("", out _), Is.False);
    }

    // ── Entity (nested struct) ──

    [Test]
    public void Entity_ParsesNestedVec3()
    {
        Assert.That(SerializerBlocks.TryScan<Entity>("(Vec3(1, 2, 3))", out Entity v), Is.True);
        Assert.That(v.Pos.X, Is.EqualTo(1f));
        Assert.That(v.Pos.Y, Is.EqualTo(2f));
        Assert.That(v.Pos.Z, Is.EqualTo(3f));
    }

    // ── XmlPoint2D ──

    [Test]
    public void XmlPoint2D_ParsesWithCommaSeparator()
    {
        Assert.That(SerializerBlocks.TryScan<XmlPoint2D>("1.5,-2.5", out XmlPoint2D v), Is.True);
        Assert.That(v.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(-2.5f).Within(1e-5f));
    }

    // ── ExternalPoint ──

    [Test]
    public void ExternalPoint_ParsesCorrectly()
    {
        Assert.That(SerializerBlocks.TryScan<ExternalPoint>("ExternalPoint(10, 20)", out ExternalPoint v), Is.True);
        Assert.That(v.A, Is.EqualTo(10f));
        Assert.That(v.B, Is.EqualTo(20f));
    }

    // ── SpellCard ──

    [Test]
    public void SpellCard_FullFormat_ParsesAllFields()
    {
        Assert.That(SerializerBlocks.TryScan<SpellCard>("SpellCard(Damage:10.5, DrawsProvide:2, StartIndex:1)", out SpellCard v), Is.True);
        Assert.That(v.Damage, Is.EqualTo(10.5f).Within(1e-5f));
        Assert.That(v.DrawsProvide, Is.EqualTo(2));
        Assert.That(v.StartIndex, Is.EqualTo(1));
    }

    [Test]
    public void SpellCard_WithoutDraw_ParsesDamageAndIndex()
    {
        Assert.That(SerializerBlocks.TryScan<SpellCard>("SpellCard(Damage:10.5, StartIndex:0)", out SpellCard v), Is.True);
        Assert.That(v.Damage, Is.EqualTo(10.5f).Within(1e-5f));
        Assert.That(v.StartIndex, Is.EqualTo(0));
    }

    [Test]
    public void SpellCard_WithoutPipe_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<SpellCard>("10.5", out _), Is.False);
    }
}
