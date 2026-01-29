using System.Globalization;
using EasySave.Core.Models;
using EasySave.Core.Resources;
using EasySave.App.Console;

namespace EasySave.Tests.Console;

public class MenuControllerTests
{
    private static void SetCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static string RunWithConsoleIO(string input, Action action)
    {
        SetCulture("en-US");
        var originalIn = System.Console.In;
        var originalOut = System.Console.Out;

        try
        {
            using var reader = new StringReader(input);
            using var writer = new StringWriter();

            System.Console.SetIn(reader);
            System.Console.SetOut(writer);

            action();

            return writer.ToString();
        }
        finally
        {
            System.Console.SetIn(originalIn);
            System.Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void Start_ShouldExit_WhenUserSelects0()
    {
        SetCulture("en-US");
        var config = AppConfig.LoadDefaults();
        var controller = new MenuController(config);

        var output = RunWithConsoleIO("0\n", () => controller.Start(Array.Empty<string>()));
        
        Assert.NotNull(output);
    }

    [Fact]
    public void Start_ShouldShowInvalidChoice_WhenUserSelectsUnknownOption()
    {
        SetCulture("en-US");
        var config = AppConfig.LoadDefaults();
        var controller = new MenuController(config);
        
        var output = RunWithConsoleIO("999\n0\n", () => controller.Start(Array.Empty<string>()));

        Assert.Contains(Strings.Error_InvalidChoice, output);
    }

    // [Fact]
    // public void Start_ShouldTriggerCreateBackupJob_WhenUserSelects1()
    // {
    //     SetCulture("en-US");
    //     var config = AppConfig.LoadDefaults();
    //     var controller = new MenuController(config);
    //     
    //     var output = RunWithConsoleIO("1\n\n0\n", () => controller.Start(Array.Empty<string>()));
    //
    //     Assert.Contains("Create backup job – TODO", output);
    // }
    //
    // [Fact]
    // public void Start_ShouldTriggerRunBackupJob_WhenUserSelects2()
    // {
    //     SetCulture("en-US");
    //     var config = AppConfig.LoadDefaults();
    //     var controller = new MenuController(config);
    //
    //     var output = RunWithConsoleIO("2\n\n0\n", () => controller.Start(Array.Empty<string>()));
    //
    //     Assert.Contains("Run backup job – TODO", output);
    // }
    
    [Fact]
    public void Start_ShouldChangeLanguageToEnglish_WhenUserSelects3Then1()
    {
        SetCulture("fr-FR");
        var config = AppConfig.LoadDefaults();
        var controller = new MenuController(config);
        
        RunWithConsoleIO("3\n1\n\n0\n", () => controller.Start(Array.Empty<string>()));
        
        Assert.Equal("en", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        Assert.Equal(EasySave.Core.Enums.Language.English, config.Language);
    }

    [Fact]
    public void Start_ShouldChangeLanguageToFrench_WhenUserSelects3Then2()
    {
        SetCulture("en-US");
        var config = AppConfig.LoadDefaults();
        var controller = new MenuController(config);
        
        RunWithConsoleIO("3\n2\n\n0\n", () => controller.Start(Array.Empty<string>()));
        
        Assert.Equal("fr", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        Assert.Equal(EasySave.Core.Enums.Language.French, config.Language);
    }

}