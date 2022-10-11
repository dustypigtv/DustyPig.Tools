using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using WebUtils;

namespace DustyPig.Tools
{
    public class Tool
    {
        private const string ROOT_URL = "https://dustypig.s3.us-central-1.wasabisys.com/bin/tools/";

        internal string Name { get; set; }
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
            Version localVersion = new Version();
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
