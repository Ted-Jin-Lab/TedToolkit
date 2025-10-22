namespace TedToolkit.Fluent.Tests;

public class BasicFluentTest
{
    [Test]
    public Task PropertyTest()
    {
        return CheckAll<BasicType<Random, int>>(
            async obj => await Assert.That(obj.DataA).IsNull(),
            async obj => await Assert.That(obj.DataA).IsNotNull(),
            fluent => fluent.WithDataA(new Random()));
    }

    [Test]
    public Task MethodTest()
    {
        return CheckAll<BasicType<Random, int>>(
            async obj => await Assert.That(obj.Name).IsEmpty(),
            async obj => await Assert.That(obj.Name).IsEqualTo(nameof(Int32)),
            fluent => fluent.DoAMethod(1));
    }

    [Test]
    public Task ContinueTest()
    {
        return CheckAll<BasicType<Random, int>>(
            async obj =>
            {
                await Assert.That(obj.DataA).IsNull();
                await Assert.That(obj.Name).IsEmpty();
            },
            async obj =>
            {
                await Assert.That(obj.DataA).IsNull();
                await Assert.That(obj.Name).IsEqualTo(nameof(Int32));
            },
            fluent => fluent.DoAMethod(1).ContinueWhen(_ => false).WithDataA(new Random()));
    }

    [Test]
    public Task ExtensionAMethodTest()
    {
        return CheckAll<BasicType<Random, int>>(
            async obj => await Assert.That(obj.Name).IsEmpty(),
            async obj => await Assert.That(obj.Name).IsEqualTo(nameof(Int32)),
            fluent => fluent.DoAMethod_BasicType(1));
    }

    [Test]
    public Task ExtensionAMethod1Test()
    {
        return CheckAll<BasicType<Random, int>>(
            async obj => await Assert.That(obj.Name).IsEmpty(),
            async obj => await Assert.That(obj.Name).IsEqualTo(nameof(Int32)),
            fluent => fluent.DoAMethod_BasicType1(1));
    }

    [Test]
    public Task ExtensionBMethodTest()
    {
        return CheckAll<BasicType<Random, int>>(
            async obj => await Assert.That(obj.Name).IsEmpty(),
            async obj => await Assert.That(obj.Name).IsEqualTo(nameof(Int32)),
            fluent => fluent.DoBMethod(1));
    }

    private static async Task CheckAll<T>(Func<T, Task> checkBefore, Func<T, Task> checkAfter,
        Action<Fluent<T>> doWithFluent) where T : class, new()
    {
        await Task.WhenAll(BasicTest(checkBefore, checkAfter, doWithFluent, FluentType.Lazy),
            BasicTest(checkBefore, checkAfter, doWithFluent, FluentType.Immediate));
    }

    private static async Task BasicTest<T>(Func<T, Task> checkBefore, Func<T, Task> checkAfter,
        Action<Fluent<T>> doWithFluent, FluentType type) where T : class, new()
    {
        var obj = new T();
        await checkBefore(obj);

        var fluent = obj.AsFluent(type);
        doWithFluent(fluent);
        switch (type)
        {
            case FluentType.Lazy:
                await checkBefore(obj);
                break;
            case FluentType.Immediate:
                await checkAfter(obj);
                break;
            default:
                return;
        }

        _ = fluent.Result;
        await checkAfter(obj);
    }
}