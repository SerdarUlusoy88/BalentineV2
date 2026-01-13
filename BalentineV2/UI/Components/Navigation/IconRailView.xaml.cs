// File: UI/Components/Navigation/IconRailView.xaml.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

using BalentineV2.Core.Features;
using BalentineV2.Core.Menu;
using BalentineV2.UI.Navigation;

namespace BalentineV2.UI.Components.Navigation;

public partial class IconRailView : ContentView
{
    private IMenuProvider? _menuProvider;
    private INavigationService? _navigationService;
    private IFeatureConfigService? _features;

    public ObservableCollection<MenuItemModel> MenuItems { get; } = new();

    private bool _isExpanded = true;
    private bool _suppressSelection;

    public bool IsExpanded
    {
        get => _isExpanded;
        private set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged(nameof(IsExpanded));
            OnPropertyChanged(nameof(IsCollapsed));
            RailWidth = _isExpanded ? 260 : 72;
        }
    }

    public bool IsCollapsed => !IsExpanded;

    private double _railWidth = 260;
    public double RailWidth
    {
        get => _railWidth;
        private set
        {
            if (_railWidth == value) return;
            _railWidth = value;
            OnPropertyChanged(nameof(RailWidth));
        }
    }

    public IconRailView()
    {
        InitializeComponent();
        BindingContext = this;

        HandlerChanged += OnHandlerChanged;
        Unloaded += OnUnloaded;
    }

    public void Toggle() => IsExpanded = !IsExpanded;

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        var sp = Handler?.MauiContext?.Services;
        if (sp is null) return;

        _menuProvider = sp.GetService(typeof(IMenuProvider)) as IMenuProvider;
        _navigationService = sp.GetService(typeof(INavigationService)) as INavigationService;

        if (_features is not null)
            _features.Changed -= OnFeaturesChanged;

        _features = sp.GetService(typeof(IFeatureConfigService)) as IFeatureConfigService;
        if (_features is not null)
            _features.Changed += OnFeaturesChanged;

        if (_navigationService is not null)
            _navigationService.RouteChanged += OnRouteChanged;

        ReloadMenu(); // ✅ sadece ilk girişte / feature değişince lazım
        ReselectByRoute(_navigationService?.CurrentRoute);
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_features is not null)
            _features.Changed -= OnFeaturesChanged;

        if (_navigationService is not null)
            _navigationService.RouteChanged -= OnRouteChanged;
    }

    private void OnFeaturesChanged(object? sender, FeatureConfig cfg)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ReloadMenu();
            ReselectByRoute(_navigationService?.CurrentRoute);
        });
    }

    private void OnRouteChanged(object? sender, string route)
    {
        // ✅ SAYFA geçişlerinde menüyü reload etmiyoruz (hız)
        MainThread.BeginInvokeOnMainThread(() => ReselectByRoute(route));
    }

    private void ReloadMenu()
    {
        if (_menuProvider is null) return;

        _suppressSelection = true;
        try
        {
            // Android’de out-of-range riskini azaltır
            if (MenuList is not null)
                MenuList.SelectedItem = null;

            MenuItems.Clear();
            foreach (var item in _menuProvider.GetMenu())
                MenuItems.Add(item);
        }
        finally
        {
            _suppressSelection = false;
        }
    }

    private void ReselectByRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route)) return;
        if (MenuList is null) return;

        var match = MenuItems.FirstOrDefault(x =>
            string.Equals(x.Route, route, StringComparison.OrdinalIgnoreCase));

        _suppressSelection = true;
        try
        {
            MenuList.SelectedItem = match; // match null olabilir
        }
        finally
        {
            _suppressSelection = false;
        }
    }

    private void OnMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection) return;
        if (e.CurrentSelection is null || e.CurrentSelection.Count == 0) return;

        if (e.CurrentSelection[0] is not MenuItemModel item) return;
        if (string.IsNullOrWhiteSpace(item.Route)) return;

        _navigationService?.NavigateTo(item.Route);
    }

    private void OnLogoTapped(object sender, EventArgs e)
    {
        _suppressSelection = true;
        try
        {
            if (MenuList?.SelectedItem != null)
                MenuList.SelectedItem = null;

            _navigationService?.NavigateTo(Routes.Home);
        }
        finally
        {
            _suppressSelection = false;
        }
    }
}
