namespace BalentineV2.Core.StatusBar;

public interface IStatusBarConfigStore
{
    /// <summary> Chip açık mı? (varsayılan true) </summary>
    bool IsEnabled(string chipId);

    /// <summary> Chip aç/kapa ve kalıcı kaydet </summary>
    void SetEnabled(string chipId, bool enabled);

    event EventHandler? Changed;
}
