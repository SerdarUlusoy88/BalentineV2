using BalentineV2.Core.Localization;

namespace BalentineV2.Infrastructure.Localization;

public sealed class ResxTextProvider : ITextProvider
{
    public string Get(string key)
        => Resources.Strings.AppStrings.ResourceManager.GetString(key)
           ?? $"!{key}!";
}
