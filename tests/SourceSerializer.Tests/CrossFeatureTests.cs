using System;
using NUnit.Framework;
using SourceSerializer;

// 实现两个接口的类型
[Template("Mlt <float V>")]
public struct MultiTag : IVector, IShape
{
    public float V;
}

[Template("DualInterface(A:<IVector A><optional>, B:<IShape B></optional>)")]
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
        Assert.That(SerializerBlocks.TryScan<IVector>("Mlt 1.5", out var v), Is.True);
        Assert.That(v, Is.InstanceOf<MultiTag>());
        Assert.That(((MultiTag)v).V, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void MultiTag_As_IShape_Scans()
    {
        Assert.That(SerializerBlocks.TryScan<IShape>("Mlt 99", out var v), Is.True);
        Assert.That(v, Is.InstanceOf<MultiTag>());
        Assert.That(((MultiTag)v).V, Is.EqualTo(99f).Within(1e-5f));
    }

    // ── 接口 + optional ──

    [Test]
    public void DualInterface_WithoutOptional()
    {
        Assert.That(SerializerBlocks.TryScan<DualInterface>("DualInterface(A:Mlt 1.5)", out var v), Is.True);
        // A=MultiTag（匹配 IVector），B 不匹配
        Assert.That(v.A, Is.InstanceOf<MultiTag>());
        Assert.That(v.B, Is.Null);
    }

    [Test]
    public void DualInterface_WithOptional()
    {
        // A=MultiTag, B=ShapeB
        Assert.That(SerializerBlocks.TryScan<DualInterface>("DualInterface(A:Mlt 1.5, B:B(\"hi\"))", out var v), Is.True);
        Assert.That(v.A, Is.InstanceOf<MultiTag>());
        Assert.That(v.B, Is.InstanceOf<ShapeB>());
    }
}
