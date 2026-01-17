using System.Diagnostics;
using BalentineV2.UI.Navigation;

namespace BalentineV2.UI.Views.Pages;

public partial class HomeView : ContentPage, IActivatablePage
{
    private const string VIDEO = "home_bg.mp4";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private CancellationTokenSource? _cts;

    private string? _cachedPath; // ✅ bir kere kopyala, reuse

    public HomeView()
    {
        InitializeComponent();
    }

    public async Task OnActivatedAsync()
    {
        Debug.WriteLine("[HomeView] ACTIVATED");

        // ✅ Önceki aktivasyon varsa iptal
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        await _gate.WaitAsync(ct);
        try
        {
            // ✅ 1) Cache kopyalama / dosya yolu hazırlama (UI thread DIŞINDA)
            _cachedPath ??= await EnsureCachedAsync(VIDEO);

            // ✅ 2) Render hazır olana kadar kısa bekleme (çok uzun değil)
            await WaitReadyAsync(ct, timeoutMs: 1200);

            if (ct.IsCancellationRequested) return;

            // ✅ 3) UI thread’de TEK sefer source set + play
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (ct.IsCancellationRequested) return;

                BgVideo.ShouldLoopPlayback = false;

                // Yumuşak reset
                try { BgVideo.Stop(); } catch { }

                // Aynı source tekrar set edilecekse null’lamak gerekli
                BgVideo.Source = null;

                // Bu delay çok küçük; donmayı büyütmesin diye kısalttık
                await Task.Delay(50);

                BgVideo.Source = _cachedPath;

                // ✅ Tek play
                await Task.Delay(80);
                try { BgVideo.Play(); } catch { }
            });
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HomeView] OnActivatedAsync ERROR: {ex}");
        }
        finally
        {
            if (_gate.CurrentCount == 0) _gate.Release();
        }
    }

    public void OnDeactivated()
    {
        Debug.WriteLine("[HomeView] DEACTIVATED");

        // ✅ Aktivasyonu iptal et
        _cts?.Cancel();

        // ✅ Video durdur
        try { BgVideo.Stop(); } catch { }
        // Source null yapmıyoruz; tekrar girişte daha stabil
    }

    private async Task WaitReadyAsync(CancellationToken ct, int timeoutMs)
    {
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < timeoutMs && !ct.IsCancellationRequested)
        {
            var ok = await MainThread.InvokeOnMainThreadAsync(() =>
                BgVideo.Handler is not null &&
                BgVideo.Width > 0 &&
                BgVideo.Height > 0 &&
                BgVideo.IsVisible);

            if (ok) return;

            await Task.Delay(60, ct);
        }
    }

    private static async Task<string> EnsureCachedAsync(string fileName)
    {
        var target = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        if (!File.Exists(target))
        {
            // ✅ Büyük dosyada donma olmasın diye: kopyalama zaten async stream
            using var s = await FileSystem.Current.OpenAppPackageFileAsync(fileName);
            using var fs = File.Create(target);
            await s.CopyToAsync(fs);
        }

        return target;
    }
}
