using System.Collections.Immutable;

using GroveGames.DependencyInjection.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GroveGames.DependencyInjection.Tests;

public sealed class InjectionAnalyzerTests
{
    [Fact]
    public async Task Analyze_InterfaceRegisteredAsImplementation_ReportsGGDI001()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public interface IService
            {
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton(typeof(IService), typeof(IService));
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI001"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_AbstractImplementation_ReportsGGDI001()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public interface IService
            {
            }

            public abstract class Service : IService
            {
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton<IService, Service>();
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI001"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_NoPublicConstructor_ReportsGGDI002()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public sealed class Service
            {
                internal Service()
                {
                }
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddTransient<Service>();
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI002"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_NonPublicAndStaticInjectMethods_ReportsGGDI003()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public sealed class View
            {
                [Inject]
                private void Construct()
                {
                }

                [Inject]
                public static void Configure()
                {
                }

                [Inject]
                public void Setup<T>()
                {
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI003", "GGDI003", "GGDI003"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_InjectMethodRefParameter_ReportsGGDI004()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public sealed class Dependency
            {
            }

            public sealed class View
            {
                [Inject]
                public void Construct(in Dependency dependency)
                {
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI004"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_RegisteredConstructorRefParameter_ReportsGGDI004()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public sealed class Dependency
            {
            }

            public sealed class Service
            {
                public Service(ref Dependency dependency)
                {
                }
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton<Service>();
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI004"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_MultipleInjectConstructors_ReportsGGDI005()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public sealed class Service
            {
                [Inject]
                public Service()
                {
                }

                [Inject]
                public Service(int value)
                {
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Equal(["GGDI005"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_PrivateRegisteredType_DoesNotReportByDefault()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton<Hidden>();
                }

                private sealed class Hidden
                {
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Analyze_PrivateRegisteredTypeWithReflectionFallbackEnabled_ReportsGGDI006()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton<Hidden>();
                }

                public static void InstallGeneric<T>(IContainerBuilder builder)
                    where T : class
                {
                    builder.AddSingleton<T>();
                }

                private sealed class Hidden
                {
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source, "GGDI006");

        Assert.Equal(["GGDI006", "GGDI006"], Ids(diagnostics));
    }

    [Fact]
    public async Task Analyze_ValidRegistrations_ReportsNothing()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            public interface IService
            {
            }

            public sealed class Service : IService
            {
                [Inject]
                public Service()
                {
                }

                [Inject]
                public void Construct(IObjectResolver resolver)
                {
                }
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton<IService, Service>();
                    builder.AddTransient(typeof(Service), typeof(Service));
                    builder.AddSingleton<IService>(_ => new Service());
                    builder.AddSingleton(new Service());
                }
            }
            """;

        var diagnostics = await AnalyzeAsync(source, "GGDI006");

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, params string[] enabledIds)
    {
        var specificOptions = new Dictionary<string, ReportDiagnostic>();

        foreach (var id in enabledIds)
        {
            specificOptions[id] = ReportDiagnostic.Info;
        }

        var compilation = CreateCompilation(source);
        var options = compilation.Options.WithSpecificDiagnosticOptions(specificOptions);
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new InjectionAnalyzer());

        return await compilation.WithOptions(options).WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }

    private static List<string> Ids(ImmutableArray<Diagnostic> diagnostics)
    {
        var ids = new List<string>();

        foreach (var diagnostic in diagnostics)
        {
            ids.Add(diagnostic.Id);
        }

        return ids;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var paths = new HashSet<string>(((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator), StringComparer.OrdinalIgnoreCase);
        paths.Add(typeof(ContainerBuilder).Assembly.Location);
        var references = new List<MetadataReference>();

        foreach (var path in paths)
        {
            references.Add(MetadataReference.CreateFromFile(path));
        }

        return CSharpCompilation.Create(
            $"Sample{Guid.NewGuid():N}",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
