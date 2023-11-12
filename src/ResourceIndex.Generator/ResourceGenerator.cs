using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Resource.Index.Data;
using Resource.Index.Extensions;
using SGF;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text;

namespace Resource.Index
{
    [Generator]
    internal class ResourceGenerator : IncrementalGenerator
    {
        private readonly Assembly s_assembly;
        private readonly IDictionary<string, HandlebarsTemplate<object, object>> m_templateMap;

        public ResourceGenerator() : base("ResourceIndex")
        {
            Type type = typeof(ResourceGenerator);
            s_assembly = type.Assembly;
            m_templateMap = new Dictionary<string, HandlebarsTemplate<object, object>>();
        }

        protected override void OnInitialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<ImmutableArray<AdditionalText>> textProvider = context.AdditionalTextsProvider.Collect();
            IncrementalValueProvider<AnalyzerConfigOptionsProvider> configProvider = context.AnalyzerConfigOptionsProvider;
            IncrementalValueProvider<(ImmutableArray<AdditionalText> Left, AnalyzerConfigOptionsProvider Right)> combined = textProvider.Combine(configProvider);
            context.RegisterSourceOutput(combined, (context, providers) => GenerateSource(context, providers.Left, providers.Right));
        }

        private void GenerateSource(SourceProductionContext context, ImmutableArray<AdditionalText> additionalText, AnalyzerConfigOptionsProvider configProvider)
        {
            string rootNamespace = configProvider.GlobalOptions.GetValue("build_property.rootnamespace", "Resource");
            string defaultResourcePrefix = $"{rootNamespace}.Resources.";
            HashSet<string> memberNames = new();
            List<ResourceEntry> entries = new();

            foreach (AdditionalText textFile in additionalText)
            {
                AnalyzerConfigOptions options = configProvider.GetOptions(textFile);
                if (!options.TryGetValue("build_metadata.embeddedresource.logicalname", out string? logicalName) || string.IsNullOrWhiteSpace(logicalName))
                {
                    continue;
                }

                string resourceName = options.GetValue("build_metadata.embeddedresource.resourcename", logicalName);

                if (resourceName.StartsWith(defaultResourcePrefix))
                {
                    resourceName = resourceName.Substring(defaultResourcePrefix.Length);
                }

                // Get unique member name 
                string memberName = Path.GetFileNameWithoutExtension(resourceName);
                memberName = memberName.ToMemberName();

                entries.Add(new ResourceEntry(memberName, logicalName));
            }

            object templateData = new Dictionary<string, object>()
            {
                ["Namespace"] = rootNamespace,
                ["Entries"] = entries
            };

            context.AddSource($"{rootNamespace}.Resources.g.cs", RenderSource("Views/Resources.hbs", templateData));
            context.AddSource($"{rootNamespace}.ResourceExtensions.g.cs", RenderSource("Views/ResourceExtensions.hbs", templateData));
        }

        private SourceText RenderSource(string templateName, object cotnext)
        {
            if (!m_templateMap.TryGetValue(templateName, out HandlebarsTemplate<object, object>? template))
            {
                using (Stream stream = s_assembly.GetManifestResourceStream(templateName))
                {
                    if(stream == null)
                    {
                        throw new ArgumentNullException($"Unable to find asset with name {templateName}");
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        template = Handlebars.Compile(content);
                        m_templateMap[templateName] = template;
                    }
                }
            }
            string classDefinition = template(cotnext);
            return SourceText.From(classDefinition, Encoding.UTF8);
        }
    }
}