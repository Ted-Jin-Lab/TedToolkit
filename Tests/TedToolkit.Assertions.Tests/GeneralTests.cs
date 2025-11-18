using TedToolkit.Assertions.Assertions.Extensions;
using TedToolkit.Assertions.Exceptions;

namespace TedToolkit.Assertions.Tests;

public class GeneralTests
{
    [Test]
    public Task TestMethodHere()
    {
        List<int> a = [1, 2, 3];
        Assert.Throws<AssertionException>(() => a.Must().Not.Contain(2));
        return Task.CompletedTask;
    }

    [Test]
    public Task TestScopeAssertion()
    {
        new Execution.AssertionScope("Checking");
        List<int> a = [1, 2, 3];
        a.Must().Not.Contain(2);
        return Task.CompletedTask;
    }

    [Test]
    public Task TestScopeRightNowAssertion()
    {
        Assert.Throws<AssertionException>(() =>
        {
            new Execution.AssertionScope("Checking");
            List<int> a = [1, 2, 3];
            a.Must().Not.Contain(2).RightNow();
        });
        return Task.CompletedTask;
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