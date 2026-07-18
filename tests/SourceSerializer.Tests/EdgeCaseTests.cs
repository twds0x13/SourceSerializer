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
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("-".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Float_PlusSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("+".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Float_DotWithoutLeadingDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan(".5".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Float_DotWithoutTrailingDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("1.".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Int failure paths ──

    [Test]
    public void Int_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<IntOnly>(out var block), Is.True);
        int r = block.Scan("-".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Int_PlusSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<IntOnly>(out var block), Is.True);
        int r = block.Scan("+".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Uint failure paths ──

    [Test]
    public void Uint_Negative_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<UintField>(out var block), Is.True);
        int r = block.Scan("-5".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Uint_NoDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<UintField>(out var block), Is.True);
        int r = block.Scan("x".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Uint_Overflow_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<UintField>(out var block), Is.True);
        int r = block.Scan("99999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Long failure path ──

    [Test]
    public void Long_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<LongOnly>(out var block), Is.True);
        int r = block.Scan("-".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Ulong failure path ──

    [Test]
    public void Ulong_NoDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<UlongOnly>(out var block), Is.True);
        int r = block.Scan("UL".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Double failure paths ──

    [Test]
    public void Double_ExponentWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<DoubleOnly>(out var block), Is.True);
        int r = block.Scan("1.5e".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Double_ExponentSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<DoubleOnly>(out var block), Is.True);
        int r = block.Scan("1.5e+".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── String failure paths ──

    [Test]
    public void String_UnquotedWithPipeTerminator()
    {
        Assert.That(SerializerBlocks.TryGet<StringOnly>(out var block), Is.True);
        int r = block.Scan("hello|".AsSpan(), 0, out StringOnly v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo("hello"));
    }

    [Test]
    public void String_EmptyInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<StringOnly>(out var block), Is.True);
        int r = block.Scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void String_UnclosedQuote_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<StringOnly>(out var block), Is.True);
        int r = block.Scan("\"no closing".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void String_QuoteWithSpaces()
    {
        Assert.That(SerializerBlocks.TryGet<StringOnly>(out var block), Is.True);
        int r = block.Scan("\"hello world\"".AsSpan(), 0, out StringOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo("hello world"));
    }

    // ── Bool ──

    [Test]
    public void Bool_ShortString_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<BoolField>(out var block), Is.True);
        int r = block.Scan("t".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }
}
