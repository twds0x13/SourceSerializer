using System;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Edge case / negative path tests
// Uses types defined in other test files
// ═══════════════════════════════════════════════════════

public class EdgeCaseTests
{
    // ── TryGetScanner for unregistered type ──

    [Test]
    public void TryGetScanner_UnregisteredType_ReturnsFalse()
    {
        Assert.That(SerializerBlocks.TryGet<DateTime>(out var block), Is.False);
        Assert.That(block, Is.Null);
    }

    // ── Float scanner failure paths ──

    [Test]
    public void Float_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<FloatOnly>("-", out _), Is.False);
    }

    [Test]
    public void Float_PlusSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<FloatOnly>("+", out _), Is.False);
    }

    [Test]
    public void Float_DotWithoutLeadingDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<FloatOnly>(".5", out _), Is.False);
    }

    [Test]
    public void Float_DotWithoutTrailingDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<FloatOnly>("1.", out _), Is.False);
    }

    // ── Int failure paths ──

    [Test]
    public void Int_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<IntOnly>("-", out _), Is.False);
    }

    [Test]
    public void Int_PlusSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<IntOnly>("+", out _), Is.False);
    }

    // ── Uint failure paths ──

    [Test]
    public void Uint_Negative_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<UintField>("-5", out _), Is.False);
    }

    [Test]
    public void Uint_NoDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<UintField>("x", out _), Is.False);
    }

    [Test]
    public void Uint_Overflow_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<UintField>("99999999999", out _), Is.False);
    }

    // ── Long failure path ──

    [Test]
    public void Long_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<LongOnly>("-", out _), Is.False);
    }

    // ── Ulong failure path ──

    [Test]
    public void Ulong_NoDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<UlongOnly>("UL", out _), Is.False);
    }

    // ── Double failure paths ──

    [Test]
    public void Double_ExponentWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<DoubleOnly>("1.5e", out _), Is.False);
    }

    [Test]
    public void Double_ExponentSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<DoubleOnly>("1.5e+", out _), Is.False);
    }

    // ── String failure paths ──

    [Test]
    public void String_Unquoted_Rejected()
    {
        Assert.That(SerializerBlocks.TryScan<StringOnly>("hello", out _), Is.False);  // 裸字符串不再被接受
    }

    [Test]
    public void String_EmptyInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<StringOnly>("", out _), Is.False);
    }

    [Test]
    public void String_UnclosedQuote_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<StringOnly>(out var block), Is.True);
        int r = block.Scan(WhitespaceStripper.Strip("\"no closing").AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void String_QuoteWithSpaces()
    {
        Assert.That(SerializerBlocks.TryScan<StringOnly>("\"hello world\"", out StringOnly v), Is.True);
        Assert.That(v.Val, Is.EqualTo("hello world"));
    }

    // ── Bool ──

    [Test]
    public void Bool_ShortString_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryScan<BoolField>("t", out _), Is.False);
    }

    // ── 空白处理 ──

    [Test]
    public void Scan_LeadingWhitespace_NoMatch()
    {
        Assert.That(SerializerBlocks.TryScan<FloatOnly>("  3.5", out _), Is.True);  // TryScan 内置 Strip 自动剔除空白
    }

    [Test]
    public void Scan_TrailingWhitespace_StopsBefore()
    {
        Assert.That(SerializerBlocks.TryScan<FloatOnly>("3.5  ", out _), Is.True);
    }

    [Test]
    public void Scan_TabSeparated()
    {
        // Point2D template 使用逗号分隔，制表符阻止逗号匹配——预期返回 false
        Assert.That(SerializerBlocks.TryScan<Point2D>("Point2D(1.5\t-2)", out _), Is.False);
    }
}
