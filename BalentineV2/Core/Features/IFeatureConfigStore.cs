using System;

namespace BalentineV2.Core.Features;

public interface IFeatureConfigStore
{
    FeatureConfig Load();
    void Save(FeatureConfig config);

    /// <summary>
    /// Kaydetme sonrası tetiklenir (Settings ekranında değişiklik anlık yansısın).
    /// </summary>
    event EventHandler<FeatureConfig>? ConfigChanged;
}
