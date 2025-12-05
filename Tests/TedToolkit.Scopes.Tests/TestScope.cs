namespace TedToolkit.Scopes.Tests;

public class TestScope(int value) : ScopeBase<TestScope>
{
    public int Value => value;
}