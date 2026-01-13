using CommunityToolkit.Maui.Views;

namespace BalentineV2.UI.Popups;

public partial class LanguageDropdownPopup : Popup
{
    public LanguageDropdownPopup(LanguageDropdownVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnBackdropTapped(object? sender, TappedEventArgs e)
    {
        Close(null); // iptal
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var pick = e.CurrentSelection?.FirstOrDefault() as LanguageItemVm;
        if (pick == null) return;

        Close(pick); // seçimi geri döndür
    }
}

public sealed class LanguageDropdownVm
{
    public string Title { get; init; } = "Dil Seç";
    public List<LanguageItemVm> Items { get; init; } = new();
}

public sealed class LanguageItemVm
{
    public string Code { get; init; } = "TR";
    public string Name { get; init; } = "Türkçe";
    public string Culture { get; init; } = "tr-TR";
}
