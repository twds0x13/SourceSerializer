// Partial class — the source generator emits scanner methods
// and registration into this class.
using System;
namespace SourceSerializer
{
    internal static partial class SerializerScanners
    {
        /// <summary>
        /// 尝试获取 TData 的生成式字面量扫描器。
        /// 若 struct 上存在 [Template] 且 source generator 已生成对应扫描器则返回 true；
        /// 否则返回 false。
        /// </summary>
        internal static bool TryGetScanner<TData>(out ScannerDelegate<TData> scanner)
            where TData : unmanaged
        {
            if (ScannerRegistry<TData>.Scanner != null)
            {
                scanner = ScannerRegistry<TData>.Scanner;
                return true;
            }
            scanner = null;
            return false;
        }

        /// <summary>每个 TData 类型一个注册项。source generator 在静态构造中填充。</summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static class ScannerRegistry<TData>
            where TData : unmanaged
        {
            // Assigned by source-generated static constructor when [Template] is present
#pragma warning disable CS0649
            public static ScannerDelegate<TData> Scanner;
#pragma warning restore CS0649
        }
    }

    /// <summary>
    /// Span 扫描器委托签名。成功返回大于 pos 的新位置，失败返回 pos 不变。
    /// </summary>
    public delegate int ScannerDelegate<T>(ReadOnlySpan<char> src, int pos, out T value) where T : unmanaged;
}
