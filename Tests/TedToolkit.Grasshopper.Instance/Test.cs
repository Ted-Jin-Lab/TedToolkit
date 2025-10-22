using TedToolkit.Grasshopper;

namespace TedToolkit.Whatever;

public class Test
{
    private const string stringArg = "stringArgValue";

    private string _testString = "What a field".Loc();

    [DocObj]
    public static int Add(int x, int y = 5)
    {
        return x + y;
    }

    [DocObj]
    public static Task<int> AddAsync(int x, int y, out string name)
    {
        "What Hell".Loc();
        name = "Localization String".Loc("Optional Key");
        var b = stringArg.Loc(stringArg);
        return Task.FromResult(x + y);
    }
}