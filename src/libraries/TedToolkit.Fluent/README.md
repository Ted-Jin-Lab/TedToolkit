# Fluent

Make your api fluent.

This is based on Source Generator to create the extensions method for you to make your API fluent.

## Usage

### Quick Start

Create your own data type.

```c#
class Point
{
    public double X { get; set; }
    public double Y { get; set; }
    public void AddX(double x) => X += x;
}
```

And then, you can modify it by using the method `AsFluent`.

```c#
var point = new Point().AsFluent()
    .WithX(5)
    .DoAddX(6)
    .Result;
```

It'll make your Properties and Fields to the method `WithXXX`, and your method to `DoXXX`.

### Api Generator

If the target's api isn't generated, you can use `FluentApi` Attribute to make it generate.

```c#
[assembly: FluentApi(typeof(BasicType<,>))]
[assembly: FluentApi(typeof(int))]

[FluentApi]
public class MyClass
{
    
}
```

Or you can just manually add your own extensions