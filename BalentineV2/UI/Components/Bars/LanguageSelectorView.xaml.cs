using BalentineV2.Core.Localization;

namespace BalentineV2.UI.Components.Bars;

public partial class LanguageSelectorView : ContentView
{
    private ILocalizationService? _locService;

    public event EventHandler? Clicked;

    public LanguageSelectorView()
    {
        InitializeComponent();
        Loaded += OnLoaded;

        // Header'a týklanýnca event fýrlat
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, __) => Clicked?.Invoke(this, EventArgs.Empty);
        Header.GestureRecognizers.Add(tap);
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        var sp = Handler?.MauiContext?.Services;
        if (sp == null) return;

        _locService = sp.GetService<ILocalizationService>();
        if (_locService == null) return;

        LangText.Text = _locService.GetDisplayLanguageCode();

        _locService.CultureChanged += (_, __) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LangText.Text = _locService.GetDisplayLanguageCode();
            });
        };
    }
}
