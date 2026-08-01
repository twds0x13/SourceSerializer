using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SourceSerializer;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromTypes(new[]
        {
            typeof(ScanAllocationBenchmarks),
            typeof(ScanThroughputBenchmarks),
        }).Run(args);
    }
}

// ═══════════════════════════════════════════════════════════════
// Scan 分配基准：三层对比 + 四种内置类型 + 一种 composite
// 裸 Registry → Block Scan（接口分发）→ TryScan（完整路径）
// ═══════════════════════════════════════════════════════════════

[ShortRunJob]
[MemoryDiagnoser]
public class ScanAllocationBenchmarks
{
    // ── 预取的 ISerializerBlock 实例 ──
    private ISerializerBlock<int> _blockInt;
    private ISerializerBlock<float> _blockFloat;
    private ISerializerBlock<bool> _blockBool;
    private ISerializerBlock<string> _blockString;
    private ISerializerBlock<Point2D> _blockPoint2D;

    // ── 输入字符串 ──
    private const string IntInput = "42";
    private const string FloatInput = "3.14";
    private const string BoolInput = "true";
    private const string StringInput = "\"hello\"";
    private const string Point2DInput = "Point2D(1.5, 2.5)";

    // ── 预 Strip 后的 compact string（供 BlockScan 直接使用）──
    private string _compactInt;
    private string _compactFloat;
    private string _compactBool;
    private string _compactString;
    private string _compactPoint2D;

    [GlobalSetup]
    public void Setup()
    {
        // 触发 EnsureInitialized（排除冷启动反射 noise）
        SerializerBlocks.TryScan<int>("0", out _);

        // 预取各类型的 ISerializerBlock
        SerializerBlocks.TryGet<int>(out _blockInt);
        SerializerBlocks.TryGet<float>(out _blockFloat);
        SerializerBlocks.TryGet<bool>(out _blockBool);
        SerializerBlocks.TryGet<string>(out _blockString);
        SerializerBlocks.TryGet<Point2D>(out _blockPoint2D);

        // 预 Strip——排除 BlockScan 方法内的 Strip 分配
        _compactInt = WhitespaceStripper.Strip(IntInput);
        _compactFloat = WhitespaceStripper.Strip(FloatInput);
        _compactBool = WhitespaceStripper.Strip(BoolInput);
        _compactString = WhitespaceStripper.Strip(StringInput);
        _compactPoint2D = WhitespaceStripper.Strip(Point2DInput);
    }

    // ── 裸 Registry 静态调用（零分配基线）──

    [Benchmark(Baseline = true)]
    public int RegistryScan_Int()
    {
        SerializerRegistry.Scan_Int(_compactInt.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float RegistryScan_Float()
    {
        SerializerRegistry.Scan_Float(_compactFloat.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool RegistryScan_Bool()
    {
        SerializerRegistry.Scan_Bool(_compactBool.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string RegistryScan_String()
    {
        SerializerRegistry.Scan_String(_compactString.AsSpan(), 0, out string v);
        return v;
    }

    // ── Block Scan（接口分发，不含 Strip/TryGet）──

    [Benchmark]
    public int BlockScan_Int()
    {
        _blockInt.Scan(_compactInt.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float BlockScan_Float()
    {
        _blockFloat.Scan(_compactFloat.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool BlockScan_Bool()
    {
        _blockBool.Scan(_compactBool.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string BlockScan_String()
    {
        _blockString.Scan(_compactString.AsSpan(), 0, out string v);
        return v;
    }

    // ── TryScan 完整路径（TryGet + Strip + block.Scan）──

    [Benchmark]
    public int TryScan_Int()
    {
        SerializerBlocks.TryScan<int>(IntInput, out int v);
        return v;
    }

    [Benchmark]
    public float TryScan_Float()
    {
        SerializerBlocks.TryScan<float>(FloatInput, out float v);
        return v;
    }

    [Benchmark]
    public bool TryScan_Bool()
    {
        SerializerBlocks.TryScan<bool>(BoolInput, out bool v);
        return v;
    }

    [Benchmark]
    public string TryScan_String()
    {
        SerializerBlocks.TryScan<string>(StringInput, out string v);
        return v;
    }

    // ── Composite（SG 生成类型，两字段 + 类型名 + 分隔符）──

    [Benchmark]
    public Point2D BlockScan_Point2D()
    {
        _blockPoint2D.Scan(_compactPoint2D.AsSpan(), 0, out Point2D v);
        return v;
    }

    [Benchmark]
    public Point2D TryScan_Point2D()
    {
        SerializerBlocks.TryScan<Point2D>(Point2DInput, out Point2D v);
        return v;
    }
}

// ═══════════════════════════════════════════════════════════════
// Scan 执行效率：同三层 + 四种内置类型，测 ops/sec
// ═══════════════════════════════════════════════════════════════

[ShortRunJob]
[MemoryDiagnoser]
public class ScanThroughputBenchmarks
{
    private ISerializerBlock<int> _blockInt;
    private ISerializerBlock<float> _blockFloat;
    private ISerializerBlock<bool> _blockBool;
    private ISerializerBlock<string> _blockString;

    private string _compactInt;
    private string _compactFloat;
    private string _compactBool;
    private string _compactString;

    private const string IntInput = "42";
    private const string FloatInput = "3.14";
    private const string BoolInput = "true";
    private const string StringInput = "\"hello\"";

    [GlobalSetup]
    public void Setup()
    {
        SerializerBlocks.TryScan<int>("0", out _);
        SerializerBlocks.TryGet<int>(out _blockInt);
        SerializerBlocks.TryGet<float>(out _blockFloat);
        SerializerBlocks.TryGet<bool>(out _blockBool);
        SerializerBlocks.TryGet<string>(out _blockString);

        _compactInt = WhitespaceStripper.Strip(IntInput);
        _compactFloat = WhitespaceStripper.Strip(FloatInput);
        _compactBool = WhitespaceStripper.Strip(BoolInput);
        _compactString = WhitespaceStripper.Strip(StringInput);
    }

    // ── 裸 Registry ──

    [Benchmark(Baseline = true)]
    public int RegistryScan_Int()
    {
        SerializerRegistry.Scan_Int(_compactInt.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float RegistryScan_Float()
    {
        SerializerRegistry.Scan_Float(_compactFloat.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool RegistryScan_Bool()
    {
        SerializerRegistry.Scan_Bool(_compactBool.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string RegistryScan_String()
    {
        SerializerRegistry.Scan_String(_compactString.AsSpan(), 0, out string v);
        return v;
    }

    // ── Block Scan（接口分发）──

    [Benchmark]
    public int BlockScan_Int()
    {
        _blockInt.Scan(_compactInt.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float BlockScan_Float()
    {
        _blockFloat.Scan(_compactFloat.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool BlockScan_Bool()
    {
        _blockBool.Scan(_compactBool.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string BlockScan_String()
    {
        _blockString.Scan(_compactString.AsSpan(), 0, out string v);
        return v;
    }

    // ── TryScan 完整路径 ──

    [Benchmark]
    public int TryScan_Int()
    {
        SerializerBlocks.TryScan<int>(IntInput, out int v);
        return v;
    }

    [Benchmark]
    public float TryScan_Float()
    {
        SerializerBlocks.TryScan<float>(FloatInput, out float v);
        return v;
    }

    [Benchmark]
    public bool TryScan_Bool()
    {
        SerializerBlocks.TryScan<bool>(BoolInput, out bool v);
        return v;
    }

    [Benchmark]
    public string TryScan_String()
    {
        SerializerBlocks.TryScan<string>(StringInput, out string v);
        return v;
    }
}

// ═══════════════════════════════════════════════════════════════
// 最简 composite 类型：验证 SG 集成 + 多字段扫描
// ═══════════════════════════════════════════════════════════════

[Template("Point2D(<float X>, <float Y>)")]
public struct Point2D
{
    public float X;
    public float Y;
}
