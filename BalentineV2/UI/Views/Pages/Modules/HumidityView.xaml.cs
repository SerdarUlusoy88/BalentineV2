using System.Threading.Tasks;


namespace BalentineV2.UI.Views.Pages.Modules;

public partial class HumidityView : ContentPage
{
    public HumidityView()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Bazý cihazlarda autoplay gecikmeli tetikleniyor
        await Task.Delay(200);

        try { BgVideo?.Play(); } catch { }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try { BgVideo?.Stop(); } catch { }
    }
}
