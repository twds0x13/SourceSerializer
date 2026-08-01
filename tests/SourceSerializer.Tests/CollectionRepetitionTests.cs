using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Collection / Repetition test types
// ═══════════════════════════════════════════════════════

[Template("DamageCompact(Damage:<float Damage><optional>, Multipliers:<List<float> Multipliers></optional>)")]
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

[Template("RepetitionMulti(X:<float X><optional>, Rest:<List<float> Rest></optional>)")]
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
        Assert.That(SerializerBlocks.TryScan<DamageCompact>("DamageCompact(Damage:42)", out DamageCompact v), Is.True);
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.Not.Null);
        Assert.That(v.Multipliers.Count, Is.EqualTo(0));
    }

    [Test]
    public void Repetition_Compact_OneExtra()
    {
        Assert.That(SerializerBlocks.TryScan<DamageCompact>("DamageCompact(Damage:42, Multipliers:List(1.5))", out DamageCompact v), Is.True);
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.Not.Null);
        Assert.That(v.Multipliers.Count, Is.EqualTo(1));
        Assert.That(v.Multipliers[0], Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Repetition_Xml_MultipleExtra()
    {
        Assert.That(SerializerBlocks.TryScan<DamageWithMultipliers>("42, List(1.5, 2.0, 3.5)", out DamageWithMultipliers v), Is.True);
        Assert.That(v.Damage, Is.EqualTo(42f));
        Assert.That(v.Multipliers, Is.Not.Null);
        Assert.That(v.Multipliers.Count, Is.EqualTo(3));
        Assert.That(v.Multipliers[0], Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Multipliers[2], Is.EqualTo(3.5f).Within(1e-5f));
    }

    [Test]
    public void Repetition_MultipleMatches_KeepsAll()
    {
        Assert.That(SerializerBlocks.TryScan<RepetitionMulti>("RepetitionMulti(X:10, Rest:List(20, 30))", out RepetitionMulti v), Is.True);
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
        var input = "List(" + string.Join(", ", Enumerable.Range(0, 100)) + ")";
        Assert.That(SerializerBlocks.TryScan<ManyFloats>(input, out var v), Is.True);
        Assert.That(v.Values.Count, Is.EqualTo(100));
        Assert.That(v.Values[0], Is.EqualTo(0f));
        Assert.That(v.Values[99], Is.EqualTo(99f));
    }
}

[Template("<List<float> Values>")]
public struct ManyFloats { public List<float> Values; }
