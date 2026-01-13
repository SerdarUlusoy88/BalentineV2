using System.Globalization;

namespace BalentineV2.Core.Localization;

public interface ILocalizationService
{
    CultureInfo CurrentCulture { get; }

    event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>
    /// App açılışında kayıtlı dili yükler (yoksa default TR).
    /// </summary>
    void Initialize();

    /// <summary>
    /// "tr-TR", "en-US" gibi culture name alır.
    /// </summary>
    void SetCulture(string cultureName);

    /// <summary>
    /// Üst barda gösterilecek kısa kod: TR / EN
    /// </summary>
    string GetDisplayLanguageCode();
}
