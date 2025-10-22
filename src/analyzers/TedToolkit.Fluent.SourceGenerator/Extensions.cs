using TedToolkit.RoslynHelper.Names;

namespace TedToolkit.Fluent.SourceGenerator;

internal static class Extensions
{
    public static MethodDeclarationSyntax AddTypeParameters(this MethodDeclarationSyntax method,
        TypeName type)
    {
        return method.AddTypeParameters(type.TypeParameters);
    }

    public static MethodDeclarationSyntax AddTypeParameters(this MethodDeclarationSyntax method,
        MethodName type)
    {
        return method.AddTypeParameters([
            ..type.ContainingType.TypeParameters.Concat(type.TypeParameters).ToImmutableHashSet()
        ]);
    }

    public static MethodDeclarationSyntax AddTypeParameters(this MethodDeclarationSyntax method,
        params TypeParamName[] typeParameters)
    {
        if (typeParameters.Length == 0) return method;
        method = method.WithTypeParameterList(TypeParameterList([..typeParameters.Select(t => t.Syntax)]));
        var constraints = typeParameters
            .Select(t => t.ConstraintClause).Where(i => i is not null).ToArray();
        if (constraints.Length == 0) return method;
        return method.WithConstraintClauses([..constraints!]);
    }

    public static MethodDeclarationSyntax AddAttributes(this MethodDeclarationSyntax method)
    {
        return method.WithAttributeLists(
        [
            GeneratedCodeAttribute(typeof(FluentGenerator)).AddAttributes(NonUserCodeAttribute())
        ]);
    }
}