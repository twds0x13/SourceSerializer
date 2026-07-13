# Managed vs Unmanaged

SourceSerializer 在编译期根据 `typeof(T)` 自动选择策略。

## Unmanaged 路径

`T : unmanaged` 时走单次解析策略。Span scanner 直接填充字段，零分配，Burst 兼容。

当前版本仅实现 unmanaged 路径。

## Managed 路径（规划中）

`T : class` 或 managed `struct` 时走两步走策略：

1. **Walk** — 遍历对象图，依次分配 int ID
2. **Serialize** — 引用字段替换为 int 编号

天然支持循环引用，无需图分析。

## 选择指南

| 场景 | 推荐 |
|------|------|
| 数值结构体（Vector3、StatBlock） | Unmanaged |
| 含字符串/列表/对象引用的类型 | Managed（规划中） |

本文档正在编写中。
