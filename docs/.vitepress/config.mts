import { defineConfig } from "vitepress";
import { withMermaid } from "vitepress-plugin-mermaid";

export default withMermaid(
  defineConfig({
    base: "/SourceSerializer/",
    title: "SourceSerializer",
    description: "Compile-time serializer generator — attribute-defined schema, source-generated parser",
    head: [
      ["link", { rel: "icon", type: "image/svg+xml", href: "/SourceSerializer/favicon.svg" }],
      ["meta", { property: "og:image", content: "https://twds0x13.github.io/SourceSerializer/logo.svg" }],
      ["meta", { property: "og:image:width", content: "512" }],
      ["meta", { property: "og:image:height", content: "512" }],
      ["meta", { name: "twitter:card", content: "summary" }],
    ],

    locales: {
      root: {
        label: "简体中文",
        lang: "zh-CN",
        themeConfig: {
          logo: "/logo.svg",
          nav: [
            { text: "首页", link: "/" },
            { text: "指南", link: "/guide/" },
            { text: "API", link: "/api/" },
            { text: "技术", link: "/technical/pipeline/overview" },
          ],
          sidebar: [
            {
              text: "指南",
              items: [
                { text: "指南目录", link: "/guide/" },
                { text: "快速入门", link: "/guide/getting-started" },
                { text: "模板语法", link: "/guide/template-syntax" },
                { text: "模板写作", link: "/guide/template-writing" },
                { text: "Managed vs Unmanaged", link: "/guide/managed-vs-unmanaged" },
                { text: "编译期诊断", link: "/guide/diagnostics" },
              ],
            },
            {
              text: "参考",
              items: [
                { text: "术语表", link: "/reference/glossary" },
              ],
            },
            {
              text: "API 参考",
              items: [
                { text: "API 总览", link: "/api/" },
                {
                  text: "属性",
                  collapsed: false,
                  items: [
                    { text: "Template", link: "/api/template-attribute" },
                    { text: "ExternalTemplate", link: "/api/external-template-attribute" },
                    { text: "Tag", link: "/api/tag-attribute" },
                    { text: "TypeAlias", link: "/api/type-alias-attribute" },
                    { text: "TemplateIgnore", link: "/api/template-ignore" },
                  ],
                },
                {
                  text: "运行时",
                  collapsed: false,
                  items: [
                    { text: "SerializerRegistry", link: "/api/serializer-registry" },
                    { text: "SerializerBlocks", link: "/api/serializer-blocks" },
                  ],
                },
              ],
            },
            {
              text: "示例",
              items: [
                { text: "基础示例", link: "/examples/basic" },
                { text: "集合示例", link: "/examples/collections" },
              ],
            },
            {
              text: "技术深度",
              items: [
                { text: "SG 管线全景", link: "/technical/pipeline/overview" },
                { text: "内部机制", link: "/technical/internals" },
                { text: "架构决策", link: "/technical/architecture-decisions" },
              ],
            },
            {
              text: "更多",
              items: [
                { text: "常见问题", link: "/faq" },
                { text: "迁移指南", link: "/migration-guide" },
                { text: "多语言协作", link: "/multilingual-collaboration" },
              ],
            },
          ],
          socialLinks: [{ icon: "github", link: "https://github.com/twds0x13/SourceSerializer" }],
          footer: { message: "基于 MIT 许可发布。", copyright: "Copyright © 2026 twds0x13" },
        },
      },

      en: {
        label: "English",
        lang: "en-US",
        themeConfig: {
          logo: "/logo.svg",
          nav: [
            { text: "Home", link: "/en/" },
            { text: "Guide", link: "/en/guide/" },
            { text: "API", link: "/en/api/" },
            { text: "Technical", link: "/en/technical/pipeline/overview" },
          ],
          sidebar: [
            {
              text: "Guide",
              items: [
                { text: "Guide Index", link: "/en/guide/" },
                { text: "Getting Started", link: "/en/guide/getting-started" },
                { text: "Template Syntax", link: "/en/guide/template-syntax" },
                { text: "Template Writing", link: "/en/guide/template-writing" },
                { text: "Managed vs Unmanaged", link: "/en/guide/managed-vs-unmanaged" },
                { text: "Diagnostics", link: "/en/guide/diagnostics" },
              ],
            },
            {
              text: "Reference",
              items: [
                { text: "Glossary", link: "/en/reference/glossary" },
              ],
            },
            {
              text: "API Reference",
              items: [
                { text: "API Overview", link: "/en/api/" },
                {
                  text: "Attributes",
                  collapsed: false,
                  items: [
                    { text: "Template", link: "/en/api/template-attribute" },
                    { text: "ExternalTemplate", link: "/en/api/external-template-attribute" },
                    { text: "Tag", link: "/en/api/tag-attribute" },
                    { text: "TypeAlias", link: "/en/api/type-alias-attribute" },
                    { text: "TemplateIgnore", link: "/en/api/template-ignore" },
                  ],
                },
                {
                  text: "Runtime",
                  collapsed: false,
                  items: [
                    { text: "SerializerRegistry", link: "/en/api/serializer-registry" },
                    { text: "SerializerBlocks", link: "/en/api/serializer-blocks" },
                  ],
                },
              ],
            },
            {
              text: "Examples",
              items: [
                { text: "Basic Examples", link: "/en/examples/basic" },
                { text: "Collection Examples", link: "/en/examples/collections" },
              ],
            },
            {
              text: "Technical Depth",
              items: [
                { text: "SG Pipeline Overview", link: "/en/technical/pipeline/overview" },
                { text: "Internals", link: "/en/technical/internals" },
                { text: "Architecture Decisions", link: "/en/technical/architecture-decisions" },
              ],
            },
            {
              text: "More",
              items: [
                { text: "FAQ", link: "/en/faq" },
                { text: "Migration Guide", link: "/en/migration-guide" },
                { text: "Multilingual Collaboration", link: "/en/multilingual-collaboration" },
              ],
            },
          ],
          socialLinks: [{ icon: "github", link: "https://github.com/twds0x13/SourceSerializer" }],
          footer: { message: "Released under the MIT License.", copyright: "Copyright © 2026 twds0x13" },
        },
      },
    },

    mermaid: {
      htmlLabels: true,
      themeCSS: `
        .label foreignObject { overflow: visible !important; }
        .nodeLabel foreignObject { overflow: visible !important; }
        .edgeLabel foreignObject { overflow: visible !important; }
        .label div, .nodeLabel div, .edgeLabel div { padding-bottom: 5px; }
      `,
      themeVariables: {
        fontFamily: '"Microsoft YaHei", "Noto Sans SC", "PingFang SC", sans-serif',
      },
    },
  })
);
