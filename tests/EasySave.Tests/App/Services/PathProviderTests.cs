using EasySave.App.Services;
using System;
using System.IO;
using Xunit;

namespace EasySave.Tests.App.Services;

public class PathProviderTests
{
    [Fact]
    public void Paths_ShouldBeInAppData()
    {
        var provider = new PathProvider();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Verifie que le chemin commence bien par %APPDATA%
        Assert.StartsWith(appDataPath, provider.LogsPath, StringComparison.OrdinalIgnoreCase);
        // Verifie que la hierarchie demandee est respectee
        var expectedSuffix = Path.Combine("ProSoft", "EasySave");
        Assert.Contains(expectedSuffix, provider.LogsPath);
    }
}
