# TedToolkit.InterpolatedParser

Heavily Inspired by the repo [InterpolatedParser](https://github.com/AntonBergaker/InterpolatedParser)
by [AntonBergaker](https://github.com/AntonBergaker).

I just make it can be used with Regex and more extensibility.

## Usage

```c#
var a = "";
"I am sooooo cool!!".Parse($"I am so+ {a}!+");
Console.WriteLine(a);
```

And you'll get `cool` in the console.