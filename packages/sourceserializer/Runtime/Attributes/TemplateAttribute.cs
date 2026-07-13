using System;

namespace SourceSerializer
{
    /// <summary>
    /// 标记 struct 或 class 的序列化模板，供 source generator 在编译期生成解析/序列化代码。
    /// 模板字符串中的 &lt;type fieldname&gt; 指令映射到内置类型或已注册类型的扫描器；
    /// &lt;optional&gt;...&lt;/optional&gt; 包裹可选块；
    /// &lt;repetition&gt;...&lt;/repetition&gt; 包裹零或多次重复块；
    /// 其余裸文字按字面量精确匹配。
    /// </summary>
    /// <example>
    /// <code>
    /// [Template("&lt;float x&gt; &lt;float y&gt; &lt;float z&gt;")]
    /// public struct Vec3 { public float x; public float y; public float z; }
    ///
    /// [Template("&lt;float damage&gt;&lt;repetition&gt;, &lt;float multipliers&gt;&lt;/repetition&gt;")]
    /// public struct DamageData { public float damage; public float multipliers; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class TemplateAttribute : Attribute
    {
        /// <summary>模板字符串（紧凑语法或 XML 格式）</summary>
        public string Template { get; }

        /// <summary>使用模板字符串标记类型。</summary>
        public TemplateAttribute(string template)
        {
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }
    }
}
