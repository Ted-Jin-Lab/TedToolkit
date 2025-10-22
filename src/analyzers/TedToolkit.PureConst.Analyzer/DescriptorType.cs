using Microsoft.CodeAnalysis;

namespace TedToolkit.PureConst.Analyzer;

[AttributeUsage(AttributeTargets.Field)]
internal class DescriptorAttribute(string id, DiagnosticSeverity severity, string category) : Attribute
{
    public string Id { get; set; } = id;
    public DiagnosticSeverity Severity { get; set; } = severity;
    public string Category { get; set; } = category;
}

public enum DescriptorType : byte
{
    [Descriptor("PC0001", DiagnosticSeverity.Error, "Usage")]
    CantUseOnAccessor,

    [Descriptor("PC0002", DiagnosticSeverity.Error, "Usage")]
    FieldConstMethod,

    [Descriptor("PC0003", DiagnosticSeverity.Error, "Usage")]
    PropertyConstMethod,

    [Descriptor("PC0004", DiagnosticSeverity.Error, "Usage")]
    MethodConstMethod,

    [Descriptor("PC0005", DiagnosticSeverity.Error, "Usage")]
    VariableConstMethod,

    [Descriptor("PC0006", DiagnosticSeverity.Warning, "Usage")]
    FieldConstMethodWarning,

    [Descriptor("PC0007", DiagnosticSeverity.Warning, "Usage")]
    PropertyConstMethodWarning,

    [Descriptor("PC0008", DiagnosticSeverity.Warning, "Usage")]
    MethodConstMethodWarning,

    [Descriptor("PC0009", DiagnosticSeverity.Warning, "Usage")]
    VariableConstMethodWarning,

    [Descriptor("PC1001",
#if DEBUG
        DiagnosticSeverity.Warning,
#else
        DiagnosticSeverity.Info,
#endif
        "Debug")]
    CheckingSymbol,

    [Descriptor("PC1002",
#if DEBUG
        DiagnosticSeverity.Warning,
#else
        DiagnosticSeverity.Info,
#endif
        "Debug")]
    CantFindSymbol,

    [Descriptor("PC1003",
#if DEBUG
        DiagnosticSeverity.Warning,
#else
        DiagnosticSeverity.Info,
#endif
        "Debug")]
    AdditionalVariable
}