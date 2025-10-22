using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace TedToolkit.Grasshopper.SourceGenerator;

public static class CategoryGenerator
{
    public static void GenerateCategories(SourceProductionContext context, IAssemblySymbol assembly,
        HashSet<string> categories)
    {
        GenerateIcons(context, assembly, categories);
        GenerateInfos(context, assembly, categories);
    }

    private static void GenerateIcons(SourceProductionContext context, IAssemblySymbol assembly,
        IEnumerable<string> categories)
    {
        var addIcon = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("LoadIcon"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("iconName")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            ]))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(CategoryGenerator))
                    .AddAttributes(NonUserCodeAttribute())
            ])
            .WithBody(Block(IfStatement(IsPatternExpression(InvocationExpression(
                            IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.GetIcon"))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(BinaryExpression(SyntaxKind.AddExpression, BinaryExpression(
                                    SyntaxKind.AddExpression, LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        Literal(assembly.Name + ".Icons.")),
                                    IdentifierName("iconName")),
                                LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(".png"))))
                        ])),
                    UnaryPattern(RecursivePattern().WithPropertyPatternClause(PropertyPatternClause())
                        .WithDesignation(SingleVariableDesignation(Identifier("icon"))))), ReturnStatement()),
                ExpressionStatement(
                    InvocationExpression(
                            IdentifierName("global::Grasshopper.Instances.ComponentServer.AddCategoryIcon"))
                        .WithArgumentList(
                            ArgumentList(
                            [
                                Argument(InvocationExpression(
                                        IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.Get"))
                                    .WithArgumentList(ArgumentList([Argument(IdentifierName("iconName"))]))),
                                Argument(IdentifierName("icon"))
                            ])))));


        SavePriority(context, assembly, [addIcon], "CategoryIcons", categories, "LoadIcon");
    }

    private static void GenerateInfos(SourceProductionContext context, IAssemblySymbol assembly,
        IEnumerable<string> categories)
    {
        var method = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("LoadInfo"))
            .WithModifiers(
                TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("iconName")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            ]))
            .WithAttributeLists(
            [
                GeneratedCodeAttribute(typeof(CategoryGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithBody(Block(
                LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                    .WithVariables([
                        VariableDeclarator(Identifier("key"))
                            .WithInitializer(EqualsValueClause(InvocationExpression(
                                    IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.Get"))
                                .WithArgumentList(ArgumentList([Argument(IdentifierName("iconName"))]))))
                    ])),
                ExpressionStatement(
                    InvocationExpression(
                            IdentifierName("global::Grasshopper.Instances.ComponentServer.AddCategoryShortName"))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("key")),
                            Argument(InvocationExpression(
                                    IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.Get"))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(BinaryExpression(SyntaxKind.AddExpression,
                                        IdentifierName("iconName"),
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(".ShortName"))))
                                ])))
                        ]))),
                ExpressionStatement(
                    InvocationExpression(
                            IdentifierName("global::Grasshopper.Instances.ComponentServer.AddCategorySymbolName"))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("key")),
                            Argument(ElementAccessExpression(InvocationExpression(
                                        IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.Get"))
                                    .WithArgumentList(
                                        ArgumentList(
                                        [
                                            Argument(BinaryExpression(SyntaxKind.AddExpression,
                                                IdentifierName("iconName"),
                                                LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                    Literal(".SymbolName"))))
                                        ])))
                                .WithArgumentList(BracketedArgumentList(
                                [
                                    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))
                                ])))
                        ])))));

        SavePriority(context, assembly, [method], "CategoryInfos", categories, "LoadInfo");
    }

    private static void SavePriority(SourceProductionContext context, IAssemblySymbol assembly,
        IEnumerable<MemberDeclarationSyntax> members,
        string name, IEnumerable<string> categories, string methodName)
    {
        var loadMethod = LoadMethod(categories.Select(i => AddInvocation(methodName, i)));


        var node = NamespaceDeclaration(assembly.Name).WithMembers([
            ClassDeclaration("AssemblyPriority" + name)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword)))
                .WithBaseList(BaseList([
                    SimpleBaseType(IdentifierName("global::Grasshopper.Kernel.GH_AssemblyPriority"))
                ]))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(CategoryGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithMembers([loadMethod, ..members])
        ]);

        context.AddSource("AssemblyPriority." + name + ".g.cs", node.NodeToString());
        return;

        static MethodDeclarationSyntax LoadMethod(IEnumerable<StatementSyntax> members)
        {
            return MethodDeclaration(IdentifierName("global::Grasshopper.Kernel.GH_LoadingInstruction"),
                    Identifier("PriorityLoad"))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(CategoryGenerator))
                        .AddAttributes(NonUserCodeAttribute())
                ])
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithBody(Block((IEnumerable<StatementSyntax>)
                [
                    ..members,
                    ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("global::Grasshopper.Kernel.GH_LoadingInstruction"),
                        IdentifierName("Proceed")))
                ]));
        }

        static StatementSyntax AddInvocation(string methodName, string name)
        {
            return ExpressionStatement(InvocationExpression(IdentifierName(methodName))
                .WithArgumentList(ArgumentList([
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name)))
                ])));
        }
    }
}