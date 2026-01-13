using System;

namespace BalentineV2.Core.Features;

public interface IFeatureConfigService
{
    FeatureConfig Current { get; }

    event EventHandler<FeatureConfig>? Changed;

    bool IsEnabled(string? featureKey);

    /// <summary>
    /// Tek bir feature'ı aç/kapa ve kalıcı kaydet.
    /// </summary>
    void SetEnabled(string featureKey, bool isEnabled);

    /// <summary>
    /// Tam config set et (toplu ayar).
    /// </summary>
    void SetConfig(FeatureConfig config);
}
