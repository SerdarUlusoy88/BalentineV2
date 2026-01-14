using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BalentineV2.UI.Views.Pages.Modules;

public partial class LampView : ContentPage
{
    // Tek aktif buton (maintained)
    private string? _activeKey;

    // Key -> Border map (aktif/pasif renklendirme)
    private readonly Dictionary<string, Border> _btns = new();

    // Key -> (giris, cikis) video dosyalarý
    private readonly Dictionary<string, (string giris, string cikis)> _videos = new()
    {
        ["tepe_lambasi"] = ("embed://tepe_lambasi_giris.mp4", "embed://tepe_lambasi_cikis.mp4"),
        ["pick_up_kayar"] = ("embed://pick_up_kayar_giris.mp4", "embed://pick_up_kayar_cikis.mp4"),
        ["ic_lamba"] = ("embed://ic_lamba_giris.mp4", "embed://ic_lamba_cikis.mp4"),
        ["on-lamba"] = ("embed://on-lamba-giris.mp4", "embed://on-lamba-cikis.mp4"),
        ["ust_kapak"] = ("embed://ust_kapak_giris.mp4", "embed://ust_kapak_cikis.mp4"),
        ["arka-lamba"] = ("embed://arka-lamba-giris.mp4", "embed://arka-lamba-cikis.mp4"),
        ["pick_up_ic_isik"] = ("embed://pick_up_ic_isik_giris.mp4", "embed://pick_up_ic_isik_cikis.mp4"),
    };

    public LampView()
    {
        InitializeComponent();

        // Border referanslarý (UI hazýr)
        _btns["tepe_lambasi"] = BtnTepeLambasi;
        _btns["pick_up_kayar"] = BtnPickUpKayar;
        _btns["ic_lamba"] = BtnIcLamba;
        _btns["on-lamba"] = BtnOnLamba;
        _btns["ust_kapak"] = BtnUstKapak;
        _btns["arka-lamba"] = BtnArkaLamba;
        _btns["pick_up_ic_isik"] = BtnPickUpIcIsik;

        // baþlangýç pasif görünüm
        foreach (var kv in _btns)
            SetActiveVisual(kv.Value, isActive: false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try { AnimVideo?.Stop(); } catch { }
        _activeKey = null;

        foreach (var kv in _btns)
            SetActiveVisual(kv.Value, isActive: false);
    }

    private async void OnButtonTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string key) return;
        if (!_videos.TryGetValue(key, out var pair)) return;

        // ayný butona tekrar basýldý -> kapat (cikis)
        if (string.Equals(_activeKey, key, StringComparison.OrdinalIgnoreCase))
        {
            _activeKey = null;

            SetActiveVisual(_btns[key], isActive: false);

            await PlayAnimationAsync(pair.cikis);
            return;
        }

        // baþka bir butona basýldý:
        // - önce aktif olaný pasife çek (tek animasyon kuralý: mevcut videoyu durdur)
        if (_activeKey is not null && _btns.TryGetValue(_activeKey, out var prevBtn))
        {
            SetActiveVisual(prevBtn, isActive: false);
        }

        _activeKey = key;

        // yeni aktif görünüm
        SetActiveVisual(_btns[key], isActive: true);

        // tek animasyon: mevcut oynuyorsa durdur, yeni giris oynat
        await PlayAnimationAsync(pair.giris);
    }

    private async Task PlayAnimationAsync(string source)
    {
        try
        {
            // tek animasyon: önce stop
            try { AnimVideo?.Stop(); } catch { }

            // source deðiþtir
            AnimVideo.Source = source;

            // autoplay bazý cihazlarda gecikmeli tetiklenebiliyor
            await Task.Delay(80);

            try { AnimVideo?.Play(); } catch { }
        }
        catch
        {
            // sessiz
        }
    }

    private static void SetActiveVisual(Border b, bool isActive)
    {
        if (isActive)
        {
            b.Stroke = Color.FromArgb("#F5A623");        // sarý
            b.BackgroundColor = Color.FromArgb("#2A2416"); // koyu sarýmsý
            b.StrokeThickness = 2;
        }
        else
        {
            b.Stroke = Color.FromArgb("#D7D7D7");
            b.BackgroundColor = Color.FromArgb("#1A1A1A");
            b.StrokeThickness = 1;
        }
    }
}
