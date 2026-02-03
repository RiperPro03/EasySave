using EasySave.App.Console.Input;

namespace EasySave.Tests.Console;

public class ArgsParserTests
{
    private readonly ArgsParser _parser = new();

    [Fact]
    public void Parse_Range_ReturnsAllIds()
    {
        var result = _parser.Parse("1-3");

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void Parse_List_ReturnsIds()
    {
        var result = _parser.Parse("1;3");

        Assert.Equal(new[] { 1, 3 }, result);
    }

    [Fact]
    public void Parse_MixedSyntax_Throws()
    {
        Assert.Throws<FormatException>(() => _parser.Parse("1-3;4"));
    }
}

