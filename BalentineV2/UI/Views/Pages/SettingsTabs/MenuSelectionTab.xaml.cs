// File: UI/Views/Pages/SettingsTabs/MenuSelectionTab.xaml.cs
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

    // ✅ Her satır için 1 kere label cache
    private readonly Dictionary<Grid, List<Label>> _rowLabels = new();

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

        // ✅ Label cache'i 1 kez hazırla (donmayı azaltır)
        BuildRowCachesOnce();

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
        SetRowState(Row_Monitoring, cfg.Monitoring);
        SetRowState(Row_Fan, cfg.Fan);
        SetRowState(Row_Humidity, cfg.Humidity);
        SetRowState(Row_Camera, cfg.Camera);

        SetRowState(Row_Lamp, cfg.Lamp);
        SetRowState(Row_Scale, cfg.Scale);
        SetRowState(Row_Hydraulic, cfg.Hydraulic);
        SetRowState(Row_Lubrication, cfg.Lubrication);
    }

    private void SetRowState(Grid row, bool enabled)
    {
        if (!_rowLabels.TryGetValue(row, out var labels))
            return;

        var c = enabled ? _enabledText : _disabledText;
        for (int i = 0; i < labels.Count; i++)
            labels[i].TextColor = c;
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

    // ✅ IView üzerinden güvenli traversal (CS1503 biter)
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
