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

    // RegistryScan/BlockScan 的输入均无空白符——直接用原始输入 AsSpan()

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

        // 触发 EnsureInitialized 后无需额外 Strip——所有输入均无空白符
    }

    // ── 裸 Registry 静态调用（零分配基线）──

    [Benchmark(Baseline = true)]
    public int RegistryScan_Int()
    {
        SerializerRegistry.Scan_Int(IntInput.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float RegistryScan_Float()
    {
        SerializerRegistry.Scan_Float(FloatInput.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool RegistryScan_Bool()
    {
        SerializerRegistry.Scan_Bool(BoolInput.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string RegistryScan_String()
    {
        SerializerRegistry.Scan_String(StringInput.AsSpan(), 0, out string v);
        return v;
    }

    // ── Block Scan（接口分发，不含 Strip/TryGet）──

    [Benchmark]
    public int BlockScan_Int()
    {
        _blockInt.Scan(IntInput.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float BlockScan_Float()
    {
        _blockFloat.Scan(FloatInput.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool BlockScan_Bool()
    {
        _blockBool.Scan(BoolInput.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string BlockScan_String()
    {
        _blockString.Scan(StringInput.AsSpan(), 0, out string v);
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
        _blockPoint2D.Scan(Point2DInput.AsSpan(), 0, out Point2D v);
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

        // 所有输入均无空白符——直接用原始输入 AsSpan()
    }

    // ── 裸 Registry ──

    [Benchmark(Baseline = true)]
    public int RegistryScan_Int()
    {
        SerializerRegistry.Scan_Int(IntInput.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float RegistryScan_Float()
    {
        SerializerRegistry.Scan_Float(FloatInput.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool RegistryScan_Bool()
    {
        SerializerRegistry.Scan_Bool(BoolInput.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string RegistryScan_String()
    {
        SerializerRegistry.Scan_String(StringInput.AsSpan(), 0, out string v);
        return v;
    }

    // ── Block Scan（接口分发）──

    [Benchmark]
    public int BlockScan_Int()
    {
        _blockInt.Scan(IntInput.AsSpan(), 0, out int v);
        return v;
    }

    [Benchmark]
    public float BlockScan_Float()
    {
        _blockFloat.Scan(FloatInput.AsSpan(), 0, out float v);
        return v;
    }

    [Benchmark]
    public bool BlockScan_Bool()
    {
        _blockBool.Scan(BoolInput.AsSpan(), 0, out bool v);
        return v;
    }

    [Benchmark]
    public string BlockScan_String()
    {
        _blockString.Scan(StringInput.AsSpan(), 0, out string v);
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
