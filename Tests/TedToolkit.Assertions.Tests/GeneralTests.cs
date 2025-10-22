using TedToolkit.Assertions.Exceptions;

namespace TedToolkit.Assertions.Tests;

public class GeneralTests
{
    [Test]
    public async Task TestMethodHere()
    {
        // List<int> a = [1, 2, 3];
        // a.Must().Contain(1).AndIt.Must.Not.Contain(2);
        //
        // new List<int>().Must();
        // using (new AssertionScope("Hi"))
        // {
        //     a.Must();
        //     a.Must().BeAssignableTo<double>("Bad reason.");
        // }
    }

    [Test]
    public Task TestMethodScopeHere()
    {
        using (new Execution.AssertionScope("Checking"))
        {
        }

        Assert.Throws<AssertionException>(() => 1.Must().Be(2));
        return Task.CompletedTask;
    }
}