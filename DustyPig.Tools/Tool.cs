using DustyPig.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace DustyPig.Tools;

public class Tool
{
    const string ROOT_URL = "https://s3.dustypig.tv/bin/tools/";

    internal Tool() { }

    public string Name { get; internal set; }

    internal string Exe { get; set; }

    static DirectoryInfo RootDir
    {
        get
        {
            string ret = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            ret = Path.Combine(ret, "DustyPig.tv");
            return Directory.CreateDirectory(ret);
        }
    }

    static DirectoryInfo VersionsDir
    {
        get
        {
            string ret = Path.Combine(RootDir.FullName, "tool.versions");
            return Directory.CreateDirectory(ret);
        }
    }

    public DirectoryInfo ExeDir
    {
        get
        {
            string ret = Path.Combine(RootDir.FullName, "tools", Name);
            return Directory.CreateDirectory(ret);
        }
    }

    public FileInfo ExePath => new(Path.Combine(ExeDir.FullName, Exe));

    FileInfo VersionPath => new(Path.Combine(VersionsDir.FullName, Name + ".ver"));

    Uri ServerVersionPath => new(ROOT_URL + Name + ".ver");

    Uri ServerZipPath => new(ROOT_URL + Name + ".zip");


    public async Task InstallAsync()
    {
        using var httpClient = new HttpClient();
        await InstallAsync(httpClient).ConfigureAwait(false);
    }

    public async Task InstallAsync(HttpClient httpClient)
    {
        Version localVersion = new();
        try { localVersion = Version.Parse(File.ReadAllText(VersionPath.FullName)); }
        catch { }


        Version serverVersion = Version.Parse(await httpClient.DownloadStringAsync(ServerVersionPath).ConfigureAwait(false));

        if (serverVersion > localVersion || !ExePath.Exists)
        {
            FileInfo tmpFile = new(Path.GetTempFileName());
            TryDeleteFile(tmpFile);
            tmpFile = new(tmpFile.FullName + ".zip");
            try
            {
                await httpClient.DownloadFileAsync(ServerZipPath, tmpFile).ConfigureAwait(false);
                ZipFile.ExtractToDirectory(tmpFile.FullName, ExeDir.FullName, true);
                File.WriteAllText(VersionPath.FullName, serverVersion.ToString());
            }
            finally
            {
                TryDeleteFile(tmpFile);
            }
        }
    }

    public void UnInstall()
    {
        try { ExeDir.Delete(true); }
        catch { }
    }

    static void TryDeleteFile(FileInfo fileInfo)
    {
        try { fileInfo.Delete(); }
        catch { }
    }
}
