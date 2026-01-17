namespace BalentineV2.UI.Views.Pages.Modules.Lamp;

public static class LampCatalog
{
    public static readonly LampDef[] Items =
    [
        new("tepe_lambasi",    "tepe_lambasi.svg",    "tepe_lambasi_giris.mp4",    "tepe_lambasi_cikis.mp4"),
        new("pick_up_kayar",   "pick_up_kayar.svg",   "pick_up_kayar_giris.mp4",   "pick_up_kayar_cikis.mp4"),
        new("ic_lamba",        "ic_lamba.svg",        "ic_lamba_giris.mp4",        "ic_lamba_cikis.mp4"),
        new("on_lamba",        "on_lamba.svg",        "on_lamba_giris.mp4",        "on_lamba_cikis.mp4"),
        new("ust_kapak",       "ust_kapak.svg",       "ust_kapak_giris.mp4",       "ust_kapak_cikis.mp4"),
        new("arka_lamba",      "arka_lamba.svg",      "arka_lamba_giris.mp4",      "arka_lamba_cikis.mp4"),
        new("pick_up_ic_isik", "pick_up_ic_isik.svg", "pick_up_ic_isik_giris.mp4", "pick_up_ic_isik_cikis.mp4"),
    ];
}

public sealed record LampDef(string Key, string IconSvg, string InMp4, string OutMp4);
