// File: UI/Views/Pages/Modules/CameraView.xaml.cs
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

using BalentineV2.Core.Settings;
using BalentineV2.UI.Navigation;

namespace BalentineV2.UI.Views.Pages.Modules;

public partial class CameraView : ContentPage, IActivatablePage
{
    private IAppSettingsStore? _store;
    private bool _subscribed;
    private bool _permissionChecked;

    public CameraView()
    {
        InitializeComponent();

        CamLeft.Started += () => MainThread.BeginInvokeOnMainThread(() => LeftPh.IsVisible = false);
        CamLeft.Stopped += () => MainThread.BeginInvokeOnMainThread(() => LeftPh.IsVisible = true);

        CamRight.Started += () => MainThread.BeginInvokeOnMainThread(() =>
        {
            if (RightPanel.IsVisible) RightPh.IsVisible = false;
        });
        CamRight.Stopped += () => MainThread.BeginInvokeOnMainThread(() => RightPh.IsVisible = true);
    }

    public async Task OnActivatedAsync()
    {
        System.Diagnostics.Debug.WriteLine("[CAMVIEW] OnActivatedAsync");

        EnsureStore();
        System.Diagnostics.Debug.WriteLine($"[CAMVIEW] store_is_null={_store is null}");

#if ANDROID
        if (!_permissionChecked)
        {
            _permissionChecked = true;

            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();
        }
#endif

        ApplyFromStore();
        SubscribeStore();
    }

    public void OnDeactivated()
    {
        System.Diagnostics.Debug.WriteLine("[CAMVIEW] OnDeactivated");
        UnsubscribeStore();
    }

    private void EnsureStore()
    {
        if (_store is not null) return;

        // ✅ CameraView DI'dan resolve edildiği için handler bazen null olabilir; fallback var.
        _store =
            Handler?.MauiContext?.Services?.GetService(typeof(IAppSettingsStore)) as IAppSettingsStore
            ?? Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(IAppSettingsStore)) as IAppSettingsStore;
    }

    private void ApplyFromStore()
    {
        if (_store is null)
        {
            System.Diagnostics.Debug.WriteLine("[CAMVIEW] ApplyFromStore skipped: store null");
            return;
        }

        var app = _store.Load();
        var cam = (app.Camera ?? new CameraSettings()).Normalize();

        System.Diagnostics.Debug.WriteLine($"[CAMVIEW] Apply Count={cam.CameraCount} M1={cam.MirrorCam1} M2={cam.MirrorCam2}");

        ApplyCameraSettings(cam);
    }

    private void SubscribeStore()
    {
        if (_store is null) return;
        if (_subscribed) return;

        _store.Changed += OnSettingsChanged;
        _subscribed = true;

        System.Diagnostics.Debug.WriteLine("[CAMVIEW] Subscribed store.Changed");
    }

    private void UnsubscribeStore()
    {
        if (_store is null) return;
        if (!_subscribed) return;

        _store.Changed -= OnSettingsChanged;
        _subscribed = false;

        System.Diagnostics.Debug.WriteLine("[CAMVIEW] Unsubscribed store.Changed");
    }

    private void OnSettingsChanged(object? sender, AppSettings settings)
    {
        var cam = (settings.Camera ?? new CameraSettings()).Normalize();

        System.Diagnostics.Debug.WriteLine($"[CAMVIEW] Changed => Count={cam.CameraCount} M1={cam.MirrorCam1} M2={cam.MirrorCam2}");

        MainThread.BeginInvokeOnMainThread(() => ApplyCameraSettings(cam));
    }

    private void ApplyCameraSettings(CameraSettings cam)
    {
        cam = cam.Normalize();

        // Mirror uygula
        CamLeft.Mirror = cam.MirrorCam1;

        var isTwo = cam.CameraCount == 2;

        // ✅ UI kesin
        RightPanel.IsVisible = isTwo;
        CamRight.IsVisible = isTwo;

        if (RootGrid.ColumnDefinitions.Count >= 2)
        {
            RootGrid.ColumnDefinitions[1].Width = isTwo ? GridLength.Star : new GridLength(0);
            RootGrid.ColumnSpacing = isTwo ? 12 : 0;
        }

        if (isTwo)
        {
            CamRight.CameraId = 1;          // ✅ sadece bu modda
            CamRight.Mirror = cam.MirrorCam2;
        }
        else
        {
            CamRight.Mirror = false;
            CamRight.CameraId = -1;         // ✅ 1 hiç kullanılmasın
            RightPh.IsVisible = true;
        }
    }
}
