# 示例: 手写序列化器与热更注册

SG 生成的 `ISerializerBlock<T>` 是常规路径。对于没有 `[Template]` 的类型（第三方库、自定义格式），手写实现提供同等的注册与查询能力。

## 手写 ISerializerBlock\<T\>

```csharp
public struct HotSword
{
    public float Atk;
    public float Crit;
}

// 格式: Sword(100, 0.15)
public readonly struct Block_HotSword : ISerializerBlock<HotSword>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out HotSword value)
    {
        value = default;
        int start = pos;

        if (!text.Slice(pos, 6).SequenceEqual("Sword(".AsSpan())) return pos;
        pos += 6;

        pos = SerializerRegistry.Scan_Float(text, pos, out float atk);
        value.Atk = atk;

        if (text[pos] != ',' || text[pos + 1] != ' ') return start;
        pos += 2;

        pos = SerializerRegistry.Scan_Float(text, pos, out float crit);
        value.Crit = crit;

        if (text[pos] != ')') return start;
        return pos + 1;
    }

    public void Emit(StringBuilder sb, HotSword value)
    {
        sb.Append("Sword(");
        SerializerRegistry.Emit_Float(sb, value.Atk);
        sb.Append(", ");
        SerializerRegistry.Emit_Float(sb, value.Crit);
        sb.Append(')');
    }
}
```

手写 block 可以调用 `SerializerRegistry` 的全部 16 种内置类型的 public static Scan/Emit 方法，以及 `GeneratedSerializers` 中 SG 生成的 `Scan_Xxx`/`Emit_Xxx` 方法。

## 注册与使用

```csharp
// 注册（泛型版本）
SerializerBlocks.AddBlock<HotSword>(new Block_HotSword());

// 注册（非泛型版本，适合热更 DLL 反射调用）
SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());

// 获取与使用
SerializerBlocks.TryGet<HotSword>(out var block);
block.Scan("Sword(100, 0.15)".AsSpan(), 0, out var v);
// v.Atk == 100f, v.Crit == 0.15f

var sb = new StringBuilder();
block.Emit(sb, v);
// sb.ToString() == "Sword(100, 0.15)"
```

## 混合注册：SG + 手写

```csharp
// 路径 A: SG 生成的 Init() 注册所有 [Template] 类型
GeneratedSerializers.Init();

// 路径 B: 补充手写 block
SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield());

// 路径 C: 按接口扩展分发链
// AddBlock<IModifier> 在接口类型上走 ChainBlock 追加，不覆盖已有注册
```

## 接口扩展（链合并）

热更 DLL 为已有接口添加新实现：

```csharp
// 服务端编译期: SG 生成 Block_IDamage{Strike, Spell, DoT}
// 热更 DLL 加载后: SG 生成 Block_IDamage{Reflect}
DLL.GeneratedSerializers.Init();
// typeof(IDamage).IsInterface → 链合并
// ChainBlock{ Block_IDamage{Strike,Spell,DoT}, Block_IDamage{Reflect} }
// "Reflect(50, 0.3)" → 先试前三者不匹配 → 试 Reflect 成功
```

运行测试参考 `HotReloadTests.cs` 中的完整用例。

## 参见

- [热更新与跨程序集注册](../guide/hot-reload): 概念讲解与完整场景
- [SerializerBlocks API](../api/serializer-blocks): AddBlock、RemoveBlock、链合并的 API 签名
- [SerializerRegistry API](../api/serializer-registry): 16 种内置类型的 Scan/Emit 方法
