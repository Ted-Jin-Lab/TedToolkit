namespace TedToolkit.InterpolatedParser.Tests;

public class FormatTest
{
    [Test]
    [MatrixDataSource]
    public async Task NumberFormatTest([Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)] int inputValue)
    {
        var value = 0;
        inputValue.ToString("X").Parse($"{value:X}");
        await Assert.That(value).IsEqualTo(inputValue);
    }
}