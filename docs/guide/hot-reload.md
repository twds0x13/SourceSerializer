# 热更新与跨程序集注册

SourceSerializer 的核心设计之一是**编译期生成与运行时注册的同一代码路径**。SG 生成的 `GeneratedSerializers.Init()` 既可在主程序集启动时调用，也可在热更 DLL 加载时调用——两边走完全相同的注册逻辑。

## 场景

一个典型的业务场景：

```
1. 服务端 v3.4 已运行多年，有 StrikeDamage、SpellDamage、DotDamage，
   均实现 IDamage 接口。SG 生成的 Block_IDamage 可以分发这三种类型。
2. 新资料片需要 ReflectDamage——也实现 IDamage，但多一个反弹比例字段。
3. 不能等客户端大版本（三个月后），需要热更 DLL 推送。
```

## 手写 ISerializerBlock\<T\>

热更 DLL 中的新类型**依赖 SG 生成**——DLL 编译时 SG 同样为其生成 `GeneratedSerializers`。但如果 DLL 中的类型没有 `[Template]`（例如用第三方类型或需要自定义格式），需手写实现。

推荐格式与文档模板风格一致——`TypeName(arg1, arg2)`：

```csharp
// 类型定义
public struct HotSword
{
    public float Atk;
    public float Crit;
}

// 手写序列化器块 —— Sword(100, 0.15)
public readonly struct Block_HotSword : ISerializerBlock<HotSword>
{
    public int Scan(ReadOnlySpan<char> text, int pos, out HotSword value)
    {
        value = default;
        if (pos + 6 > text.Length) return pos;
        int start = pos;

        // "Sword("
        if (!text.Slice(pos, 6).SequenceEqual("Sword(".AsSpan())) return pos;
        pos += 6;

        int pre = pos;
        pos = SerializerRegistry.Scan_Float(text, pos, out float atk);
        if (pos == pre) return start;
        value.Atk = atk;

        // ", "
        if (pos + 1 >= text.Length || text[pos] != ',' || text[pos + 1] != ' ') return start;
        pos += 2;

        pos = SerializerRegistry.Scan_Float(text, pos, out float crit);
        if (pos == pre) return start;
        value.Crit = crit;

        // ")"
        if (pos >= text.Length || text[pos] != ')') return start;
        pos++;

        return pos;
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

手写 block 可调用 `SerializerRegistry` 的全部 13 种内置类型的 public static Scan/Emit 方法，以及 SG 为同程序集类型生成的 `GeneratedSerializers.Scan_Xxx/Emit_Xxx` 方法。

## 注册与初始化

DLL 加载后的初始化入口：

```csharp
// 路径 A：DLL 内有 [Template] 类型，SG 已生成 GeneratedSerializers
GeneratedSerializers.Init();
// → 注册 DLL 内所有类型的 Block_Xxx

// 路径 B：手写 block，逐一注册
SerializerBlocks.AddBlock(typeof(HotSword), new Block_HotSword());
SerializerBlocks.AddBlock(typeof(HotShield), new Block_HotShield());

// 路径 C：混合 —— 先 Init() 注册 SG 生成的，再 AddBlock 补充手写的
```

主程序集的 `EnsureInitialized()` 在首次 `TryGet<T>` 时通过 AppDomain 反射扫描所有已加载程序集的 `GeneratedSerializers.Init()`。但热更 DLL 的 `Init()` 需要在 DLL 加载后**显式调用**——因为它在首次扫描之后才被加载。

`Init()` 是幂等的——`_initCalled` 守护字段确保二次调用直接返回。

## 接口扩展（链合并）

这是热更新最关键的机制。当新类型实现了已有接口时，`AddBlock` 自动追加到分发链：

```csharp
// === 服务端（编译期） ===
// SG 生成的 Init() 注册了：
//   AddBlock<IDamage>(Block_IDamage{StrikeDamage, SpellDamage, DotDamage})
// → _blocks[IDamage] = Block_IDamage{Strike, Spell, DoT}

// === 热更 DLL 加载后 ===
// DLL 的 Init() 注册了：
//   AddBlock<IDamage>(Block_IDamage{ReflectDamage})
// → typeof(IDamage).IsInterface → RegisterBlock 链合并
// → _blocks[IDamage] = ChainBlock{ Block_IDamage{Strike,Spell,DoT}, Block_IDamage{Reflect} }

// 反序列化 "Reflect(50, 0.3, Spell(200, Fire))"：
// ChainBlock.Scan → link0 试 Strike/Spell/DoT → 都不匹配
//                 → link1 试 Reflect → 成功！
```

对于非接口类型，`AddBlock` 仍然是覆盖语义——后注册替换先注册。只有接口类型走链合并路径。

## 移除与版本管理

```csharp
// 下线旧类型
SerializerBlocks.RemoveBlock(typeof(HotSword));
// 接口类型移除整条链
SerializerBlocks.RemoveBlock<IDamage>();
```

`RemoveBlock` 从字典中删除 key。对于接口类型，删除的是整个 `ChainBlock`——所有链节同时移除。不需要单独移除链节的能力。

## 参见

- [接口链合并内部原理](../technical/internals#接口链合并-chainblock-t)
- [SerializerBlocks API](../api/serializer-blocks)
- [HotReloadTests](https://github.com/twds0x13/SourceSerializer/blob/main/tests/SourceSerializer.Tests/HotReloadTests.cs) — 可运行的完整测试用例
