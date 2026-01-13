using System;
using Microsoft.Maui.Controls;

using BalentineV2.Core.Settings;

namespace BalentineV2.UI.Views.Pages.SettingsTabs;

public partial class CameraTab : ContentView
{
    private IAppSettingsStore? _store;
    private CameraSettings _camera = new();

    private bool _wired;
    private bool _isInit;

    public CameraTab()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_store is null)
        {
            // ✅ 1) Önce bu view’in handler’ından
            var sp = this.Handler?.MauiContext?.Services;

            // ✅ 2) Fallback: App handler üzerinden (SettingsTabs’ta en stabil yöntem)
            sp ??= Application.Current?.Handler?.MauiContext?.Services;

            _store = sp?.GetService(typeof(IAppSettingsStore)) as IAppSettingsStore;
        }

        if (_store is null) return;

        _isInit = true;

        // ✅ load + ui bas
        var app = _store.Load();
        _camera = app.Camera;

        // Picker default seçimi garanti
        CameraCountPicker.SelectedItem = (_camera.CameraCount >= 2) ? "2" : "1";

        MirrorCam1Switch.IsToggled = _camera.MirrorCam1;
        MirrorCam2Switch.IsToggled = _camera.MirrorCam2;

        ApplyCameraCountUi(_camera.CameraCount);

        _isInit = false;

        if (_wired) return;
        _wired = true;

        MirrorCam1Switch.Toggled += (_, ev) =>
        {
            if (_isInit) return;
            Save(_camera with { MirrorCam1 = ev.Value });
        };

        MirrorCam2Switch.Toggled += (_, ev) =>
        {
            if (_isInit) return;
            Save(_camera with { MirrorCam2 = ev.Value });
        };
    }

    private void OnCameraCountChanged(object sender, EventArgs e)
    {
        if (_isInit) return;

        var count = 1;

        if (CameraCountPicker.SelectedItem is string s && int.TryParse(s, out var parsed))
            count = parsed;
        else if (CameraCountPicker.SelectedIndex == 1)
            count = 2;

        ApplyCameraCountUi(count);
        Save(_camera with { CameraCount = count });
    }

    private void ApplyCameraCountUi(int count)
    {
        Camera2Box.IsVisible = (count == 2);

        if (count == 1 && MirrorCam2Switch.IsToggled)
        {
            _isInit = true;
            MirrorCam2Switch.IsToggled = false;
            _isInit = false;
        }
    }

    private void Save(CameraSettings next)
    {
        _camera = next;

        if (_camera.CameraCount <= 1 && _camera.MirrorCam2)
            _camera = _camera with { MirrorCam2 = false };

        _store?.SaveCamera(_camera);
    }
}
