using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.EasyLog.Options;
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
        Assert.Equal(LogFormat.Json, cfg.LogFormat);
        Assert.Equal(LogStorageMode.LocalOnly, cfg.LogStorageMode);
        Assert.Equal("localhost", cfg.LogServerHost);
        Assert.Equal(9696, cfg.LogServerPort);
    }

    [Fact]
    public void ChangeLanguage_ShouldUpdateLanguage()
    {
        var cfg = AppConfig.LoadDefaults();

        cfg.ChangeLanguage(Language.French);

        Assert.Equal(Language.French, cfg.Language);
    }

    [Fact]
    public void ChangeLogFormat_ShouldUpdateValue()
    {
        var cfg = AppConfig.LoadDefaults();

        cfg.ChangeLogFormat(LogFormat.Xml);

        Assert.Equal(LogFormat.Xml, cfg.LogFormat);
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

    [Fact]
    public void ChangeLogStorageMode_ShouldUpdateValue()
    {
        var cfg = AppConfig.LoadDefaults();

        cfg.ChangeLogStorageMode(LogStorageMode.LocalAndServer);

        Assert.Equal(LogStorageMode.LocalAndServer, cfg.LogStorageMode);
    }

    [Fact]
    public void UpdateLogServerConnection_ShouldUpdateValues()
    {
        var cfg = AppConfig.LoadDefaults();

        cfg.UpdateLogServerConnection("192.168.1.50", 8080);

        Assert.Equal("192.168.1.50", cfg.LogServerHost);
        Assert.Equal(8080, cfg.LogServerPort);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(70000)]
    public void UpdateLogServerConnection_ShouldThrow_IfPortInvalid(int port)
    {
        var cfg = AppConfig.LoadDefaults();

        Assert.Throws<ArgumentOutOfRangeException>(() => cfg.UpdateLogServerConnection("localhost", port));
    }
}
