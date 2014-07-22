namespace MediaToolkit.Options
{
    public enum Target
    {
        Default,
        VCD,
        SVCD,
        DVD,
        DV,
        DV50
    }

    public enum TargetStandard
    {
        Default,
        PAL,
        NTSC,
        FILM
    }

    public enum AudioSampleRate
    {
        Default,
        Hz22050,
        Hz44100,
        Hz48000
    }

    public enum VideoAspectRatio
    {
        Default,
        R3_2,
        R4_3,
        R5_3,
        R5_4,
        R16_9,
        R16_10,
        R17_9
    }

    public enum VideoSize
    {
        Default,
        _16Cif,
        _2K,
        _2Kflat,
        _2Kscope,
        _4Cif,
        _4K,
        _4Kflat,
        _4Kscope,
        Cga,
        Cif,
        Ega,
        Film,
        Fwqvga,
        Hd1080,
        Hd480,
        Hd720,
        Hqvga,
        Hsxga,
        Hvga,
        Nhd,
        Ntsc,
        Ntsc_Film,
        Pal,
        Qcif,
        Qhd,
        Qntsc,
        Qpal,
        Qqvga,
        Qsxga,
        Qvga,
        Qxga,
        Sntsc,
        Spal,
        Sqcif,
        Svga,
        Sxga,
        Uxga,
        Vga,
        Whsxga,
        Whuxga,
        Woxga,
        Wqsxga,
        Wquxga,
        Wqvga,
        Wsxga,
        Wuxga,
        Wvga,
        Wxga,
        Xga,
        Custom
    }
}