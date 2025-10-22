namespace TedToolkit.Pure.Tests;

public class PureConstAnalyzerTests
{
    [Test]
    public async Task SetSpeedHugeSpeedSpecified_AlertDiagnostic()
    {
        const string text = @"
public class Program
{
    public void Main()
    {
        var spaceship = new Spaceship();
        spaceship.SetSpeed(300000000);
    }
}

public class Spaceship
{
    public void SetSpeed(long speed) {}
}
";

        // var expected = Verify.Diagnostic(diagnosticId:"abc").WithLocation(7, 28)
        //     .WithArguments("300000000");

        //await Verify.VerifyAnalyzerAsync(text, expected);
    }
}