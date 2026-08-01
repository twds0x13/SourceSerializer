using System;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Test type: readonly struct with matching constructor
// AND <optional> block — exercises the hoisting mechanism
// in ScanCodeEmitter.EmitHoistedDecls.
// ═══════════════════════════════════════════════════════

[Template("OptWithCtor(X:<float X><optional>, Y:<float Y></optional>)")]
public readonly struct OptWithCtor
{
    public readonly float X;
    public readonly float Y;

    public OptWithCtor(float x, float y)
    {
        X = x;
        Y = y;
    }
}

// ═══════════════════════════════════════════════════════
// Tests for constructor + optional block interaction
// ═══════════════════════════════════════════════════════

public class OptionalWithCtorTests
{
    [Test]
    public void WithoutOptional_YDefaultsToZero()
    {
        Assert.That(SerializerBlocks.TryScan<OptWithCtor>("OptWithCtor(X:3.5)", out OptWithCtor v), Is.True);
        Assert.That(v.X, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(0f));
    }

    [Test]
    public void WithOptional_YParsed()
    {
        Assert.That(SerializerBlocks.TryScan<OptWithCtor>("OptWithCtor(X:3.5, Y:7.2)", out OptWithCtor v), Is.True);
        Assert.That(v.X, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(7.2f).Within(1e-5f));
    }

    [Test]
    public void Roundtrip_WithOptional()
    {
        Assert.That(SerializerBlocks.TryGet<OptWithCtor>(out var block), Is.True);
        var original = new OptWithCtor(1.5f, 2.5f);
        var sb = new StringBuilder();
        block.Emit(sb, original);
        Assert.That(SerializerBlocks.TryScan<OptWithCtor>(sb.ToString(), out OptWithCtor parsed), Is.True);
        Assert.That(parsed.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(parsed.Y, Is.EqualTo(2.5f).Within(1e-5f));
    }

    [Test]
    public void Roundtrip_WithoutOptional()
    {
        Assert.That(SerializerBlocks.TryGet<OptWithCtor>(out var block), Is.True);
        var original = new OptWithCtor(1.5f, 0f);
        var sb = new StringBuilder();
        block.Emit(sb, original);
        Assert.That(SerializerBlocks.TryScan<OptWithCtor>(sb.ToString(), out OptWithCtor parsed), Is.True);
        Assert.That(parsed.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(parsed.Y, Is.EqualTo(0f));
    }
}
