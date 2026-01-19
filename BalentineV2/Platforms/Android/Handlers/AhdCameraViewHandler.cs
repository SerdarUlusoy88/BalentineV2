#if ANDROID
// File: Platforms/Android/Handlers/AhdCameraViewHandler.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using global::Android.Graphics;
using global::Android.Views;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

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
        };

    public static readonly CommandMapper<AhdCameraView, AhdCameraViewHandler> CommandMapper
        = new(ViewHandler.ViewCommandMapper);

    public AhdCameraViewHandler() : base(PropertyMapper, CommandMapper) { }

    private Camera1? _camera;
    private bool _isPreviewing;
    private int _cameraId;
    private SurfaceListener? _listener;

    // Mirror'ı ilk frame geldikten sonra bir kez daha uygula (TextureView resetleyebiliyor)
    private bool _pendingMirrorKick;

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

        if (_cameraId < 0)
        {
            StopPreview();
            ApplyMirror(); // reset
            return;
        }

        if (platformView.IsAvailable && platformView.SurfaceTexture != null)
            OpenAndStart(platformView.SurfaceTexture);
    }

    protected override void DisconnectHandler(TextureView platformView)
    {
        StopPreview();

        if (platformView != null)
            platformView.SurfaceTextureListener = null;

        _listener = null;

        base.DisconnectHandler(platformView);
    }

    private void OpenAndStart(SurfaceTexture surface)
    {
        if (_cameraId < 0)
        {
            StopPreview();
            ApplyMirror();
            return;
        }

        try
        {
            StopPreview();

            _camera = Camera1.Open(_cameraId);
            if (_camera is null)
                throw new Exception("Camera.Open returned null");

            ApplyParameters(_camera);

            _camera.SetPreviewTexture(surface);
            _camera.StartPreview();
            _isPreviewing = true;

            // ✅ preview başladıktan sonra mirror yeniden uygulanacak
            _pendingMirrorKick = true;

            ApplyMirror();
            VirtualView.OnStarted();

            // ✅ bazı cihazlarda StartPreview sonrası 1-2 tick gerekir
            ScheduleMirrorApply();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AHD] start failed CameraId={_cameraId} Ex={ex}");
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

        cam.SetParameters(p);
        cam.SetDisplayOrientation(0);
    }

    private void ApplyMirror()
    {
        if (PlatformView == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (PlatformView == null) return;

            if (!VirtualView.Mirror)
            {
                // reset
                PlatformView.SetTransform(null);
                PlatformView.RotationY = 0f;
                PlatformView.ScaleX = 1f;
                return;
            }

            // ✅ EN GARANTİ: GPU ile Y ekseninde çevir
            // (TextureView transform/ScaleX bazı cihazlarda preview'a etki etmiyor)
            PlatformView.SetTransform(null);
            PlatformView.ScaleX = 1f;
            PlatformView.RotationY = 180f;
        });
    }


    private void ScheduleMirrorApply()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                // iki kez dene (StartPreview sonrası reset olabiliyor)
                await Task.Delay(60);
                ApplyMirror();
                await Task.Delay(140);
                ApplyMirror();
            }
            catch { }
        });
    }

    private void StopPreview()
    {
        try
        {
            if (_isPreviewing)
            {
                _camera?.StopPreview();
                _isPreviewing = false;
                VirtualView.OnStopped();
            }
        }
        catch { }
        finally
        {
            try { _camera?.Release(); } catch { }
            _camera = null;
            _pendingMirrorKick = false;
        }
    }

    private void Restart()
    {
        if (_cameraId < 0)
        {
            StopPreview();
            ApplyMirror();
            return;
        }

        if (PlatformView?.IsAvailable == true && PlatformView.SurfaceTexture != null)
            OpenAndStart(PlatformView.SurfaceTexture);
    }

    private sealed class SurfaceListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private readonly AhdCameraViewHandler _owner;
        public SurfaceListener(AhdCameraViewHandler owner) => _owner = owner;

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            if (_owner._cameraId < 0) return;
            _owner.OpenAndStart(surface);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _owner.StopPreview();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
            => _owner.ApplyMirror();

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            // ✅ ilk frame geldikten sonra TextureView bazen transformu sıfırlar → bir kere kick
            if (_owner._pendingMirrorKick)
            {
                _owner._pendingMirrorKick = false;
                _owner.ApplyMirror();
            }
        }
    }

    private static void MapCameraId(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah._cameraId = v.CameraId;

        if (ah._cameraId < 0)
        {
            ah.StopPreview();
            ah.ApplyMirror();
            return;
        }

        ah.Restart();
    }

    private static void MapMirror(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;

        // ✅ Mirror değişince bazen tek apply yetmez
        ah.ApplyMirror();
        ah.ScheduleMirrorApply();
    }

    private static void MapResolution(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah.Restart();
    }
}
#endif
