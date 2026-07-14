using System;
using System.Collections.Generic;
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
        Assert.That(SerializerScanners.TryGetScanner<NamedValue>(out var scan), Is.True);
        int r = scan("sword|3.5".AsSpan(), 0, out NamedValue v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Name, Is.EqualTo("sword"));
        Assert.That(v.Value, Is.EqualTo(3.5f).Within(1e-5f));
    }

    [Test]
    public void NamedValue_QuotedString()
    {
        Assert.That(SerializerScanners.TryGetScanner<NamedValue>(out var scan), Is.True);
        int r = scan("\"fire sword\"|10".AsSpan(), 0, out NamedValue v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Name, Is.EqualTo("fire sword"));
        Assert.That(v.Value, Is.EqualTo(10f));
    }

    [Test]
    public void Pair_ParsesNestedClasses()
    {
        Assert.That(SerializerScanners.TryGetScanner<Pair>(out var scan), Is.True);
        int r = scan("(sword|1) , (shield|2)".AsSpan(), 0, out Pair v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.A.Name, Is.EqualTo("sword"));
        Assert.That(v.A.Value, Is.EqualTo(1f));
        Assert.That(v.B.Name, Is.EqualTo("shield"));
        Assert.That(v.B.Value, Is.EqualTo(2f));
    }

    [Test]
    public void Modifiable_WithManagedList()
    {
        Assert.That(SerializerScanners.TryGetScanner<Modifiable>(out var scan), Is.True);
        int r = scan("100, sword|1.5, shield|2.5".AsSpan(), 0, out Modifiable v);
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
        Assert.That(SerializerScanners.TryGetScanner<Modifiable>(out var scan), Is.True);
        int r = scan("100".AsSpan(), 0, out Modifiable v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Base, Is.EqualTo(100f));
        Assert.That(v.Mods, Is.Not.Null);
        Assert.That(v.Mods.Count, Is.EqualTo(0));
    }

    [Test]
    public void InventoryItem_ManagedStruct()
    {
        Assert.That(SerializerScanners.TryGetScanner<InventoryItem>(out var scan), Is.True);
        int r = scan("item001 5".AsSpan(), 0, out InventoryItem v);
        Assert.That(r, Is.GreaterThan(0));
        Assert.That(v.Id, Is.EqualTo("item001"));
        Assert.That(v.Count, Is.EqualTo(5));
    }
}
