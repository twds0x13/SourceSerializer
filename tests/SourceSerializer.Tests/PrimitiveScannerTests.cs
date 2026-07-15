using System;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Single-field types for precise built-in scanner testing
// ═══════════════════════════════════════════════════════

[Template("<float Val>")]
public struct FloatOnly { public float Val; }

[Template("<int Val>")]
public struct IntOnly { public int Val; }

[Template("<long Val>")]
public struct LongOnly { public long Val; }

[Template("<ulong Val>")]
public struct UlongOnly { public ulong Val; }

[Template("<double Val>")]
public struct DoubleOnly { public double Val; }

[Template("<string Val>")]
public struct StringOnly { public string Val; }

[Template("<long Val>")]
public struct LongField { public long Val; }

[Template("<ulong Val>")]
public struct UlongField { public ulong Val; }

[Template("<short Val>")]
public struct ShortField { public short Val; }

[Template("<ushort Val>")]
public struct UshortField { public ushort Val; }

[Template("<byte Val>")]
public struct ByteField { public byte Val; }

[Template("<sbyte Val>")]
public struct SbyteField { public sbyte Val; }

[Template("<bool Val>")]
public struct BoolField { public bool Val; }

[Template("<char Val>")]
public struct CharField { public char Val; }

[Template("<double Val>")]
public struct DoubleField { public double Val; }

[Template("<uint Val>")]
public struct UintField { public uint Val; }

// ═══════════════════════════════════════════════════════
// Primitive scanner tests
// ═══════════════════════════════════════════════════════

public class PrimitiveScannerTests
{
    // ── Float ──

    [Test]
    public void Float_AtEndOfInput()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("42".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(42f));
    }

    [Test]
    public void Float_FSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("1.5f".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Float_UpperCaseFSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("1.5F".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Float_UpperCaseDSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("1.5D".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Float_ExponentNotHandled_StopsAtE()
    {
        Assert.That(SerializerScanners.TryGetScanner<FloatOnly>(out var scan), Is.True);
        int r = scan("1e3".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.EqualTo(1));
        Assert.That(v.Val, Is.EqualTo(1f));
    }

    // ── Int ──

    [Test]
    public void Int_Overflow_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<IntOnly>(out var scan), Is.True);
        int r = scan("99999999999999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Long ──

    [Test]
    public void LongField_ParsesWithLSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<LongField>(out var scan), Is.True);
        int r = scan("123L".AsSpan(), 0, out LongField v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.EqualTo(123L));
    }

    [Test]
    public void LongField_ParsesNegative()
    {
        Assert.That(SerializerScanners.TryGetScanner<LongField>(out var scan), Is.True);
        int r = scan("-456".AsSpan(), 0, out LongField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(-456L));
    }

    [Test]
    public void Long_Overflow_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<LongOnly>(out var scan), Is.True);
        int r = scan("99999999999999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Ulong ──

    [Test]
    public void UlongField_ParsesWithUlSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<UlongField>(out var scan), Is.True);
        int r = scan("123UL".AsSpan(), 0, out UlongField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo(123UL));
    }

    [Test]
    public void UlongField_ParsesWithLowercaseSuffix()
    {
        Assert.That(SerializerScanners.TryGetScanner<UlongField>(out var scan), Is.True);
        int r = scan("456ul".AsSpan(), 0, out UlongField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo(456UL));
    }

    [Test]
    public void Ulong_Overflow_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<UlongOnly>(out var scan), Is.True);
        int r = scan("9999999999999999999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Short / Ushort / Byte / Sbyte ──

    [Test]
    public void ShortField_Parses()
    {
        Assert.That(SerializerScanners.TryGetScanner<ShortField>(out var scan), Is.True);
        int r = scan("-42".AsSpan(), 0, out ShortField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((short)-42));
    }

    [Test]
    public void UshortField_Parses()
    {
        Assert.That(SerializerScanners.TryGetScanner<UshortField>(out var scan), Is.True);
        int r = scan("7".AsSpan(), 0, out UshortField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((ushort)7));
    }

    [Test]
    public void ByteField_Parses()
    {
        Assert.That(SerializerScanners.TryGetScanner<ByteField>(out var scan), Is.True);
        int r = scan("255".AsSpan(), 0, out ByteField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((byte)255));
    }

    [Test]
    public void SbyteField_ParsesNegative()
    {
        Assert.That(SerializerScanners.TryGetScanner<SbyteField>(out var scan), Is.True);
        int r = scan("-128".AsSpan(), 0, out SbyteField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((sbyte)-128));
    }

    // ── Bool ──

    [Test]
    public void BoolField_ParsesTrue()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("true".AsSpan(), 0, out BoolField v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.True);
    }

    [Test]
    public void BoolField_ParsesFalse()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("false".AsSpan(), 0, out BoolField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.False);
    }

    [Test]
    public void BoolField_Invalid_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<BoolField>(out var scan), Is.True);
        int r = scan("maybe".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Char ──

    [Test]
    public void CharField_ParsesSingle()
    {
        Assert.That(SerializerScanners.TryGetScanner<CharField>(out var scan), Is.True);
        int r = scan("A".AsSpan(), 0, out CharField v);
        Assert.That(r, Is.EqualTo(1));
        Assert.That(v.Val, Is.EqualTo('A'));
    }

    [Test]
    public void CharField_EmptyString_ReturnsStart()
    {
        Assert.That(SerializerScanners.TryGetScanner<CharField>(out var scan), Is.True);
        int r = scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Double ──

    [Test]
    public void DoubleField_ParsesWithDecimal()
    {
        Assert.That(SerializerScanners.TryGetScanner<DoubleField>(out var scan), Is.True);
        int r = scan("3.14d".AsSpan(), 0, out DoubleField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(3.14d).Within(1e-9));
    }

    [Test]
    public void DoubleField_WithExponent()
    {
        Assert.That(SerializerScanners.TryGetScanner<DoubleField>(out var scan), Is.True);
        int r = scan("1.5e10".AsSpan(), 0, out DoubleField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5e10));
    }

    [Test]
    public void Double_EPlusExponent()
    {
        Assert.That(SerializerScanners.TryGetScanner<DoubleOnly>(out var scan), Is.True);
        int r = scan("1.5e+10".AsSpan(), 0, out DoubleOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5e10));
    }

    [Test]
    public void Double_EMinusExponent()
    {
        Assert.That(SerializerScanners.TryGetScanner<DoubleOnly>(out var scan), Is.True);
        int r = scan("1.5E-2".AsSpan(), 0, out DoubleOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5e-2));
    }

    // ── Uint ──

    [Test]
    public void UintField_Parses()
    {
        Assert.That(SerializerScanners.TryGetScanner<UintField>(out var scan), Is.True);
        int r = scan("42".AsSpan(), 0, out UintField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(42u));
    }
}
