using EasySave.Core.Common;
using EasySave.Core.Enums;

namespace EasySave.Tests.Core.Common;

public class LocalizationTests
{
    [Fact]
    public void GetCulture_ShouldReturnFrenchCulture()
    {
        var culture = Localization.GetCulture(Language.French);

        Assert.Equal("fr-FR", culture.Name);
    }

    [Fact]
    public void GetCulture_ShouldReturnEnglishCulture_ByDefault()
    {
        var culture = Localization.GetCulture(Language.English);

        Assert.Equal("en-US", culture.Name);
    }
}
