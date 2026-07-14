# API 参考

## Attributes

| Attribute | 目标 | 说明 |
|-----------|------|------|
| [`[Template]`](./template-attribute) | struct, class | 声明类型的文本模板 |
| [`[ExternalTemplate]`](./external-template-attribute) | assembly, class, struct | 为第三方类型声明模板 |
| [`[Tag]`](./tag-attribute) | enum field | 为枚举成员声明字符串标签 |
| [`[TypeAlias]`](./type-alias-attribute) | assembly | 注册类型别名 |

## Runtime

| 类型 | 说明 |
|------|------|
| [`SerializerRegistry`](./serializer-registry) | 12 种内置类型的零分配 span 扫描器与发射器 |
| [`SerializerScanners`](./serializer-scanners) | 反序列化注册入口，`TryGetScanner<T>` 获取生成的解析器 |
| [`SerializerEmitters`](./serializer-emitters) | 序列化注册入口，`TryGetEmitter<T>` 获取生成的发射器 |

## 类型关系

```mermaid
flowchart TD
    A["[Template] / [ExternalTemplate]"] --> B[Source Generator]
    C["[Tag]"] --> B
    D["[TypeAlias]"] --> B
    B --> E[SerializerScanners.g.cs]
    B --> E2[SerializerEmitters.g.cs]
    E --> F[TryGetScanner]
    F --> G[ScannerDelegate]
    H[SerializerRegistry] --> G
    E2 --> K[TryGetEmitter]
    K --> J[EmitterDelegate]
    H --> I[Emit_Xxx 方法]
    I --> J
```
