using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using BalentineV2.Core.Menu;

namespace BalentineV2.UI.ViewModels.Navigation;

public sealed class MenuItemVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; }
    public string Title { get; }
    public string Route { get; }

    // Dosya adı: "fan.svg" / "camera.svg" gibi
    public string? IconFile { get; }

    public ImageSource? IconSource => string.IsNullOrWhiteSpace(IconFile)
        ? null
        : ImageSource.FromFile(IconFile);

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IconTint));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(SelectedBackground));
        }
    }

    // ✅ Default tint: açık gri (ikon “kaybolmasın”)
    public Color IconTint => IsSelected ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#AAB0B7");
    public Color TextColor => IsSelected ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#BFC5CC");
    public Color SelectedBackground => IsSelected ? Color.FromArgb("#E67E22") : Colors.Transparent;

    public MenuItemVm(MenuItemModel m)
    {
        Id = m.Id;
        Title = m.Title;
        Route = m.Route;
        IconFile = m.IconSvg; // MenuItemModel.IconSvg dolduracağız
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
