using EasySave.Core.Resources;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model for the About page placeholder.
/// </summary>
public sealed class AboutViewModel : ViewModelBase
{
    public string ProductName => "EasySave";
    public string Version => "v2.0.0";
    
    
    // -- Strings pour les différentes langues --
    // On expose le proxy au lieu de la classe Strings directement
    public StringsProxy strings { get; } = new();


    // Cette petite classe fait le pont avec tes ressources statiques
    public class StringsProxy
    {
        // On redirige chaque propriété vers la ressource statique correspondante
        public string About_Intro => EasySave.Core.Resources.Strings.About_Intro;
        public string Guide_Title => EasySave.Core.Resources.Strings.Guide_Title;
        public string Step1_Title => EasySave.Core.Resources.Strings.Step1_Title;
        public string Step1_Desc => EasySave.Core.Resources.Strings.Step1_Desc;
        public string Step2_Title => EasySave.Core.Resources.Strings.Step2_Title;
        public string Step2_Desc => EasySave.Core.Resources.Strings.Step2_Desc;
        public string Step3_Title => EasySave.Core.Resources.Strings.Step3_Title;
        public string Step3_Desc => EasySave.Core.Resources.Strings.Step3_Desc;
        public string Team_Title => EasySave.Core.Resources.Strings.Team_Title;
    }

    // Méthode pour forcer la mise à jour de l'UI quand on change de langue
    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(strings));
    }
    
    
}
