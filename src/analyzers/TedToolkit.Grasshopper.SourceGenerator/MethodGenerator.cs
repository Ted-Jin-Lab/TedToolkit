using System.Text;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Grasshopper.SourceGenerator;

public class MethodGenerator : BasicGenerator
{
    private readonly List<MethodParamItem> _parameters;

    public readonly bool IsAwaiter;

    public readonly MethodName Name;

    public MethodGenerator(ISymbol symbol) : base(symbol)
    {
        if (symbol is not IMethodSymbol methodSymbol)
            throw new ArgumentException("Symbol is not a type method symbol");
        Name = methodSymbol.GetName();
        IsAwaiter = methodSymbol.ReturnType.GetMembers("GetAwaiter")
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 0 &&
                      m.ReturnType.GetMembers().Any(m2 => m2.Name == "IsCompleted") &&
                      m.ReturnType.GetMembers().Any(m2 => m2.Name == "GetResult"));

        var owner = Name.ContainingType;
        var items = Name.Parameters.Select(p => new MethodParamItem(this, p, owner));
        if (methodSymbol.ReturnType.SpecialType is not SpecialType.System_Void
            && (!IsAwaiter || methodSymbol.ReturnType is INamedTypeSymbol { IsGenericType: true }))
            items = items.Append(new MethodParamItem(this, "result", methodSymbol.ReturnType.GetName(), ParamType.Out,
                owner,
                methodSymbol.GetReturnTypeAttributes()));

        _parameters = items.ToList();
    }

    public static string[] NeedIdNames { get; set; } = [];

    protected override bool NeedId => NeedIdNames.Contains(ClassName);

    protected override string IdName
    {
        get
        {
            var builder = new StringBuilder();
            var sig = Name.Signature;
            builder.Append(sig.ContainingType.GetName().FullName);
            builder.Append('.');
            builder.Append(sig.MethodName);
            builder.Append('.');
            builder.Append(sig.TypeArgumentsCount);
            builder.Append('(');
            builder.Append(string.Join(", ", sig.ParameterTypes.Select(type => type.GetName().FullName)));
            builder.Append(")(");
            builder.Append(string.Join(", ", sig.RefKinds.Select(type => type.ToString())));
            builder.Append(')');
            return builder.ToString();
        }
    }

    public override string ClassName => "Component_" + Name.Name;
    protected override char IconType => 'C';

    public static ITypeSymbol GlobalBaseComponent { get; set; } = null!;
    public static Func<string, INamedTypeSymbol> CreateSymbol { get; set; } = null!;

    protected override ClassDeclarationSyntax ModifyClass(ClassDeclarationSyntax classSyntax)
    {
        var baseComponent = DocumentObjectGenerator.GetBaseComponent(Name.Symbol.GetAttributes()) ??
                            (IsAwaiter ? CreateSymbol(RealClassName + ".TaskResult") : GlobalBaseComponent);

        var inputMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("RegisterInputParams"))
            .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(ParameterList([
                Parameter(Identifier("pManager"))
                    .WithType(IdentifierName("GH_InputParamManager"))
            ]))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithBody(Block(_parameters.Where(p => p.Type.HasFlag(ParamType.In)).Select((p, i) => p.IoBlock(true, i))));

        var outputMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("RegisterOutputParams"))
            .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(ParameterList([
                Parameter(Identifier("pManager"))
                    .WithType(IdentifierName("GH_OutputParamManager"))
            ]))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithBody(
                Block(_parameters.Where(p => p.Type.HasFlag(ParamType.Out)).Select((p, i) => p.IoBlock(false, i))));

        ExpressionSyntax invocation = InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(Name.ContainingType.FullName), IdentifierName(Name.Name)))
            .WithArgumentList(
                ArgumentList(
                [
                    .._parameters.Where(i => i.Parameter is not null)
                        .Select(i =>
                        {
                            var arg = Argument(IdentifierName(i.ParameterName));
                            return i.Parameter?.Symbol.RefKind switch
                            {
                                RefKind.Ref => arg.WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                                RefKind.In => arg.WithRefOrOutKeyword(Token(SyntaxKind.InKeyword)),
                                RefKind.Out => Argument(DeclarationExpression(IdentifierName("var"),
                                        SingleVariableDesignation(Identifier(i.ParameterName))))
                                    .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword)),
                                _ => arg
                            };
                        })
                ]));

        if (IsAwaiter) invocation = AwaitExpression(invocation);

        StatementSyntax invocationStatement = _parameters.Any(p => p.Parameter is null)
            ? LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .WithVariables([
                    VariableDeclarator(Identifier("result")).WithInitializer(EqualsValueClause(invocation))
                ]))
            : ExpressionStatement(invocation);

        IEnumerable<StatementSyntax> computeMembers;

        if (IsAwaiter)
        {
            computeMembers =
            [
                IfStatement(IdentifierName("InPreSolve"),
                    Block((IEnumerable<StatementSyntax>)
                    [
                        .._parameters.Where(p => p.Type.HasFlag(ParamType.In)).Select((p, i) => p.GetData(i)),

                        ExpressionStatement(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("TaskList"), IdentifierName("Add")))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(InvocationExpression(IdentifierName("Compute"))
                                    .WithArgumentList(
                                        ArgumentList(
                                        [
                                            .._parameters.Where(p => p.Type.HasFlag(ParamType.In))
                                                .Select(p => Argument(IdentifierName(p.Name)))
                                        ])))
                            ]))),
                        ReturnStatement()
                    ])),
                IfStatement(PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                        InvocationExpression(IdentifierName("GetSolveResults"))
                            .WithArgumentList(
                                ArgumentList(
                                [
                                    Argument(IdentifierName("DA")),
                                    Argument(DeclarationExpression(IdentifierName("var"),
                                            SingleVariableDesignation(Identifier("data"))))
                                        .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                                ]))),
                    Block((IEnumerable<StatementSyntax>)
                    [
                        .._parameters.Where(p => p.Type.HasFlag(ParamType.In)).Select((p, i) => p.GetData(i)),
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName("data"),
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                InvocationExpression(IdentifierName("Compute"))
                                    .WithArgumentList(ArgumentList(
                                    [
                                        .._parameters.Where(p => p.Type.HasFlag(ParamType.In))
                                            .Select(p => Argument(IdentifierName(p.Name)))
                                    ])),
                                IdentifierName("Result"))))
                    ]))
            ];

            if (_parameters.Any(p => p.Type.HasFlag(ParamType.Out)))
                computeMembers = computeMembers.Append(Block((IEnumerable<StatementSyntax>)
                [
                    .._parameters.Where(p => p.Type.HasFlag(ParamType.Out)).Select((p, i) => p.SetData(i, "data."))
                ]));
        }
        else
        {
            computeMembers =
            [
                .._parameters.Where(p => p.Type.HasFlag(ParamType.In)).Select((p, i) => p.GetData(i)),
                invocationStatement,
                .._parameters.Where(p => p.Type.HasFlag(ParamType.Out)).Select((p, i) => p.SetData(i))
            ];
        }

        var computeMethod =
            MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("SolveInstance"))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(
                    ParameterList(
                    [
                        Parameter(Identifier("DA"))
                            .WithType(IdentifierName("global::Grasshopper.Kernel.IGH_DataAccess"))
                    ]))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block(computeMembers));

        var readMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("Read"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(ParameterList([
                Parameter(Identifier("reader")).WithType(
                    IdentifierName("global::GH_IO.Serialization.GH_IReader"))
            ]))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithBody(Block((IEnumerable<StatementSyntax>)
            [
                .._parameters
                    .Where(p => p.Type is ParamType.Field && p.Io)
                    .Select(p => p.ReadData()),
                ReturnStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        BaseExpression(), IdentifierName("Read")))
                    .WithArgumentList(ArgumentList([Argument(IdentifierName("reader"))])))
            ]));

        var writeMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("Write"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(ParameterList([
                Parameter(Identifier("writer")).WithType(
                    IdentifierName("global::GH_IO.Serialization.GH_IWriter"))
            ]))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithBody(Block((IEnumerable<StatementSyntax>)
            [
                .._parameters
                    .Where(p => p.Type is ParamType.Field && p.Io)
                    .Select(p => p.WriteData()),
                ReturnStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        BaseExpression(), IdentifierName("Write")))
                    .WithArgumentList(ArgumentList([Argument(IdentifierName("writer"))])))
            ]));

        var addedMembers = baseComponent
            .GetBaseTypesAndThis()
            .SelectMany(t => t.GetMembers())
            .Where(m => m.DeclaredAccessibility switch
                {
                    Accessibility.Private => false,
                    Accessibility.ProtectedAndInternal when !baseComponent.ContainingAssembly
                        .Equals(Symbol.ContainingAssembly, SymbolEqualityComparer.Default) => false,
                    _ => true
                }
            )
            .Select(i => i.Name)
            .ToArray();

        classSyntax = classSyntax.WithParameterList(ParameterList())
            .WithBaseList(BaseList(
            [
                PrimaryConstructorBaseType(IdentifierName(baseComponent.GetName().FullName))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(GetArgumentKeyedString(".Component.Name", ObjName)),
                        Argument(GetArgumentKeyedString(".Component.Nickname", ObjNickname)),
                        Argument(GetArgumentKeyedString(".Component.Description", ObjDescription)),
                        Argument(GetArgumentCategory(Category)),
                        Argument(GetArgumentRawString("Subcategory." + (Subcategory ?? BaseSubcategory),
                            Subcategory ?? BaseSubcategory))
                    ]))
            ]))
            .AddMembers(
            [
                .._parameters
                    .Where(p => p.Type is ParamType.Field)
                    .Where(p => !addedMembers.Contains(p.ParameterName))
                    .Select(p => p.Field()),
                inputMethod,
                outputMethod,
                computeMethod,
                readMethod,
                writeMethod
            ]);


        if ((DocumentObjectGenerator.GetBaseAttribute(Name.Symbol.GetAttributes())
             ?? BaseAttribute) is { } attributeName)
            classSyntax = classSyntax.AddMembers(MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier("CreateAttributes"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block(ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, IdentifierName("m_attributes"),
                    ObjectCreationExpression(IdentifierName(attributeName))
                        .WithArgumentList(ArgumentList([Argument(ThisExpression())])))))));

        if (IsAwaiter)
            classSyntax = classSyntax.AddMembers(
                RecordDeclaration(SyntaxKind.RecordStructDeclaration,
                        Token(SyntaxKind.RecordKeyword), Identifier("TaskResult"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)))
                    .WithClassOrStructKeyword(Token(SyntaxKind.StructKeyword))
                    .WithParameterList(
                        ParameterList(
                        [
                            .._parameters.Where(p => p.Type.HasFlag(ParamType.Out))
                                .Select(p =>
                                    Parameter(Identifier(p.Name)).WithType(IdentifierName(p.TypeNameNoIoTask.FullName)))
                        ]))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                MethodDeclaration(GenericName(Identifier("global::System.Threading.Tasks.Task"))
                            .WithTypeArgumentList(TypeArgumentList([IdentifierName("TaskResult")])),
                        Identifier("Compute"))
                    .WithModifiers(
                        TokenList(Token(SyntaxKind.PrivateKeyword)))
                    .WithParameterList(
                        ParameterList(
                        [
                            .._parameters.Where(p => p.Type.HasFlag(ParamType.In))
                                .Select(p =>
                                    Parameter(Identifier(p.Name)).WithType(IdentifierName(p.TypeNameNoIoTask.FullName)))
                        ]))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(MethodGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.Threading.Tasks.Task"), IdentifierName("Run")))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(ParenthesizedLambdaExpression()
                                .WithAsyncKeyword(Token(SyntaxKind.AsyncKeyword))
                                .WithBlock(Block(
                                    invocationStatement,
                                    ReturnStatement(ObjectCreationExpression(IdentifierName("TaskResult"))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            .._parameters.Where(p => p.Type.HasFlag(ParamType.Out))
                                                .Select(p =>
                                                    Argument(IdentifierName(p.Name)))
                                        ])))))),
                            Argument(IdentifierName("CancelToken"))
                        ]))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );

        return classSyntax;
    }
}