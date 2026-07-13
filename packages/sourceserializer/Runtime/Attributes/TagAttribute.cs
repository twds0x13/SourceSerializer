using System;

namespace SourceSerializer
{
    /// <summary>
    /// 标记枚举成员的字面量标签，供 Source Generator 在编译期生成 switch-on-string 扫描器。
    /// </summary>
    /// <example>
    /// <code>
    /// public enum Element : byte
    /// {
    ///     Physical = 0,
    ///     [Tag("fire")]  Fire,
    ///     [Tag("ice")]   Ice,
    ///     [Tag("magic")] Magic,
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class TagAttribute : Attribute
    {
        /// <summary>标签字符串</summary>
        public string Tag { get; }

        /// <summary>为枚举成员声明字面量标签。</summary>
        public TagAttribute(string tag)
        {
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }
    }
}
