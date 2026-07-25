# 已知限制

## class 类型的变量作用域

`[Template]` 标注在 `class` 上有变量作用域问题。涉及 `NamedPoint` 等 class 类型时，SG 生成的 `Scan` 方法可能引用不存在的变量（如 `_Y_42`）。

根因：`CodeEmitter` 在处理 `NeedsHeapAlloc` 路径（class 类型）时，变量声明和作用域管理策略与 struct 路径不一致。该路径的代码生成逻辑尚未完成排查。

影响范围：仅 `class` 类型。`struct`（含 `readonly struct`）不受影响。

临时绕过方式：
- 使用 struct 包装：将 class 的序列化字段提取到 struct，在 class 和 struct 之间做转换
- 手写 `ISerializerBlock<T>` 实现：不依赖 SG 生成，直接实现 `Scan` 和 `Emit` 方法，参考 [热更新与跨程序集注册](/guide/hot-reload) 中的手写示例

## 参见

- [内部机制](./internals)：CodeEmitter 和 NeedsHeapAlloc 路径的源码细节
- [Managed vs Unmanaged](/guide/managed-vs-unmanaged)：class 与 struct 的分配策略差异
- [示例: 手写序列化器](/examples/hot-reload)：手写 ISerializerBlock 的完整示例
