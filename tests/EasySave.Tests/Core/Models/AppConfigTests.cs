using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.tests.Helpers.Assertions;

namespace EasySave.Tests.Core.Models;

public class AppConfigTests
{
    [Fact]
    public void LoadDefaults_ShouldReturnValidConfig()
    {
        var cfg = AppConfig.LoadDefaults();

        Assert.True(cfg.LogDirectory.Length > 0);
        Assert.True(cfg.Language == Language.English || cfg.Language == Language.French);
    }

    [Fact]
    public void ChangeLanguage_ShouldUpdateLanguage()
    {
        var cfg = AppConfig.LoadDefaults();

        cfg.ChangeLanguage(Language.French);

        Assert.Equal(Language.French, cfg.Language);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateLogDirectory_ShouldThrow_IfInvalid(string? dir)
    {
        var cfg = AppConfig.LoadDefaults();

        ExceptionAssert.ThrowsArgumentException(
            () => cfg.UpdateLogDirectory(dir!),
            "logDirectory");
    }

    [Fact]
    public void UpdateLogDirectory_ShouldUpdateValue_IfValid()
    {
        var cfg = AppConfig.LoadDefaults();

        cfg.UpdateLogDirectory(@"C:\Logs");

        Assert.Equal(@"C:\Logs", cfg.LogDirectory);
    }
}