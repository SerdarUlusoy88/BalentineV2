using System.Collections.ObjectModel;
using BalentineV2.Core.Localization;
using BalentineV2.Core.StatusBar;
using BalentineV2.UI.Localization;

namespace BalentineV2.UI.Components.Bars;

public partial class TopStatusBarView : ContentView
{
    private ILocalizationService? _locService;
    private Loc? _loc;
    private IStatusBarProvider? _provider;

    private readonly ObservableCollection<ChipVm> _chips = new();
    private readonly List<NoticeVm> _notices = new();
    private int _noticeIndex = -1;

    private bool _isInitialized;
    private bool _timersStarted;

    public static readonly BindableProperty CurrentNoticeTextProperty =
        BindableProperty.Create(nameof(CurrentNoticeText), typeof(string), typeof(TopStatusBarView), string.Empty);

    public static readonly BindableProperty CurrentNoticeIconProperty =
        BindableProperty.Create(nameof(CurrentNoticeIcon), typeof(string), typeof(TopStatusBarView), string.Empty);

    public string CurrentNoticeText
    {
        get => (string)GetValue(CurrentNoticeTextProperty);
        set => SetValue(CurrentNoticeTextProperty, value);
    }

    public string CurrentNoticeIcon
    {
        get => (string)GetValue(CurrentNoticeIconProperty);
        set => SetValue(CurrentNoticeIconProperty, value);
    }

    public TopStatusBarView()
    {
        InitializeComponent();
        ChipList.ItemsSource = _chips;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_isInitialized) return;
        _isInitialized = true;

        var sp = Handler?.MauiContext?.Services;
        if (sp == null) return;

        _locService = sp.GetService<ILocalizationService>();
        _loc = sp.GetService<Loc>();
        _provider = sp.GetService<IStatusBarProvider>();

        HookLanguageDropdown();
        HookLocalizationChanges();

        // ✅ Provider event’ine bağlan (chip listesi feature'a göre değişecek)
        HookProviderChanges();

        BuildChips();
        BuildNotices();

        HookIconRailAndPositionNotice();

        StartRefreshTimers();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        // ✅ leak önle
        if (_provider is not null)
            _provider.Changed -= OnProviderChanged;
    }

    private void HookProviderChanges()
    {
        if (_provider is null) return;

        // güvenli olsun diye önce sök
        _provider.Changed -= OnProviderChanged;
        _provider.Changed += OnProviderChanged;
    }

    private void OnProviderChanged(object? sender, EventArgs e)
    {
        // ✅ Chip listesi değişti -> yeniden kur
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BuildChips();
        });
    }

    // -----------------------------
    // Language dropdown
    // -----------------------------
    private void HookLanguageDropdown()
    {
        LangSelector.Clicked += (_, __) =>
        {
            LangDropPanel.IsVisible = !LangDropPanel.IsVisible;
        };
    }

    private void CloseLang() => LangDropPanel.IsVisible = false;

    private void OnPickTR(object? sender, EventArgs e)
    {
        _locService?.SetCulture("tr-TR");
        CloseLang();
        RebuildLocalizedNotices();
    }

    private void OnPickEN(object? sender, EventArgs e)
    {
        _locService?.SetCulture("en-US");
        CloseLang();
        RebuildLocalizedNotices();
    }

    private void HookLocalizationChanges()
    {
        if (_locService == null) return;

        _locService.CultureChanged += (_, __) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RebuildLocalizedNotices();
            });
        };
    }

    // -----------------------------
    // Chips
    // -----------------------------
    private void BuildChips()
    {
        _chips.Clear();

        if (_provider == null) return;

        foreach (var c in _provider.GetChips())
            _chips.Add(new ChipVm(c));
    }

    private void RefreshChips()
    {
        foreach (var c in _chips) c.Refresh();
    }

    // -----------------------------
    // Notices
    // -----------------------------
    private void BuildNotices()
    {
        _notices.Clear();

        if (_loc == null)
        {
            _notices.Add(new NoticeVm("status_ok.svg", "Balya bağlandı"));
            _notices.Add(new NoticeVm("status_pressure.svg", "IP KOPTU"));
        }
        else
        {
            _notices.Add(new NoticeVm("status_ok.svg", _loc["Notice_BaleTied"]));
            _notices.Add(new NoticeVm("status_pressure.svg", _loc["Notice_IpLost"]));
        }

        RotateNotice();
    }

    private void RebuildLocalizedNotices()
    {
        if (_loc == null) return;
        if (_notices.Count == 0) return;

        for (int i = 0; i < _notices.Count; i++)
        {
            var icon = _notices[i].IconSvg;

            var text = icon.Contains("pressure", StringComparison.OrdinalIgnoreCase)
                ? _loc["Notice_IpLost"]
                : _loc["Notice_BaleTied"];

            _notices[i] = new NoticeVm(icon, text);
        }

        RotateNotice();
    }

    private void RotateNotice()
    {
        if (_notices.Count == 0)
        {
            CurrentNoticeIcon = string.Empty;
            CurrentNoticeText = string.Empty;
            return;
        }

        _noticeIndex = (_noticeIndex + 1) % _notices.Count;
        CurrentNoticeIcon = _notices[_noticeIndex].IconSvg;
        CurrentNoticeText = _notices[_noticeIndex].Text;
    }

    // -----------------------------
    // Timers
    // -----------------------------
    private void StartRefreshTimers()
    {
        if (_timersStarted) return;
        _timersStarted = true;

        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            RefreshChips();
            return true;
        });

        Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            RotateNotice();
            return true;
        });
    }

    // -----------------------------
    // IconRail width -> Notice offset
    // -----------------------------
    private void HookIconRailAndPositionNotice()
    {
        var iconRail = FindIconRailByName("IconRail");
        if (iconRail == null)
        {
            NoticeHost.TranslationX = 0;
            return;
        }

        void apply()
        {
            var w = iconRail.Width;
            if (w < 1) w = iconRail.WidthRequest;
            if (w < 1) w = 0;

            NoticeHost.TranslationX = w + 8;
        }

        iconRail.SizeChanged += (_, __) => apply();
        apply();
    }

    private VisualElement? FindIconRailByName(string name)
    {
        Element? cur = this;
        while (cur != null)
        {
            var found = (cur as Element)?.FindByName<VisualElement>(name);
            if (found != null) return found;
            cur = cur.Parent;
        }
        return null;
    }

    // -----------------------------
    // VMs
    // -----------------------------
    private sealed class ChipVm : BindableObject
    {
        private readonly StatusBarChipModel _model;
        private string _value = string.Empty;

        public string IconSvg => _model.IconSvg;
        public string UnitText => _model.UnitText;

        public string DisplayText =>
            string.IsNullOrWhiteSpace(UnitText) ? _value : $"{_value} {UnitText}";

        public ChipVm(StatusBarChipModel model)
        {
            _model = model;
            Refresh();
        }

        public void Refresh()
        {
            _value = _model.ValueProvider?.Invoke() ?? string.Empty;
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    private sealed record NoticeVm(string IconSvg, string Text);
}
