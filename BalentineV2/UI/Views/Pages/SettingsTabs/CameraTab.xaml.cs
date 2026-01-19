using System;
using System.Threading;
using Microsoft.Maui.Controls;

using BalentineV2.Core.Settings;

namespace BalentineV2.UI.Views.Pages.SettingsTabs;

public partial class CameraTab : ContentView
{
    private IAppSettingsStore? _store;
    private CameraSettings _camera = new();

    private bool _initialized;
    private bool _isInitUi;

    private CancellationTokenSource? _saveCts;

    public CameraTab()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null) return;
        if (_initialized) return;
        _initialized = true;

        Init();
    }

    private void Init()
    {
        // ✅ En stabil: bu control’un kendi ServiceProvider’ı
        _store = Handler?.MauiContext?.Services?.GetService(typeof(IAppSettingsStore)) as IAppSettingsStore;

        // Fallback (nadiren gerekir)
        _store ??= Application.Current?.Handler?.MauiContext?.Services?.GetService(typeof(IAppSettingsStore)) as IAppSettingsStore;

        if (_store is null) return;

        // ✅ Event wiring (1 kere)
        MirrorCam1Switch.Toggled += OnMirror1Toggled;
        MirrorCam2Switch.Toggled += OnMirror2Toggled;

        _isInitUi = true;

        var app = _store.Load();
        _camera = (app.Camera ?? new CameraSettings()).Normalize();

        ApplyToUi(_camera);

        _isInitUi = false;
    }

    private void ApplyToUi(CameraSettings cam)
    {
        cam = cam.Normalize();
        _camera = cam;

        CameraCountPicker.SelectedItem = (cam.CameraCount == 2) ? "2" : "1";
        MirrorCam1Switch.IsToggled = cam.MirrorCam1;
        MirrorCam2Switch.IsToggled = cam.MirrorCam2;

        ApplyCameraCountUi(cam.CameraCount);
    }

    private void ApplyCameraCountUi(int count)
    {
        count = (count < 1) ? 1 : (count > 2 ? 2 : count);
        var isTwo = (count == 2);

        Camera2Box.IsVisible = isTwo;
        MirrorCam2Switch.IsEnabled = isTwo;

        if (!isTwo)
        {
            if (MirrorCam2Switch.IsToggled)
            {
                _isInitUi = true;
                MirrorCam2Switch.IsToggled = false;
                _isInitUi = false;
            }

            _camera = _camera with { CameraCount = 1, MirrorCam2 = false };
        }
        else
        {
            _camera = _camera with { CameraCount = 2 };
        }
    }

    private void OnMirror1Toggled(object? sender, ToggledEventArgs ev)
    {
        if (_isInitUi) return;
        QueueSave((_camera with { MirrorCam1 = ev.Value }).Normalize());
    }

    private void OnMirror2Toggled(object? sender, ToggledEventArgs ev)
    {
        if (_isInitUi) return;

        if (_camera.CameraCount != 2 && ev.Value)
        {
            _isInitUi = true;
            MirrorCam2Switch.IsToggled = false;
            _isInitUi = false;
            return;
        }

        QueueSave((_camera with { MirrorCam2 = ev.Value }).Normalize());
    }

    private void OnCameraCountChanged(object sender, EventArgs e)
    {
        if (_isInitUi) return;

        var count = 1;

        if (CameraCountPicker.SelectedItem is string s && int.TryParse(s, out var parsed))
            count = parsed;
        else if (CameraCountPicker.SelectedIndex == 1)
            count = 2;

        ApplyCameraCountUi(count);

        // ✅ senin ekrandaki derleme hatasını da burada düzeltiyorum:
        QueueSave((_camera with { CameraCount = count }).Normalize());
    }

    private void QueueSave(CameraSettings next)
    {
        System.Diagnostics.Debug.WriteLine($"[CAMTAB] Save CameraCount={_camera.CameraCount} M1={_camera.MirrorCam1} M2={_camera.MirrorCam2}");

        next = (next ?? new CameraSettings()).Normalize();
        _camera = next;

        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        var ct = _saveCts.Token;

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
        {
            if (ct.IsCancellationRequested) return;
            _store?.SaveCamera(_camera);
        });
    }
}
