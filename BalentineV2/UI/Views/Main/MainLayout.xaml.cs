// File: UI/Views/Main/MainLayout.xaml.cs
using System;
using Microsoft.Maui.ApplicationModel;
using BalentineV2.UI.Navigation;

namespace BalentineV2.UI.Views.Main;

public partial class MainLayout : ContentPage
{
    private readonly INavigationService _navigationService;
    private bool _wired;

    public MainLayout(INavigationService navigationService)
    {
        InitializeComponent();

        _navigationService = navigationService;
        _navigationService.SetContentHost(ContentHost);

        // Toggle click
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, __) =>
        {
            IconRail.Toggle();
            PositionToggle();
        };
        RailToggle.GestureRecognizers.Add(tap);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_wired) return;
        _wired = true;

        // Dinamik ölçümler için
        SizeChanged += OnAnySizeChanged;
        IconRail.SizeChanged += OnAnySizeChanged;
        TopBar.SizeChanged += OnAnySizeChanged;

        // İlk yerleşim
        Dispatcher.Dispatch(PositionToggle);

        // İlk sayfa
        _navigationService.NavigateTo(Routes.Home);
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (!_wired) return;
        _wired = false;

        SizeChanged -= OnAnySizeChanged;
        IconRail.SizeChanged -= OnAnySizeChanged;
        TopBar.SizeChanged -= OnAnySizeChanged;
    }

    private void OnAnySizeChanged(object? sender, EventArgs e)
        => PositionToggle();

    private void PositionToggle()
    {
        // "<<", ">>" XAML'e yazmıyoruz
        RailToggle.Text = IconRail.IsExpanded ? "<<" : ">>";

        // X: Rail'in SAĞI
        RailToggle.TranslationX = IconRail.RailWidth + 8;

        // Y: TopBar yüksekliğinin altı (gerçek ölçüm)
        var topH = TopBar?.Height ?? 0;
        if (topH < 1 && TopBar != null) topH = TopBar.HeightRequest; // fallback

        RailToggle.TranslationY = topH + 8;
    }
}
