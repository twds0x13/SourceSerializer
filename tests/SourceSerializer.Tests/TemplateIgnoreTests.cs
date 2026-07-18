using System;
using NUnit.Framework;
using SourceSerializer;

// ═══════════════════════════════════════════════════════
// [TemplateIgnore] 与 SSR004 Error 测试
// ═══════════════════════════════════════════════════════

/// <summary>未注册的类型，故意不加 [Template]</summary>
public struct UnregisteredType
{
    public float X;
}

/// <summary>[TemplateIgnore] 字段不出现在模板中——正常编译</summary>
[Template("<float Value>")]
public struct ContainerWithIgnoredField
{
    public float Value;
    [TemplateIgnore] public UnregisteredType InternalData;
}

/// <summary>无 [TemplateIgnore]——模板引用未注册类型——应触发 SSR004 Error</summary>
// [Template("<float Value>|<UnregisteredType Extra>")]  // 取消注释会导致编译失败
public struct ContainerWithoutIgnore
{
    public float Value;
    public UnregisteredType Extra;
}

public class TemplateIgnoreTests
{
    // ── [TemplateIgnore] 正常跳过字段 ──

    [Test]
    public void IgnoredField_SkipsSerialization()
    {
        Assert.That(SerializerBlocks.TryGet<ContainerWithIgnoredField>(out var block), Is.True);
        int pos = block.Scan("3.5".AsSpan(), 0, out ContainerWithIgnoredField v);
        Assert.That(pos, Is.GreaterThan(0));
        Assert.That(v.Value, Is.EqualTo(3.5f));
        Assert.That(v.InternalData.X, Is.EqualTo(0f)); // default, 未被扫描
    }

    // ── 无条件 [TemplateIgnore] 且模板不引用被忽略类型——不触发 SSR004 ──

    [Test]
    public void IgnoredField_DoesNotTriggerSSR004()
    {
        // 如果 SSR004 触发，编译会失败，此测试根本不会运行
        Assert.That(SerializerBlocks.TryGet<ContainerWithIgnoredField>(out _), Is.True);
    }

    // ── 无 [TemplateIgnore] 但有 Template —— 正常的反序列化工作 ──

    [Test]
    public void TryGetScanner_RegisteredType_ReturnsTrue()
    {
        Assert.That(SerializerBlocks.TryGet<ContainerWithIgnoredField>(out var block), Is.True);
        Assert.That(block, Is.Not.Null);
    }
}
