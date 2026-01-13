namespace BalentineV2.Core.StatusBar;

public sealed class StatusBarChipModel
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string IconSvg { get; set; } = "";
    public string UnitText { get; set; } = "";
    public bool IsVisible { get; set; } = true;
    public Func<string>? ValueProvider { get; set; }

    // ✅ yeni: chip hangi feature'a bağlı (null = her zaman)
    public string? FeatureKey { get; set; }
}
