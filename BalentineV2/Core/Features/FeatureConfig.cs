namespace BalentineV2.Core.Features;



public sealed record FeatureConfig
{
    public bool Fan { get; init; } = true;
    public bool Humidity { get; init; } = true;
    public bool Camera { get; init; } = true;
    public bool Lamp { get; init; } = true;
    public bool Scale { get; init; } = true;
    public bool Hydraulic { get; init; } = true;
    public bool Lubrication { get; init; } = true;
    public bool Monitoring { get; init; } = true;
    public bool IsEnabled(string? featureKey)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
            return true;

        return featureKey switch
        {
            FeatureKeys.Fan => Fan,
            FeatureKeys.Humidity => Humidity,
            FeatureKeys.Camera => Camera,
            FeatureKeys.Lamp => Lamp,
            FeatureKeys.Scale => Scale,
            FeatureKeys.Hydraulic => Hydraulic,
            FeatureKeys.Lubrication => Lubrication,
            FeatureKeys.Monitoring => Monitoring,
            _ => false
        };
    }
}


public static class FeatureKeys
{
    public const string Fan = "fan";
    public const string Humidity = "humidity";
    public const string Camera = "camera";
    public const string Lamp = "lamp";
    public const string Scale = "scale";
    public const string Hydraulic = "hydraulic";
    public const string Lubrication = "lubrication";
    public const string Monitoring = "monitoring";
}
