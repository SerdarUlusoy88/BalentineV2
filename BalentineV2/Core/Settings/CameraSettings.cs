// File: Core/Settings/CameraSettings.cs
namespace BalentineV2.Core.Settings;

public sealed record CameraSettings
{
    public int CameraCount { get; init; } = 1;
    public bool MirrorCam1 { get; init; } = false;
    public bool MirrorCam2 { get; init; } = false;

    public CameraSettings Normalize()
    {
        var count = CameraCount <= 1 ? 1 : 2;

        // count=1 iken MirrorCam2 kesin false
        var m2 = (count == 2) ? MirrorCam2 : false;

        if (count == CameraCount && m2 == MirrorCam2)
            return this;

        return this with
        {
            CameraCount = count,
            MirrorCam2 = m2
        };
    }
}
