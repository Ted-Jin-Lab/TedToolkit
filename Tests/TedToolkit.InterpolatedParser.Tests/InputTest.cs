using TedToolkit.InterpolatedParser.Options;

namespace TedToolkit.InterpolatedParser.Tests;

public class InputTest
{
    [Test]
    [MatrixDataSource]
    public async Task InputNumberTest(
        [Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 123, 456, 789)]
        int inputValue,
        [Matrix("Hello", "Hi", "Great", "Nice")]
        string inputText
    )
    {
        var text = "";
        $"value is {inputValue}, and the text is {inputText}."
            .Parse(new ParseOptions
                {
                    ParameterOptions = [nameof(inputValue)]
                },
                $"value is {inputValue}, and the text is {text}\\.");
        await Assert.That(text).IsEqualTo(inputText);
    }
}