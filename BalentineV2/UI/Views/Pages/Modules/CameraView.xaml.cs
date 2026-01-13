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

        // Placeholder kontrolü (Started/Stopped eventleri AhdCameraView'de var)
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

        if (status != PermissionStatus.Granted)
        {
            ErrLbl.Text = "Kamera izni yok";
            return;
        }
#endif

        // En basit sabit kurulum: 0 ve 1
        // (Gerekirse burada ID değiştirip denersin)
        InfoLbl.Text = "2 Kamera: ID 0 ve 1";
        ErrLbl.Text = "";

        CamLeft.CameraId = 0;
        CamRight.CameraId = 1;

        CamLeft.CameraResolution = 720;
        CamRight.CameraResolution = 720;

        CamLeft.Mirror = false;
        CamRight.Mirror = false;
    }
}
