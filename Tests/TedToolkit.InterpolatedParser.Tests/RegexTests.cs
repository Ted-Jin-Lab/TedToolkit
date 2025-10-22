namespace TedToolkit.InterpolatedParser.Tests;

public class RegexTests
{
    [Test]
    [MatrixDataSource]
    public async Task SooooTest(
        [Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
        int count,
        [Matrix("cute", "nice", "great", "handsome")]
        string inputText)
    {
        var text = "";
        $"I am s{new string('o', count)} {inputText}!"
            .Parse($"I am so+ {text}!");
        await Assert.That(text).IsEqualTo(inputText);
    }
}