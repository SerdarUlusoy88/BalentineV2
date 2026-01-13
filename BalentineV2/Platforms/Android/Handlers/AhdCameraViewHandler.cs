#if ANDROID
// File: Platforms/Android/Handlers/AhdCameraViewHandler.cs
using System;
using System.Linq;
using global::Android.Graphics;
using global::Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using BalentineV2.UI.Controls;
using Camera1 = global::Android.Hardware.Camera;

namespace BalentineV2.Platforms.Android.Handlers;

public class AhdCameraViewHandler : ViewHandler<AhdCameraView, TextureView>
{
    public static readonly IPropertyMapper<AhdCameraView, AhdCameraViewHandler> PropertyMapper
        = new PropertyMapper<AhdCameraView, AhdCameraViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(AhdCameraView.CameraId)] = MapCameraId,
            [nameof(AhdCameraView.Mirror)] = MapMirror,
            [nameof(AhdCameraView.CameraResolution)] = MapResolution,
            [nameof(AhdCameraView.IsActive)] = MapIsActive, // ✅
        };

    public static readonly CommandMapper<AhdCameraView, AhdCameraViewHandler> CommandMapper
        = new(ViewHandler.ViewCommandMapper);

    public AhdCameraViewHandler() : base(PropertyMapper, CommandMapper) { }

    private Camera1? _camera;
    private bool _isPreviewing;
    private int _cameraId;

    private SurfaceListener? _listener;
    private SurfaceTexture? _surface; // ✅ surface sakla

    protected override TextureView CreatePlatformView()
    {
        var tv = new TextureView(Context);
        _listener = new SurfaceListener(this);
        tv.SurfaceTextureListener = _listener;
        return tv;
    }

    protected override void ConnectHandler(TextureView platformView)
    {
        base.ConnectHandler(platformView);

        _cameraId = VirtualView.CameraId;

        // Surface zaten hazırsa sakla
        if (platformView.IsAvailable && platformView.SurfaceTexture != null)
            _surface = platformView.SurfaceTexture;

        // ✅ aktifse dene
        EnsureStarted();
    }

    protected override void DisconnectHandler(TextureView platformView)
    {
        StopPreview();
        if (platformView != null) platformView.SurfaceTextureListener = null;
        _listener = null;
        _surface = null;
        base.DisconnectHandler(platformView);
    }

    private void EnsureStarted()
    {
        if (VirtualView == null) return;
        if (!VirtualView.IsActive) { StopPreview(); return; }

        if (_isPreviewing) return;

        var s = _surface ?? PlatformView?.SurfaceTexture;
        if (PlatformView?.IsAvailable == true && s != null)
            OpenAndStart(s);
    }

    private void OpenAndStart(SurfaceTexture surface)
    {
        try
        {
            // aktif değilse başlamasın
            if (!VirtualView.IsActive) return;

            _camera = Camera1.Open(_cameraId);
            if (_camera is null) throw new Exception("Camera.Open returned null");

            ApplyParameters(_camera);
            _camera.SetPreviewTexture(surface);
            _camera.StartPreview();
            _isPreviewing = true;

            ApplyMirror();
            VirtualView.OnStarted();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AHD] start failed CameraId={_cameraId} Active={VirtualView.IsActive} Ex={ex}");
            StopPreview();
        }
    }

    private void ApplyParameters(Camera1 cam)
    {
        var p = cam.GetParameters();

        int targetH = VirtualView.CameraResolution <= 480 ? 480 :
                      VirtualView.CameraResolution <= 720 ? 720 : 1080;

        var sizes = p.SupportedPreviewSizes;
        if (sizes != null && sizes.Any())
        {
            var best = sizes.OrderBy(s => Math.Abs(s.Height - targetH)).First();
            p.SetPreviewSize(best.Width, best.Height);
        }

        var fpsRanges = p.SupportedPreviewFpsRange;
        if (fpsRanges != null && fpsRanges.Count > 0)
        {
            var mid = fpsRanges[0];
            p.SetPreviewFpsRange(mid[0], mid[1]);
        }

        var focusModes = p.SupportedFocusModes;
        if (focusModes != null && focusModes.Contains(Camera1.Parameters.FocusModeContinuousVideo))
            p.FocusMode = Camera1.Parameters.FocusModeContinuousVideo;

        cam.SetParameters(p);
        cam.SetDisplayOrientation(0);
    }

    private void ApplyMirror()
    {
        if (PlatformView == null) return;

        if (!VirtualView.Mirror)
        {
            PlatformView.SetTransform(null);
            return;
        }

        // Width/Height 0 olabiliyor -> yine de transform uygula, size gelince MapMirror tekrar çağrılır
        var w = PlatformView.Width <= 0 ? 1 : PlatformView.Width;
        var h = PlatformView.Height <= 0 ? 1 : PlatformView.Height;

        var m = new Matrix();
        m.SetScale(-1, 1, w / 2f, h / 2f);
        PlatformView.SetTransform(m);
    }

    private void StopPreview()
    {
        try
        {
            if (_isPreviewing)
            {
                _camera?.StopPreview();
                _isPreviewing = false;
                VirtualView?.OnStopped();
            }
        }
        catch { }
        finally
        {
            try { _camera?.Release(); } catch { }
            _camera = null;
        }
    }

    private void Restart()
    {
        StopPreview();
        EnsureStarted();
    }

    private sealed class SurfaceListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private readonly AhdCameraViewHandler _owner;
        public SurfaceListener(AhdCameraViewHandler owner) => _owner = owner;

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _owner._surface = surface;
            _owner.EnsureStarted();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _owner._surface = null;
            _owner.StopPreview();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
            => _owner.ApplyMirror();

        public void OnSurfaceTextureUpdated(SurfaceTexture surface) { }
    }

    // Mapper callbacks
    static void MapCameraId(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah._cameraId = v.CameraId;
        ah.Restart();
    }

    static void MapMirror(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah.ApplyMirror();
    }

    static void MapResolution(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah.Restart();
    }

    static void MapIsActive(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah.EnsureStarted();
    }
}
#endif
