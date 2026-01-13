using System;
using Microsoft.Maui.Controls;
using BalentineV2.UI.Localization;

namespace BalentineV2.UI.Components.Common;

public partial class PageFooterView : ContentView
{
    private Loc? _loc;

    public PageFooterView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        var sp = Handler?.MauiContext?.Services;
        _loc = sp?.GetService(typeof(Loc)) as Loc;

        // Resource key’i sen belirle:
        // örn: Footer_PrismaticBaler = "PRİZMATİK BALYA MAKİNESİ"
        FooterText.Text = _loc is null ? "PRİZMATİK BALYA MAKİNESİ" : _loc["Footer_PrismaticBaler"];
    }
}
