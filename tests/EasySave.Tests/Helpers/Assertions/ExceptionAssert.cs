namespace EasySave.tests.Helpers.Assertions;

/// <summary>
/// Helpers d'assertions pour éviter de répéter Assert.Throws partout.
/// </summary>
internal static class ExceptionAssert
{
    public static void ThrowsArgumentNull(Action action)
        => Assert.Throws<ArgumentNullException>(action);

    public static void ThrowsArgumentException(Action action)
        => Assert.Throws<ArgumentException>(action);

    public static void ThrowsArgumentException(Action action, string paramName)
    {
        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal(paramName, ex.ParamName);
    }

    public static void ThrowsArgumentNull(Action action, string paramName)
    {
        var ex = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(paramName, ex.ParamName);
    }
}