using System;

namespace BalentineV2.Core.Settings;

public interface IAppSettingsStore
{
    AppSettings Current { get; }

    event EventHandler<AppSettings>? Changed;

    AppSettings Load();
    void Save(AppSettings settings);

    // küçük kolaylık: sadece Camera güncelle
    void SaveCamera(CameraSettings camera);
}
