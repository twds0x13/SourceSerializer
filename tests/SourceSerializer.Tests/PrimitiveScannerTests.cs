using System;
using System.Text;
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
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("42".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(42f));
    }

    [Test]
    public void Float_FSuffix()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("1.5f".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Float_UpperCaseFSuffix()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("1.5F".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Float_UpperCaseDSuffix()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("1.5D".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.EqualTo(1.5f).Within(1e-5f));
    }

    [Test]
    public void Float_ExponentNotHandled_StopsAtE()
    {
        Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var block), Is.True);
        int r = block.Scan("1e3".AsSpan(), 0, out FloatOnly v);
        Assert.That(r, Is.EqualTo(1));
        Assert.That(v.Val, Is.EqualTo(1f));
    }

    // ── Int ──

    [Test]
    public void Int_Overflow_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<IntOnly>(out var block), Is.True);
        int r = block.Scan("99999999999999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Long ──

    [Test]
    public void LongField_ParsesWithLSuffix()
    {
        Assert.That(SerializerBlocks.TryGet<LongField>(out var block), Is.True);
        int r = block.Scan("123L".AsSpan(), 0, out LongField v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.EqualTo(123L));
    }

    [Test]
    public void LongField_ParsesNegative()
    {
        Assert.That(SerializerBlocks.TryGet<LongField>(out var block), Is.True);
        int r = block.Scan("-456".AsSpan(), 0, out LongField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(-456L));
    }

    [Test]
    public void Long_Overflow_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<LongOnly>(out var block), Is.True);
        int r = block.Scan("99999999999999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Ulong ──

    [Test]
    public void UlongField_ParsesWithUlSuffix()
    {
        Assert.That(SerializerBlocks.TryGet<UlongField>(out var block), Is.True);
        int r = block.Scan("123UL".AsSpan(), 0, out UlongField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo(123UL));
    }

    [Test]
    public void UlongField_ParsesWithLowercaseSuffix()
    {
        Assert.That(SerializerBlocks.TryGet<UlongField>(out var block), Is.True);
        int r = block.Scan("456ul".AsSpan(), 0, out UlongField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.EqualTo(456UL));
    }

    [Test]
    public void Ulong_Overflow_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<UlongOnly>(out var block), Is.True);
        int r = block.Scan("9999999999999999999999999".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Short / Ushort / Byte / Sbyte ──

    [Test]
    public void ShortField_Parses()
    {
        Assert.That(SerializerBlocks.TryGet<ShortField>(out var block), Is.True);
        int r = block.Scan("-42".AsSpan(), 0, out ShortField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((short)-42));
    }

    [Test]
    public void UshortField_Parses()
    {
        Assert.That(SerializerBlocks.TryGet<UshortField>(out var block), Is.True);
        int r = block.Scan("7".AsSpan(), 0, out UshortField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((ushort)7));
    }

    [Test]
    public void ByteField_Parses()
    {
        Assert.That(SerializerBlocks.TryGet<ByteField>(out var block), Is.True);
        int r = block.Scan("255".AsSpan(), 0, out ByteField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((byte)255));
    }

    [Test]
    public void SbyteField_ParsesNegative()
    {
        Assert.That(SerializerBlocks.TryGet<SbyteField>(out var block), Is.True);
        int r = block.Scan("-128".AsSpan(), 0, out SbyteField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo((sbyte)-128));
    }

    // ── Bool ──

    [Test]
    public void BoolField_ParsesTrue()
    {
        Assert.That(SerializerBlocks.TryGet<BoolField>(out var block), Is.True);
        int r = block.Scan("true".AsSpan(), 0, out BoolField v);
        Assert.That(r, Is.EqualTo(4));
        Assert.That(v.Val, Is.True);
    }

    [Test]
    public void BoolField_ParsesFalse()
    {
        Assert.That(SerializerBlocks.TryGet<BoolField>(out var block), Is.True);
        int r = block.Scan("false".AsSpan(), 0, out BoolField v);
        Assert.That(r, Is.EqualTo(5));
        Assert.That(v.Val, Is.False);
    }

    [Test]
    public void BoolField_Invalid_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<BoolField>(out var block), Is.True);
        int r = block.Scan("maybe".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Char ──

    [Test]
    public void CharField_ParsesSingle()
    {
        Assert.That(SerializerBlocks.TryGet<CharField>(out var block), Is.True);
        int r = block.Scan("A".AsSpan(), 0, out CharField v);
        Assert.That(r, Is.EqualTo(1));
        Assert.That(v.Val, Is.EqualTo('A'));
    }

    [Test]
    public void CharField_EmptyString_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<CharField>(out var block), Is.True);
        int r = block.Scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Double ──

    [Test]
    public void DoubleField_ParsesWithDecimal()
    {
        Assert.That(SerializerBlocks.TryGet<DoubleField>(out var block), Is.True);
        int r = block.Scan("3.14d".AsSpan(), 0, out DoubleField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(3.14d).Within(1e-9));
    }

    [Test]
    public void DoubleField_WithExponent()
    {
        Assert.That(SerializerBlocks.TryGet<DoubleField>(out var block), Is.True);
        int r = block.Scan("1.5e10".AsSpan(), 0, out DoubleField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5e10));
    }

    [Test]
    public void Double_EPlusExponent()
    {
        Assert.That(SerializerBlocks.TryGet<DoubleOnly>(out var block), Is.True);
        int r = block.Scan("1.5e+10".AsSpan(), 0, out DoubleOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5e10));
    }

    [Test]
    public void Double_EMinusExponent()
    {
        Assert.That(SerializerBlocks.TryGet<DoubleOnly>(out var block), Is.True);
        int r = block.Scan("1.5E-2".AsSpan(), 0, out DoubleOnly v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(1.5e-2));
    }

    // ── Uint ──

    [Test]
    public void UintField_Parses()
    {
        Assert.That(SerializerBlocks.TryGet<UintField>(out var block), Is.True);
        int r = block.Scan("42".AsSpan(), 0, out UintField v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Val, Is.EqualTo(42u));
    }

    [Test] public void FloatOnly_Roundtrip() => Roundtrip(new FloatOnly { Val = 3.5f }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val).Within(1e-5f)));
    [Test] public void IntOnly_Roundtrip() => Roundtrip(new IntOnly { Val = -42 }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void LongOnly_Roundtrip() => Roundtrip(new LongOnly { Val = 123L }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void UlongOnly_Roundtrip() => Roundtrip(new UlongOnly { Val = 99UL }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void DoubleOnly_Roundtrip() => Roundtrip(new DoubleOnly { Val = 3.14d }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val).Within(1e-9)));
    [Test] public void StringOnly_Roundtrip() => Roundtrip(new StringOnly { Val = "hello" }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void LongField_Roundtrip() => Roundtrip(new LongField { Val = -1L }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void UlongField_Roundtrip() => Roundtrip(new UlongField { Val = 1UL }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void ShortField_Roundtrip() => Roundtrip(new ShortField { Val = 7 }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void UshortField_Roundtrip() => Roundtrip(new UshortField { Val = 8 }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void ByteField_Roundtrip() => Roundtrip(new ByteField { Val = 255 }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void SbyteField_Roundtrip() => Roundtrip(new SbyteField { Val = -128 }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void BoolField_Roundtrip() => Roundtrip(new BoolField { Val = true }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void CharField_Roundtrip() => Roundtrip(new CharField { Val = 'X' }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));
    [Test] public void DoubleField_Roundtrip() => Roundtrip(new DoubleField { Val = -0.5d }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val).Within(1e-9)));
    [Test] public void UintField_Roundtrip() => Roundtrip(new UintField { Val = 42u }, (a, b) => Assert.That(a.Val, Is.EqualTo(b.Val)));

    private static void Roundtrip<T>(T original, Action<T, T> assert)
    {
        Assert.That(SerializerBlocks.TryGet<T>(out var b), Is.True);
        var sb = new StringBuilder();
        b.Emit(sb, original);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        assert(original, parsed);
    }

    // ── 原语边界值 ──

    [Test] public void Scan_Int_MinValue() { Assert.That(SerializerBlocks.TryGet<IntOnly>(out var b), Is.True); int r = b.Scan(int.MinValue.ToString().AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(int.MinValue)); }
    [Test] public void Scan_Int_MaxValue() { Assert.That(SerializerBlocks.TryGet<IntOnly>(out var b), Is.True); int r = b.Scan(int.MaxValue.ToString().AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(int.MaxValue)); }
    [Test] public void Scan_Uint_MaxValue() { Assert.That(SerializerBlocks.TryGet<UintField>(out var b), Is.True); int r = b.Scan(uint.MaxValue.ToString().AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(uint.MaxValue)); }
    [Test] public void Scan_Short_MinValue() { Assert.That(SerializerBlocks.TryGet<ShortField>(out var b), Is.True); int r = b.Scan(short.MinValue.ToString().AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(short.MinValue)); }
    [Test] public void Scan_Double_MaxValue() { Assert.That(SerializerBlocks.TryGet<DoubleOnly>(out var b), Is.True); int r = b.Scan(double.MaxValue.ToString("G17").AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(double.MaxValue)); }
    [Test] public void Scan_Double_MinValue() { Assert.That(SerializerBlocks.TryGet<DoubleOnly>(out var b), Is.True); int r = b.Scan(double.MinValue.ToString("G17").AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(double.MinValue)); }
    [Test] public void Scan_Float_NegZero() { Assert.That(SerializerBlocks.TryGet<FloatOnly>(out var b), Is.True); int r = b.Scan("-0".AsSpan(), 0, out var v); Assert.That(r, Is.GreaterThan(0)); Assert.That(v.Val, Is.EqualTo(-0f)); }
}