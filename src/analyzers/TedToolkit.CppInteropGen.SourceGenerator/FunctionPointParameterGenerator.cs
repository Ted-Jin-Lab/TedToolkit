using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SyntaxExtensions = TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.CppInteropGen.SourceGenerator;

using static SyntaxExtensions;
using static SyntaxFactory;

public class FunctionPointParameterGenerator(string methodName, string name, string returnType, string args, string[] allClassNames, Dictionary<string, string> parameterTypeReplace)
    : BaseParameterGenerator
{
    private readonly IReadOnlyList<BaseParameterGenerator> _parameters =
        GenerateParameters(methodName + "_" + name, args, allClassNames, parameterTypeReplace).ToArray();

    private readonly ParameterGenerator _return = new(returnType, allClassNames, parameterTypeReplace);
    private readonly string _delegateName = methodName + "_" + name + "_Delegate";
    public override string Name { get; } = name;
    public override bool HasHandle => false;
    public override bool IsRefOrOut => false;

    public override TypeSyntax PublicType => IdentifierName(_delegateName);

    public override TypeSyntax InnerType => IdentifierName(_delegateName);
    public override TypeSyntax InnerCppType => IdentifierName(_delegateName);

    public override ParameterSyntax GenerateParameter()
    {
        return Parameter(Identifier(Name)).WithType(PublicType);
    }

    public override IEnumerable<DelegateDeclarationSyntax> GenerateDelegates()
    {
        foreach (var delegateDeclaration in _parameters.SelectMany(i => i.GenerateDelegates()))
        {
            yield return delegateDeclaration;
        }

        yield return DelegateDeclaration(_return.InnerType, Identifier(_delegateName))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(FunctionPointParameterGenerator)).AddAttributes(Attribute(
                        IdentifierName("global::System.Runtime.InteropServices.UnmanagedFunctionPointer"))
                    .WithArgumentList(AttributeArgumentList(
                    [
                        AttributeArgument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.Runtime.InteropServices.CallingConvention"),
                            IdentifierName("Cdecl")))
                    ])))
            ])
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(ParameterList(
            [
                .._parameters.Select(i =>
                    Parameter(Identifier(i.Name)).WithType(i.InnerCppType)),
            ]));
    }

    public override IEnumerable<LocalDeclarationStatementSyntax> GenerateLocalDeclaration()
    {
        return [];
    }


    public override ArgumentSyntax GenerateArgument()
    {
        return Argument(IdentifierName(Name));
    }

    public override ExpressionStatementSyntax GenerateAssignment()
    {
        throw new NotImplementedException();
    }
}