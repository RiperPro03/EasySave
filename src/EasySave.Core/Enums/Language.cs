namespace EasySave.Core.Enums;
/// <summary>
/// Supported UI languages.
/// </summary>

public enum Language
{
    English,
    French
}

public static class LanguageExtensions
{
    public static Language[] GetValues() =>
        (Language[])Enum.GetValues(typeof(Language));
}



