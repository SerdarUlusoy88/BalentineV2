// File: UI/Views/Pages/Modules/CameraView.xaml.cs
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Views.Pages.Modules;

public partial class CameraView : ContentPage
{
    public CameraView()
    {
        InitializeComponent();

        CamLeft.Started += () => MainThread.BeginInvokeOnMainThread(() => LeftPh.IsVisible = false);
        CamLeft.Stopped += () => MainThread.BeginInvokeOnMainThread(() => LeftPh.IsVisible = true);

        CamRight.Started += () => MainThread.BeginInvokeOnMainThread(() => RightPh.IsVisible = false);
        CamRight.Stopped += () => MainThread.BeginInvokeOnMainThread(() => RightPh.IsVisible = true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Camera>();
#endif
    }
}
