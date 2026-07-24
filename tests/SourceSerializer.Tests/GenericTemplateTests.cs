using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// 用户自定义泛型类型 — 测试类型定义
// ═══════════════════════════════════════════════════════

/// <summary>单类型参数 struct 包装器（unmanaged 约束）</summary>
[Template("<T Value>")]
public struct Wrapper<T> where T : unmanaged
{
    public T Value;
}

/// <summary>使用 Wrapper&lt;float&gt; 的容器</summary>
[Template("<Wrapper<float> W>")]
public struct UsesWrapper
{
    public Wrapper<float> W;
}

/// <summary>双类型参数 struct（类似 ValueTuple）</summary>
[Template("<T1 First>, <T2 Second>")]
public struct Pair<T1, T2>
    where T1 : unmanaged
    where T2 : unmanaged
{
    public T1 First;
    public T2 Second;
}

/// <summary>使用 Pair&lt;float, int&gt; 的容器</summary>
[Template("<Pair<float,int> P>")]
public struct UsesPair
{
    public Pair<float, int> P;
}

/// <summary>managed 类型参数 — class（验证 NeedsHeapAlloc/NeedsWalkPhase）</summary>
[Template("<T Value>")]
public class Box<T>
{
    public T Value;
}

/// <summary>使用 Box&lt;string&gt; 的容器</summary>
[Template("<Box<string> B>")]
public class UsesBox
{
    public Box<string> B;
}

/// <summary>使用 Pair&lt;int,bool&gt; 的容器</summary>
[Template("<Pair<int,bool> P>")]
public struct UsesPairIntBool
{
    public Pair<int, bool> P;
}

/// <summary>使用 Pair&lt;double,long&gt; 的容器</summary>
[Template("<Pair<double,long> P>")]
public struct UsesPairDoubleLong
{
    public Pair<double, long> P;
}

/// <summary>使用 Wrapper&lt;int&gt; 的容器</summary>
[Template("<Wrapper<int> W>")]
public struct UsesWrapperInt
{
    public Wrapper<int> W;
}

/// <summary>使用 List&lt;Wrapper&lt;float&gt;&gt; 的容器（嵌套泛型传递闭包）</summary>
[Template("<List<Wrapper<float>> Items>")]
public struct HasListOfWrapper
{
    public List<Wrapper<float>> Items;
}

[Template("<HashSet<float> Items>")]
public struct UsesHashSet { public HashSet<float> Items; }

[Template("<string Name>")]
public struct UsesString { public string Name; }

[Template("<List<HashSet<float>> Nested>")]
public struct HasNestedHashSet { public List<HashSet<float>> Nested; }

// ═══════════════════════════════════════════════════════
// 用户自定义泛型类型 — 测试
// ═══════════════════════════════════════════════════════

public class GenericTemplateTests
{
    // ── 单类型参数：Wrapper<float> ──

    [Test]
    public void Wrapper_Float_Scan()
    {
        Assert.That(SerializerBlocks.TryGet<UsesWrapper>(out var block), Is.True);
        int r = block.Scan("3.5".AsSpan(), 0, out UsesWrapper v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.W.Value, Is.EqualTo(3.5f).Within(1e-5f));
    }

    [Test]
    public void Wrapper_Float_Emit()
    {
        Assert.That(SerializerBlocks.TryGet<UsesWrapper>(out var block), Is.True);
        var sb = new StringBuilder();
        var val = new UsesWrapper { W = new Wrapper<float> { Value = 3.5f } };
        block.Emit(sb, val);
        Assert.That(sb.ToString(), Is.EqualTo("3.5"));
    }

    [Test]
    public void Wrapper_Float_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<UsesWrapper>(out var block), Is.True);

        var original = new UsesWrapper { W = new Wrapper<float> { Value = -1.5f } };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.W.Value, Is.EqualTo(-1.5f).Within(1e-5f));
    }

    // ── 双类型参数：Pair<float, int> ──

    [Test]
    public void Pair_FloatInt_Scan()
    {
        Assert.That(SerializerBlocks.TryGet<UsesPair>(out var block), Is.True);
        int r = block.Scan("3.5, 42".AsSpan(), 0, out UsesPair v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.P.First, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v.P.Second, Is.EqualTo(42));
    }

    [Test]
    public void Pair_FloatInt_Emit()
    {
        Assert.That(SerializerBlocks.TryGet<UsesPair>(out var block), Is.True);
        var sb = new StringBuilder();
        var val = new UsesPair { P = new Pair<float, int> { First = 3.5f, Second = 42 } };
        block.Emit(sb, val);
        Assert.That(sb.ToString(), Is.EqualTo("3.5, 42"));
    }

    [Test]
    public void Pair_FloatInt_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<UsesPair>(out var block), Is.True);

        var original = new UsesPair { P = new Pair<float, int> { First = 7.5f, Second = -3 } };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.P.First, Is.EqualTo(7.5f).Within(1e-5f));
        Assert.That(parsed.P.Second, Is.EqualTo(-3));
    }

    // ── Managed 类型参数：Box<string> ──

    [Test]
    public void Box_String_Scan()
    {
        Assert.That(SerializerBlocks.TryGet<UsesBox>(out var block), Is.True);
        int r = block.Scan("hello".AsSpan(), 0, out UsesBox v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.B.Value, Is.EqualTo("hello"));
    }

    [Test]
    public void Box_String_Emit()
    {
        Assert.That(SerializerBlocks.TryGet<UsesBox>(out var block), Is.True);
        var sb = new StringBuilder();
        var val = new UsesBox { B = new Box<string> { Value = "world" } };
        block.Emit(sb, val);
        Assert.That(sb.ToString(), Is.EqualTo("\"world\""));
    }

    // ── 不同内置类型组合 ──

    [Test]
    public void Pair_IntBool_Scan()
    {
        // Pair<int,bool> — 整数和布尔组合
        Assert.That(SerializerBlocks.TryGet<Pair<int, bool>>(out var block), Is.True);
        int r = block.Scan("5, true".AsSpan(), 0, out Pair<int, bool> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.First, Is.EqualTo(5));
        Assert.That(v.Second, Is.True);
    }

    [Test]
    public void Pair_DoubleLong_Scan()
    {
        // Pair<double,long> — 更多内置类型组合
        Assert.That(SerializerBlocks.TryGet<Pair<double, long>>(out var block), Is.True);
        int r = block.Scan("3.14, -999".AsSpan(), 0, out Pair<double, long> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.First, Is.EqualTo(3.14d).Within(1e-9));
        Assert.That(v.Second, Is.EqualTo(-999L));
    }

    // ── 直接使用合成泛型类型（不通过容器） ──

    [Test]
    public void Wrapper_Float_Scan_Direct()
    {
        Assert.That(SerializerBlocks.TryGet<Wrapper<float>>(out var block), Is.True);
        int r = block.Scan("99".AsSpan(), 0, out Wrapper<float> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Value, Is.EqualTo(99f).Within(1e-5f));
    }

    [Test]
    public void Pair_FloatInt_Scan_Direct()
    {
        Assert.That(SerializerBlocks.TryGet<Pair<float, int>>(out var block), Is.True);
        int r = block.Scan("1.5, 10".AsSpan(), 0, out Pair<float, int> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.First, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Second, Is.EqualTo(10));
    }

    // ── 嵌套泛型（List<Wrapper<float>> 传递闭包） ──

    [Test]
    public void ListOfWrapper_Float_Scan()
    {
        Assert.That(SerializerBlocks.TryGet<List<Wrapper<float>>>(out var block), Is.True);
        int r = block.Scan("List(3.5, 7, -1)".AsSpan(), 0, out List<Wrapper<float>> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Count, Is.EqualTo(3));
        Assert.That(v[0].Value, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v[1].Value, Is.EqualTo(7f).Within(1e-5f));
        Assert.That(v[2].Value, Is.EqualTo(-1f).Within(1e-5f));
    }

    [Test]
    public void ListOfWrapper_Float_Empty()
    {
        Assert.That(SerializerBlocks.TryGet<List<Wrapper<float>>>(out var block), Is.True);
        int r = block.Scan("List()".AsSpan(), 0, out List<Wrapper<float>> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Count, Is.EqualTo(0));
    }

    // ── Optional 块内的泛型字段 ──

    [Test]
    public void Pair_FloatInt_InOptional_Present()
    {
        Assert.That(SerializerBlocks.TryGet<Pair<float, int>>(out var block), Is.True);
        int r = block.Scan("3.5, 10".AsSpan(), 0, out Pair<float, int> v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.First, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(v.Second, Is.EqualTo(10));
    }

    [Test]
    public void Pair_FloatInt_InvalidInput()
    {
        Assert.That(SerializerBlocks.TryGet<Pair<float, int>>(out var block), Is.True);
        int r = block.Scan("not_a_number".AsSpan(), 0, out Pair<float, int> v);
        Assert.That(r, Is.EqualTo(0));
    }

    // ── Box<class> 序列化往返 ──

    [Test]
    public void Box_String_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<UsesBox>(out var block), Is.True);

        var original = new UsesBox { B = new Box<string> { Value = "roundtrip" } };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.B.Value, Is.EqualTo("roundtrip"));
    }

    // ── Wrapper<int> 直接 Emit + 往返 ──

    [Test]
    public void Wrapper_Int_EmitDirect()
    {
        Assert.That(SerializerBlocks.TryGet<Wrapper<int>>(out var block), Is.True);
        var sb = new StringBuilder();
        block.Emit(sb, new Wrapper<int> { Value = 42 });
        Assert.That(sb.ToString(), Is.EqualTo("42"));
    }

    [Test]
    public void Wrapper_Int_RoundtripDirect()
    {
        Assert.That(SerializerBlocks.TryGet<Wrapper<int>>(out var block), Is.True);

        var original = new Wrapper<int> { Value = -7 };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out Wrapper<int> parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.Value, Is.EqualTo(-7));
    }

    // ── 新容器类型 Emit + 往返 ──

    [Test]
    public void Pair_IntBool_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<UsesPairIntBool>(out var block), Is.True);

        var original = new UsesPairIntBool { P = new Pair<int, bool> { First = 1, Second = true } };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.P.First, Is.EqualTo(1));
        Assert.That(parsed.P.Second, Is.True);
    }

    [Test]
    public void Pair_DoubleLong_Roundtrip()
    {
        Assert.That(SerializerBlocks.TryGet<UsesPairDoubleLong>(out var block), Is.True);

        var original = new UsesPairDoubleLong { P = new Pair<double, long> { First = 6.28d, Second = 123L } };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.P.First, Is.EqualTo(6.28d).Within(1e-9));
        Assert.That(parsed.P.Second, Is.EqualTo(123L));
    }

    [Test]
    public void Wrapper_Int_RoundtripWithContainer()
    {
        Assert.That(SerializerBlocks.TryGet<UsesWrapperInt>(out var block), Is.True);

        var original = new UsesWrapperInt { W = new Wrapper<int> { Value = -7 } };
        var sb = new StringBuilder();
        block.Emit(sb, original);
        int r = block.Scan(sb.ToString().AsSpan(), 0, out var parsed);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(parsed.W.Value, Is.EqualTo(-7));
    }

    // ── Wrapper<float> 失败路径 ──

    [Test]
    public void Wrapper_Float_InvalidInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<Wrapper<float>>(out var block), Is.True);
        int r = block.Scan("abc".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test]
    public void Wrapper_Float_EmptyInput_ReturnsStart()
    {
        Assert.That(SerializerBlocks.TryGet<Wrapper<float>>(out var block), Is.True);
        int r = block.Scan("".AsSpan(), 0, out _);
        Assert.That(r, Is.EqualTo(0));
    }

    [Test] public void HasListOfWrapper_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<HasListOfWrapper>(out var b), Is.True);
        var o = new HasListOfWrapper { Items = new List<Wrapper<float>> { new Wrapper<float> { Value = 1.5f }, new Wrapper<float> { Value = 2.5f } } };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.Items, Has.Count.EqualTo(2));
    }
    [Test] public void UsesHashSet_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<UsesHashSet>(out var b), Is.True);
        var o = new UsesHashSet { Items = new HashSet<float> { 1.5f, 2.5f } };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.Items, Is.EquivalentTo(o.Items));
    }
    [Test] public void UsesString_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<UsesString>(out var b), Is.True);
        var o = new UsesString { Name = "hello" };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.Name, Is.EqualTo("hello"));
    }
    [Test] public void HasNestedHashSet_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<HasNestedHashSet>(out var b), Is.True);
        var o = new HasNestedHashSet { Nested = new List<HashSet<float>> { new HashSet<float> { 1.5f, 2.5f } } };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.Nested, Has.Count.EqualTo(1));
    }
    [Test] public void List_Wrapper_float_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<List<Wrapper<float>>>(out var b), Is.True);
        var o = new List<Wrapper<float>> { new Wrapper<float> { Value = 1.5f }, new Wrapper<float> { Value = 2.5f } };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p, Has.Count.EqualTo(2));
    }
    [Test] public void HashSet_float_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<HashSet<float>>(out var b), Is.True);
        var o = new HashSet<float> { 1.5f, 2.5f };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p, Is.EquivalentTo(o));
    }
    [Test] public void List_HashSet_float_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<List<HashSet<float>>>(out var b), Is.True);
        var o = new List<HashSet<float>> { new HashSet<float> { 1.5f, 2.5f } };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p, Has.Count.EqualTo(1));
    }
    [Test] public void Block_Scan() {
        Assert.That(SerializerBlocks.TryGet<UsesWrapper>(out var b), Is.True);
        int r = b.Scan("3.5".AsSpan(), 0, out var v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.W.Value, Is.EqualTo(3.5f).Within(1e-5f));
    }
    [Test] public void Block_Emit() {
        Assert.That(SerializerBlocks.TryGet<UsesWrapper>(out var b), Is.True);
        var o = new UsesWrapper { W = new Wrapper<float> { Value = 3.5f } };
        var sb = new StringBuilder(); b.Emit(sb, o);
        Assert.That(sb.ToString(), Is.EqualTo("3.5"));
    }
    [Test] public void Block_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<Point2D>(out var b), Is.True);
        var o = new Point2D { X = 3.5f, Y = -2.1f };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.X, Is.EqualTo(3.5f).Within(1e-5f));
        Assert.That(p.Y, Is.EqualTo(-2.1f).Within(1e-5f));
    }
    [Test] public void Serialize_Deserialize_Roundtrip() {
        var o = new UsesWrapper { W = new Wrapper<float> { Value = 3.5f } };
        string text = SerializerBlocks.Serialize(o);
        Assert.That(text, Is.EqualTo("3.5"));
        var loaded = SerializerBlocks.Deserialize<UsesWrapper>(text);
        Assert.That(loaded.W.Value, Is.EqualTo(3.5f).Within(1e-5f));
    }
}