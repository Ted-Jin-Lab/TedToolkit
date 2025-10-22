using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Grasshopper.SourceGenerator;

public class UpgraderGenerator(string nameSpace, string className, IEnumerable<UpgraderGenerator.UpgraderItem> items)
{
    public static UpgraderGenerator? Create(MethodGenerator generator)
    {
        var upgraderItems = new List<UpgraderItem>();
        if (GetUpgradeToAttribute(generator.Symbol.GetAttributes(), generator.Id) is { } item) upgraderItems.Add(item);
        upgraderItems.AddRange(GetUpgradeFromAttribute(generator.Symbol.GetAttributes(), generator.RealClassName));
        if (upgraderItems.Count is 0) return null;
        return new UpgraderGenerator(generator.NameSpace, generator.RealClassName, upgraderItems);
    }

    private static DateTime GetDateTime(AttributeData attr, int startIndex)
    {
        var year = attr.ConstructorArguments[startIndex++].Value as int? ?? 2025;
        var month = attr.ConstructorArguments[startIndex++].Value as int? ?? 1;
        var day = attr.ConstructorArguments[startIndex++].Value as int? ?? 1;
        var hour = attr.ConstructorArguments[startIndex++].Value as int? ?? 0;
        var minute = attr.ConstructorArguments[startIndex++].Value as int? ?? 0;
        var second = attr.ConstructorArguments[startIndex].Value as int? ?? 0;
        return new DateTime(year, month, day, hour, minute, second);
    }


    public void GenerateSource(SourceProductionContext context)
    {
        var item = NamespaceDeclaration(nameSpace)
            .WithMembers([..items.Select(i => i.Generate(className))]);
        context.AddSource(nameSpace + "." + className + ".Upgraders.g.cs", item.NodeToString());
    }

    private static UpgraderItem? GetUpgradeToAttribute(IEnumerable<AttributeData> attributes, Guid id)
    {
        foreach (var attr in attributes)
        {
            if (attr.ConstructorArguments.Length is not 6) continue;
            if (attr.AttributeClass is not { } attributeClass) continue;
            if (!attributeClass.IsGenericType) continue;
            if (attributeClass.TypeArguments.Length < 1) continue;
            if (attributeClass.ConstructUnboundGenericType().GetName().FullName
                is not "global::TedToolkit.Grasshopper.UpgradeToAttribute<>") continue;
            // TODO: The full name doesn't work, you may need to make it with full name!
            var type = attr.AttributeClass!.TypeArguments[0].GetName().FullName;
            return new UpgraderItem(type, id, GetDateTime(attr, 0));
        }

        return null;
    }

    private static IEnumerable<UpgraderItem> GetUpgradeFromAttribute(IEnumerable<AttributeData> attributes, string type)
    {
        foreach (var attr in attributes)
        {
            if (attr.ConstructorArguments.Length is not 7) continue;
            if (attr.AttributeClass is not { } attributeClass) continue;
            if (attributeClass.GetName().FullName
                is not "global::TedToolkit.Grasshopper.UpgradeFromAttribute") continue;
            if (attr.ConstructorArguments[0].Value?.ToString() is { } guidString
                && Guid.TryParse(guidString, out var guid))
                yield return new UpgraderItem(type, guid, GetDateTime(attr, 1));
        }
    }

    public class UpgraderItem(string toType, Guid guid, DateTime time)
    {
        public ClassDeclarationSyntax Generate(string className)
        {
            var id = guid.ToString("D");

            return ClassDeclaration("Upgrader_" + className + "_" + id.Substring(0, 8))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword)))
                .WithParameterList(ParameterList())
                .WithBaseList(BaseList(
                [
                    PrimaryConstructorBaseType(
                            GenericName(Identifier("global::TedToolkit.Grasshopper.ComponentUpgrader"))
                                .WithTypeArgumentList(TypeArgumentList([IdentifierName(toType)])))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(ImplicitObjectCreationExpression().WithArgumentList(ArgumentList(
                            [
                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(id)))
                            ]))),
                            Argument(ImplicitObjectCreationExpression().WithArgumentList(ArgumentList(
                            [
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(time.Year))),
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(time.Month))),
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(time.Day))),
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(time.Hour))),
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(time.Minute))),
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(time.Second)))
                            ])))
                        ]))
                ]))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(UpgraderGenerator)).AddAttributes(NonUserCodeAttribute())
                ]);
        }
    }
}