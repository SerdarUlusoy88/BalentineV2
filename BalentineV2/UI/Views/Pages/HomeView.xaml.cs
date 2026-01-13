using System.Threading.Tasks;

namespace BalentineV2.UI.Views.Pages;

public partial class HomeView : ContentPage
{
    public HomeView()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Kontroller kapalıyken bazı cihazlarda autoplay tetiklenmeyebiliyor.
        await Task.Delay(200);

        try { BgVideo?.Play(); } catch { }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try { BgVideo?.Stop(); } catch { }
    }
}
