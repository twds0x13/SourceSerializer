using System;
using NUnit.Framework;
using SourceSerializer;

[Template("<string Name><optional>, <int Age></optional>")]
public class Person
{
    public string Name;
    public int Age;
}

/// <summary>
/// null 处理：null 字段的 Emit 不崩溃、空输入反序列化。
/// 注意：null 字符串字段 emit 为空后无法再 deserialize（模板要求字段必须存在），
/// 此为 SourceSerializer 模板模型的设计限制，非 bug。
/// </summary>
public class NullHandlingTests
{
    [Test]
    public void Emit_NullStringField_NoCrash()
    {
        Assert.That(SerializerBlocks.TryGet<Person>(out _), Is.True);
        var person = new Person { Name = null!, Age = 0 };
        Assert.That(() => SerializerBlocks.Serialize(person), Throws.Nothing);
    }

    [Test]
    public void Roundtrip_ClassWithValues()
    {
        Assert.That(SerializerBlocks.TryGet<Person>(out _), Is.True);
        var original = new Person { Name = "Alice", Age = 30 };
        string emitted = SerializerBlocks.Serialize(original);
        var parsed = SerializerBlocks.Deserialize<Person>(emitted);
        Assert.That(parsed.Name, Is.EqualTo("Alice"));
        Assert.That(parsed.Age, Is.EqualTo(30));
    }

    [Test]
    public void Deserialize_EmptySpan_ThrowsFormatException()
    {
        Assert.That(SerializerBlocks.TryGet<Person>(out _), Is.True);
        Assert.That(() => SerializerBlocks.Deserialize<Person>(""),
            Throws.InstanceOf<FormatException>());
    }
}
