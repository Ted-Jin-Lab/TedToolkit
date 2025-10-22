namespace TedToolkit.Assertions.Assertions;

public record CallerInfo(string MemberName, string FilePath, int LineCount)
{
    public override string ToString()
    {
        return $"at {MemberName} in {FilePath}:line {LineCount}";
    }
}