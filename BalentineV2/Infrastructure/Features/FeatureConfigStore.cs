using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

using BalentineV2.Core.Features;

namespace BalentineV2.Infrastructure.Features;

public sealed class FeatureConfigStore : IFeatureConfigStore
{
    private const string PrefKey = "feature_config_v1";

    public event EventHandler<FeatureConfig>? ConfigChanged;

    public FeatureConfig Load()
    {
        try
        {
            var json = Preferences.Get(PrefKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new FeatureConfig();

            var cfg = JsonSerializer.Deserialize<FeatureConfig>(json);
            return cfg ?? new FeatureConfig();
        }
        catch
        {
            return new FeatureConfig();
        }
    }

    public void Save(FeatureConfig config)
    {
        // ✅ UI thread'i kilitleme: Preferences yazma işini arka plana at
        _ = Task.Run(() =>
        {
            try
            {
                var json = JsonSerializer.Serialize(config);
                Preferences.Set(PrefKey, json);
            }
            catch
            {
                // sessiz geç
            }

            // ✅ Event'i UI thread'de tetikle (UI subscriber'lar safe)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConfigChanged?.Invoke(this, config);
            });
        });
    }
}
