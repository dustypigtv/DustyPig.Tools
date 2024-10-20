﻿namespace DustyPig.Tools;

public static class All
{
    public static Tool BifTool => new() { Exe = "biftool.exe", Name = "biftool" };

    public static Tool CCExtractor => new() { Exe = "ccextractorwin.exe", Name = "ccextractor" };

    public static Tool FFMpeg => new() { Exe = "ffmpeg.exe", Name = "ffmpeg" };

    public static Tool FFProbe => new() { Exe = "ffprobe.exe", Name = "ffmpeg" };

    public static Tool FFPlay => new() { Exe = "ffplay.exe", Name = "ffmpeg" };

    public static Tool HandBrakeCLI => new() { Exe = "handbrakecli.exe", Name = "handbrakecli" };

    public static Tool Rclone => new() { Exe = "rclone.exe", Name = "rclone" };

    public static Tool Tagger => new() { Exe = "Tagger.exe", Name = "Tagger" };

    public static Tool MPV => new() { Exe = "mpv.exe", Name = "mpv" };

    public static Tool FasterWhisperXXL => new() { Exe = "faster-whisper-xxl.exe", Name = "Faster-Whisper-XXL" };

    public static Tool MkvPropEdit => new() { Exe = "mkvpropedit.exe", Name = "mkvpropedit" };

    public static Tool FFmpegLibs => new() { Exe = "avcodec-58.dll", Name = "ffmpeg-libs" };
}
