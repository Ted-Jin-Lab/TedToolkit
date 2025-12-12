using System.Text;
using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;

namespace TedToolkit.RoslynHelper.Names;

/// <summary>
/// </summary>
public class MethodName : TypeParametersName<IMethodSymbol>
{
    internal MethodName(IMethodSymbol methodSymbol) : base(methodSymbol)
    {
        Parameters = methodSymbol.Parameters.GetNames().ToArray();
        ReturnType = methodSymbol.ReturnType.GetName();
        ContainingType = methodSymbol.ContainingType.GetName();
        Signature = new MethodSignature(methodSymbol);
    }

    /// <summary>
    ///     The signature of the method
    /// </summary>
    public MethodSignature Signature { get; }

    /// <summary>
    /// </summary>
    public ParameterName[] Parameters { get; }

    /// <summary>
    ///     Return types.
    /// </summary>
    public TypeName ReturnType { get; }

    /// <summary>
    ///     ContainingType
    /// </summary>
    public TypeName ContainingType { get; }

    private protected override IEnumerable<ITypeParameterSymbol> GetTypeParameters(IMethodSymbol symbol)
    {
        return symbol.TypeParameters;
    }

    private protected override string GetSummaryName()
    {
        var builder = new StringBuilder(ContainingType.SummaryName)
            .Append('.')
            .Append(base.GetSummaryName());
        builder.Append('(').Append(string.Join(",", Parameters.Select(p =>
        {
            var stringBuilder = new StringBuilder();
            if (p.Symbol.ScopedKind is not ScopedKind.None)
            {
                stringBuilder.Append("scoped ");
            }

            switch (p.Symbol.RefKind)
            {
                case RefKind.Ref:
                    stringBuilder.Append("ref ");
                    break;
                case RefKind.Out:
                    stringBuilder.Append("in ");
                    break;
                case RefKind.In:
                    stringBuilder.Append("out ");
                    break;
            }

            return stringBuilder.Append(p.Type.SummaryName).ToString();
        }))).Append(')');
        return builder.ToString();
    }
}