// File: Core/Menu/MenuItemModel.cs
using Microsoft.Maui.Controls;

namespace BalentineV2.Core.Menu;

public sealed class MenuItemModel : BindableObject
{
    public string Id { get; init; } = "";

    private string _title = "";
    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            OnPropertyChanged();
        }
    }

    public string Route { get; init; } = "";

    public string? IconSvg { get; init; }          // "ico_fan.svg"
    public string IconGlyph { get; init; } = "";   // fallback

    public string? FeatureKey { get; init; }

    public bool IsEnabled { get; init; } = true;

    public int Order { get; init; } = 0;

    public bool HasSvgIcon => !string.IsNullOrWhiteSpace(IconSvg);
    public bool HasGlyphIcon => !string.IsNullOrWhiteSpace(IconGlyph);
}
