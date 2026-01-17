using System.Collections.ObjectModel;
using System.Diagnostics;
using BalentineV2.UI.Navigation;
using BalentineV2.UI.Views.Pages.Modules.Lamp;
using CommunityToolkit.Maui.Views;

namespace BalentineV2.UI.Views.Pages.Modules;

public partial class LampView : ContentPage, IActivatablePage
{
    public ObservableCollection<LampItem> Items { get; } = new();

    private readonly SemaphoreSlim _gate = new(1, 1);
    private CancellationTokenSource? _cts;

    // mp4 cache (Raw -> CacheDirectory)
    private readonly Dictionary<string, string> _cached = new(StringComparer.OrdinalIgnoreCase);

    private bool _active;

    public LampView()
    {
        InitializeComponent();
        BindingContext = this;

        foreach (var d in LampCatalog.Items)
            Items.Add(new LampItem(d.Key, d.IconSvg, d.InMp4, d.OutMp4));
    }

    public Task OnActivatedAsync()
    {
        _active = true;
        Debug.WriteLine("[LampView] ACTIVATED");
        return Task.CompletedTask;
    }

    public void OnDeactivated()
    {
        _active = false;
        Debug.WriteLine("[LampView] DEACTIVATED");

        _cts?.Cancel();

        try { AnimVideo.Stop(); } catch { }
        try { AnimVideo.Source = null; } catch { }

        // sayfadan çıkarken görünür kalsın (sonraki girişte 1'e çekiyoruz)
        try { AnimVideo.Opacity = 1; } catch { }
    }

    private async void OnLampTapped(object sender, TappedEventArgs e)
    {
        if (!_active) return;
        if (e?.Parameter is not string key) return;

        var item = Items.FirstOrDefault(x => x.Key == key);
        if (item is null) return;

        await ToggleAsync(item);
    }

    private async Task ToggleAsync(LampItem item)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        await _gate.WaitAsync(ct);
        try
        {
            // off->on: giriş / on->off: çıkış
            var nextOn = !item.IsOn;
            var file = nextOn ? item.InMp4 : item.OutMp4;

            Debug.WriteLine($"[LampView] {item.Key} => {(nextOn ? "IN" : "OUT")} : {file}");

            var path = await EnsureCachedAsync(file);
            if (ct.IsCancellationRequested) return;

            await WaitReadyAsync(ct, timeoutMs: 1200);
            if (ct.IsCancellationRequested) return;

            // Event tabanlı: Opened + Ended
            var openedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var endedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnOpened(object? s, EventArgs e)
            {
                AnimVideo.MediaOpened -= OnOpened;
                openedTcs.TrySetResult(true);
            }

            void OnEnded(object? s, EventArgs e)
            {
                AnimVideo.MediaEnded -= OnEnded;
                endedTcs.TrySetResult(true);
            }

            AnimVideo.MediaOpened += OnOpened;
            AnimVideo.MediaEnded += OnEnded;

            // ✅ reset + source set (UI thread) + kararma maskeleme
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (ct.IsCancellationRequested) return;

                try { AnimVideo.Stop(); } catch { }

                // 🔥 Siyah frame’i maskele: kısa fade-out
                await AnimVideo.FadeTo(0, 5, Easing.CubicOut);

                // 2. tık bug'ını kıran reset
                AnimVideo.Source = null;
                await Task.Delay(25);

                if (ct.IsCancellationRequested) return;

                AnimVideo.Source = path;

                // Burada Play yok; önce MediaOpened bekliyoruz
            });

            // ✅ Açılmayı bekle
            var opened = await Task.WhenAny(openedTcs.Task, Task.Delay(1500, ct));
            if (ct.IsCancellationRequested) return;

            if (opened != openedTcs.Task)
            {
                Debug.WriteLine("[LampView] MediaOpened timeout");
                // geri görünür yapalım
                await MainThread.InvokeOnMainThreadAsync(() => AnimVideo.FadeTo(1, 80, Easing.CubicOut));
                return;
            }

            // ✅ Açıldı → önce görünür yap, sonra Play
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (ct.IsCancellationRequested) return;

                await AnimVideo.FadeTo(1, 15, Easing.CubicOut);

                try { AnimVideo.Play(); } catch { }
            });

            // ✅ Bitişi bekle (tek sefer)
            var completed = await Task.WhenAny(endedTcs.Task, Task.Delay(4000, ct));
            if (ct.IsCancellationRequested) return;

            if (completed != endedTcs.Task)
            {
                Debug.WriteLine("[LampView] MediaEnded timeout");
                return;
            }

            // ✅ State commit
            item.IsOn = nextOn;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LampView] ToggleAsync ERROR: {ex}");
        }
        finally
        {
            if (_gate.CurrentCount == 0) _gate.Release();
        }
    }

    private async Task<string> EnsureCachedAsync(string fileName)
    {
        if (_cached.TryGetValue(fileName, out var existing) && File.Exists(existing))
            return existing;

        var target = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        if (!File.Exists(target))
        {
            using var s = await FileSystem.Current.OpenAppPackageFileAsync(fileName);
            using var fs = File.Create(target);
            await s.CopyToAsync(fs);
        }

        _cached[fileName] = target;
        return target;
    }

    private async Task WaitReadyAsync(CancellationToken ct, int timeoutMs)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs && !ct.IsCancellationRequested)
        {
            var ok = await MainThread.InvokeOnMainThreadAsync(() =>
                AnimVideo.Handler is not null &&
                AnimVideo.Width > 0 &&
                AnimVideo.Height > 0 &&
                AnimVideo.IsVisible);

            if (ok) return;

            await Task.Delay(60, ct);
        }
    }
}
