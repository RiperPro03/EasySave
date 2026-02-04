using EasySave.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using EasySave.App.Services;

namespace EasySave.Tests;

public class PathProviderTests
{
    [Fact] 
    public void Paths_ShouldBeInProgramData()
    {
        var provider = new PathProvider();

        // Vérifie que le chemin commence bien par le dossier système Windows
        Assert.Contains("ProgramData", provider.LogsPath);
        // Vérifie que la hiérarchie demandée est respectée
        Assert.Contains("ProSoft\\EasySave", provider.LogsPath);
    }
}
