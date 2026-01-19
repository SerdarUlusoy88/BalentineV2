using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

using BalentineV2.Core.Features;

namespace BalentineV2.UI.Views.Pages.SettingsTabs;

public partial class MenuSelectionTab : ContentView
{
    private IFeatureConfigService? _features;

    private Color _enabledText;
    private Color _disabledText;

    private Color _accent;
    private Color _line;

    // ✅ Her satır için 1 kere label cache
    private readonly Dictionary<Grid, List<Label>> _rowLabels = new();

    // ✅ Buton cache
    private readonly Dictionary<string, (Button add, Button remove)> _buttons =
        new(StringComparer.OrdinalIgnoreCase);

    public MenuSelectionTab()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        var sp = Handler?.MauiContext?.Services;
        _features = sp?.GetService(typeof(IFeatureConfigService)) as IFeatureConfigService;

        _enabledText = GetColorOrFallback("ST_Text", Colors.White);
        _disabledText = GetColorOrFallback("ST_TextDim", Colors.Gray);

        _accent = GetColorOrFallback("ST_Accent", Colors.Gold);
        _line = GetColorOrFallback("ST_Line", Colors.LightGray);

        // ✅ Label cache'i 1 kez hazırla
        BuildRowCachesOnce();

        // ✅ button cache (x:Name ile)
        BuildButtonCachesOnce();

        if (_features is not null)
            _features.Changed += OnFeaturesChanged;

        // ✅ İlk state
        Dispatcher.Dispatch(() =>
        {
            if (_features is not null)
                ApplyState(_features.Current);
        });
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_features is not null)
            _features.Changed -= OnFeaturesChanged;
    }

    private void OnFeaturesChanged(object? sender, FeatureConfig cfg)
    {
        MainThread.BeginInvokeOnMainThread(() => ApplyState(cfg));
    }

    private void OnEnableClicked(object sender, EventArgs e)
    {
        if (_features is null) return;
        if (sender is not Button b) return;
        if (b.CommandParameter is not string key) return;

        _features.SetEnabled(key, true);
    }

    private void OnDisableClicked(object sender, EventArgs e)
    {
        if (_features is null) return;
        if (sender is not Button b) return;
        if (b.CommandParameter is not string key) return;

        _features.SetEnabled(key, false);
    }

    private void ApplyState(FeatureConfig cfg)
    {
        SetRowState("monitoring", Row_Monitoring, cfg.Monitoring);
        SetRowState("fan", Row_Fan, cfg.Fan);
        SetRowState("humidity", Row_Humidity, cfg.Humidity);
        SetRowState("camera", Row_Camera, cfg.Camera);

        SetRowState("lamp", Row_Lamp, cfg.Lamp);
        SetRowState("scale", Row_Scale, cfg.Scale);
        SetRowState("hydraulic", Row_Hydraulic, cfg.Hydraulic);
        SetRowState("lubrication", Row_Lubrication, cfg.Lubrication);
    }

    private void SetRowState(string key, Grid row, bool enabled)
    {
        // labels
        if (_rowLabels.TryGetValue(row, out var labels))
        {
            var c = enabled ? _enabledText : _disabledText;
            for (int i = 0; i < labels.Count; i++)
                labels[i].TextColor = c;
        }

        // buttons highlight (no background fill)
        if (_buttons.TryGetValue(key, out var pair))
        {
            // Enabled ise: remove (X) sarı — çünkü "çıkar" aksiyonu daha anlamlıdır
            // Disabled ise: add (+) sarı — çünkü "ekle" aksiyonu gerekir
            ApplyButtonHighlight(pair.add, pair.remove, enabled);
        }
    }

    private void ApplyButtonHighlight(Button add, Button remove, bool enabled)
    {
        // asla dolgu yok
        add.BackgroundColor = Colors.Transparent;
        remove.BackgroundColor = Colors.Transparent;

        // “beyaz” olarak ST_Text kullanıyoruz
        var white = _enabledText;

        if (enabled)
        {
            // ✅ Menü VAR: Ekle sarı, Çıkar beyaz
            add.BorderColor = _accent;
            add.TextColor = _accent;
            add.Opacity = 1.0;

            remove.BorderColor = _line;
            remove.TextColor = white;
            remove.Opacity = 0.85;
        }
        else
        {
            // ✅ Menü YOK: Çıkar sarı, Ekle beyaz
            remove.BorderColor = _accent;
            remove.TextColor = _accent;
            remove.Opacity = 1.0;

            add.BorderColor = _line;
            add.TextColor = white;
            add.Opacity = 0.85;
        }
    }


    private void BuildButtonCachesOnce()
    {
        _buttons.Clear();

        _buttons["monitoring"] = (BtnAdd_monitoring, BtnRemove_monitoring);
        _buttons["fan"] = (BtnAdd_fan, BtnRemove_fan);
        _buttons["humidity"] = (BtnAdd_humidity, BtnRemove_humidity);
        _buttons["camera"] = (BtnAdd_camera, BtnRemove_camera);

        _buttons["lamp"] = (BtnAdd_lamp, BtnRemove_lamp);
        _buttons["scale"] = (BtnAdd_scale, BtnRemove_scale);
        _buttons["hydraulic"] = (BtnAdd_hydraulic, BtnRemove_hydraulic);
        _buttons["lubrication"] = (BtnAdd_lubrication, BtnRemove_lubrication);
    }

    private void BuildRowCachesOnce()
    {
        _rowLabels.Clear();

        CacheRow(Row_Monitoring);
        CacheRow(Row_Fan);
        CacheRow(Row_Humidity);
        CacheRow(Row_Camera);

        CacheRow(Row_Lamp);
        CacheRow(Row_Scale);
        CacheRow(Row_Hydraulic);
        CacheRow(Row_Lubrication);
    }

    private void CacheRow(Grid row)
    {
        var list = new List<Label>(8);
        CollectLabels(row, list);
        _rowLabels[row] = list;
    }

    // ✅ IView üzerinden güvenli traversal
    private void CollectLabels(IView root, List<Label> acc)
    {
        if (root is Label lbl)
        {
            acc.Add(lbl);
            return;
        }

        if (root is Layout layout)
        {
            foreach (var child in layout.Children)
                CollectLabels(child, acc);
            return;
        }

        if (root is Grid grid)
        {
            foreach (var child in grid.Children)
                CollectLabels(child, acc);
            return;
        }

        if (root is ContentView cv && cv.Content is View cvContent)
        {
            CollectLabels(cvContent, acc);
            return;
        }

        if (root is Border b && b.Content is View borderContent)
        {
            CollectLabels(borderContent, acc);
            return;
        }

        if (root is ScrollView sv && sv.Content is View scrollContent)
        {
            CollectLabels(scrollContent, acc);
            return;
        }
    }

    private Color GetColorOrFallback(string key, Color fallback)
    {
        try
        {
            if (Application.Current?.Resources.TryGetValue(key, out var obj) == true && obj is Color col)
                return col;
        }
        catch { }
        return fallback;
    }
}
