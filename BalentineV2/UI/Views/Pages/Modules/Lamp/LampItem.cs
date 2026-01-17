using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BalentineV2.UI.Views.Pages.Modules.Lamp;

public sealed class LampItem : INotifyPropertyChanged
{
    public string Key { get; }
    public string IconSvg { get; }
    public string InMp4 { get; }
    public string OutMp4 { get; }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (_isOn == value) return;
            _isOn = value;
            OnPropertyChanged();
        }
    }

    public LampItem(string key, string iconSvg, string inMp4, string outMp4)
    {
        Key = key;
        IconSvg = iconSvg;
        InMp4 = inMp4;
        OutMp4 = outMp4;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
