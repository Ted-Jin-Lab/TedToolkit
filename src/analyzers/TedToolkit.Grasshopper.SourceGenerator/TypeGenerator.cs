using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Grasshopper.SourceGenerator;

public class TypeGenerator : BasicGenerator
{
    public readonly TypeName Name;
    public string ObjTypeName, ObjTypeDescription;

    public TypeGenerator(ISymbol symbol) : base(symbol)
    {
        if (symbol is not ITypeSymbol typeSymbol)
            throw new ArgumentException("Symbol is not a type symbol");
        Name = typeSymbol.GetName();
        ObjTypeName = ObjTypeDescription = Name.Name;
        GetObjNames(Symbol.GetAttributes(), ref ObjTypeName, ref ObjTypeDescription);
    }

    protected override string IdName => Name.FullName;

    public override string ClassName => "Param_" + Name.Name;
    protected override char IconType => 'P';

    public static ITypeSymbol BaseGoo { get; set; } = null!;

    private static void GetObjNames(IEnumerable<AttributeData> attributes, ref string name, ref string description)
    {
        if (attributes
                .FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                    is "global::TedToolkit.Grasshopper.TypeDescAttribute") is not
            { ConstructorArguments.Length: 2 } attr) return;

        var relay = attr.ConstructorArguments[0].Value?.ToString();
        if (!string.IsNullOrEmpty(relay)) name = relay!;
        relay = attr.ConstructorArguments[1].Value?.ToString();
        if (!string.IsNullOrEmpty(relay)) description = relay!;
    }

    protected override ClassDeclarationSyntax ModifyClass(ClassDeclarationSyntax classSyntax)
    {
        List<SimpleBaseTypeSyntax> baseTypes =
        [
            SimpleBaseType(GenericName(Identifier("global::Grasshopper.Kernel.GH_PersistentParam"))
                .WithTypeArgumentList(TypeArgumentList([
                    QualifiedName(
                        IdentifierName(RealClassName), IdentifierName("Goo"))
                ])))
        ];

        var basicGoo = (INamedTypeSymbol)(DocumentObjectGenerator.GetTypeAttribute(Name.Symbol.GetAttributes(),
                                              "global::TedToolkit.Grasshopper.BaseGooAttribute<>")?.Symbol
                                          ?? ((INamedTypeSymbol)BaseGoo).Construct(Name.Symbol));

        var gooClass = ClassDeclaration("Goo").WithModifiers(
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword),
                    Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(BaseList([
                SimpleBaseType(IdentifierName(basicGoo.GetName().FullName))
            ]))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithMembers(
            [
                ConstructorDeclaration(Identifier("Goo"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block()),

                ConstructorDeclaration(Identifier("Goo"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("value")).WithType(IdentifierName(Name.FullName))
                    ]))
                    .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                        ArgumentList([Argument(IdentifierName("value"))])))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block()),

                ConstructorDeclaration(Identifier("Goo"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList([Parameter(Identifier("other")).WithType(IdentifierName("Goo"))]))
                    .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                        ArgumentList(
                        [
                            Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("other"), IdentifierName("Value")))
                        ])))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block()),

                MethodDeclaration(IdentifierName("global::Grasshopper.Kernel.Types.IGH_Goo"), Identifier("Duplicate"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(ObjectCreationExpression(IdentifierName("Goo"))
                        .WithArgumentList(ArgumentList([Argument(IdentifierName("Value"))]))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("ToString"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(BinaryExpression(SyntaxKind.CoalesceExpression,
                        ConditionalAccessExpression(IdentifierName("Value"),
                            InvocationExpression(MemberBindingExpression(IdentifierName("ToString")))),
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("<null>")))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("IsValid"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(LiteralExpression(SyntaxKind.TrueLiteralExpression)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("TypeName"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(GetArgumentKeyedString(".TypeName", ObjTypeName)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("TypeDescription"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(
                        ArrowExpressionClause(GetArgumentKeyedString(".TypeDescription", ObjTypeDescription)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("CastFrom"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithParameterList(ParameterList([
                        Parameter(Identifier("source"))
                            .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                    ]))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block(IfStatement(InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("global::TedToolkit.Grasshopper.ActiveObjectHelper"),
                                        GenericName(Identifier("CastFrom"))
                                            .WithTypeArgumentList(TypeArgumentList([IdentifierName(Name.FullName)]))))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(IdentifierName("source")),
                                    Argument(DeclarationExpression(IdentifierName("var"),
                                            SingleVariableDesignation(Identifier("value"))))
                                        .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                                ])),
                            Block(ExpressionStatement(
                                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("Value"), IdentifierName("value"))),
                                ReturnStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression))))
                        .WithElse(ElseClause(Block(ReturnStatement(InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, BaseExpression(),
                                    IdentifierName("CastFrom")))
                            .WithArgumentList(ArgumentList([Argument(IdentifierName("source"))])))))))),

                MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("CastTo"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithTypeParameterList(TypeParameterList([TypeParameter(Identifier("Q"))]))
                    .WithParameterList(ParameterList([
                        Parameter(Identifier("target"))
                            .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                            .WithType(IdentifierName("Q"))
                    ]))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block(ReturnStatement(BinaryExpression(SyntaxKind.LogicalOrExpression,
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("global::TedToolkit.Grasshopper.ActiveObjectHelper"),
                                IdentifierName("CastTo")))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(IdentifierName("Value")),
                                Argument(IdentifierName("target")).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword))
                            ])),
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                BaseExpression(), IdentifierName("CastTo")))
                            .WithArgumentList(ArgumentList([
                                Argument(IdentifierName("target")).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword))
                            ]))))))
            ]);

        classSyntax = classSyntax.AddMembers(gooClass,
            ConstructorDeclaration(Identifier(RealClassName)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithInitializer(ConstructorInitializer(SyntaxKind.ThisConstructorInitializer,
                    ArgumentList(
                    [
                        Argument(GetArgumentKeyedString(".Name", ObjName)),
                        Argument(GetArgumentKeyedString(".Nickname", ObjNickname)),
                        Argument(GetArgumentKeyedString(".Description", ObjDescription)),
                        Argument(GetArgumentCategory(Category)),
                        Argument(GetArgumentRawString("Subcategory." + (Subcategory ?? "Parameter"),
                            Subcategory ?? "Parameter"))
                    ])))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block()),
            ConstructorDeclaration(Identifier(RealClassName)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList([
                    Parameter(Identifier("nTag"))
                        .WithType(IdentifierName("global::Grasshopper.Kernel.GH_InstanceDescription"))
                ]))
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    ArgumentList([Argument(IdentifierName("nTag"))])))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block()),
            ConstructorDeclaration(Identifier(RealClassName)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("name"))
                        .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                    Parameter(Identifier("nickname"))
                        .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                    Parameter(Identifier("description"))
                        .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                    Parameter(Identifier("category"))
                        .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                    Parameter(Identifier("subcategory"))
                        .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                ]))
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    ArgumentList(
                    [
                        Argument(IdentifierName("name")),
                        Argument(IdentifierName("nickname")),
                        Argument(IdentifierName("description")),
                        Argument(IdentifierName("category")),
                        Argument(IdentifierName("subcategory"))
                    ])))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block()),
            MethodDeclaration(IdentifierName("global::Grasshopper.Kernel.GH_GetterResult"),
                    Identifier("Prompt_Singular"))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("value")).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                        .WithType(IdentifierName("Goo"))
                ]))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block(
                    LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                        .WithVariables(
                        [
                            VariableDeclarator(Identifier("result"))
                                .WithInitializer(EqualsValueClause(MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("global::Grasshopper.Kernel.GH_GetterResult"),
                                    IdentifierName("cancel"))))
                        ])),
                    ExpressionStatement(InvocationExpression(IdentifierName("PromptSingular"))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("value")).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                            Argument(IdentifierName("result")).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword))
                        ]))),
                    ReturnStatement(IdentifierName("result")))),
            MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("PromptSingular"))
                .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("value"))
                        .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword))).WithType(IdentifierName("Goo")),
                    Parameter(Identifier("result")).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                        .WithType(IdentifierName("global::Grasshopper.Kernel.GH_GetterResult"))
                ]))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
            MethodDeclaration(IdentifierName("global::Grasshopper.Kernel.GH_GetterResult"), Identifier("Prompt_Plural"))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(
                    ParameterList(
                    [
                        Parameter(Identifier("values")).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                            .WithType(GenericName(Identifier("global::System.Collections.Generic.List"))
                                .WithTypeArgumentList(TypeArgumentList([IdentifierName("Goo")])))
                    ]))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block(
                    LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                        .WithVariables(
                        [
                            VariableDeclarator(Identifier("result")).WithInitializer(EqualsValueClause(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("global::Grasshopper.Kernel.GH_GetterResult"),
                                    IdentifierName("cancel"))))
                        ])),
                    ExpressionStatement(InvocationExpression(IdentifierName("PromptPlural"))
                        .WithArgumentList(
                            ArgumentList(
                            [
                                Argument(IdentifierName("values")).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                                Argument(IdentifierName("result")).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword))
                            ]))),
                    ReturnStatement(IdentifierName("result")))),
            MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("PromptPlural"))
                .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("values")).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                        .WithType(GenericName(Identifier("global::System.Collections.Generic.List"))
                            .WithTypeArgumentList(TypeArgumentList([IdentifierName("Goo")]))),
                    Parameter(Identifier("result")).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                        .WithType(IdentifierName("global::Grasshopper.Kernel.GH_GetterResult"))
                ]))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        );

        if (DocumentObjectGenerator.GetBaseAttribute(Name.Symbol.GetAttributes()) is { } attributeName)
            classSyntax = classSyntax.AddMembers(MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier("CreateAttributes"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithBody(Block(ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, IdentifierName("m_attributes"),
                    ObjectCreationExpression(IdentifierName(attributeName))
                        .WithArgumentList(ArgumentList([Argument(ThisExpression())])))))));

        if (Name.Symbol.AllInterfaces.Any(i => i.GetName().FullName
                is "global::Grasshopper.Kernel.IGH_PreviewData"))
        {
            baseTypes.Add(SimpleBaseType(IdentifierName("global::Grasshopper.Kernel.IGH_PreviewObject")));
            var gooClassPreview = ClassDeclaration("Goo").WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(BaseList([SimpleBaseType(IdentifierName("global::Grasshopper.Kernel.IGH_PreviewData"))]))
                .WithMembers(
                [
                    PropertyDeclaration(IdentifierName("global::Rhino.Geometry.BoundingBox"), Identifier("ClippingBox"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithExpressionBody(ArrowExpressionClause(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("Value"), IdentifierName("ClippingBox"))))
                        .WithAttributeLists([
                            GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                        ])
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("DrawViewportWires"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithParameterList(ParameterList(
                        [
                            Parameter(Identifier("args"))
                                .WithType(IdentifierName("global::Grasshopper.Kernel.GH_PreviewWireArgs"))
                        ]))
                        .WithExpressionBody(ArrowExpressionClause(
                            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("Value"), IdentifierName("DrawViewportWires")))
                                .WithArgumentList(ArgumentList([Argument(IdentifierName("args"))]))))
                        .WithAttributeLists([
                            GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                        ])
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("DrawViewportMeshes"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithParameterList(ParameterList(
                        [
                            Parameter(Identifier("args"))
                                .WithType(IdentifierName("global::Grasshopper.Kernel.GH_PreviewMeshArgs"))
                        ]))
                        .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("Value"), IdentifierName("DrawViewportMeshes")))
                            .WithArgumentList(ArgumentList([Argument(IdentifierName("args"))]))))
                        .WithAttributeLists([
                            GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                        ])
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                ]);

            classSyntax = classSyntax.AddMembers(gooClassPreview,
                PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)),
                        Identifier("Hidden")).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(AccessorList(
                    [
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ]))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ]),
                PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("IsPreviewCapable"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(LiteralExpression(SyntaxKind.TrueLiteralExpression)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                PropertyDeclaration(IdentifierName("global::Rhino.Geometry.BoundingBox"), Identifier("ClippingBox"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("global::TedToolkit.Grasshopper.ActiveObjectHelper"),
                                IdentifierName("GetClippingBox")))
                            .WithArgumentList(ArgumentList([Argument(IdentifierName("m_data"))]))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("DrawViewportWires"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("args")).WithType(
                            IdentifierName("global::Grasshopper.Kernel.IGH_PreviewArgs"))
                    ]))
                    .WithExpressionBody(ArrowExpressionClause(
                        InvocationExpression(IdentifierName("Preview_DrawWires"))
                            .WithArgumentList(ArgumentList([Argument(IdentifierName("args"))]))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("DrawViewportMeshes"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("args")).WithType(
                            IdentifierName("global::Grasshopper.Kernel.IGH_PreviewArgs"))
                    ]))
                    .WithExpressionBody(ArrowExpressionClause(
                        InvocationExpression(IdentifierName("Preview_DrawMeshes"))
                            .WithArgumentList(ArgumentList([Argument(IdentifierName("args"))]))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );
        }

        if (Name.Symbol.AllInterfaces.Any(i => i.GetName().FullName
                is "global::Grasshopper.Kernel.IGH_BakeAwareData"))
        {
            baseTypes.Add(SimpleBaseType(IdentifierName("global::Grasshopper.Kernel.IGH_BakeAwareObject")));
            var gooClassBake = ClassDeclaration("Goo").WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(
                    BaseList([SimpleBaseType(IdentifierName("global::Grasshopper.Kernel.IGH_BakeAwareData"))]))
                .WithMembers(
                [
                    MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("BakeGeometry"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithParameterList(
                            ParameterList(
                            [
                                Parameter(Identifier("doc"))
                                    .WithType(IdentifierName("global::Rhino.RhinoDoc")),
                                Parameter(Identifier("att"))
                                    .WithType(IdentifierName("global::Rhino.DocObjects.ObjectAttributes")),
                                Parameter(Identifier("obj_guid"))
                                    .WithModifiers(TokenList(Token(SyntaxKind.OutKeyword)))
                                    .WithType(IdentifierName("global::System.Guid"))
                            ]))
                        .WithExpressionBody(ArrowExpressionClause(
                            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("Value"), IdentifierName("BakeGeometry")))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(IdentifierName("doc")),
                                    Argument(IdentifierName("att")),
                                    Argument(IdentifierName("obj_guid"))
                                        .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                                ]))))
                        .WithAttributeLists([
                            GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                        ])
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                ]);

            classSyntax = classSyntax.AddMembers(gooClassBake,
                PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("IsBakeCapable"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(
                        PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("m_data"), IdentifierName("IsEmpty")))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("BakeGeometry"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                        [
                            Parameter(Identifier("doc")).WithType(
                                IdentifierName("global::Rhino.RhinoDoc")),
                            Parameter(Identifier("obj_ids"))
                                .WithType(GenericName(Identifier("global::System.Collections.Generic.List"))
                                    .WithTypeArgumentList(TypeArgumentList([IdentifierName("global::System.Guid")])))
                        ]))
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(IdentifierName("BakeGeometry"))
                        .WithArgumentList(
                            ArgumentList(
                            [
                                Argument(IdentifierName("doc")),
                                Argument(PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression,
                                    LiteralExpression(SyntaxKind.NullLiteralExpression))),
                                Argument(IdentifierName("obj_ids"))
                            ]))))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("BakeGeometry"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("doc"))
                            .WithType(IdentifierName("global::Rhino.RhinoDoc")),
                        Parameter(Identifier("att"))
                            .WithType(IdentifierName("global::Rhino.DocObjects.ObjectAttributes")),
                        Parameter(Identifier("obj_ids"))
                            .WithType(GenericName(Identifier("global::System.Collections.Generic.List"))
                                .WithTypeArgumentList(TypeArgumentList([IdentifierName("global::System.Guid")])))
                    ]))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(TypeGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block(
                        LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                            .WithVariables(
                            [
                                VariableDeclarator(Identifier("utility"))
                                    .WithInitializer(EqualsValueClause(ObjectCreationExpression(
                                            IdentifierName("global::Grasshopper.Kernel.GH_BakeUtility"))
                                        .WithArgumentList(ArgumentList(
                                            [Argument(InvocationExpression(IdentifierName("OnPingDocument")))]))))
                            ])),
                        ExpressionStatement(InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("utility"), IdentifierName("BakeObjects")))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(IdentifierName("m_data")),
                                Argument(IdentifierName("att")),
                                Argument(IdentifierName("doc"))
                            ]))),
                        ExpressionStatement(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("obj_ids"), IdentifierName("AddRange")))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("utility"), IdentifierName("BakedIds")))
                            ])))))
            );
        }

        return classSyntax.WithBaseList(BaseList([..baseTypes]));
    }
}