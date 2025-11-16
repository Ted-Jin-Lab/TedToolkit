using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

public sealed class UnitAttributeGenerator(DataCollection data)
{
    public readonly record struct QuantityUnit(Quantity Quantity, string Unit)
    {
        public string UnitName => Quantity.UnitName;
        public string Name => Quantity.Name;
    }

    public IEnumerable<QuantityUnit> QuantityUnits => data.Quantities.Where(q => q.IsBasic).Select(q =>
    {
        var unit = q.Units
            .Select(u => data.Units[u])
            .OrderBy(u => u.DistanceToDefault)
            .First().GetUnitName(data.Units.Values);
        return new QuantityUnit(q, unit);
    });
    
    public void Generate(SourceProductionContext context)
    {
        var c = ClassDeclaration("UnitsAttribute")
            .WithAttributeLists(
            [
                GeneratedCodeAttribute(typeof(UnitAttributeGenerator)).AddAttributes(Attribute(
                        IdentifierName("global::System.AttributeUsage"))
                    .WithArgumentList(AttributeArgumentList(
                    [
                        AttributeArgument(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.AttributeTargets"),
                            IdentifierName("Assembly")))
                    ])))
            ])
            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword)))
            .WithTypeParameterList(TypeParameterList([TypeParameter(Identifier("TData"))]))
            .WithMembers(
            [
                ..QuantityUnits.Select(q =>
                    PropertyDeclaration(IdentifierName(q.UnitName),
                            Identifier(q.Name))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitAttributeGenerator))])
                        .WithAccessorList(AccessorList(
                        [
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                            AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        ]))
                        .WithInitializer(
                            EqualsValueClause(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(q.UnitName),
                                    IdentifierName(q.Unit))))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))),
                
                PropertyDeclaration(IdentifierName("global::TedToolkit.Quantities.UnitFlag"),
                        Identifier("Flag"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitAttributeGenerator))])
                    .WithAccessorList(AccessorList(
                    [
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ]))
                    .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            ])
            .WithBaseList(BaseList(
            [
                SimpleBaseType(IdentifierName("global::System.Attribute"))
            ]))
            .WithConstraintClauses(
            [
                TypeParameterConstraintClause(IdentifierName("TData"))
                    .WithConstraints(
                    [
                        ClassOrStructConstraint(SyntaxKind.StructConstraint),
                        TypeConstraint(IdentifierName("global::System.IConvertible"))
                    ])
            ])
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        context.AddSource("_UnitsAttribute.g.cs",
            NamespaceDeclaration("TedToolkit.Quantities").WithMembers([c]).NodeToString());
    }
}