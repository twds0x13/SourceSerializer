using System;

// Partial class — the source generator emits scanner methods
// and registration into this class.
namespace SourceSerializer
{
    internal static partial class SerializerScanners
    {
        /// <summary>
        /// 尝试获取 T 的生成式扫描器。
        /// 若类型上存在 [Template] 且 source generator 已生成对应扫描器则返回 true；
        /// 否则返回 false。
        /// </summary>
        internal static bool TryGetScanner<T>(out ScannerDelegate<T> scanner)
        {
            if (ScannerRegistry<T>.Scanner != null)
            {
                scanner = ScannerRegistry<T>.Scanner;
                return true;
            }
            scanner = null;
            return false;
        }

        /// <summary>每个 T 类型一个注册项。source generator 在静态构造中填充。</summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static class ScannerRegistry<T>
        {
            // Assigned by source-generated static constructor when [Template] is present
#pragma warning disable CS0649
            public static ScannerDelegate<T> Scanner;
#pragma warning restore CS0649
        }
    }

    /// <summary>
    /// Span 扫描器委托签名：成功返回 &gt;pos 的新位置；失败返回 pos 不变。
    /// </summary>
    public delegate int ScannerDelegate<T>(ReadOnlySpan<char> src, int pos, out T value);
}
