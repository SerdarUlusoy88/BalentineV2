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

    // ✅ StartPreview sonrası bazı cihazlar transformu resetliyor → ilk frame’de bir kick
    private bool _pendingMirrorKick;

    // ✅ Aynı anda start çağrısı gelmesini engelle
    private volatile bool _isStarting;
    private int _startToken;

    // ✅ Preview size cache (aynı cameraId + resolution için tekrar arama yapma)
    private int _cachedForCameraId = int.MinValue;
    private int _cachedForRes = int.MinValue;
    private int _cachedW;
    private int _cachedH;

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

        // ✅ CameraId < 0 => kesin açma / açıksa kapat
        if (_cameraId < 0)
        {
            StopPreview();
            ApplyMirror(); // reset
            return;
        }

        // View hazırsa başlat
        if (platformView.IsAvailable && platformView.SurfaceTexture != null)
            StartAsync(platformView.SurfaceTexture);
    }

    protected override void DisconnectHandler(TextureView platformView)
    {
        // token artır → arka plandaki start sonuçları ignore edilsin
        unchecked { _startToken++; }

        StopPreview();

        if (platformView != null)
            platformView.SurfaceTextureListener = null;

        _listener = null;

        base.DisconnectHandler(platformView);
    }

    private void StartAsync(SurfaceTexture surface)
    {
        if (_cameraId < 0)
        {
            StopPreview();
            ApplyMirror();
            return;
        }

        // ✅ double-start engeli
        if (_isStarting) return;
        _isStarting = true;

        var token = ++_startToken;

        _ = Task.Run(() =>
        {
            try
            {
                // Eğer handler bu sırada dispose olduysa / token değiştiyse iptal
                if (token != _startToken) return;

                OpenAndStart(surface, token);
            }
            finally
            {
                _isStarting = false;
            }
        });
    }

    private void OpenAndStart(SurfaceTexture surface, int token)
    {
        // ✅ güvenlik: CameraId < 0 ise kesin kapalı kal
        if (_cameraId < 0 || token != _startToken)
        {
            StopPreview();
            ApplyMirror();
            return;
        }

        try
        {
            StopPreview();

            // Camera.Open bazen bloklar → background thread’deyiz
            var cam = Camera1.Open(_cameraId);
            if (cam is null)
                throw new Exception("Camera.Open returned null");

            // Token değiştiyse bu kamerayı hemen bırak
            if (token != _startToken)
            {
                try { cam.Release(); } catch { }
                return;
            }

            _camera = cam;

            ApplyParameters(cam);

            cam.SetPreviewTexture(surface);
            cam.StartPreview();
            _isPreviewing = true;

            _pendingMirrorKick = true;

            // ✅ Started event’i UI tarafında placeholder kapatır
            VirtualView.OnStarted();

            // Mirror UI thread’de
            ApplyMirror();
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

        // ✅ cache: aynı camId + resolution ise tekrar size arama yapma
        if (_cachedForCameraId == _cameraId && _cachedForRes == targetH && _cachedW > 0 && _cachedH > 0)
        {
            p.SetPreviewSize(_cachedW, _cachedH);
        }
        else
        {
            var sizes = p.SupportedPreviewSizes;
            if (sizes != null && sizes.Any())
            {
                var best = sizes.OrderBy(s => Math.Abs(s.Height - targetH)).First();
                p.SetPreviewSize(best.Width, best.Height);

                _cachedForCameraId = _cameraId;
                _cachedForRes = targetH;
                _cachedW = best.Width;
                _cachedH = best.Height;
            }
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
                PlatformView.SetTransform(null);
                PlatformView.RotationY = 0f;
                PlatformView.ScaleX = 1f;
                return;
            }

            // ✅ Sende çalışan garanti yöntem
            PlatformView.SetTransform(null);
            PlatformView.ScaleX = 1f;
            PlatformView.RotationY = 180f;
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
        catch
        {
            // ignore
        }
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
            StartAsync(PlatformView.SurfaceTexture);
    }

    private sealed class SurfaceListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private readonly AhdCameraViewHandler _owner;
        public SurfaceListener(AhdCameraViewHandler owner) => _owner = owner;

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            if (_owner._cameraId < 0) return;
            _owner.StartAsync(surface);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            // token artır → arka plan start sonuçları ignore
            unchecked { _owner._startToken++; }
            _owner.StopPreview();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
            => _owner.ApplyMirror();

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            // ✅ ilk frame geldikten sonra bir kere daha mirror uygula
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

        // ✅ -1 ise kesin kapat + reset
        if (ah._cameraId < 0)
        {
            unchecked { ah._startToken++; }
            ah.StopPreview();
            ah.ApplyMirror();
            return;
        }

        ah.Restart();
    }

    private static void MapMirror(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;
        ah.ApplyMirror();

        // Mirror değişince bazen frame resetlenir; bir sonraki frame’de de kick eder zaten
        ah._pendingMirrorKick = true;
    }

    private static void MapResolution(IElementHandler h, AhdCameraView v)
    {
        var ah = (AhdCameraViewHandler)h;

        // cache geçersiz (resolution değişti)
        ah._cachedForCameraId = int.MinValue;
        ah._cachedForRes = int.MinValue;
        ah._cachedW = 0;
        ah._cachedH = 0;

        ah.Restart();
    }
}
#endif
