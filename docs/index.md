---
layout: home

hero:
  name: "SourceSerializer"
  text: 编译期序列化生成器
  tagline: 用 attribute 声明 Schema，source generator 在编译期生成专用解析器。零反射，零装箱。
  image:
    src: /logo.svg
    alt: SourceSerializer
  actions:
    - theme: brand
      text: 快速入门
      link: /guide/getting-started
    - theme: alt
      text: API 参考
      link: /api/

features:
  - title: 零反射，零装箱
    details: 编译期 SG 输出 C# span scanner。运行时 零堆分配，无堆内存，Burst 兼容。
  - title: Managed / Unmanaged 双策略
    details: unmanaged 类型走单次 span 扫描，零堆分配；managed 类型的扫描已实现，通过 Roslyn IsUnmanagedType 编译期判定。
  - title: 四种 XML 原语
    details: 裸文字（text）、字段（field）、可选块（optional）、重复块（repetition）。compact 语法与 XML 语法等价互换。
  - title: 12 种内置类型扫描器
    details: float、double、int、uint、long、ulong、short、ushort、byte、sbyte、bool、char。零分配手写 span 扫描器。
  - title: 编译期错误诊断
    details: 循环依赖检测、只读字段拒绝、缺失类型警告、重复块标量字段错误。所有错误在编译期报告，不等到运行时 NRE。
  - title: 编译期发射器
    details: 同时生成 SerializerEmitters 管线。struct 到 StringBuilder 的序列化零分配，支持内置类型、自定义嵌套类型、枚举标签。
---
