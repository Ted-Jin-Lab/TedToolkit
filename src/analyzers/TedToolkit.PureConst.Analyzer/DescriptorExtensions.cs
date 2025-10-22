using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TedToolkit.PureConst.Analyzer;

public static class DescriptorExtensions
{
    private static readonly ConcurrentDictionary<DescriptorType, DiagnosticDescriptor> Cache = [];

    public static void Report(this SyntaxNodeAnalysisContext context, DescriptorType diagnosticType,
        Location location, params object?[]? messageArguments)
    {
        var diagnostic = Diagnostic.Create(GetDiagnosticDescriptor(diagnosticType),
            location, messageArguments);
        context.ReportDiagnostic(diagnostic);
    }

    public static DiagnosticDescriptor GetDiagnosticDescriptor(this DescriptorType diagnosticType)
    {
        return Cache.GetOrAdd(diagnosticType, static type =>
        {
            var attr = GetAttribute<DescriptorAttribute, DescriptorType>(type);
            if (attr is null) throw new NullReferenceException();
            return GetDiagnostic(attr.Id, attr.Severity, attr.Category);
        });

        static DiagnosticDescriptor GetDiagnostic(string diagnosticId, DiagnosticSeverity severity, string category)
        {
            return new DiagnosticDescriptor(diagnosticId,
                GetLocalizableString(diagnosticId + "Tittle"),
                GetLocalizableString(diagnosticId + "MessageFormat"),
                category, severity, severity > DiagnosticSeverity.Info,
                GetLocalizableString(diagnosticId + "Description"));

            static LocalizableString GetLocalizableString(string name)
            {
                return new LocalizableResourceString(name,
                    Resources.ResourceManager,
                    typeof(Resources));
            }
        }

        static TAttr? GetAttribute<TAttr, TEnum>(TEnum value) where TAttr : Attribute where TEnum : Enum
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString());
            return memberInfo.FirstOrDefault()?.GetCustomAttribute<TAttr>();
        }
    }
}