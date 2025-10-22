using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TedToolkit.CppInteropGen.SourceGenerator;

public abstract class BaseParameterGenerator
{
    public enum ParameterType : byte
    {
        None,
        Ref,
        Out
    }
    public abstract string Name { get; }

    public abstract bool HasHandle { get; }
    public abstract bool IsRefOrOut { get; }
    public abstract TypeSyntax PublicType{ get; }
    public abstract TypeSyntax InnerType{ get; }
    public abstract ParameterSyntax GenerateParameter();
    public abstract IEnumerable<DelegateDeclarationSyntax> GenerateDelegates();
    public abstract IEnumerable<LocalDeclarationStatementSyntax> GenerateLocalDeclaration();
    public abstract ArgumentSyntax GenerateArgument();
    public abstract ExpressionStatementSyntax GenerateAssignment();
    public static IEnumerable<BaseParameterGenerator> GenerateParameters(string methodName, string parameterNames)
    {
        var funcPtrRegex = new Regex(
            @"(?<retType>\w[\w\s\*&]*)\s*\(\*(?<name>\w+)\)\s*\((?<args>[^\)]*)\)"
        );
        foreach (var parameterName in GetParameterNames(parameterNames))
        {
            var match = funcPtrRegex.Match(parameterName);
            if (match.Success)
            {
                var name = match.Groups["name"].Value;
                var args = match.Groups["args"].Value;
                var retType = match.Groups["retType"].Value;
                yield return new FunctionPointParameterGenerator(methodName, name, retType, args);
            }
            else
            {
                yield return new ParameterGenerator(parameterName);
            }
        }
    }


    private static IEnumerable<string> GetParameterNames(string input)
    {
        var level = 0;
        var start = 0;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            switch (c)
            {
                case '(':
                    level++;
                    break;
                case ')':
                    level--;
                    break;
                case ',' when level == 0:
                    yield return input.Substring(start, i - start).Trim();
                    start = i + 1;
                    break;
            }
        }

        yield return input.Substring(start).Trim();
    }

    protected static string GetTypeNameFromCpp(string typeName)
    {
        return typeName switch
        {
            // 整型
            "char" => "sbyte", // 通常为 signed char
            "signed char" => "sbyte",
            "unsigned char" => "byte",
            "short" or "short int" or "signed short" or "signed short int" => "short",
            "unsigned short" or "unsigned short int" => "ushort",
            "int" or "signed int" or "signed" or "long" => "int", // 注意 C++ 中 long == int（Windows）
            "unsigned" or "unsigned int" => "uint",
            "long long" or "signed long long" => "long",
            "unsigned long long" => "ulong",

            // 特殊整数类型
            "size_t" => "global::System.UIntPtr",
            "ptrdiff_t" => "global::System.IntPtr",

            // 字符
            "wchar_t" or "char16_t" => "char", // 注意 C++ wchar_t 通常是 UTF-16 或 UTF-32，平台相关
            "char32_t" => "int", // 可考虑使用 Rune 或自定义结构封装 Unicode Code Point

            // 浮点数
            "float" => "float",
            "double" => "double",
            "long double" => "double", // C# 没有 long double，通常映射为 double

            // 布尔
            "bool" => "bool",

            // void 类型
            "void" => "void",
            _ => typeName + ".Data"
        };
    }
}