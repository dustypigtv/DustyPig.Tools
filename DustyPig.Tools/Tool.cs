using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace DustyPig.Tools
{
    public class Tool
    {
        private const string ROOT_URL = "https://dustypig.s3.us-central-1.wasabisys.com/bin/tools/";

        internal string Name { get; set; }
        internal string Exe { get; set; }

        private string ExeDir
        {
            get
            {
                string ret = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                ret = Path.Combine(ret, "DustyPig.tv", "tools", Name);
                return Directory.CreateDirectory(ret).FullName;
            }
        }

        public string ExePath => Path.Combine(ExeDir, Exe);

        public bool IsInstalled => File.Exists(ExePath);

        public void Install()
        {
            if (IsInstalled)
                return;

            string tmpFile = Path.GetTempFileName();
            TryDeleteFile(tmpFile);
            tmpFile += ".zip";
            try
            {
                using var wc = new WebClient();
                wc.DownloadFile(ROOT_URL + Name + ".zip", tmpFile);
                ZipFile.ExtractToDirectory(tmpFile, ExeDir);
            }
            finally
            {
                TryDeleteFile(tmpFile);
            }
        }

        public void UnInstall()
        {
            if (Directory.Exists(ExeDir))
                Directory.Delete(ExeDir, true);
        }

        private static void TryDeleteFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
            catch { }
        }

    }
}
