using FluentResults;

namespace TedToolkit.ValidResults;

public class NullReferenceError : Error
{
    public string Name { get; }

    public NullReferenceError(string name) : base(($"The {name} is null"))
    {
        Name = name;
        Metadata[nameof(Name)] = name;
    }
}