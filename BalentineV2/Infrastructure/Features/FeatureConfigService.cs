using System;

using BalentineV2.Core.Features;

namespace BalentineV2.Infrastructure.Features;

public sealed class FeatureConfigService : IFeatureConfigService
{
    private readonly IFeatureConfigStore _store;

    public FeatureConfig Current { get; private set; }

    public event EventHandler<FeatureConfig>? Changed;

    public FeatureConfigService(IFeatureConfigStore store)
    {
        _store = store;

        Current = _store.Load();

        _store.ConfigChanged += (_, cfg) =>
        {
            Current = cfg;
            Changed?.Invoke(this, cfg);
        };
    }

    public bool IsEnabled(string? featureKey) => Current.IsEnabled(featureKey);

    public void SetEnabled(string featureKey, bool isEnabled)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            return;

        var next = featureKey switch
        {
            FeatureKeys.Fan => Current with { Fan = isEnabled },
            FeatureKeys.Humidity => Current with { Humidity = isEnabled },
            FeatureKeys.Camera => Current with { Camera = isEnabled },
            FeatureKeys.Lamp => Current with { Lamp = isEnabled },
            FeatureKeys.Scale => Current with { Scale = isEnabled },
            FeatureKeys.Hydraulic => Current with { Hydraulic = isEnabled },
            FeatureKeys.Lubrication => Current with { Lubrication = isEnabled },
            FeatureKeys.Monitoring => Current with { Monitoring = isEnabled },
            _ => Current
        };

        SetConfig(next);
    }

    public void SetConfig(FeatureConfig config)
    {
        // ✅ sadece Save; Changed store event'i ile gelecek
        Current = config;
        _store.Save(config);
    }
}
