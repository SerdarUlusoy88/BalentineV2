// File: UI/Views/Pages/SettingsView.xaml.cs
using System;
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

    public SettingsView()
    {
        InitializeComponent();

        // ✅ İlk açılışta UI çizilsin, sonra içerik bas (ilk geçiş kasmasını azaltır)
        Dispatcher.Dispatch(() =>
        {
            ApplyTabColors();
            TabContentHost.Content = Connection;
        });

        // İstersen (opsiyonel) Menü tabını hafif pre-warm:
        // Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(250), () => _menuSelectionTab ??= new MenuSelectionTab());
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
        if (_isSwapping) return; // ✅ hızlı tıklamada üst üste swap olmasın

        _active = tab;
        ApplyTabColors();

        _isSwapping = true;

        // ✅ Hemen ağır tabı basma: önce küçük bir placeholder göster
        TabContentHost.Content = BuildLoadingPlaceholder();

        // ✅ 1 tick sonraya bırak: UI thread nefes alsın
        Dispatcher.Dispatch(() =>
        {
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
        // ✅ Lightweight placeholder (layout hesaplaması çok ucuz)
        var textColor = (Color)Resources["ST_TextDim"];

        return new Grid
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Children =
            {
                new Label
                {
                    Text = "Yükleniyor...",
                    TextColor = textColor,
                    FontSize = 13,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }

    private void ApplyTabColors()
    {
        var accent = (Color)Resources["ST_Accent"];
        var normal = (Color)Resources["ST_Text"];

        SetTab(T_Baglanti, _active == TabKey.Baglanti, accent, normal);
        SetTab(T_Menu, _active == TabKey.Menu, accent, normal);
        SetTab(T_Kamera, _active == TabKey.Kamera, accent, normal);
        SetTab(T_Basinc, _active == TabKey.Basinc, accent, normal);
        SetTab(T_Kayitlar, _active == TabKey.Kayitlar, accent, normal);
        SetTab(T_Makine, _active == TabKey.Makine, accent, normal);
        SetTab(T_Veri, _active == TabKey.Veri, accent, normal);
        SetTab(T_Tarti, _active == TabKey.Tarti, accent, normal);
    }

    private static void SetTab(Label lbl, bool active, Color accent, Color normal)
    {
        lbl.TextColor = active ? accent : normal;
        lbl.Opacity = active ? 1.0 : 0.85;
    }
}
