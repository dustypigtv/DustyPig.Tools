namespace DustyPig.Tools
{
    public static class All
    {
        public static Tool BifTool => new Tool { Exe = "biftool.exe", Name = "biftool" };

        public static Tool CCExtractor => new Tool { Exe = "ccextractorwin.exe", Name = "ccextractor" };

        public static Tool FFMpeg => new Tool { Exe = "ffmpeg.exe", Name = "ffmpeg" };

        public static Tool FFProbe => new Tool { Exe = "ffprobe.exe", Name = "ffmpeg" };

        public static Tool FFPlay => new Tool { Exe = "ffplay.exe", Name = "ffmpeg" };

        public static Tool HandBrakeCLI => new Tool { Exe = "handbrakecli.exe", Name = "handbrakecli" };

        public static Tool Rclone => new Tool { Exe = "rclone.exe", Name = "rclone" };

        public static Tool Tagger => new Tool {Exe = "Tagger.exe", Name = "Tagger"};
        
        public static Tool MPV => new Tool { Exe = "mpv.exe", Name = "mpv" };
    
    }
}
