using System.ComponentModel;
using System.Globalization;

namespace BalentineV2.UI.Localization;

/// <summary>
/// XAML'den resx stringlerini çekmek için.
/// Culture değişince UI otomatik refresh olur.
/// </summary>
public sealed class Loc : INotifyPropertyChanged
{
    private CultureInfo _culture = CultureInfo.CurrentUICulture;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            _culture = value;
            // Tüm binding'leri yenilemek için null gönderiyoruz
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    // XAML: {Binding [Key]}
    public string this[string key]
        => Resources.Strings.AppStrings.ResourceManager.GetString(key, _culture) ?? $"!{key}!";
}
