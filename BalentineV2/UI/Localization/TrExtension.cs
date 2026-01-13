using System.Globalization;
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Localization;

[ContentProperty(nameof(Key))]
public sealed class TrExtension : IMarkupExtension<string>
{
    public string Key { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(Key))
            return string.Empty;

        // En güncel culture ile çek
        var culture = CultureInfo.CurrentUICulture;
        return Resources.Strings.AppStrings.ResourceManager.GetString(Key, culture) ?? $"!{Key}!";
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}
