# TedToolkit.Assertions.Logging

Just for making TedToolkit.Assertions to logging.

## Usage

```c#
using var services = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.AddTedToolkitAssertion();
    })
    .BuildServiceProvider();

var logger = services.GetRequiredService<ILogger<Program>>();

var a = "Hello, My World!";
var b = new List<int> { 1, 2, 3 };

using (logger.BeginAssertionScope("Nice Scope{Cool}", a))
{
    b.Should().HaveCount(2, new AssertionParams()
    {
        ReasonFormat = "You are good",
        Tag = new EventId(15, "Nice event"),
    }).And.ContainSingle(3).Which.Could.Be(2);
}
```