using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Collection / Repetition test types
// ═══════════════════════════════════════════════════════

[Template("<float Damage><optional>, <List<float> Multipliers></optional>")]
public struct DamageCompact
{
    public float Damage;
    public List<float> Multipliers;
}

[Template(@"
  <literal-template>
    <field type=""float"" name=""Damage""/>
    <optional>
      <text>, </text>
      <field type=""List&lt;float&gt;"" name=""Multipliers""/>
    </optional>
  </literal-template>")]
public struct DamageWithMultipliers
{
    public float Damage;
    public List<float> Multipliers;
}

[Template("<float X><optional>, <List<float> Rest></optional>")]
public struct RepetitionMulti
{
    public float X;
    public List<float> Rest;
}

// ═══════════════════════════════════════════════════════
// Collection / Repetition tests
// ═══════════════════════════════════════════════════════

public class CollectionRepetitionTests
{
    [Test]
    public void Repetition_Compact_ZeroExtra()
    {
        Assert.That(SerializerBlocks.TryGet<DamageCompact>(out var block), Is.True);
        int r = block.Scan("42".AsSpan(), 0, out DamageCompact v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.Not.Null);
        Assert.That(v.Multipliers.Count, Is.EqualTo(0));
    }

    [Test]
    public void Repetition_Compact_OneExtra()
    {
        Assert.That(SerializerBlocks.TryGet<DamageCompact>(out var block), Is.True);
        int r = block.Scan("42, List(1.5)".AsSpan(), 0, out DamageCompact v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.Not.Null);
        Assert.That(v.Multipliers.Count, Is.EqualTo(1));
        Assert.That(v.Multipliers[0], Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Repetition_Xml_MultipleExtra()
    {
        Assert.That(SerializerBlocks.TryGet<DamageWithMultipliers>(out var block), Is.True);
        int r = block.Scan("42, List(1.5, 2.0, 3.5)".AsSpan(), 0, out DamageWithMultipliers v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.Not.Null);
        Assert.That(v.Multipliers.Count, Is.EqualTo(3));
        Assert.That(v.Multipliers[0], Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Multipliers[2], Is.EqualTo(3.5f).Within(1e-5f));
    }

    [Test]
    public void Repetition_MultipleMatches_KeepsAll()
    {
        Assert.That(SerializerBlocks.TryGet<RepetitionMulti>(out var block), Is.True);
        int r = block.Scan("10, List(20, 30)".AsSpan(), 0, out RepetitionMulti v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(10f));
        Assert.That(v.Rest, Is.Not.Null);
        Assert.That(v.Rest.Count, Is.EqualTo(2));
        Assert.That(v.Rest[0], Is.EqualTo(20f).Within(1e-5f));
        Assert.That(v.Rest[1], Is.EqualTo(30f).Within(1e-5f));
    }

    // ── 大列表压力 ──

    [Test]
    public void LargeList_100_Elements()
    {
        Assert.That(SerializerBlocks.TryGet<ManyFloats>(out var block), Is.True);
        var input = "List(" + string.Join(", ", Enumerable.Range(0, 100)) + ")";
        int r = block.Scan(input.AsSpan(), 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Values.Count, Is.EqualTo(100));
        Assert.That(v.Values[0], Is.EqualTo(0f));
        Assert.That(v.Values[99], Is.EqualTo(99f));
    }
}

[Template("<List<float> Values>")]
public struct ManyFloats { public List<float> Values; }
