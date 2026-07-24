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
    details: 编译期 SG 输出 C# span scanner。运行时零堆分配，无 GC，Burst 兼容。
  - title: Managed / Unmanaged 全覆盖
    details: unmanaged 类型零堆分配；class、string、List、Dictionary 等 managed 类型完整支持 Scan + Emit。Roslyn IsUnmanagedType 编译期判定。
  - title: 跨程序集 + 热更
    details: GeneratedSerializers.Init() 反射自动发现。接口分发链合并（ChainBlock），热更 DLL 可动态扩展已有接口的具现类型。
  - title: 四种 XML 原语 + 集合格式
    details: 裸文字、字段、可选块、重复块。List()/Dict()/HashSet() 函数式集合格式，字符串始终加引号。
  - title: 13 种内置类型扫描器
    details: float、double、int、uint、long、ulong、short、ushort、byte、sbyte、bool、char、string。零分配手写 span 扫描器，public API。
  - title: 编译期全覆盖诊断
    details: 6 种编译期诊断（SSR001-SSR006）：语法错误、循环依赖、只读字段、缺失类型、重复块标量、模板歧义。全部在编译期拦截。
  - title: 泛型合成 + 接口分派
    details: 开放泛型模板自动合成具体实例。接口字段自动分派到具现类型，首个推进者胜出。
  - title: 便利 API
    details: SerializerBlocks.Serialize&lt;T&gt;() / Deserialize&lt;T&gt;() 一行式调用。Builder 流式注册链。AddBlock/RemoveBlock 泛型与非泛型双 API。
---
