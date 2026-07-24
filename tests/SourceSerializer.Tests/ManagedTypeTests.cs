using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// Managed test types (class + managed struct)
// ═══════════════════════════════════════════════════════

[Template("<string Name>|<float Value>")]
public class NamedValue
{
    public string Name;
    public float Value;
}

[Template("(<NamedValue A>) , (<NamedValue B>)")]
public class Pair
{
    public NamedValue A;
    public NamedValue B;
}

[Template("<float Base><optional>, <List<NamedValue> Mods></optional>")]
public class Modifiable
{
    public float Base;
    public List<NamedValue> Mods;
}

[Template("<string Id> <int Count>")]
public struct InventoryItem
{
    public string Id;
    public int Count;
}

// ═══════════════════════════════════════════════════════
// Managed type tests
// ═══════════════════════════════════════════════════════

public class ManagedTypeTests
{
    [Test]
    public void NamedValue_ParsesStringAndFloat()
    {
        Assert.That(SerializerBlocks.TryGet<NamedValue>(out var block), Is.True);
        int r = block.Scan("sword|3.5".AsSpan(), 0, out NamedValue v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Name, Is.EqualTo("sword"));
        Assert.That(v.Value, Is.EqualTo(3.5f).Within(1e-5f));
    }

    [Test]
    public void NamedValue_QuotedString()
    {
        Assert.That(SerializerBlocks.TryGet<NamedValue>(out var block), Is.True);
        int r = block.Scan("\"fire sword\"|10".AsSpan(), 0, out NamedValue v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Name, Is.EqualTo("fire sword"));
        Assert.That(v.Value, Is.EqualTo(10f));
    }

    [Test]
    public void Pair_ParsesNestedClasses()
    {
        Assert.That(SerializerBlocks.TryGet<Pair>(out var block), Is.True);
        int r = block.Scan("(sword|1) , (shield|2)".AsSpan(), 0, out Pair v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.A.Name, Is.EqualTo("sword"));
        Assert.That(v.A.Value, Is.EqualTo(1f));
        Assert.That(v.B.Name, Is.EqualTo("shield"));
        Assert.That(v.B.Value, Is.EqualTo(2f));
    }

    [Test]
    public void Modifiable_WithManagedList()
    {
        Assert.That(SerializerBlocks.TryGet<Modifiable>(out var block), Is.True);
        int r = block.Scan("100, List(sword|1.5, shield|2.5)".AsSpan(), 0, out Modifiable v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Mods, Is.Not.Null);
        Assert.That(v.Mods.Count, Is.EqualTo(2));
        Assert.That(v.Mods[0].Name, Is.EqualTo("sword"));
        Assert.That(v.Mods[0].Value, Is.EqualTo(1.5f).Within(1e-5f));
        Assert.That(v.Mods[1].Name, Is.EqualTo("shield"));
    }

    [Test]
    public void Modifiable_EmptyMods()
    {
        Assert.That(SerializerBlocks.TryGet<Modifiable>(out var block), Is.True);
        int r = block.Scan("100".AsSpan(), 0, out Modifiable v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Mods, Is.Not.Null);
        Assert.That(v.Mods.Count, Is.EqualTo(0));
    }

    [Test]
    public void InventoryItem_ManagedStruct()
    {
        Assert.That(SerializerBlocks.TryGet<InventoryItem>(out var block), Is.True);
        int r = block.Scan("item001 5".AsSpan(), 0, out InventoryItem v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Id, Is.EqualTo("item001"));
        Assert.That(v.Count, Is.EqualTo(5));
    }

    [Test] public void NamedValue_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<NamedValue>(out var b), Is.True);
        var o = new NamedValue { Name = "sword", Value = 1.5f };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.Name, Is.EqualTo("sword"));
        Assert.That(p.Value, Is.EqualTo(1.5f).Within(1e-5f));
    }
    [Test] public void InventoryItem_Roundtrip() {
        Assert.That(SerializerBlocks.TryGet<InventoryItem>(out var b), Is.True);
        var o = new InventoryItem { Id = "item001", Count = 5 };
        var sb = new StringBuilder(); b.Emit(sb, o);
        int r = b.Scan(sb.ToString().AsSpan(), 0, out var p);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(p.Id, Is.EqualTo("item001"));
        Assert.That(p.Count, Is.EqualTo(5));
    }
}