using System.Diagnostics;

// ReSharper disable All
#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.ValidResults;

[Conditional("GENERATE_VALID_RESULTS")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GenerateValidResultAttribute(Type type, string className = "") : Attribute;

[Conditional("GENERATE_VALID_RESULTS")]
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateValidResultAttribute<T> : Attribute;