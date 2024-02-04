using DustyPig.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DustyPig.Tools
{
    public class Tool
    {
        private const string ROOT_URL = "https://s3.dustypig.tv/bin/tools/";

        public string Name { get; internal set; }
        internal string Exe { get; set; }

        private static string RootDir
        {
            get
            {
                string ret = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                ret = Path.Combine(ret, "DustyPig.tv");
                return Directory.CreateDirectory(ret).FullName;
            }
        }

        private static string VersionsDir
        {
            get
            {
                string ret = Path.Combine(RootDir, "tool.versions");
                return Directory.CreateDirectory(ret).FullName;
            }
        }

        public string ExeDir
        {
            get
            {
                string ret = Path.Combine(RootDir, "tools", Name);
                return Directory.CreateDirectory(ret).FullName;
            }
        }

        public string ExePath => Path.Combine(ExeDir, Exe);

        private string VersionPath => Path.Combine(VersionsDir, Name + ".ver");

        private string ServerVersionPath => ROOT_URL + Name + ".ver";

        private string ServerZipPath => ROOT_URL + Name + ".zip";



        public async Task InstallAsync()
        {
            Version localVersion = new();
            try { localVersion = Version.Parse(File.ReadAllText(VersionPath)); }
            catch { }


            Version serverVersion = Version.Parse(await SimpleDownloader.DownloadStringAsync(ServerVersionPath).ConfigureAwait(false));

            if (serverVersion > localVersion || !File.Exists(ExePath))
            {
                string tmpFile = Path.GetTempFileName();
                TryDeleteFile(tmpFile);
                tmpFile += ".zip";
                try
                {
                    await SimpleDownloader.DownloadFileAsync(ServerZipPath, tmpFile).ConfigureAwait(false);
                    ZipFile.ExtractToDirectory(tmpFile, ExeDir, true);
                    File.WriteAllText(VersionPath, serverVersion.ToString());
                }
                finally
                {
                    TryDeleteFile(tmpFile);
                }
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
