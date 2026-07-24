using System;
using NUnit.Framework;
using SourceSerializer;

// 实现两个接口的类型
[Template("Mlt <float V>")]
public struct MultiTag : IVector, IShape
{
    public float V;
}

[Template("<IVector A><optional>, <IShape B></optional>")]
public struct DualInterface
{
    public IVector A;
    public IShape B;
}

/// <summary>
/// 跨功能组合测试。
/// </summary>
public class CrossFeatureTests
{
    // ── 多接口实现 ──

    [Test]
    public void MultiTag_As_IVector_Scans()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);
        int r = block.Scan("Mlt 1.5".AsSpan(), 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<MultiTag>());
        Assert.That(((MultiTag)v).V, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void MultiTag_As_IShape_Scans()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        int r = block.Scan("Mlt 99".AsSpan(), 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<MultiTag>());
        Assert.That(((MultiTag)v).V, Is.EqualTo(99f).Within(1e-5f));
    }

    // ── 接口 + optional ──

    [Test]
    public void DualInterface_WithoutOptional()
    {
        Assert.That(SerializerBlocks.TryGet<DualInterface>(out var block), Is.True);
        // A=MultiTag（匹配 IVector），B 不匹配
        int r = block.Scan("Mlt 1.5".AsSpan(), 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.A, Is.InstanceOf<MultiTag>());
        Assert.That(v.B, Is.Null);
    }

    [Test]
    public void DualInterface_WithOptional()
    {
        Assert.That(SerializerBlocks.TryGet<DualInterface>(out var block), Is.True);
        // A=MultiTag, B=ShapeB
        int r = block.Scan("Mlt 1.5, B(\"hi\")".AsSpan(), 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.A, Is.InstanceOf<MultiTag>());
        Assert.That(v.B, Is.InstanceOf<ShapeB>());
    }
}
