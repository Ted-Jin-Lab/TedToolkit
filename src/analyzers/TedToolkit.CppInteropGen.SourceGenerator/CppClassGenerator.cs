using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SyntaxExtensions = TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.CppInteropGen.SourceGenerator;

using static SyntaxExtensions;
using static SyntaxFactory;

public class CppClassGenerator
{
    private readonly string _className;
    private readonly string _fileName = string.Empty;
    private readonly Config _config;
    private readonly IReadOnlyList<string> _fields;
    private readonly IReadOnlyList<CMethodGenerator> _methods;

    public CppClassGenerator(SourceText text, string className, Config config, string[] allClassNames)
    {
        _config = config;
        _className = className;
        List<string> fields = new(4);
        List<CMethodGenerator> methods = [];
        StringBuilder? builder = null;

        Dictionary<string, string> typeDefinitions = [];

        foreach (var line in text.Lines)
        {
            var lineText = line.ToString().Trim();
            if (IsStartWith(ref lineText, "//STRUCT:"))
            {
                fields.Add(lineText.Trim());
            }
            else if (IsStartWith(ref lineText, "//FILE_NAME:"))
            {
                _fileName = lineText.Trim();
            }
            else if (IsStartWith(ref lineText, "CSHARP_WRAPPER("))
            {
                if (builder is not null) CheckBuilder(builder);
                builder = new StringBuilder(lineText.Trim());
            }
            else if (builder is not null)
            {
                builder.Append(lineText.Trim());
            }
            else if (IsStartWith(ref lineText, "typedef"))
            {
                var types = lineText.Substring(0, lineText.Length - 1).Split(' ');
                if (types.Length is not 2) continue;
                var replacedName = BaseParameterGenerator.GetTypeNameFromCpp(types[0], []);
                if (replacedName
                    is not "sbyte" and not "byte"
                    and not "short" and not "ushort"
                    and not "int" and not "uint"
                    and not "long" and not "ulong"
                    and not "global::System.IntPtr" and not "global::System.UIntPtr"
                    and not "char"
                    and not "float" and not "double")
                {
                    continue;
                }

                typeDefinitions[types[1]] = replacedName;
            }
        }

        if (builder is not null) CheckBuilder(builder);
        _fields = fields;
        _methods = methods;
        return;

        void CheckBuilder(StringBuilder stringBuilder)
        {
            var str = stringBuilder.ToString();
            var privateIt = str.Contains("//PRIVATE");
            var index = str.IndexOf('{');
            if (index < 0) return;
            var methodDeclare = str.Substring(0, index);
            methods.Add(new CMethodGenerator(methodDeclare, className, privateIt, allClassNames, typeDefinitions));
        }
    }

    private static bool IsStartWith(ref string content, string start)
    {
        if (!content.StartsWith(start)) return false;
        content = content.Substring(start.Length).Trim();
        return true;
    }

    public ClassDeclarationSyntax Generate()
    {
        IEnumerable<MemberDeclarationSyntax> members =
        [
            FieldDeclaration(VariableDeclaration(PointerType(IdentifierName("Data")))
                    .WithVariables([
                        VariableDeclarator(Identifier("Ptr"))
                    ]))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(CppClassGenerator))])
                .WithModifiers(
                    TokenList(Token(_config.IsInternal ? SyntaxKind.InternalKeyword : SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword))),

            StructDeclaration("Data")
                .WithAttributeLists(
                [
                    _fields.Any()
                        ? GeneratedCodeAttribute(typeof(CppClassGenerator))
                            .AddAttributes(Attribute(
                                    IdentifierName("global::System.Runtime.InteropServices.StructLayout"))
                                .WithArgumentList(AttributeArgumentList(
                                [
                                    AttributeArgument(MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("global::System.Runtime.InteropServices.LayoutKind"),
                                        IdentifierName("Sequential")))
                                ])))
                        : GeneratedCodeAttribute(typeof(CppClassGenerator))
                ])
                .WithModifiers(TokenList(
                    Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword)))
                .WithMembers([
                    .._fields.Select(f => ParseMemberDeclaration(f.Trim().TrimEnd(';') + ";"))
                        .OfType<MemberDeclarationSyntax>()
                        .Select(m => m
                            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(CppClassGenerator))]))
                ]),

            ConstructorDeclaration(Identifier(_className))
                .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(CMethodGenerator))])
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("ptr"))
                        .WithType(PointerType(IdentifierName("Data"))),
                    Parameter(Identifier("disposed"))
                        .WithType(PredefinedType(Token(SyntaxKind.BoolKeyword)))
                        .WithDefault(EqualsValueClause(
                            LiteralExpression(SyntaxKind.TrueLiteralExpression)))
                ]))
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    ArgumentList(
                    [
                        Argument(IdentifierName("disposed"))
                    ])))
                .WithBody(Block(
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("Ptr"), IdentifierName("ptr")))))
        ];
        if (!string.IsNullOrEmpty(_fileName))
            members = members.Append(
                PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("FileName"))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(CppClassGenerator))])
                    .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(LiteralExpression(SyntaxKind.StringLiteralExpression,
                        Literal(_fileName))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        return ClassDeclaration(_className)
            .WithModifiers(
                TokenList(Token(_config.IsInternal ? SyntaxKind.InternalKeyword : SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.SealedKeyword), Token(SyntaxKind.UnsafeKeyword), Token(SyntaxKind.PartialKeyword)))
            .WithAttributeLists([GeneratedCodeAttribute(typeof(CppClassGenerator))])
            .WithBaseList(BaseList(
            [
                SimpleBaseType(IdentifierName("global::TedToolkit.CppInteropGen.CppObject"))
            ]))
            .WithMembers(
            [
                ..members,
                .._methods.SelectMany(p => p.Generate())
            ]);
    }

    private class CMethodGenerator
    {
        private readonly string _className;
        private readonly string _csMethodName;
        private readonly bool _isPrivate;
        private readonly string _methodName;
        private readonly IReadOnlyList<BaseParameterGenerator> _parameters;

        public CMethodGenerator(string methodDeclare, string className, bool isPrivate,
            string[] allClassesName, Dictionary<string, string> parameterTypeReplace)
        {
            _className = className;
            var startIndex = methodDeclare.IndexOf('(');
            _methodName = methodDeclare.Substring(0, startIndex);
            _csMethodName = string.Join("_",
                _methodName.Substring(className.Length).Split('_').Where(x => !string.IsNullOrEmpty(x)));
            var endIndex = methodDeclare.LastIndexOf(')');

            _parameters = BaseParameterGenerator.GenerateParameters(_methodName,
                methodDeclare.Substring(startIndex + 1, endIndex - startIndex - 1), allClassesName,
                parameterTypeReplace).ToArray();

            _isPrivate = isPrivate || _parameters.Any(p => p.HasHandle);
        }

        public IEnumerable<MemberDeclarationSyntax> Generate()
        {
            if (_methodName.Contains("Create", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return ConstructorDeclaration(_className)
                    .WithModifiers(TokenList(Token(_isPrivate ? SyntaxKind.PrivateKeyword : SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(CMethodGenerator))])
                    .WithParameterList(ParameterList(
                    [
                        .._parameters.Take(_parameters.Count - 1).Select(p => p.GenerateParameter())
                    ]))
                    .WithBody(Block(
                        GetFunctionPointer(_parameters.Select((p, i) =>
                            FunctionPointerParameter(i == _parameters.Count-1 ? p.InnerType : p.InnerCppType))),
                        FixedStatement(VariableDeclaration(PointerType(PointerType(IdentifierName("Data"))))
                                .WithVariables([
                                    VariableDeclarator(Identifier("ptr"))
                                        .WithInitializer(EqualsValueClause(PrefixUnaryExpression(
                                            SyntaxKind.AddressOfExpression, IdentifierName("Ptr"))))
                                ]),
                            ExpressionStatement(InvocationExpression(IdentifierName("ThrowIfError"))
                                .WithArgumentList(ArgumentList([
                                    Argument(InvocationExpression(IdentifierName("__method"))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            .._parameters.Take(_parameters.Count - 1).Select(p => p.GenerateArgument()),
                                            Argument(IdentifierName("ptr"))
                                        ])))
                                ]))))
                    ));
                yield break;
            }

            if (_methodName.Contains("Delete", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Delete"))
                    .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(CMethodGenerator))])
                    .WithBody(Block(
                        GetFunctionPointer(_parameters.Select((p, i) =>
                            FunctionPointerParameter(i == 0 ? p.InnerType : p.InnerCppType))),
                        ExpressionStatement(InvocationExpression(IdentifierName("ThrowIfError"))
                            .WithArgumentList(ArgumentList([
                                Argument(InvocationExpression(IdentifierName("__method"))
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(IdentifierName("Ptr")),
                                    ])))
                            ])))
                    ));
                yield break;
            }

            foreach (var delegateDeclaration in _parameters.SelectMany(i => i.GenerateDelegates()))
            {
                yield return delegateDeclaration;
            }

            yield return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier(_isPrivate ? "_" + _csMethodName : _csMethodName))
                .WithModifiers(TokenList(Token(_isPrivate ? SyntaxKind.PrivateKeyword : SyntaxKind.PublicKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(CMethodGenerator))])
                .WithParameterList(ParameterList(
                [
                    .._parameters.Skip(1).Select(p => p.GenerateParameter())
                ]))
                .WithBody(Block((StatementSyntax[])
                [
                    .._parameters.Skip(1).Where(p => p.IsRefOrOut).SelectMany(p => p.GenerateLocalDeclaration()),

                    GetFunctionPointer(_parameters.Select((p, i) =>
                        FunctionPointerParameter(i == 0 ? p.InnerType : p.InnerCppType))),
                    ExpressionStatement(InvocationExpression(IdentifierName("ThrowIfError"))
                        .WithArgumentList(ArgumentList([
                            Argument(InvocationExpression(IdentifierName("__method"))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(IdentifierName("Ptr")),
                                    .._parameters.Skip(1).Select(p => p.GenerateArgument())
                                ])))
                        ]))),
                    .._parameters.Skip(1).Where(p => p.IsRefOrOut).Select(p => p.GenerateAssignment())
                ]));
            yield break;

            LocalDeclarationStatementSyntax GetFunctionPointer(IEnumerable<FunctionPointerParameterSyntax> parameters)
            {
                return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                    [
                        VariableDeclarator(Identifier("__method")).WithInitializer(EqualsValueClause(
                            CastExpression(FunctionPointerType().WithCallingConvention(
                                        FunctionPointerCallingConvention(Token(SyntaxKind.UnmanagedKeyword))
                                            .WithUnmanagedCallingConventionList(
                                                FunctionPointerUnmanagedCallingConventionList(
                                                [
                                                    FunctionPointerUnmanagedCallingConvention(
                                                        Identifier("Cdecl"))
                                                ])))
                                    .WithParameterList(FunctionPointerParameterList(
                                    [
                                        ..parameters,
                                        FunctionPointerParameter(IdentifierName("global::System.IntPtr"))
                                    ])),
                                InvocationExpression( IdentifierName("GetFunctionPointer"))
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            Literal(_methodName)))
                                    ])))))
                    ]));
            }
        }
    }
}