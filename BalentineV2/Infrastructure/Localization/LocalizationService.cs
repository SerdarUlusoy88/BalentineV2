using System.Globalization;
using Microsoft.Maui.Storage;
using BalentineV2.Core.Localization;

namespace BalentineV2.Infrastructure.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private const string PrefKey = "app.culture";
    private CultureInfo _current = CultureInfo.CurrentUICulture;

    public CultureInfo CurrentCulture => _current;

    public event EventHandler<CultureInfo>? CultureChanged;

    public void Initialize()
    {
        // 1) Kaydedilmiş kültür varsa onu kullan
        var saved = Preferences.Get(PrefKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(saved))
        {
            SetCulture(saved);
            return;
        }

        // 2) Yoksa varsayılan Türkçe
        SetCulture("tr-TR");
    }

    public void SetCulture(string cultureName)
    {
        CultureInfo culture;
        try
        {
            culture = new CultureInfo(cultureName);
        }
        catch
        {
            culture = new CultureInfo("tr-TR");
        }

        _current = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        // Default thread kültürleri (Android/Windows için önemli)
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Preferences.Set(PrefKey, culture.Name);

        CultureChanged?.Invoke(this, culture);
    }

    public string GetDisplayLanguageCode()
    {
        return _current.TwoLetterISOLanguageName.ToUpperInvariant(); // TR / EN
    }
}
