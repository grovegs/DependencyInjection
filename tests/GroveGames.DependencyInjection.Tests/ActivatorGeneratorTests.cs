using System.Reflection;

using GroveGames.DependencyInjection.Generated;
using GroveGames.DependencyInjection.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GroveGames.DependencyInjection.Tests;

public sealed class ActivatorGeneratorTests
{
    [Fact]
    public void Generate_GenericRegistration_EmitsConstructor()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public sealed class Dependency
            {
            }

            public sealed class Service
            {
                public Service(Dependency dependency)
                {
                }
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton<Dependency>();
                    builder.AddSingleton<Service>();
                }
            }
            """;

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Contains("registry.AddConstructor(typeof(global::Sample.Dependency), global::System.Type.EmptyTypes, arguments => new global::Sample.Dependency());", result.Code);
        Assert.Contains("registry.AddConstructor(typeof(global::Sample.Service), new global::System.Type[] { typeof(global::Sample.Dependency) }, arguments => new global::Sample.Service((global::Sample.Dependency)arguments[0]));", result.Code);
    }

    [Fact]
    public void Generate_ServiceAndImplementationRegistration_EmitsImplementationConstructor()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public interface IService
            {
            }

            public sealed class Service : IService
            {
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddTransient<IService, Service>();
                }
            }
            """;

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Contains("registry.AddConstructor(typeof(global::Sample.Service),", result.Code);
        Assert.DoesNotContain("typeof(global::Sample.IService)", result.Code);
    }

    [Fact]
    public void Generate_TypeOfRegistration_EmitsImplementationConstructor()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public interface IService
            {
            }

            public sealed class Service : IService
            {
            }

            public static class Installer
            {
                public static void Install(IContainerBuilder builder)
                {
                    builder.AddSingleton(typeof(IService), typeof(Service));
                }
            }
            """;

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Contains("registry.AddConstructor(typeof(global::Sample.Service),", result.Code);
    }

    [Fact]
    public void Generate_InjectConstructor_SelectsAttributedConstructor()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public sealed class Dependency
            {
            }

            public sealed class Service
            {
                public Service(Dependency first, Dependency second)
                {
                }

                [Inject]
                public Service(Dependency dependency)
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

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Contains("arguments => new global::Sample.Service((global::Sample.Dependency)arguments[0]));", result.Code);
    }

    [Fact]
    public void Generate_InjectMethod_EmitsInjectMethod()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public sealed class Dependency
            {
            }

            public sealed class View
            {
                [Inject]
                public void Construct(Dependency dependency)
                {
                }
            }
            """;

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Contains("registry.AddInjectMethod(typeof(global::Sample.View), new global::System.Type[] { typeof(global::Sample.Dependency) }, (instance, arguments) => ((global::Sample.View)instance).Construct((global::Sample.Dependency)arguments[0]));", result.Code);
        Assert.DoesNotContain("AddConstructor", result.Code);
    }

    [Fact]
    public void Generate_OverriddenInjectMethod_EmitsMethodOnce()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public sealed class Dependency
            {
            }

            public abstract class BaseView
            {
                [Inject]
                public virtual void Construct(Dependency dependency)
                {
                }
            }

            public sealed class View : BaseView
            {
                public override void Construct(Dependency dependency)
                {
                }
            }
            """;

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Single(FindAll(result.Code, "registry.AddInjectMethod(typeof(global::Sample.View)"));
        Assert.DoesNotContain("typeof(global::Sample.BaseView)", result.Code);
    }

    [Fact]
    public void Generate_PrivateNestedType_EmitsNothing()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

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

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Null(result.Code);
    }

    [Fact]
    public void Generate_RefParameterConstructor_EmitsNothing()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public sealed class Dependency
            {
            }

            public sealed class Service
            {
                public Service(in Dependency dependency)
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

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Null(result.Code);
    }

    [Fact]
    public void Generate_OpenGenericRegistration_EmitsNothing()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public static class Installer
            {
                public static void Install<T>(IContainerBuilder builder)
                    where T : class
                {
                    builder.AddSingleton<T>();
                }
            }
            """;

        var result = Generate(source);

        Assert.Empty(result.Errors);
        Assert.Null(result.Code);
    }

    [Fact]
    public void Generate_CompiledAssembly_ResolvesAndInjectsThroughGeneratedCode()
    {
        const string source = """
            using GroveGames.DependencyInjection;

            namespace Sample;

            public sealed class Dependency
            {
            }

            public sealed class Service
            {
                public Dependency Dependency { get; }

                public Service(Dependency dependency)
                {
                    Dependency = dependency;
                }
            }

            public sealed class View
            {
                public Dependency Dependency { get; private set; }

                [Inject]
                public void Construct(Dependency dependency)
                {
                    Dependency = dependency;
                }
            }

            public static class Entry
            {
                public static object[] Run()
                {
                    var builder = new ContainerBuilder();
                    builder.AddSingleton<Dependency>();
                    builder.AddSingleton<Service>();
                    var container = builder.Build();
                    var view = new View();
                    container.Inject(view);
                    return new object[] { container.Resolve<Dependency>(), container.Resolve<Service>().Dependency, view.Dependency };
                }
            }
            """;

        var assembly = Compile(source);

        var result = (object[])assembly.GetType("Sample.Entry")!.GetMethod("Run")!.Invoke(null, null)!;

        Assert.Same(result[0], result[1]);
        Assert.Same(result[0], result[2]);
        Assert.True(GeneratedActivators.TryGetConstructor(assembly.GetType("Sample.Service")!, out _));
        Assert.True(GeneratedActivators.TryGetInjectMethods(assembly.GetType("Sample.View")!, out _));
    }

    private static TestGeneratorResult Generate(string source)
    {
        var compilation = CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ActivatorGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);
        var errors = new List<Diagnostic>();

        foreach (var diagnostic in generatorDiagnostics.Concat(output.GetDiagnostics()))
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                errors.Add(diagnostic);
            }
        }

        var results = driver.GetRunResult().Results;
        var code = results.Length == 0 || results[0].GeneratedSources.Length == 0
            ? null
            : results[0].GeneratedSources[0].SourceText.ToString();

        return new TestGeneratorResult(code, errors);
    }

    private static Assembly Compile(string source)
    {
        var compilation = CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ActivatorGenerator());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        using var stream = new MemoryStream();
        var emitResult = output.Emit(stream);

        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        return Assembly.Load(stream.ToArray());
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

    private static List<int> FindAll(string? text, string value)
    {
        var indices = new List<int>();

        for (var index = text?.IndexOf(value, StringComparison.Ordinal) ?? -1; index >= 0; index = text!.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            indices.Add(index);
        }

        return indices;
    }

    private sealed class TestGeneratorResult
    {
        private readonly string? _code;
        private readonly List<Diagnostic> _errors;

        public string? Code => _code;
        public List<Diagnostic> Errors => _errors;

        public TestGeneratorResult(string? code, List<Diagnostic> errors)
        {
            _code = code;
            _errors = errors;
        }
    }
}
