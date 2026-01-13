namespace BalentineV2.Core.Settings;

public sealed record CameraSettings
{
    public int CameraCount { get; init; } = 1;
    public bool MirrorCam1 { get; init; } = false;
    public bool MirrorCam2 { get; init; } = false;
}
