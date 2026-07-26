using System;

namespace SourceSerializer
{
    /// <summary>
    /// 抑制 SSR007 诊断：允许模板中的 string 字段接收未加引号的输入。
    /// 仅在调用方保证输入字符串不含歧义空白符时使用——
    /// 滥用会导致反序列化时静默数据损坏。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class AllowUnquotedStringsAttribute : Attribute
    {
    }
}
