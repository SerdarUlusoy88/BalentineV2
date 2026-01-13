// File: UI/Controls/AhdCameraView.cs
using System;
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Controls;

public class AhdCameraView : View
{
    public static readonly BindableProperty CameraIdProperty =
        BindableProperty.Create(nameof(CameraId), typeof(int), typeof(AhdCameraView), 0);

    public int CameraId
    {
        get => (int)GetValue(CameraIdProperty);
        set => SetValue(CameraIdProperty, value);
    }

    public static readonly BindableProperty CameraResolutionProperty =
        BindableProperty.Create(nameof(CameraResolution), typeof(int), typeof(AhdCameraView), 720);

    public int CameraResolution
    {
        get => (int)GetValue(CameraResolutionProperty);
        set => SetValue(CameraResolutionProperty, value);
    }

    public static readonly BindableProperty MirrorProperty =
        BindableProperty.Create(nameof(Mirror), typeof(bool), typeof(AhdCameraView), false);

    public bool Mirror
    {
        get => (bool)GetValue(MirrorProperty);
        set => SetValue(MirrorProperty, value);
    }

    public event Action? Started;
    public event Action? Stopped;

    internal void OnStarted() => Started?.Invoke();
    internal void OnStopped() => Stopped?.Invoke();
}
