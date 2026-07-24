using SourceSerializer;
using NUnit.Framework;

/// <summary>
/// 诊断代码覆盖状态。
///
/// SSR001-SSR006 均为编译期 source generator 诊断，无法用 NUnit 运行时测试。
/// 需要 Roslyn 内存编译测试（AdhocWorkspace / CSharpCompilation）来验证触发路径。
/// 本文件记录当前状态和待补缺口。
/// </summary>
public class DiagnosticTests
{
    // SSR001 — Template Parse Error
    //   状态: 现有测试的模板字符串全部合法，无可触发。
    //   待补: Roslyn 内存编译注入非法模板字符串 → 验证 SSR001 触发。

    // SSR002 — Circular template dependency
    //   状态: 现有测试无循环依赖类型。
    //   待补: 内存编译 struct A { B b; } struct B { A a; } → 验证 SSR002 触发。

    // SSR003 — Readonly field
    //   状态: 现有 ReadonlyStructTests 通过构造器绕开该诊断，未验证触发路径。
    //   待补: 内存编译不含构造器的 readonly struct + [Template] → 验证 SSR003 触发。

    // SSR004 — Missing template dependency
    //   触发路径: [Template("<NoTemplate V>")] struct Wrapper { NoTemplate V; }
    //   此处 struct NoTemplate 无 [Template] 且非内置类型 → SSR004 触发。
    //   因触发会导致编译失败，无法用 NUnit 验证。TemplateIgnoreTests 覆盖非触发路径。

    // SSR005 — Scalar inside repetition
    //   触发路径: [Template("<repetition><int V></repetition>")] → SSR005。
    //   现有测试用 #pragma warning disable SSR005 抑制，未验证触发路径。

    // SSR006 — Template ambiguity
    //   触发路径：同接口下 A 模板是 B 前缀（或全等）→ SSR006。
    //   InterfaceDispatchTests 覆盖非触发路径（所有模板互不包含）。

    [Test]
    public void Diagnostic_Gaps_Documented()
    {
        // 本测试仅作为占位，确保诊断缺口已被记录。
        // 所有 SSR00X 触发路径均需 Roslyn 内存编译测试，不在当前 NUnit 覆盖范围内。
        Assert.Pass("SSR001-SSR006 诊断缺口已记录，需 Roslyn 内存编译测试。");
    }
}
