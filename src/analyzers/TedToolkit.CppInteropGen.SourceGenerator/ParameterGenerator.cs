using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxExtensions = TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.CppInteropGen.SourceGenerator;

using static SyntaxFactory;

public sealed class ParameterGenerator : BaseParameterGenerator
{
    private readonly int _ptrCount;
    private readonly string _rawTypeName;
    private readonly string _cppTypeName;
    private readonly ParameterType _type;
    public override string Name { get; }

    public ParameterGenerator(string parameterString, string[] allClassNames,
        Dictionary<string, string> parameterTypeReplace)
    {
        var cleanedString = parameterString
            .Replace("const", string.Empty)
            .Replace('&', '*')
            .Trim();
        _ptrCount = cleanedString.Count(c => c == '*');
        var beautifulStrings = cleanedString
            .Replace("*", "")
            .Split(' ');
        var typeName = beautifulStrings.First(s => !string.IsNullOrEmpty(s));
        Name = beautifulStrings.Last(s => !string.IsNullOrEmpty(s));
        _rawTypeName = GetTypeNameFromCpp(typeName, allClassNames);
        _cppTypeName = parameterTypeReplace.TryGetValue(typeName, out var replaceName)
            ? replaceName
            : _rawTypeName;
        _type = CheckType();
        return;

        ParameterType CheckType()
        {
            var hasConst = parameterString.Contains("const");
            var hasRef = parameterString.Contains('&');
            if (hasConst || Name is "self" || !hasRef && !parameterString.Contains('*')) return ParameterType.None;

            return hasRef ? ParameterType.Out : ParameterType.Ref;
        }
    }


    public override bool HasHandle => Name is not "self" and not "handle"
                                      && MethodTypeName.Contains('*');

    public override bool IsRefOrOut => _type is ParameterType.Ref or ParameterType.Out;

    private string TypeNameWithPointer => _type switch
    {
        ParameterType.Ref or ParameterType.Out => _rawTypeName + new string('*', _ptrCount - 1),
        _ => _rawTypeName + new string('*', _ptrCount)
    };

    private string MethodTypeName
    {
        get
        {
            var s = TypeNameWithPointer;
            return s.EndsWith(".Data*") ? s.Substring(0, s.Length - 6) : s;
        }
    }

    public override TypeSyntax PublicType => IdentifierName(MethodTypeName);

    private bool IsData => TypeNameWithPointer.EndsWith(".Data*");

    public override ParameterSyntax GenerateParameter()
    {
        var parameter = Parameter(Identifier(Name)).WithType(PublicType);

        return _type switch
        {
            ParameterType.Ref => parameter.WithModifiers(TokenList(Token(SyntaxKind.RefKeyword))),
            ParameterType.Out => parameter.WithModifiers(TokenList(Token(SyntaxKind.OutKeyword))),
            _ => parameter
        };
    }

    public override IEnumerable<DelegateDeclarationSyntax> GenerateDelegates()
    {
        return [];
    }

    public override TypeSyntax InnerType => IdentifierName(_rawTypeName + new string('*', _ptrCount));
    public override TypeSyntax InnerCppType => IdentifierName(_cppTypeName + new string('*', _ptrCount));

    public override ArgumentSyntax GenerateArgument()
    {
        var name = IsData && _type is not ParameterType.Out ? Name + ".Ptr" : Name;
        if (_type is ParameterType.Out or ParameterType.Ref) name = "__" + name;
        if (_rawTypeName != _cppTypeName)
        {
            return Argument(PrefixUnaryExpression(SyntaxKind.PointerIndirectionExpression,
                CastExpression(PointerType(IdentifierName(_cppTypeName)),
                    PrefixUnaryExpression(SyntaxKind.AddressOfExpression, IdentifierName(name)))));
        }

        return Argument(IdentifierName(name));
    }

    public override IEnumerable<LocalDeclarationStatementSyntax> GenerateLocalDeclaration()
    {
        if (_type is ParameterType.Ref)
            yield return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .WithVariables(
                [
                    VariableDeclarator(Identifier("_" + Name))
                        .WithInitializer(EqualsValueClause(IdentifierName(Name)))
                ]));
        else
            yield return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(_rawTypeName + new string('*', _ptrCount - 1)))
                    .WithVariables(
                    [
                        VariableDeclarator(Identifier("_" + Name))
                            .WithInitializer(EqualsValueClause(LiteralExpression(
                                SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword))))
                    ]));

        yield return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
            .WithVariables(
            [
                VariableDeclarator(Identifier("__" + Name))
                    .WithInitializer(EqualsValueClause(PrefixUnaryExpression(
                        SyntaxKind.AddressOfExpression, IdentifierName("_" + Name))))
            ]));
    }

    public override ExpressionStatementSyntax GenerateAssignment()
    {
        return ExpressionStatement(AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(Name),
            IsData
                ? ImplicitObjectCreationExpression()
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("_" + Name)),
                        Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression))
                    ]))
                : IdentifierName("_" + Name)));
    }
}