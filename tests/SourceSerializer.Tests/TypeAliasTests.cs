using System;
using NUnit.Framework;
using SourceSerializer;

// ── Type alias registration ──
[assembly: TypeAlias("Distance", "float")]

// ═══════════════════════════════════════════════════════
// TypeAlias test types
// ═══════════════════════════════════════════════════════

[Template("<Distance Range>")]
public struct DistanceWrapper
{
    public float Range;
}

// ═══════════════════════════════════════════════════════
// TypeAlias tests
// ═══════════════════════════════════════════════════════

public class TypeAliasTests
{
    [Test]
    public void DistanceWrapper_ParsesWithAlias()
    {
        Assert.That(SerializerBlocks.TryGet<DistanceWrapper>(out var block), Is.True);
        int r = block.Scan("42.5".AsSpan(), 0, out DistanceWrapper v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Range, Is.EqualTo(42.5f).Within(1e-5f));
    }
}
