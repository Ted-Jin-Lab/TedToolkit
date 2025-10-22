using System.Collections.Immutable;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Grasshopper.SourceGenerator;

[Flags]
public enum ParamType : byte
{
    Da = 1 << 0,
    Component = 1 << 1,
    Field = 1 << 2,
    In = 1 << 3,
    Out = 1 << 4
}

public enum ParamAccess : byte
{
    Item,
    List,
    Tree
}

public class MethodParamItem(
    MethodGenerator generator,
    string name,
    TypeName type,
    ParamType paramType,
    TypeName owner,
    ImmutableArray<AttributeData> attributes,
    bool io = false)
{
    private static readonly string[] IoNames =
    [
        "global::System.Threading.Tasks.Task<>",
        "global::System.Threading.Tasks.ValueTask<>",
        "global::TedToolkit.Grasshopper.Io<>"
    ];

    public readonly ParamAccess Access = GetParamAccess(GetTypeWithoutIo(type.Symbol));
    public readonly ImmutableArray<AttributeData> Attributes = attributes;
    public readonly bool Io = io;
    public readonly string Name = name;
    public readonly ParameterName? Parameter;
    public readonly ParamType Type = paramType;
    public readonly TypeName TypeName = type;
    public readonly TypeName TypeNameNoIoTask = GetTypeWithoutIo(type.Symbol).GetName();

    public MethodParamItem(MethodGenerator generator, ParameterName name, TypeName owner)
        : this(generator, name.Name, name.Type,
            GetParamType(name, out var io), owner,
            name.Symbol.GetAttributes(), io)
    {
        Parameter = name;
    }

    public static Dictionary<string, string> TypeDictionary { get; set; } = [];

    public string ParameterName => Type switch
    {
        ParamType.Component => "this",
        ParamType.Da => "DA",
        ParamType.Field => "_" + Name,
        _ => Name
    };

    protected string GetParamClassName(TypeName name)
    {
        var typeName = name.Symbol.TypeKind is TypeKind.Enum ? "int" : name.FullName;
        return TypeDictionary.TryGetValue(typeName, out var className)
            ? className
            : "global::Grasshopper.Kernel.Parameters.Param_GenericObject";
    }

    public LocalDeclarationStatementSyntax GetData(int index)
    {
        return LocalDeclarationStatement(VariableDeclaration(IdentifierName(TypeName.FullName))
            .WithVariables(
            [
                VariableDeclarator(Identifier(ParameterName)).WithInitializer(EqualsValueClause(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::TedToolkit.Grasshopper.ActiveObjectHelper"),
                            GenericName(Identifier("GetData" + Access)).WithTypeArgumentList(
                                TypeArgumentList([IdentifierName(GetInnerType(TypeName.Symbol).GetName().FullName)]))))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("DA")),
                            Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(index)))
                        ]))))
            ]));
    }

    public ExpressionStatementSyntax SetData(int index, string prefix = "")
    {
        var convertType = TypeNameNoIoTask.FullName;
        return ExpressionStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("global::TedToolkit.Grasshopper.ActiveObjectHelper"),
                IdentifierName("SetData" + Access)))
            .WithArgumentList(
                ArgumentList(
                [
                    Argument(IdentifierName("DA")),
                    Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(index))),
                    Argument(CastExpression(IdentifierName(convertType), IdentifierName(prefix + ParameterName)))
                ])));
    }

    public FieldDeclarationSyntax Field()
    {
        return FieldDeclaration(VariableDeclaration(IdentifierName(TypeName.FullName))
                .WithVariables([VariableDeclarator(Identifier(ParameterName))]))
            .WithAttributeLists([GeneratedCodeAttribute(typeof(MethodParamItem))])
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
    }

    public ExpressionStatementSyntax ReadData()
    {
        return ExpressionStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("global::TedToolkit.Grasshopper.IoHelper"), IdentifierName("Read")))
            .WithArgumentList(ArgumentList(
            [
                Argument(IdentifierName("reader")),
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(ParameterName))),
                Argument(IdentifierName(ParameterName)).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword))
            ])));
    }

    public ExpressionStatementSyntax WriteData()
    {
        return ExpressionStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("global::TedToolkit.Grasshopper.IoHelper"), IdentifierName("Write")))
            .WithArgumentList(ArgumentList(
            [
                Argument(IdentifierName("writer")),
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(ParameterName))),
                Argument(IdentifierName(ParameterName))
            ])));
    }

    public BlockSyntax IoBlock(bool isIn, int index)
    {
        return Block(BlockItems());

        IEnumerable<StatementSyntax> BlockItems()
        {
            yield return CreateParameter();
            if (Attributes.Any(a => a.AttributeClass?.GetName().FullName
                    is "global::TedToolkit.Grasshopper.HiddenAttribute"))
                yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                        IdentifierName("Hidden")), LiteralExpression(SyntaxKind.TrueLiteralExpression)));

            if (Attributes.FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                        is "global::TedToolkit.Grasshopper.ParamTagAttribute")
                    is { ConstructorArguments.Length: > 0 } tagAttr
                && tagAttr.ConstructorArguments[0].Value is byte b)
            {
                if (HasFlag(1 << 0) && isIn) // Principal
                    yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("PrincipalParameterIndex"), LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(index))));

                if (HasFlag(1 << 1)) // Reverse
                    yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                            IdentifierName("Reverse")), LiteralExpression(SyntaxKind.TrueLiteralExpression)));

                if (HasFlag(1 << 2)) // Flatten
                    yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("param"), IdentifierName("DataMapping")),
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::Grasshopper.Kernel.GH_DataMapping"), IdentifierName("Flatten"))));

                if (HasFlag(1 << 3)) // Graft
                    yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("param"), IdentifierName("DataMapping")),
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::Grasshopper.Kernel.GH_DataMapping"), IdentifierName("Graft"))));

                if (HasFlag(1 << 4)) // Simplify
                    yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                            IdentifierName("Simplify")), LiteralExpression(SyntaxKind.TrueLiteralExpression)));

                bool HasFlag(byte flag)
                {
                    return (b & flag) == flag;
                }
            }

            if (isIn && Attributes.Any(a => a.AttributeClass?.GetName().FullName
                    is "global::TedToolkit.Grasshopper.OptionalAttribute"))
                yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                        IdentifierName("Optional")), LiteralExpression(SyntaxKind.TrueLiteralExpression)));

            if (isIn && Attributes.FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                             is "global::TedToolkit.Grasshopper.PersistentDataAttribute")
                         is { ConstructorArguments.Length: > 0 } persistentAttribute1
                     && persistentAttribute1.ConstructorArguments[0].Value?.ToString() is { } property1)
                yield return PersistentData(owner.FullName, property1);

            if (isIn && Attributes.FirstOrDefault(a =>
                         {
                             if (a.AttributeClass is not { IsGenericType : true } attr) return false;
                             return attr.ConstructUnboundGenericType().GetName().FullName is
                                 "global::TedToolkit.Grasshopper.PersistentDataAttribute<>";
                         })
                         is
                         {
                             ConstructorArguments.Length: > 0, AttributeClass.TypeArguments.Length: > 0
                         } persistentAttribute2
                     && persistentAttribute2.ConstructorArguments[0].Value?.ToString() is { } property2
                     && persistentAttribute2.AttributeClass?.TypeArguments[0].GetName().FullName is { } customClass)
                yield return PersistentData(customClass, property2);

            if (isIn && (Parameter?.Symbol.HasExplicitDefaultValue ?? false))
            {
                if (GetLiteralExpression(Parameter.Symbol.ExplicitDefaultValue)
                    is { } literal)
                    yield return SetPersistentData(literal);

                LiteralExpressionSyntax? GetLiteralExpression(object? value)
                {
                    return value switch
                    {
                        null => LiteralExpression(SyntaxKind.NullLiteralExpression),
                        bool bo => LiteralExpression(bo
                            ? SyntaxKind.TrueLiteralExpression
                            : SyntaxKind.FalseLiteralExpression),
                        sbyte sb => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(sb)),
                        byte b => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(b)),
                        short s => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(s)),
                        ushort us => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(us)),
                        int i => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i)),
                        uint ui => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(ui)),
                        long l => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(l)),
                        ulong ul => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(ul)),
                        float f => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(f)),
                        double d => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(d)),
                        decimal m => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(m)),
                        char c => LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal(c)),
                        string s => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(s)),
                        _ => null
                    };
                }

                ExpressionStatementSyntax SetPersistentData(LiteralExpressionSyntax valueExpression)
                {
                    return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                            IdentifierName("SetPersistentData")))
                        .WithArgumentList(ArgumentList([Argument(valueExpression)])));
                }
            }

            if (GetInnerType(TypeName.Symbol) is { TypeKind: TypeKind.Enum } enumType)
                foreach (var fieldSymbol in enumType.GetMembers().OfType<IFieldSymbol>().Where(s => s.IsConst))
                    yield return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                            IdentifierName("AddNamedValue")))
                        .WithArgumentList(
                            ArgumentList(
                            [
                                Argument(BasicGenerator.GetArgumentRawString(
                                    enumType.GetName().FullNameNoGlobal + "." + fieldSymbol.Name, fieldSymbol.Name)),
                                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                    Literal(Convert.ToInt32(fieldSymbol.ConstantValue))))
                            ])));

            yield return AddParameter();
        }

        ExpressionStatementSyntax PersistentData(string className, string propertyName)
        {
            return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, IdentifierName("param"),
                    IdentifierName("SetPersistentData")))
                .WithArgumentList(ArgumentList([
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(className),
                        IdentifierName(propertyName)))
                ])));
        }

        LocalDeclarationStatementSyntax CreateParameter()
        {
            var guidAttribute = Attributes.FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                is "global::TedToolkit.Grasshopper.ParamTypeAttribute");
            if (guidAttribute is not null
                && guidAttribute.ConstructorArguments.Length > 0
                && guidAttribute.ConstructorArguments[0].Value?.ToString() is { } guidString)
                return LocalDeclarationStatement(VariableDeclaration(IdentifierName("dynamic"))
                    .WithVariables(
                    [
                        VariableDeclarator(Identifier("param")).WithInitializer(EqualsValueClause(InvocationExpression(
                                IdentifierName("global::Grasshopper.Instances.ComponentServer.EmitObject"))
                            .WithArgumentList(ArgumentList([
                                Argument(ImplicitObjectCreationExpression()
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            Literal(guidString)))
                                    ])))
                            ]))))
                    ]));

            var typeAttribute = Attributes.FirstOrDefault(a =>
                                {
                                    var attr = a.AttributeClass;
                                    if (attr is null) return false;
                                    if (!attr.IsGenericType) return false;
                                    if (attr.TypeArguments.Length < 1) return false;
                                    return attr.ConstructUnboundGenericType().GetName().FullName
                                        is "global::TedToolkit.Grasshopper.ParamTypeAttribute<>";
                                })?.AttributeClass?.TypeArguments[0].GetName().FullName
                                ?? GetParamClassName(GetInnerType(TypeName.Symbol).GetName());
            return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .WithVariables([
                    VariableDeclarator(Identifier("param"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName(typeAttribute))
                                .WithArgumentList(ArgumentList())))
                ]));
        }

        StatementSyntax AddParameter()
        {
            string name = Name, nickname = Name, description = Name;
            DocumentObjectGenerator.GetObjNames(Attributes, ref name, ref nickname, ref description);
            return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, IdentifierName("pManager"),
                    IdentifierName("AddParameter")))
                .WithArgumentList(ArgumentList(
                [
                    Argument(CastExpression(IdentifierName("global::Grasshopper.Kernel.IGH_Param"),
                        IdentifierName(Identifier("param")))),
                    Argument(generator.GetArgumentKeyedString("." + Name + ".Name", name)),
                    Argument(generator.GetArgumentKeyedString("." + Name + ".Nickname", nickname)),
                    Argument(generator.GetArgumentKeyedString("." + Name + ".Description", description)),
                    Argument(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::Grasshopper.Kernel.GH_ParamAccess"),
                            IdentifierName(Access switch
                            {
                                ParamAccess.List => "list",
                                ParamAccess.Tree => "tree",
                                _ => "item"
                            })))
                ])));
        }
    }

    private static ITypeSymbol GetInnerType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType) return type;

        if (namedType.AllInterfaces.FirstOrDefault(i =>
            {
                if (!i.IsGenericType) return false;
                if (i.TypeArguments.Length is 0) return false;
                return i.ConstructUnboundGenericType().GetName().FullName
                    is "global::System.Collections.Generic.IList<>";
            })?.TypeArguments[0] is { } listItem)
            return GetInnerType(listItem);

        if (!namedType.IsGenericType) return type;
        if (namedType.TypeArguments.Length is 0) return type;
        switch (namedType.ConstructUnboundGenericType().GetName().FullName)
        {
            case "global::TedToolkit.Grasshopper.Io<>":
            case "global::Grasshopper.Kernel.Data.GH_Structure<>":
            case "global::Grasshopper.DataTree<>":
                var innerType = namedType.TypeArguments[0];
                return GetInnerType(innerType);

            default:
                return type;
        }
    }


    private static ParamType GetParamType(ParameterName name, out bool io)
    {
        ParamType paramType = 0;
        var typeSymbol = name.Type.Symbol;
        if (name.Symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                is "global::TedToolkit.Grasshopper.ObjFieldAttribute") is { } fieldAttribute)
        {
            io = (bool?)fieldAttribute.ConstructorArguments.FirstOrDefault().Value ?? false;
            return ParamType.Field;
        }

        io = false;
        foreach (var type in typeSymbol.AllInterfaces.Append(typeSymbol))
            switch (type.GetName().FullName)
            {
                case "global::Grasshopper.Kernel.IGH_Component":
                    return ParamType.Component;
                case "global::Grasshopper.Kernel.IGH_DataAccess":
                    return ParamType.Da;
            }

        if (name.IsIn) paramType |= ParamType.In;
        if (name.IsOut) paramType |= ParamType.Out;
        return paramType;
    }

    private static ITypeSymbol GetTypeWithoutIo(ITypeSymbol typeSymbol)
    {
        return IoNames.Aggregate(typeSymbol, GetTypeWithoutType);

        static ITypeSymbol GetTypeWithoutType(ITypeSymbol typeSymbol, string typeName)
        {
            if (typeSymbol is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedTypeSymbol
                && namedTypeSymbol.ConstructUnboundGenericType().GetName().FullName ==
                typeName)
                return namedTypeSymbol.TypeArguments[0];
            return typeSymbol;
        }
    }

    private static ParamAccess GetParamAccess(ITypeSymbol typeSymbol)
    {
        foreach (var type in typeSymbol.AllInterfaces.Append(typeSymbol))
            switch (type.GetName().FullName)
            {
                case "global::Grasshopper.Kernel.Data.IGH_Structure":
                case "global::Grasshopper.Kernel.Data.IGH_DataTree":
                    return ParamAccess.Tree;
                case "global::System.Collections.IList":
                    return ParamAccess.List;
            }

        return ParamAccess.Item;
    }
}