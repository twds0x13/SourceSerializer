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
        Assert.That(SerializerScanners.TryGetScanner<DateTime>(out var scan), Is.False);
        Assert.That(scan, Is.Null);
    }

    // ── Float scanner failure paths ──

    [Test]
    public void Float_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("-".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Float_PlusSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("+".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Float_DotWithoutLeadingDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan(".5".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Float_DotWithoutTrailingDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("1.".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Int failure paths ──

    [Test]
    public void Int_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<IntOnly>(out var scan), Is.True);
        int r = scan("-".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Int_PlusSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<IntOnly>(out var scan), Is.True);
        int r = scan("+".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Uint failure paths ──

    [Test]
    public void Uint_Negative_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<UintField>(out var scan), Is.True);
        int r = scan("-5".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Uint_NoDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<UintField>(out var scan), Is.True);
        int r = scan("x".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Uint_Overflow_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<UintField>(out var scan), Is.True);
        int r = scan("99999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Long failure path ──

    [Test]
    public void Long_SignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<LongOnly>(out var scan), Is.True);
        int r = scan("-".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Ulong failure path ──

    [Test]
    public void Ulong_NoDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<UlongOnly>(out var scan), Is.True);
        int r = scan("UL".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Double failure paths ──

    [Test]
    public void Double_ExponentWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<DoubleOnly>(out var scan), Is.True);
        int r = scan("1.5e".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Double_ExponentSignWithoutDigits_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<DoubleOnly>(out var scan), Is.True);
        int r = scan("1.5e+".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── String failure paths ──

    [Test]
    public void String_UnquotedWithPipeTerminator()
    {
        Assert.That(SerializerScanners.TryGetScanner<StringOnly>(out var scan), Is.True);
        int r = scan("hello|".AsSpan(), 0, out StringOnly v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo("hello"));
    }

    [Test]
    public void String_EmptyInput_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<StringOnly>(out var scan), Is.True);
        int r = scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void String_UnclosedQuote_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<StringOnly>(out var scan), Is.True);
        int r = scan("\"no closing".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void String_QuoteWithSpaces()
    {
        Assert.That(SerializerScanners.TryGetScanner<StringOnly>(out var scan), Is.True);
        int r = scan("\"hello world\"".AsSpan(), 0, out StringOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo("hello world"));
    }

    // ── Bool ──

    [Test]
    public void Bool_ShortString_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("t".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }
}
