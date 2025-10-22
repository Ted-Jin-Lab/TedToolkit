namespace TedToolkit.InterpolatedParser.Tests;

public class BasicTests
{
    [Test]
    [MatrixDataSource]
    public async Task OnlyOneTest([Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)] int inputValue)
    {
        var value = 0;
        inputValue.ToString().Parse($"{value}");
        await Assert.That(value).IsEqualTo(inputValue);
    }

    [Test]
    [MatrixDataSource]
    public async Task StartingTest([Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)] int inputValue)
    {
        var value = 0;
        $"{inputValue}ABC".Parse($"{value}ABC");
        await Assert.That(value).IsEqualTo(inputValue);
    }

    [Test]
    [MatrixDataSource]
    public async Task EndingTest([Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)] int inputValue)
    {
        var value = 0;
        $"ABC{inputValue}".Parse($"ABC{value}");
        await Assert.That(value).IsEqualTo(inputValue);
    }

    [Test]
    [MatrixDataSource]
    public async Task MiddleTest([Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)] int inputValue)
    {
        var value = 0;
        $"ABC{inputValue}DEF".Parse($"ABC{value}DEF");
        await Assert.That(value).IsEqualTo(inputValue);
    }

    [Test]
    [MatrixDataSource]
    public async Task MultiValueTest(
        [Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)]
        int inputValue,
        [Matrix("Hello", "Hi", "Great", "Nice")]
        string inputText
    )
    {
        var value = 0;
        var text = "";
        $"value is {inputValue}, and the text is {inputText}."
            .Parse($"value is {value}, and the text is {text}\\.");
        await Assert.That(value).IsEqualTo(inputValue);
        await Assert.That(text).IsEqualTo(inputText);
    }
}