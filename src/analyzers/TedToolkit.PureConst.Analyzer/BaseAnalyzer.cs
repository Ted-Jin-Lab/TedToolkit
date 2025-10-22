using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TedToolkit.PureConst.Analyzer;

public abstract class BaseAnalyzer : DiagnosticAnalyzer
{
    protected abstract IReadOnlyList<DescriptorType> Descriptors { get; }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [..Descriptors.Select(DescriptorExtensions.GetDiagnosticDescriptor)];
}