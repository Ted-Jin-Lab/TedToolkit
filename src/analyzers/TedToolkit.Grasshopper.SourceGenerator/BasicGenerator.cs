using System.Security.Cryptography;
using System.Text;
using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TedToolkit.Grasshopper.SourceGenerator;

public abstract class BasicGenerator
{
    private readonly Guid? _guid;
    public readonly ISymbol Symbol;

    public string ObjName, ObjNickname, ObjDescription;

    protected BasicGenerator(ISymbol symbol)
    {
        Symbol = symbol;
        ObjName = ObjNickname = ObjDescription = Symbol.Name;
        DocumentObjectGenerator.GetObjNames(Symbol.GetAttributes(), ref ObjName, ref ObjNickname, ref ObjDescription);
        var docObj = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.GetName().FullName == "global::TedToolkit.Grasshopper.DocObjAttribute");
        var keyName = docObj?.ConstructorArguments.Length > 0 ? docObj.ConstructorArguments[0].Value?.ToString() : null;
        KeyName = keyName ?? string.Empty;

        if (Symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                    is "global::TedToolkit.Grasshopper.ObjGuidAttribute") is
                { ConstructorArguments.Length: 1 } attr
            && attr.ConstructorArguments[0].Value?.ToString() is { } guid)
            try
            {
                _guid = new Guid(guid);
            }
            catch
            {
                _guid = null;
            }

        if (symbol.GetAttributes().Any(a =>
                a.AttributeClass?.GetName().FullName == "global::System.ObsoleteAttribute"))
        {
            IsObsolete = true;
            Exposure = "-1";
        }
        else
        {
            IsObsolete = false;
            var exposure = symbol.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.GetName().FullName == "global::TedToolkit.Grasshopper.ExposureAttribute");
            Exposure = exposure?.ConstructorArguments[0].Value?.ToString();
        }

        var s = symbol;
        while (s is not null)
        {
            var attributes = s.GetAttributes();
            Category ??= DocumentObjectGenerator.GetBaseCategory(attributes);
            Subcategory ??= DocumentObjectGenerator.GetBaseSubcategory(attributes);
            s = s.ContainingSymbol;
        }
    }

    protected virtual bool NeedId => false;
    public string NameSpace => Symbol.ContainingNamespace.ToString();
    protected abstract string IdName { get; }

    public abstract string ClassName { get; }

    public string RealClassName => ToRealName(ClassName);

    protected abstract char IconType { get; }

    public string KeyName
    {
        get => string.IsNullOrEmpty(field) ? NameSpace + "." + ToRealNameNoTags(ClassName) : field;
        set;
    }

    public static string BaseCategory { get; set; } = null!;
    public static string BaseSubcategory { get; set; } = null!;
    public static string? BaseAttribute { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }

    public string? Exposure { get; set; }
    public bool IsObsolete { get; }
    public Guid Id => _guid ?? StringToGuid(IdName);

    internal static HashSet<string> Icons { get; } = [];
    internal static HashSet<string> Categories { get; } = [];
    internal static Dictionary<string, string> Translations { get; } = [];

    public static Dictionary<string, CateInfo> CategoryInfos { get; set; } = [];

    private string ToRealNameNoTags(string className)
    {
        return NeedId ? className + "_" + Id.ToString("N").Substring(0, 8) : className;
    }

    protected string ToRealName(string name)
    {
        name = ToRealNameNoTags(name);
        return IsObsolete ? name + "_OBSOLETE" : name;
    }

    private static Guid StringToGuid(string input)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes); // Use the first 16 bytes
    }

    public sealed override string ToString()
    {
        return IdName;
    }

    protected abstract ClassDeclarationSyntax ModifyClass(ClassDeclarationSyntax classSyntax);

    public void GenerateSource(SourceProductionContext context)
    {
        var keyField = FieldDeclaration(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword))).WithVariables(
                [
                    VariableDeclarator(Identifier("ResourceKey")).WithInitializer(
                        EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(KeyName))))
                ]))
            .WithAttributeLists([GeneratedCodeAttribute(typeof(BasicGenerator))])
            .WithModifiers(
                TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ConstKeyword)));

        var guidProperty = PropertyDeclaration(IdentifierName("global::System.Guid"), Identifier("ComponentGuid"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithExpressionBody(ArrowExpressionClause(ImplicitObjectCreationExpression().WithArgumentList(ArgumentList(
            [
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Id.ToString("D"))))
            ]))))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(DocumentObjectGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var attributes = GeneratedCodeAttribute(typeof(BasicGenerator)).AddAttributes(NonUserCodeAttribute());
        if (IsObsolete) attributes = attributes.AddAttributes(ObsoleteAttribute());

        var iconProperty = PropertyDeclaration(IdentifierName("global::System.Drawing.Bitmap"), Identifier("Icon"))
            .WithModifiers([Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)])
            .WithExpressionBody(ArrowExpressionClause(PostfixUnaryExpression(
                SyntaxKind.SuppressNullableWarningExpression, InvocationExpression(
                        IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.GetIcon"))
                    .WithArgumentList(ArgumentList([
                        Argument(BinaryExpression(SyntaxKind.AddExpression,
                            BinaryExpression(SyntaxKind.AddExpression, LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(Symbol.ContainingAssembly.Name + ".Icons.")),
                                IdentifierName("ResourceKey")),
                            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(".png"))))
                    ])))))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(BasicGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var classSyntax = ClassDeclaration(RealClassName)
            .WithModifiers(
            [
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword),
                Token(SyntaxKind.PartialKeyword)
            ])
            .WithAttributeLists([attributes])
            .WithMembers([keyField, guidProperty, iconProperty]);

        if (Exposure is not null && int.TryParse(Exposure, out var exposure))
        {
            var exposureProperty = PropertyDeclaration(IdentifierName("global::Grasshopper.Kernel.GH_Exposure"),
                    Identifier("Exposure"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithExpressionBody(ArrowExpressionClause(CastExpression(
                    IdentifierName("global::Grasshopper.Kernel.GH_Exposure"), ParenthesizedExpression(LiteralExpression(
                        SyntaxKind.NumericLiteralExpression, Literal(exposure))))))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(BasicGenerator)).AddAttributes(NonUserCodeAttribute())
                ])
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            classSyntax = classSyntax.AddMembers(exposureProperty);
        }

        var item = NamespaceDeclaration(NameSpace)
            .WithMembers([ModifyClass(classSyntax)]);

        context.AddSource(NameSpace + "." + RealClassName + ".g.cs", item.NodeToString());
        Icons.Add(IconType + KeyName);
    }

    public static InvocationExpressionSyntax GetArgumentCategory(string? category)
    {
        var cateKey = category ?? string.Empty;
        if (!CategoryInfos.TryGetValue(cateKey, out var info)) info = new CateInfo();

        category ??= BaseCategory;
        var key = "Category." + category;
        Categories.Add(key);
        Icons.Add("c" + key);
        Translations[key + ".ShortName"] = string.IsNullOrEmpty(info.ShortName) ? ToShort(category) : info.ShortName;
        Translations[key + ".SymbolName"] =
            info.SymbolName is null ? char.ToUpper(category[0]).ToString() : info.SymbolName.ToString();

        return GetArgumentRawString(key, category);

        static string ToShort(string str)
        {
            var shorter = str.Where((c, i) => i == 0 || char.IsUpper(c)).ToArray();
            if (shorter.Any()) return new string(shorter).ToUpper();
            return char.ToUpper(str[0]).ToString();
        }
    }

    public static InvocationExpressionSyntax GetArgumentRawString(string key, string value)
    {
        Translations[key] = value;
        return GetArgumentString(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(key))));
    }

    public InvocationExpressionSyntax GetArgumentKeyedString(string key, string value)
    {
        Translations[KeyName + key] = value;
        return GetArgumentString(Argument(BinaryExpression(SyntaxKind.AddExpression,
            IdentifierName("ResourceKey"), LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(key)))));
    }

    public static InvocationExpressionSyntax GetArgumentString(ArgumentSyntax argument)
    {
        return InvocationExpression(
                IdentifierName("global::TedToolkit.Grasshopper.TedToolkitResources.Get"))
            .WithArgumentList(ArgumentList([argument]));
    }
}