using System;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// 缩进测试类型
// ═══════════════════════════════════════════════════════

[Template("Point(<indent><float X>,<indent><float Y></indent></indent>)")]
public struct NestedPoint
{
    public float X;
    public float Y;
}

[Template("<indent><float A></indent>")]
public struct SingleIndent
{
    public float A;
}

[Template("<indent></indent><float A>")]
public struct EmptyIndent
{
    public float A;
}

[Template(@"
Config(
    <indent>
        <float Base>,
        <optional>
            <indent>
                <float Multiplier>
            </indent>
        </optional>
    </indent>
)")]
public struct ConfigWithOptional
{
    public float Base;
    public float Multiplier;
}

// ═══════════════════════════════════════════════════════
// 缩进 Emit 测试
// ═══════════════════════════════════════════════════════

public class IndentEmitTests
{
    [Test]
    public void NestedPoint_Emit_ProducesIndentedOutput()
    {
        Assert.That(SerializerBlocks.TryGet<NestedPoint>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new NestedPoint { X = 1.5f, Y = 2.0f });
        Assert.That(sb.ToString(), Is.EqualTo("Point(\n\t1.5,\n\t\t2\n\t\n)"));
    }

    [Test]
    public void NestedPoint_Roundtrip()
    {
        var original = new NestedPoint { X = 1.5f, Y = 2.0f };
        var serialized = SerializerBlocks.Serialize(original);
        var parsed = SerializerBlocks.Deserialize<NestedPoint>(serialized);
        Assert.That(parsed.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(parsed.Y, Is.EqualTo(2.0f).Within(1e-5f));
    }

    [Test]
    public void NestedPoint_Scan_IgnoresIndent()
    {
        Assert.That(SerializerBlocks.TryGet<NestedPoint>(out var block), Is.True);
        // Scan 路径无视缩进——输入不需要也不应该有 \n \t
        using var compact = new WhitespaceStripper("Point(1.5,2.0)");
        int r = block.Scan(compact.Span, 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.X, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Y, Is.EqualTo(2.0f).Within(1e-5f));
    }

    [Test]
    public void SingleIndent_Emit()
    {
        Assert.That(SerializerBlocks.TryGet<SingleIndent>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new SingleIndent { A = 42f });
        Assert.That(sb.ToString(), Is.EqualTo("\n\t42\n"));
    }

    [Test]
    public void SingleIndent_Roundtrip()
    {
        var original = new SingleIndent { A = 42f };
        var serialized = SerializerBlocks.Serialize(original);
        var parsed = SerializerBlocks.Deserialize<SingleIndent>(serialized);
        Assert.That(parsed.A, Is.EqualTo(42f).Within(1e-5f));
    }

    [Test]
    public void EmptyIndent_ProducesOnlyNewline()
    {
        Assert.That(SerializerBlocks.TryGet<EmptyIndent>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new EmptyIndent { A = 5f });
        // <indent></indent> 空 body: 开口 \n\t + 闭口 \n + 字段 5
        Assert.That(sb.ToString(), Is.EqualTo("\n\t\n5"));
    }

    [Test]
    public void EmptyIndent_Roundtrip()
    {
        var original = new EmptyIndent { A = 5f };
        var serialized = SerializerBlocks.Serialize(original);
        var parsed = SerializerBlocks.Deserialize<EmptyIndent>(serialized);
        Assert.That(parsed.A, Is.EqualTo(5f).Within(1e-5f));
    }

    [Test]
    public void ConfigWithOptional_WithoutMultiplier()
    {
        Assert.That(SerializerBlocks.TryGet<ConfigWithOptional>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new ConfigWithOptional { Base = 100f, Multiplier = 0f });
        // optional 不满足 → 跳过缩进块。空格来自 @"" 模板中的字面文本
        Assert.That(sb.ToString(), Is.EqualTo("Config( \n\t 100,  \n )"));
    }

    [Test]
    public void ConfigWithOptional_WithMultiplier_Roundtrip()
    {
        var original = new ConfigWithOptional { Base = 100f, Multiplier = 1.5f };
        var serialized = SerializerBlocks.Serialize(original);
        var parsed = SerializerBlocks.Deserialize<ConfigWithOptional>(serialized);
        Assert.That(parsed.Base, Is.EqualTo(100f).Within(1e-5f));
        Assert.That(parsed.Multiplier, Is.EqualTo(1.5f).Within(1e-5f));
    }

}
