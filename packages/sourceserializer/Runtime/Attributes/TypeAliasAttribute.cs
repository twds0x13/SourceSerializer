using System;

namespace SourceSerializer
{
    /// <summary>
    /// 声明一个模板类型别名，映射到内置 C# 值类型。
    /// 例如 <c>[assembly: TypeAlias("Distance", "float")]</c> 允许模板中写 <c>&lt;Distance range&gt;</c>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class TypeAliasAttribute : Attribute
    {
        /// <summary>模板中使用的别名</summary>
        public string Alias { get; }

        /// <summary>映射到的 C# 值类型名</summary>
        public string CSharpType { get; }

        /// <summary>注册类型别名。</summary>
        public TypeAliasAttribute(string alias, string csharpType)
        {
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
            CSharpType = csharpType ?? throw new ArgumentNullException(nameof(csharpType));
        }
    }
}
