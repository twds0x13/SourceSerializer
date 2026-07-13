import { defineConfig } from "vitepress";
import { withMermaid } from "vitepress-plugin-mermaid";

export default withMermaid(
  defineConfig({
    base: "/SourceSerializer/",
    title: "SourceSerializer",
    description: "Compile-time serializer generator — attribute-defined schema, source-generated parser",
    locales: {
      root: { label: "简体中文", lang: "zh-CN" },
      en: { label: "English", lang: "en-US" },
    },
    themeConfig: {
      logo: "/logo.png",
      nav: [
        { text: "指南", link: "/guide/getting-started" },
        { text: "API", link: "/api/" },
      ],
      sidebar: {
        "/guide/": [
          { text: "Getting Started", link: "/guide/getting-started" },
          { text: "Template Syntax", link: "/guide/template-syntax" },
          { text: "Managed vs Unmanaged", link: "/guide/managed-vs-unmanaged" },
        ],
        "/en/guide/": [
          { text: "Getting Started", link: "/en/guide/getting-started" },
          { text: "Template Syntax", link: "/en/guide/template-syntax" },
          { text: "Managed vs Unmanaged", link: "/en/guide/managed-vs-unmanaged" },
        ],
      },
      socialLinks: [{ icon: "github", link: "https://github.com/twds0x13/SourceSerializer" }],
    },
    mermaid: {
      themeCSS: ".label foreignObject { overflow: visible; }",
    },
  })
);
