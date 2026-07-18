using System;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Readonly struct test types
// ═══════════════════════════════════════════════════════

/// <summary>有匹配构造器 → 构造器路径（贪心优先级 1）</summary>
[Template("<float Attack> <float CritRate>")]
public readonly struct Damage
{
    public readonly float Attack;
    public readonly float CritRate;
    public Damage(float attack, float critRate) { Attack = attack; CritRate = critRate; }
}

/// <summary>构造器参数名与字段名大小写不同 → 映射测试</summary>
[Template("<float X> <float Y>")]
public readonly struct ReadonlyPoint2D
{
    public readonly float X;
    public readonly float Y;
    public ReadonlyPoint2D(float x, float y) { X = x; Y = y; }
}

/// <summary>internal 构造器 → 同程序集可访问</summary>
[Template("<float Value>")]
public readonly struct InternalCtor
{
    public readonly float Value;
    internal InternalCtor(float value) { Value = value; }
}

/// <summary>三字段 readonly struct + 匹配构造器</summary>
[Template("<float Attack> <float CritRate> <float Defense>")]
public readonly struct FullDamage
{
    public readonly float Attack;
    public readonly float CritRate;
    public readonly float Defense;
    public FullDamage(float attack, float critRate, float defense)
    {
        Attack = attack; CritRate = critRate; Defense = defense;
    }
}

// 注：无构造器的 readonly struct 触发 SSR003 编译错误。
// 测试 SSR003 诊断需要 Roslyn 内存编译，不在当前测试覆盖范围。

// ═══════════════════════════════════════════════════════
// Readonly struct tests
// ═══════════════════════════════════════════════════════

public class ReadonlyStructTests
{
    [Test]
    public void ReadonlyStruct_ParsesViaConstructor()
    {
        Assert.That(SerializerBlocks.TryGet<Damage>(out var block), Is.True);
        int r = block.Scan("10.5 0.25".AsSpan(), 0, out Damage v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Attack, Is.EqualTo(10.5f).Within(1e-5f));
        Assert.That(v.CritRate, Is.EqualTo(0.25f).Within(1e-5f));
    }

    [Test]
    public void ReadonlyStruct_TryGetScanner_ReturnsTrue()
    {
        Assert.That(SerializerBlocks.TryGet<Damage>(out _), Is.True);
        Assert.That(SerializerBlocks.TryGet<ReadonlyPoint2D>(out _), Is.True);
    }

    [Test]
    public void ReadonlyStruct_TryGetEmitter_ReturnsTrue()
    {
        Assert.That(SerializerBlocks.TryGet<Damage>(out _), Is.True);
    }

    [Test]
    public void ReadonlyStruct_Emit_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<ReadonlyPoint2D>(out var block), Is.True);
        Assert.That(
        int r = block.Scan("1.5 -3".AsSpan(), 0, out ReadonlyPoint2D parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(parsed.Y, Is.EqualTo(-3f).Within(1e-5f));

        var sb = new StringBuilder();
        block.Emit(sb, parsed);
        Assert.That(sb.ToString(), Is.EqualTo("1.5 -3"));
    }

    [Test]
    public void ReadonlyStruct_InternalCtor_IsMatched()
    {
        Assert.That(SerializerBlocks.TryGet<InternalCtor>(out var block), Is.True);
        int r = block.Scan("42".AsSpan(), 0, out InternalCtor v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Value, Is.EqualTo(42f).Within(1e-5f));
    }

    [Test]
    public void ReadonlyStruct_ThreeFields_ParsesCorrectly()
    {
        Assert.That(SerializerBlocks.TryGet<FullDamage>(out var block), Is.True);
        int r = block.Scan("100 0.5 50".AsSpan(), 0, out FullDamage v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Attack, Is.EqualTo(100f).Within(1e-5f));
        Assert.That(v.CritRate, Is.EqualTo(0.5f).Within(1e-5f));
        Assert.That(v.Defense, Is.EqualTo(50f).Within(1e-5f));
    }

    // ── 往返测试 ──

    [Test]
    public void Damage_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<Damage>(out var block), Is.True);
        Assert.That(        var original = new Damage(100f, 0.25f);
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.Attack, Is.EqualTo(100f).Within(1e-5f));
        Assert.That(parsed.CritRate, Is.EqualTo(0.25f).Within(1e-5f));
    }

    [Test]
    public void FullDamage_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<FullDamage>(out var block), Is.True);
        Assert.That(        var original = new FullDamage(100f, 0.5f, 50f);
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.Attack, Is.EqualTo(100f).Within(1e-5f));
        Assert.That(parsed.CritRate, Is.EqualTo(0.5f).Within(1e-5f));
        Assert.That(parsed.Defense, Is.EqualTo(50f).Within(1e-5f));
    }
}
