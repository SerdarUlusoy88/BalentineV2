namespace BalentineV2.Core.Settings;

public sealed record AppSettings
{
    public CameraSettings Camera { get; init; } = new();

    public AppSettings Normalize()
        => this with { Camera = (Camera ?? new CameraSettings()).Normalize() };
}
