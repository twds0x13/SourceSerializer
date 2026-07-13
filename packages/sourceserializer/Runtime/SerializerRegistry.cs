using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SourceSerializer
{
    /// <summary>
    /// 内置类型注册表。定义所有 C# 内置 unmanaged 值类型的零分配 span 扫描方法。
    /// </summary>
    public static class SerializerRegistry
    {
        /// <summary>内置类型别名字典</summary>
        internal static readonly Dictionary<string, (string Pattern, string DisplayName)> BuiltinTypes = new()
        {
            ["float"]  = (@"-?\d+(?:\.\d+)?[fFdD]?", "float"),
            ["double"] = (@"-?\d+(?:\.\d+)?[dD]?",     "double"),
            ["int"]    = (@"-?\d+",                     "int"),
            ["uint"]   = (@"\d+",                       "uint"),
            ["long"]   = (@"-?\d+[lL]?",                "long"),
            ["ulong"]  = (@"\d+[uU]?[lL]?",             "ulong"),
            ["short"]  = (@"-?\d+",                     "short"),
            ["ushort"] = (@"\d+",                       "ushort"),
            ["byte"]   = (@"\d+",                       "byte"),
            ["sbyte"]  = (@"-?\d+",                     "sbyte"),
            ["bool"]   = (@"true|false",                "bool"),
            ["char"]   = (@".",                         "char"),
        };

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static bool IsBuiltinType(string alias) => BuiltinTypes.ContainsKey(alias);

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal static string GetScannerMethodName(string alias)
        {
            if (BuiltinTypes.TryGetValue(alias, out var info))
                return $"Scan_{info.DisplayName[0].ToString().ToUpperInvariant()}{info.DisplayName.Substring(1)}";
            return null;
        }

        // ═══════════════════════════════════════════════════════
        // 零分配 Span 扫描方法
        // 签名: static int Scan_Xxx(ReadOnlySpan<char> src, int pos, out Xxx value)
        // ═══════════════════════════════════════════════════════

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Float(ReadOnlySpan<char> src, int pos, out float value)
        {
            value = default;
            if (pos >= src.Length) return pos;
            int start = pos;
            if (src[pos] == '+' || src[pos] == '-') pos++;
            if (pos >= src.Length || !char.IsDigit(src[pos])) return start;
            while (pos < src.Length && char.IsDigit(src[pos])) pos++;
            if (pos < src.Length && src[pos] == '.')
            {
                pos++;
                if (pos >= src.Length || !char.IsDigit(src[pos])) return start;
                while (pos < src.Length && char.IsDigit(src[pos])) pos++;
            }
            int parseEnd = pos;
            if (pos < src.Length && (src[pos] == 'f' || src[pos] == 'F' || src[pos] == 'd' || src[pos] == 'D')) pos++;
#if NET6_0_OR_GREATER
            if (!float.TryParse(src.Slice(start, parseEnd - start), NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return start;
#else
            if (!float.TryParse(src.Slice(start, parseEnd - start).ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return start;
#endif
            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Int(ReadOnlySpan<char> src, int pos, out int value)
        {
            value = default;
            if (pos >= src.Length) return pos;
            int start = pos;
            if (src[pos] == '+' || src[pos] == '-') pos++;
            if (pos >= src.Length || !char.IsDigit(src[pos])) return start;
            while (pos < src.Length && char.IsDigit(src[pos])) pos++;
#if NET6_0_OR_GREATER
            if (!int.TryParse(src.Slice(start, pos - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return start;
#else
            if (!int.TryParse(src.Slice(start, pos - start).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return start;
#endif
            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Double(ReadOnlySpan<char> src, int pos, out double value)
        {
            value = default;
            if (pos >= src.Length) return pos;
            int start = pos;
            if (src[pos] == '+' || src[pos] == '-') pos++;
            if (pos >= src.Length || !char.IsDigit(src[pos])) return start;
            while (pos < src.Length && char.IsDigit(src[pos])) pos++;
            if (pos < src.Length && src[pos] == '.')
            {
                pos++;
                if (pos >= src.Length || !char.IsDigit(src[pos])) return start;
                while (pos < src.Length && char.IsDigit(src[pos])) pos++;
            }
            if (pos < src.Length && (src[pos] == 'e' || src[pos] == 'E'))
            {
                pos++;
                if (pos < src.Length && (src[pos] == '+' || src[pos] == '-')) pos++;
                if (pos >= src.Length || !char.IsDigit(src[pos])) return start;
                while (pos < src.Length && char.IsDigit(src[pos])) pos++;
            }
            int parseEnd = pos;
            if (pos < src.Length && (src[pos] == 'd' || src[pos] == 'D')) pos++;
#if NET6_0_OR_GREATER
            if (!double.TryParse(src.Slice(start, parseEnd - start), NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return start;
#else
            if (!double.TryParse(src.Slice(start, pos - start).ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return start;
#endif
            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Uint(ReadOnlySpan<char> src, int pos, out uint value)
        {
            value = default;
            if (pos >= src.Length) return pos;
            int start = pos;
            if (!char.IsDigit(src[pos])) return start;
            while (pos < src.Length && char.IsDigit(src[pos])) pos++;
#if NET6_0_OR_GREATER
            if (!uint.TryParse(src.Slice(start, pos - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return start;
#else
            if (!uint.TryParse(src.Slice(start, pos - start).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return start;
#endif
            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Bool(ReadOnlySpan<char> src, int pos, out bool value)
        {
            value = default;
            if (pos + 4 <= src.Length && src[pos] == 't' && src[pos + 1] == 'r' && src[pos + 2] == 'u' && src[pos + 3] == 'e')
            { value = true; return pos + 4; }
            if (pos + 5 <= src.Length && src[pos] == 'f' && src[pos + 1] == 'a' && src[pos + 2] == 'l' && src[pos + 3] == 's' && src[pos + 4] == 'e')
            { value = false; return pos + 5; }
            return pos;
        }

        // Delegating scanners (reuse the full implementations above)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Long(ReadOnlySpan<char> src, int pos, out long value)
        {
            value = default;
            int result = Scan_Int(src, pos, out int v);
            if (result > pos)
            {
                value = v;
                pos = result;
                // Consume L/l suffix if present
                if (pos < src.Length && (src[pos] == 'l' || src[pos] == 'L'))
                    pos++;
            }
            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Ulong(ReadOnlySpan<char> src, int pos, out ulong value)
        {
            value = default;
            int result = Scan_Uint(src, pos, out uint v);
            if (result > pos)
            {
                value = v;
                pos = result;
                // Consume U/u suffix if present
                if (pos < src.Length && (src[pos] == 'u' || src[pos] == 'U'))
                    pos++;
                // Consume L/l suffix if present
                if (pos < src.Length && (src[pos] == 'l' || src[pos] == 'L'))
                    pos++;
            }
            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Short(ReadOnlySpan<char> src, int pos, out short value)
        { int r = Scan_Int(src, pos, out int v); value = r > pos ? (short)v : default; return r; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Ushort(ReadOnlySpan<char> src, int pos, out ushort value)
        { int r = Scan_Uint(src, pos, out uint v); value = r > pos ? (ushort)v : default; return r; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Byte(ReadOnlySpan<char> src, int pos, out byte value)
        { int r = Scan_Uint(src, pos, out uint v); value = r > pos ? (byte)v : default; return r; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Sbyte(ReadOnlySpan<char> src, int pos, out sbyte value)
        { int r = Scan_Int(src, pos, out int v); value = r > pos ? (sbyte)v : default; return r; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Scan_Char(ReadOnlySpan<char> src, int pos, out char value)
        { value = default; if (pos >= src.Length) return pos; value = src[pos]; return pos + 1; }
    }
}
