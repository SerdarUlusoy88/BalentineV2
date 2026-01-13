// File: Infrastructure/Menu/MenuProvider.cs
using System.Collections.Generic;
using System.Linq;

using BalentineV2.Core.Features;
using BalentineV2.Core.Menu;
using BalentineV2.Core.Localization;
using BalentineV2.UI.Navigation;

namespace BalentineV2.Infrastructure.Menu;

public sealed class MenuProvider : IMenuProvider
{
    private readonly IFeatureConfigService _features;
    private readonly ITextProvider _t;
    private readonly ILocalizationService _loc;

    // ✅ Aynı nesneleri UI’ya veriyoruz (IconRail aynı referansları tutacak)
    private readonly List<MenuItemModel> _all = new();
    private IReadOnlyList<MenuItemModel>? _cachedFiltered;

    public MenuProvider(IFeatureConfigService features, ITextProvider textProvider, ILocalizationService loc)
    {
        _features = features;
        _t = textProvider;
        _loc = loc;

        BuildAllOnce();

        // ✅ Dil değişince Title'lar güncellensin + cache kırılsın
        _loc.CultureChanged += (_, __) =>
        {
            UpdateTitles();
            InvalidateCache();
        };

        // ✅ Feature değişince MENÜ CACHE'İ KIR (anlık güncellenmesi için şart)
        _features.Changed += (_, __) =>
        {
            InvalidateCache();
        };
    }

    // Eski kodla uyumluluk
    public IReadOnlyList<MenuItemModel> BuildMenu() => GetMenu();

    public IReadOnlyList<MenuItemModel> GetMenu()
    {
        // Cache: Feature set değişmedikçe aynı liste döner.
        _cachedFiltered ??= _all
            .Where(m => m.FeatureKey is null || _features.IsEnabled(m.FeatureKey))
            .OrderBy(m => m.Order)
            .ToList();

        return _cachedFiltered;
    }

    private void InvalidateCache()
    {
        _cachedFiltered = null;
    }

    private void BuildAllOnce()
    {
        _all.Clear();

        _all.AddRange(new List<MenuItemModel>
        {
            new()
            {
                Id = "settings",
                Title = _t.Get("Menu_Settings"),
                Route = Routes.Settings,
                IconSvg = "ico_settings.svg",
                FeatureKey = null,
                Order = 10
            },
            new()
            {
                Id = "fan",
                Title = _t.Get("Menu_Fan"),
                Route = Routes.Fan,
                IconSvg = "ico_fan.svg",
                FeatureKey = FeatureKeys.Fan,
                Order = 20
            },
            new()
            {
                Id = "humidity",
                Title = _t.Get("Menu_Humidity"),
                Route = Routes.Humidity,
                IconSvg = "ico_humidity.svg",
                FeatureKey = FeatureKeys.Humidity,
                Order = 30
            },
            new()
            {
                Id = "camera",
                Title = _t.Get("Menu_Camera"),
                Route = Routes.Camera,
                IconSvg = "ico_camera.svg",
                FeatureKey = FeatureKeys.Camera,
                Order = 40
            },
            new()
            {
                Id = "lamp",
                Title = _t.Get("Menu_Lamp"),
                Route = Routes.Lamp,
                IconSvg = "ico_lamp.svg",
                FeatureKey = FeatureKeys.Lamp,
                Order = 50
            },
            new()
            {
                Id = "scale",
                Title = _t.Get("Menu_Scale"),
                Route = Routes.Scale,
                IconSvg = "ico_scale.svg",
                FeatureKey = FeatureKeys.Scale,
                Order = 60
            },
            new()
            {
                Id = "hydraulic",
                Title = _t.Get("Menu_Hydraulic"),
                Route = Routes.Hydraulic,
                IconSvg = "ico_hydraulic.svg",
                FeatureKey = FeatureKeys.Hydraulic,
                Order = 70
            },
            new()
            {
                Id = "lubrication",
                Title = _t.Get("Menu_Lubrication"),
                Route = Routes.Lubrication,
                IconSvg = "ico_lubrication.svg",
                FeatureKey = FeatureKeys.Lubrication,
                Order = 80
            },
            new()
            {
                Id = "monitoring",
                Title = _t.Get("Menu_Monitoring"),
                Route = Routes.Monitoring,
                IconSvg = "ico_monitoring.svg",
                FeatureKey = FeatureKeys.Monitoring,
                Order = 90
            },
        });
    }

    private void UpdateTitles()
    {
        // Aynı nesnelerin Title'ını değiştiriyoruz -> UI anında güncellenecek
        foreach (var m in _all)
        {
            m.Title = m.Id switch
            {
                "settings" => _t.Get("Menu_Settings"),
                "fan" => _t.Get("Menu_Fan"),
                "humidity" => _t.Get("Menu_Humidity"),
                "camera" => _t.Get("Menu_Camera"),
                "lamp" => _t.Get("Menu_Lamp"),
                "scale" => _t.Get("Menu_Scale"),
                "hydraulic" => _t.Get("Menu_Hydraulic"),
                "lubrication" => _t.Get("Menu_Lubrication"),
                "monitoring" => _t.Get("Menu_Monitoring"),
                _ => m.Title
            };
        }
    }
}
