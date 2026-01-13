// File: Infrastructure/StatusBar/StatusBarProvider.cs
using System;
using System.Collections.Generic;
using System.Linq;

using BalentineV2.Core.Features;
using BalentineV2.Core.StatusBar;

namespace BalentineV2.Infrastructure.StatusBar;

public sealed class StatusBarProvider : IStatusBarProvider
{
    private readonly IFeatureConfigService _features;

    private readonly List<StatusBarChipModel> _all = new();
    private IReadOnlyList<StatusBarChipModel>? _cached;

    public event EventHandler? Changed;

    public StatusBarProvider(IFeatureConfigService features)
    {
        _features = features;

        BuildAllOnce();

        // ✅ Feature değişince chip listesi değişebilir -> cache kır + event fırlat
        _features.Changed += (_, __) =>
        {
            _cached = null;
            Changed?.Invoke(this, EventArgs.Empty);
        };
    }

    public IReadOnlyList<StatusBarChipModel> GetChips()
    {
        _cached ??= _all
            .Where(c => c.IsVisible)
            .Where(c => c.FeatureKey is null || _features.IsEnabled(c.FeatureKey))
            .ToList();

        return _cached;
    }

    private void BuildAllOnce()
    {
        _all.Clear();

        _all.AddRange(new[]
        {
            new StatusBarChipModel
            {
                Id = "weight",
                Title = "Ağırlık",
                IconSvg = "status_weight.svg",
                UnitText = "Kg",
                IsVisible = true,
                FeatureKey = FeatureKeys.Scale,      // ✅ Tartı ile birlikte
                ValueProvider = () => "96.00"
            },
            new StatusBarChipModel
            {
                Id = "pressure",
                Title = "Basınç",
                IconSvg = "status_pressure.svg",
                UnitText = "bar",
                IsVisible = true,
                FeatureKey = FeatureKeys.Hydraulic,  // ✅ Hidrolik ile birlikte
                ValueProvider = () => "60"
            },
            new StatusBarChipModel
            {
                Id = "humidity",
                Title = "Nem",
                IconSvg = "status_humidity.svg",
                UnitText = "%",
                IsVisible = true,
                FeatureKey = FeatureKeys.Humidity,   // ✅ Nem sayfası ile birlikte
                ValueProvider = () => "45"
            },
            new StatusBarChipModel
            {
                Id = "rpm",
                Title = "RPM",
                IconSvg = "statusrpm.svg", // dosya adı böyle
                UnitText = "rpm",
                IsVisible = true,
                FeatureKey = null,         // ✅ her zaman
                ValueProvider = () => "900"
            },
            new StatusBarChipModel
            {
                Id = "bale_count",
                Title = "Balya",
                IconSvg = "status_balecount.svg",
                UnitText = "adet",
                IsVisible = true,
                FeatureKey = null,         // ✅ her zaman
                ValueProvider = () => "184.000"
            },
            new StatusBarChipModel
            {
                Id = "working_time",
                Title = "Süre",
                IconSvg = "status_workingtime.svg",
                UnitText = "saat",
                IsVisible = true,
                FeatureKey = null,         // ✅ her zaman
                ValueProvider = () => "540"
            },
        });
    }
}
