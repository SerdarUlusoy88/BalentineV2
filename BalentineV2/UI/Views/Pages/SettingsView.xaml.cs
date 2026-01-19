using System;
using System.Threading;
using Microsoft.Maui.Controls;

using BalentineV2.UI.Views.Pages.SettingsTabs;

namespace BalentineV2.UI.Views.Pages;

public partial class SettingsView : ContentPage
{
    private enum TabKey { Baglanti, Menu, Kamera, Basinc, Kayitlar, Makine, Veri, Tarti }

    private TabKey _active = TabKey.Baglanti;

    // ✅ Lazy cache (tablar 1 kez yaratılır)
    private ConnectionTab? _connectionTab;
    private MenuSelectionTab? _menuSelectionTab;
    private CameraTab? _cameraTab;
    private PressureCalibrationTab? _pressureTab;
    private LogsTab? _logsTab;
    private MachineInfoTab? _machineTab;
    private DataResetTab? _dataResetTab;
    private ScaleCalibrationTab? _scaleTab;

    private ConnectionTab Connection => _connectionTab ??= new ConnectionTab();
    private MenuSelectionTab Menu => _menuSelectionTab ??= new MenuSelectionTab();
    private CameraTab Camera => _cameraTab ??= new CameraTab();
    private PressureCalibrationTab Pressure => _pressureTab ??= new PressureCalibrationTab();
    private LogsTab Logs => _logsTab ??= new LogsTab();
    private MachineInfoTab Machine => _machineTab ??= new MachineInfoTab();
    private DataResetTab DataReset => _dataResetTab ??= new DataResetTab();
    private ScaleCalibrationTab Scale => _scaleTab ??= new ScaleCalibrationTab();

    private bool _isSwapping;

    // ✅ Swap cancel (hizli tıklamada son seçime sadık kal)
    private CancellationTokenSource? _swapCts;

    // ✅ Prewarm 1 kez
    private bool _prewarmed;

    // ✅ Resource cache (lookup maliyetini düşürür)
    private Color _accent;
    private Color _normal;
    private Color _dim;

    public SettingsView()
    {
        InitializeComponent();

        CacheColors();

        // ✅ İlk açılışta UI çizilsin, sonra içerik bas
        Dispatcher.Dispatch(() =>
        {
            ApplyTabColors();
            TabContentHost.Content = Connection;
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // ✅ tabları sırayla ısıt (UI donmasın)
        if (_prewarmed) return;
        _prewarmed = true;

        // Hafiften ağıra doğru (senin projede en ağır genelde Camera/Machine/Pressure)
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), () => _menuSelectionTab ??= new MenuSelectionTab());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(320), () => _cameraTab ??= new CameraTab());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(520), () => _pressureTab ??= new PressureCalibrationTab());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(720), () => _machineTab ??= new MachineInfoTab());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(920), () => _logsTab ??= new LogsTab());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1120), () => _dataResetTab ??= new DataResetTab());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1320), () => _scaleTab ??= new ScaleCalibrationTab());
        // Connection genelde hafif ama Entry/keyboard init bazen jank yapar; en sona koymak daha stabil
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1520), () => _connectionTab ??= new ConnectionTab());
    }

    private void CacheColors()
    {
        // Global dictionary’ye aldığımız için Resources burada hep bulunacak
        _accent = (Color)Application.Current!.Resources["ST_Accent"];
        _normal = (Color)Application.Current!.Resources["ST_Text"];
        _dim = (Color)Application.Current!.Resources["ST_TextDim"];
    }

    private void OnTabBaglanti(object sender, EventArgs e) => SelectTab(TabKey.Baglanti);
    private void OnTabMenu(object sender, EventArgs e) => SelectTab(TabKey.Menu);
    private void OnTabKamera(object sender, EventArgs e) => SelectTab(TabKey.Kamera);
    private void OnTabBasinc(object sender, EventArgs e) => SelectTab(TabKey.Basinc);
    private void OnTabKayitlar(object sender, EventArgs e) => SelectTab(TabKey.Kayitlar);
    private void OnTabMakine(object sender, EventArgs e) => SelectTab(TabKey.Makine);
    private void OnTabVeri(object sender, EventArgs e) => SelectTab(TabKey.Veri);
    private void OnTabTarti(object sender, EventArgs e) => SelectTab(TabKey.Tarti);

    private void SelectTab(TabKey tab)
    {
        if (_active == tab) return;
        if (_isSwapping) return;

        _active = tab;
        ApplyTabColors();

        _isSwapping = true;

        _swapCts?.Cancel();
        _swapCts = new CancellationTokenSource();
        var ct = _swapCts.Token;

        TabContentHost.Content = BuildLoadingPlaceholder();

        // ✅ 1 tick sonraya bırak: UI thread nefes alsın
        Dispatcher.Dispatch(() =>
        {
            if (ct.IsCancellationRequested)
            {
                _isSwapping = false;
                return;
            }

            try
            {
                TabContentHost.Content = tab switch
                {
                    TabKey.Baglanti => Connection,
                    TabKey.Menu => Menu,
                    TabKey.Kamera => Camera,
                    TabKey.Basinc => Pressure,
                    TabKey.Kayitlar => Logs,
                    TabKey.Makine => Machine,
                    TabKey.Veri => DataReset,
                    TabKey.Tarti => Scale,
                    _ => Connection
                };
            }
            finally
            {
                _isSwapping = false;
            }
        });
    }

    private View BuildLoadingPlaceholder()
    {
        // ✅ super lightweight placeholder
        return new Grid
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Children =
            {
                new Label
                {
                    Text = "Yükleniyor...",
                    TextColor = _dim,
                    FontSize = 13,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }

    private void ApplyTabColors()
    {
        SetTab(T_Baglanti, _active == TabKey.Baglanti);
        SetTab(T_Menu, _active == TabKey.Menu);
        SetTab(T_Kamera, _active == TabKey.Kamera);
        SetTab(T_Basinc, _active == TabKey.Basinc);
        SetTab(T_Kayitlar, _active == TabKey.Kayitlar);
        SetTab(T_Makine, _active == TabKey.Makine);
        SetTab(T_Veri, _active == TabKey.Veri);
        SetTab(T_Tarti, _active == TabKey.Tarti);
    }

    private void SetTab(Label lbl, bool active)
    {
        lbl.TextColor = active ? _accent : _normal;
        lbl.Opacity = active ? 1.0 : 0.85;
    }
}
