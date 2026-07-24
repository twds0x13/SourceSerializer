using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// 接口自动分发 — 测试类型定义
// ═══════════════════════════════════════════════════════

public interface IVector { }

[Template("Vec3D(<float X>, <float Y>, <float Z>)")]
public struct Vec3D : IVector
{
    public float X;
    public float Y;
    public float Z;
}

[Template("Vec2(<float X>, <float Y>)")]
public struct Vec2 : IVector
{
    public float X;
    public float Y;
}

/// <summary>使用 IVector 接口字段的 struct</summary>
[Template("<IVector V>")]
public struct VectorWrapper
{
    public IVector V;
}

/// <summary>使用 List&lt;IVector&gt; 的 struct — 异质集合</summary>
[Template("<float Base><optional>, <List<IVector> Targets></optional>")]
public struct Attack
{
    public float Base;
    public List<IVector> Targets;
}

/// <summary>多实现接口（更多变体）</summary>
public interface IShape { }

// 三种类型种类实现同一接口：unmanaged struct / managed struct / class
[Template("A(<float V>)")]
public struct ShapeA : IShape
{
    public float V;
}

[Template("B(<string V>)")]
public struct ShapeB : IShape
{
    public string V;
}

[Template("C(<int V>)")]
public class ShapeC : IShape
{
    public int V;
}

[Template("<float Base><optional>, <List<IShape> Items></optional>")]
public struct ShapeBag
{
    public float Base;
    public List<IShape> Items;
}

// ═══════════════════════════════════════════════════════

public interface IValue { }

[Template("<int Val>")]
public struct IntValue : IValue
{
    public int Val;
}

[Template("<string Val>")]
public class StringValue : IValue
{
    public string Val;
}

[Template("<IValue V>")]
public struct ValueWrapper
{
    public IValue V;
}

// ═══════════════════════════════════════════════════════
// 接口自动分发 — 测试
// ═══════════════════════════════════════════════════════

public class InterfaceDispatchTests
{
    // ── IVector 单字段分发 ──

    [Test]
    public void IVector_Scan_Vec2()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);
        int r = block.Scan("Vec2(1.5, -2)".AsSpan(), 0, out IVector v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<Vec2>());
        var v2 = (Vec2)v;
        Assert.That(v2.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v2.Y, Is.EqualTo(-2f).Within(1e-5f));
    }

    [Test]
    public void IVector_Scan_Vec3D()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);
        int r = block.Scan("Vec3D(3, 5, 7)".AsSpan(), 0, out IVector v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<Vec3D>());
        var v3 = (Vec3D)v;
        Assert.That(v3.X, Is.EqualTo(3f).Within(1e-5f));
        Assert.That(v3.Y, Is.EqualTo(5f).Within(1e-5f));
        Assert.That(v3.Z, Is.EqualTo(7f).Within(1e-5f));
    }

    // ── 容器 struct 内使用接口字段 ──

    [Test]
    public void VectorWrapper_Scan()
    {
        Assert.That(SerializerBlocks.TryGet<VectorWrapper>(out var block), Is.True);
        int r = block.Scan("Vec2(1.5, -2)".AsSpan(), 0, out VectorWrapper v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.V, Is.InstanceOf<Vec2>());
    }

    // ── List<IVector> 异质集合 ──

    [Test]
    public void Attack_Scan_EmptyTargets()
    {
        Assert.That(SerializerBlocks.TryGet<Attack>(out var block), Is.True);
        int r = block.Scan("100".AsSpan(), 0, out Attack v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Targets, Is.Not.Null);
        Assert.That(v.Targets.Count, Is.EqualTo(0));
    }

    [Test]
    public void Attack_Scan_MixedTargets()
    {
        Assert.That(SerializerBlocks.TryGet<Attack>(out var block), Is.True);
        int r = block.Scan("100, List(Vec3D(1.5, -2, 3), Vec2(5, 7))".AsSpan(), 0, out Attack v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Targets.Count, Is.EqualTo(2));
        Assert.That(v.Targets[0], Is.InstanceOf<Vec3D>());
        Assert.That(v.Targets[1], Is.InstanceOf<Vec2>());
        var v3 = (Vec3D)v.Targets[0];
        Assert.That(v3.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v3.Y, Is.EqualTo(-2f).Within(1e-5f));
        Assert.That(v3.Z, Is.EqualTo(3f).Within(1e-5f));
        var v2 = (Vec2)v.Targets[1];
        Assert.That(v2.X, Is.EqualTo(5f).Within(1e-5f));
        Assert.That(v2.Y, Is.EqualTo(7f).Within(1e-5f));
    }

    // ── Emit + Roundtrip ──

    [Test]
    public void IVector_Emit_Vec2()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new Vec2 { X = 1.5f, Y = -2f });
        Assert.That(sb.ToString(), Is.EqualTo("Vec2(1.5, -2)"));
    }

    [Test]
    public void IVector_Emit_Vec3D()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new Vec3D { X = 3f, Y = 5f, Z = 7f });
        Assert.That(sb.ToString(), Is.EqualTo("Vec3D(3, 5, 7)"));
    }

    [Test]
    public void IVector_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);

        var original = new Vec3D { X = 3f, Y = 5f, Z = 7f };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out IVector parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed, Is.InstanceOf<Vec3D>());
        var v3 = (Vec3D)parsed;
        Assert.That(v3.X, Is.EqualTo(3f).Within(1e-5f));
        Assert.That(v3.Y, Is.EqualTo(5f).Within(1e-5f));
        Assert.That(v3.Z, Is.EqualTo(7f).Within(1e-5f));
    }

    // ── IValue 多实现接口（struct + class） ──

    [Test]
    public void IValue_Scan_IntValue()
    {
        Assert.That(SerializerBlocks.TryGet<IValue>(out var block), Is.True);
        int r = block.Scan("42".AsSpan(), 0, out IValue v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<IntValue>());
        Assert.That(((IntValue)v).Val, Is.EqualTo(42));
    }

    [Test]
    public void IValue_Scan_StringValue()
    {
        Assert.That(SerializerBlocks.TryGet<IValue>(out var block), Is.True);
        int r = block.Scan("hello".AsSpan(), 0, out IValue v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<StringValue>());
        Assert.That(((StringValue)v).Val, Is.EqualTo("hello"));
    }

    // ── 失败路径 ──

    [Test]
    public void IVector_InvalidInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<IVector>(out var block), Is.True);
        int r = block.Scan("not_a_vector".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ═══════════════════════════════════════════════════════
    // IShape — unmanaged struct / managed struct / class 混合
    // ═══════════════════════════════════════════════════════

    [Test]
    public void IShape_Scan_ShapeA()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        int r = block.Scan("A(1.5)".AsSpan(), 0, out IShape v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<ShapeA>());
        Assert.That(((ShapeA)v).V, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void IShape_Scan_ShapeB()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        int r = block.Scan("B(\"hello\")".AsSpan(), 0, out IShape v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<ShapeB>());
        Assert.That(((ShapeB)v).V, Is.EqualTo("hello"));
    }

    [Test]
    public void IShape_Scan_ShapeC()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        int r = block.Scan("C(42)".AsSpan(), 0, out IShape v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v, Is.InstanceOf<ShapeC>());
        Assert.That(((ShapeC)v).V, Is.EqualTo(42));
    }

    [Test]
    public void IShape_Emit_ShapeA()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new ShapeA { V = 1.5f });
        Assert.That(sb.ToString(), Is.EqualTo("A(1.5)"));
    }

    [Test]
    public void IShape_Emit_ShapeB()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new ShapeB { V = "hello" });
        Assert.That(sb.ToString(), Is.EqualTo("B(\"hello\")"));
    }

    [Test]
    public void IShape_Emit_ShapeC()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new ShapeC { V = 42 });
        Assert.That(sb.ToString(), Is.EqualTo("C(42)"));
    }

    [Test]
    public void IShape_Roundtrip_ShapeA()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        var original = new ShapeA { V = 2.5f };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out IShape parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed, Is.InstanceOf<ShapeA>());
        Assert.That(((ShapeA)parsed).V, Is.EqualTo(2.5f).Within(1e-5f));
    }

    [Test]
    public void IShape_Roundtrip_ShapeB()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        var original = new ShapeB { V = "world" };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out IShape parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed, Is.InstanceOf<ShapeB>());
        Assert.That(((ShapeB)parsed).V, Is.EqualTo("world"));
    }

    [Test]
    public void IShape_Roundtrip_ShapeC()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        var original = new ShapeC { V = 99 };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out IShape parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed, Is.InstanceOf<ShapeC>());
        Assert.That(((ShapeC)parsed).V, Is.EqualTo(99));
    }

    [Test]
    public void ShapeBag_Empty()
    {
        Assert.That(SerializerBlocks.TryGet<ShapeBag>(out var block), Is.True);
        int r = block.Scan("100".AsSpan(), 0, out ShapeBag v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Items, Is.Not.Null);
        Assert.That(v.Items.Count, Is.EqualTo(0));
    }

    [Test]
    public void ShapeBag_Mixed()
    {
        Assert.That(SerializerBlocks.TryGet<ShapeBag>(out var block), Is.True);
        int r = block.Scan("100, List(C(42), A(1.5), B(\"hi\"))".AsSpan(), 0, out ShapeBag v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Items.Count, Is.EqualTo(3));
        Assert.That(v.Items[0], Is.InstanceOf<ShapeC>());
        Assert.That(v.Items[1], Is.InstanceOf<ShapeA>());
        Assert.That(v.Items[2], Is.InstanceOf<ShapeB>());
        Assert.That(((ShapeC)v.Items[0]).V, Is.EqualTo(42));
        Assert.That(((ShapeA)v.Items[1]).V, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(((ShapeB)v.Items[2]).V, Is.EqualTo("hi"));
    }

    [Test]
    public void IShape_InvalidInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<IShape>(out var block), Is.True);
        int r = block.Scan("not_a_shape".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }
}
