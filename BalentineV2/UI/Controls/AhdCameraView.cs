// File: UI/Controls/AhdCameraView.cs
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace BalentineV2.UI.Controls;

public class AhdCameraView : View
{
    public static readonly BindableProperty CameraIdProperty =
        BindableProperty.Create(
            nameof(CameraId),
            typeof(int),
            typeof(AhdCameraView),
            0,
            propertyChanged: OnAnyBindableChanged);

    public int CameraId
    {
        get => (int)GetValue(CameraIdProperty);
        set => SetValue(CameraIdProperty, value);
    }

    public static readonly BindableProperty CameraResolutionProperty =
        BindableProperty.Create(
            nameof(CameraResolution),
            typeof(int),
            typeof(AhdCameraView),
            720,
            propertyChanged: OnAnyBindableChanged);

    public int CameraResolution
    {
        get => (int)GetValue(CameraResolutionProperty);
        set => SetValue(CameraResolutionProperty, value);
    }

    public static readonly BindableProperty MirrorProperty =
        BindableProperty.Create(
            nameof(Mirror),
            typeof(bool),
            typeof(AhdCameraView),
            false,
            propertyChanged: OnAnyBindableChanged);

    public bool Mirror
    {
        get => (bool)GetValue(MirrorProperty);
        set => SetValue(MirrorProperty, value);
    }

    public event Action? Started;
    public event Action? Stopped;

    internal void OnStarted() => Started?.Invoke();
    internal void OnStopped() => Stopped?.Invoke();

    private static void OnAnyBindableChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // ✅ Handler'a "bu property değişti" bilgisini it: native taraf map'liyse anında uygulanır
        if (bindable is AhdCameraView v && v.Handler is IViewHandler h)
        {
            // Hangi property değiştiyse UpdateValue onu çağırmak ideal ama tek callback'te
            // hepsini güvenli güncelliyoruz:
            h.UpdateValue(nameof(CameraId));
            h.UpdateValue(nameof(CameraResolution));
            h.UpdateValue(nameof(Mirror));
        }
    }
}
