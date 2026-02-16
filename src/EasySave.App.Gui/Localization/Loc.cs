using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Core.Enums;
using EasySave.Core.Resources;
using EasySave.Core.Common;

namespace EasySave.App.Gui.Localization;

public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    // Permet: Loc.Instance["Gui_Nav_Settings"]
    public string this[string key] =>
        Strings.ResourceManager.GetString(key, Strings.Culture) ?? $"!{key}!";

    public Language Language
    {
        get
        {
            var c = Strings.Culture ?? CultureInfo.CurrentUICulture;
            return c.TwoLetterISOLanguageName == "fr" ? Language.French : Language.English;
        }
        set
        {
            var culture = Core.Common.Localization.GetCulture(value);
            Strings.Culture = culture;

            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            // refresh tous les bindings utilisant l'indexer
            OnPropertyChanged("Item[]");
        }
    }

    public void SetLanguage(Language language) => Language = language;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
