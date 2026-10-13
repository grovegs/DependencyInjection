using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

using UnityEditor;

namespace GroveGames.DependencyInjection.Unity.Editor.Tests
{
    public sealed class DependencyInjectionLinkerProcessorTests
    {
        [Test]
        public void BuildLinkXml_InjectMethods_PreservesMethodsByDeclaringType()
        {
            var methods = TypeCache.GetMethodsWithAttribute<InjectAttribute>().Where(method => method.DeclaringType == typeof(TestInjected));

            var linker = XElement.Parse(DependencyInjectionLinkerProcessor.BuildLinkXml(methods));

            var assembly = linker.Elements("assembly").Single();
            var type = assembly.Elements("type").Single();
            Assert.AreEqual(typeof(TestInjected).Assembly.GetName().Name, (string)assembly.Attribute("fullname"));
            Assert.AreEqual(typeof(TestInjected).FullName!.Replace('+', '/'), (string)type.Attribute("fullname"));
            Assert.AreEqual("nothing", (string)type.Attribute("preserve"));
            CollectionAssert.AreEqual(new[] { nameof(TestInjected.Construct), nameof(TestInjected.Configure) }.OrderBy(name => name, System.StringComparer.Ordinal), type.Elements("method").Select(method => (string)method.Attribute("name")));
        }

        [Test]
        public void BuildLinkXml_NoMethods_ReturnsEmptyLinker()
        {
            var linker = XElement.Parse(DependencyInjectionLinkerProcessor.BuildLinkXml(Enumerable.Empty<System.Reflection.MethodInfo>()));

            Assert.AreEqual("linker", linker.Name.LocalName);
            Assert.IsFalse(linker.HasElements);
        }

        private sealed class TestInjected
        {
            [Inject]
            public void Construct()
            {
            }

            [Inject]
            public void Configure()
            {
            }

            public void NotInjected()
            {
            }
        }
    }
}
