using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal sealed class DependencyInjectionLinkerProcessor : IUnityLinkerProcessor
    {
        public const string FileName = "GroveGames.DependencyInjection.link.xml";

        public int callbackOrder => 0;

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var path = Path.Combine(data.inputDirectory, FileName);
            File.WriteAllText(path, BuildLinkXml(TypeCache.GetMethodsWithAttribute<InjectAttribute>()));
            return path;
        }

        public static string BuildLinkXml(IEnumerable<MethodInfo> methods)
        {
            var assemblies = new SortedDictionary<string, SortedDictionary<string, SortedSet<string>>>(StringComparer.Ordinal);

            foreach (var method in methods)
            {
                var type = method.DeclaringType;

                if (type == null || type.FullName == null)
                {
                    continue;
                }

                var assemblyName = type.Assembly.GetName().Name;

                if (!assemblies.TryGetValue(assemblyName, out var types))
                {
                    types = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
                    assemblies.Add(assemblyName, types);
                }

                var typeName = type.FullName.Replace('+', '/');

                if (!types.TryGetValue(typeName, out var methodNames))
                {
                    methodNames = new SortedSet<string>(StringComparer.Ordinal);
                    types.Add(typeName, methodNames);
                }

                methodNames.Add(method.Name);
            }

            var linker = new XElement("linker");

            foreach (var assembly in assemblies)
            {
                var assemblyElement = new XElement("assembly", new XAttribute("fullname", assembly.Key));

                foreach (var type in assembly.Value)
                {
                    var typeElement = new XElement("type", new XAttribute("fullname", type.Key), new XAttribute("preserve", "nothing"));

                    foreach (var methodName in type.Value)
                    {
                        typeElement.Add(new XElement("method", new XAttribute("name", methodName)));
                    }

                    assemblyElement.Add(typeElement);
                }

                linker.Add(assemblyElement);
            }

            return linker.ToString();
        }
    }
}
