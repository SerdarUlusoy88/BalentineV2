using Microsoft.Extensions.DependencyInjection;
using BalentineV2.UI.Views.Main;

using BalentineV2.Core.Localization;
using BalentineV2.UI.Localization;

namespace BalentineV2;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();

        var locService = services.GetRequiredService<ILocalizationService>();
        var loc = services.GetRequiredService<Loc>();

        locService.Initialize();
        loc.Culture = locService.CurrentCulture;

        // ✅ XAML her yerden erişsin:
        Resources["Loc"] = loc;

        locService.CultureChanged += (_, culture) =>
        {
            loc.Culture = culture;
        };

        MainPage = services.GetRequiredService<MainLayout>();
    }
}
