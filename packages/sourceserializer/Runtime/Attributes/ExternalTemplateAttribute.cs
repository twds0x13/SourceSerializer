using System;

namespace SourceSerializer
{
    /// <summary>
    /// 为无法直接修改源码的第三方类型声明序列化模板。
    /// 可置于 assembly、class 或 struct 上。
    /// </summary>
    /// <example>
    /// <code>
    /// [assembly: ExternalTemplate(typeof(UnityEngine.Vector3),
    ///     "&lt;float x&gt; &lt;float y&gt; &lt;float z&gt;")]
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class ExternalTemplateAttribute : Attribute
    {
        /// <summary>目标类型</summary>
        public Type TargetType { get; }

        /// <summary>模板字符串</summary>
        public string Template { get; }

        /// <summary>为给定类型注册模板。</summary>
        public ExternalTemplateAttribute(Type targetType, string template)
        {
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }
    }
}
