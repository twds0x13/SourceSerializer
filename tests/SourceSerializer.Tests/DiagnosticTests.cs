using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace SourceSerializer.Tests
{
    [TestFixture]
    public class DiagnosticTests
    {
        private static ImmutableArray<Diagnostic> GetGeneratorDiagnostics(params string[] sources)
        {
            var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();

            var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "")
                .Split(Path.PathSeparator);
            var references = trustedAssemblies
                .Where(p =>
                {
                    var name = Path.GetFileName(p);
                    return name.StartsWith("System") || name == "netstandard.dll"
                        || name == "mscorlib.dll" || name == "System.Runtime.dll";
                })
                .Select(p => MetadataReference.CreateFromFile(p))
                .Cast<MetadataReference>()
                .ToList();

            references.Add(MetadataReference.CreateFromFile(
                typeof(SourceSerializer.SerializerBlocks).Assembly.Location));

            var compilation = CSharpCompilation.Create("TestAssembly",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new SourceSerializer.Generator.SerializerGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator });
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiags);

            System.Console.WriteLine($"=== ALL GENERATOR DIAGS ({generatorDiags.Length}) ===");
            foreach (var d in generatorDiags)
                System.Console.WriteLine($"  [{d.Severity}] {d.Id}: {d.GetMessage()}");

            return generatorDiags
                .Where(d => d.Id.StartsWith("SSR"))
                .ToImmutableArray();
        }

        private static string WrapTemplate(string template, string structName, string fields)
        {
            return $@"
using SourceSerializer;
[assembly: TypeAlias(""int"", ""int"")]
[Template(""{template}"")]
public struct {structName} {{ {fields} }}
";
        }

        // ═══════════════════════════════════════════════════════
        // SSR001 — bare <repetition> without <first> and <body>
        // ═══════════════════════════════════════════════════════

        [Test]
        public void SSR001_Repetition_Without_First_And_Body()
        {
            var source = WrapTemplate(
                "S(<repetition><int X></repetition>)",
                "BadRep", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<repetition> without <first> and <body>"));
        }

        [Test]
        public void SSR001_First_Without_Body()
        {
            var source = WrapTemplate(
                "S(<repetition><first><int X></first></repetition>)",
                "FirstOnly", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<first> without <body>"));
        }

        [Test]
        public void SSR001_Body_Without_First()
        {
            var source = WrapTemplate(
                "S(<repetition><body><int X></body></repetition>)",
                "BodyOnly", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<body> without <first>"));
        }

        [Test]
        public void SSR001_Duplicate_First()
        {
            var source = WrapTemplate(
                "S(<repetition><first><int X></first><first><int X></first><body><int X></body></repetition>)",
                "DupFirst", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("Duplicate <first>"));
        }

        [Test]
        public void SSR001_Duplicate_Body()
        {
            var source = WrapTemplate(
                "S(<repetition><first><int X></first><body><int X></body><body><int X></body></repetition>)",
                "DupBody", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("Duplicate <body>"));
        }

        [Test]
        public void SSR001_Body_Before_First()
        {
            var source = WrapTemplate(
                "S(<repetition><body><int X></body><first><int X></first></repetition>)",
                "BodyFirst", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<body> before <first>"));
        }

        [Test]
        public void SSR001_Extra_Element_Inside_Repetition()
        {
            // <optional> passes CompactToXml but is rejected by XmlTemplateParser
            // inside <repetition> (only <first> and <body> allowed)
            var source = WrapTemplate(
                "S(<repetition><first><int X></first><optional></optional><body><int X></body></repetition>)",
                "ExtraElem", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("not allowed inside <repetition>"));
        }

        [Test]
        public void SSR001_Multiple_Repetition()
        {
            var source = WrapTemplate(
                "S(<repetition><first><int X></first><body><int X></body></repetition><repetition><first><int Y></first><body><int Y></body></repetition>)",
                "MultiRep", "public int X; public int Y;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("Only one <repetition> per template"));
        }

        // ═══════════════════════════════════════════════════════
        // SSR001 — invalid structure via XML format
        // (bypasses CompactToXml, hits ParseChildren / ParseRepetition directly)
        // ═══════════════════════════════════════════════════════

        private static string WrapXmlTemplate(string xmlBody, string structName, string fields)
        {
            // Use single quotes for XML attributes to avoid C# string escaping complexity
            return $@"
using SourceSerializer;
[assembly: TypeAlias(""int"", ""int"")]
[Template(""<literal-template>{xmlBody}</literal-template>"")]
public struct {structName} {{ {fields} }}
";
        }

        [Test]
        public void SSR001_First_At_Root_Xml()
        {
            var source = WrapXmlTemplate(
                "<first><field type='int' name='X'/></first>",
                "FirstAtRoot", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<first> must be inside <repetition>"));
        }

        [Test]
        public void SSR001_Body_At_Root_Xml()
        {
            var source = WrapXmlTemplate(
                "<body><field type='int' name='X'/></body>",
                "BodyAtRoot", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<body> must be inside <repetition>"));
        }

        [Test]
        public void SSR001_Repetition_Empty_Xml()
        {
            // XML format: <repetition></repetition> — CompactToXml not involved,
            // ParseRepetition detects missing <first> and <body>
            var source = WrapXmlTemplate(
                "<repetition></repetition>",
                "EmptyRep", "public int X;");
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR001"));
            Assert.That(diags[0].GetMessage(), Does.Contain("<repetition> without <first> and <body>"));
        }

        // ═══════════════════════════════════════════════════════
        // SSR004 — scalar field inside <repetition>
        // ═══════════════════════════════════════════════════════

        [Test]
        public void SSR004_Scalar_Inside_Repetition()
        {
            var source = @"
using SourceSerializer;
[Template(""S(<repetition><first><int V></first><body>, <int V></body></repetition>)"")]
public struct ScalarRep { public int V; }
";
            var diags = GetGeneratorDiagnostics(source);
            Assert.That(diags.Length, Is.EqualTo(1));
            Assert.That(diags[0].Id, Is.EqualTo("SSR004"));
            Assert.That(diags[0].GetMessage(), Does.Contain("repetition"));
        }

    }
}
