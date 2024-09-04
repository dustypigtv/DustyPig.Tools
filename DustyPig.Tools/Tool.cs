using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace DustyPig.Tools;

public class Tool
{
    const string ROOT_URL = "https://s3.dustypig.tv/bin/tools/";
    const int BUFFER_SIZE = 81920;

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
            string ret = null;

            try
            {
                if (InstallPointer.Exists)
                    ret = File.ReadAllText(InstallPointer.FullName).Trim();
            }
            catch { }

            if(string.IsNullOrWhiteSpace(ret))
                ret = Path.Combine(RootDir.FullName, "tools", Name);

            return Directory.CreateDirectory(ret);
        }
    }

    public FileInfo ExePath => new(Path.Combine(ExeDir.FullName, Exe));

    FileInfo VersionPath => new(Path.Combine(VersionsDir.FullName, Name + ".ver"));

    FileInfo InstallPointer => new(Path.Combine(VersionsDir.FullName, Name + ".install"));

    Uri ServerVersionPath => new(ROOT_URL + Name + ".ver");

    Uri ServerZipPath => new(ROOT_URL + Name + ".zip");

    public void OverrideInstallDirectory(DirectoryInfo dirInfo) =>
        File.WriteAllText(InstallPointer.FullName, dirInfo.FullName);

    public void ResetInstallDirectory()
    {
        if (InstallPointer.Exists)
            InstallPointer.Delete();
    }

    public async Task InstallAsync(IProgress<InstallProgress> progress = null, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        await InstallAsync(httpClient, progress, cancellationToken).ConfigureAwait(false);
    }

    public async Task InstallAsync(HttpClient httpClient, IProgress<InstallProgress> progress = null, CancellationToken cancellationToken = default)
    {
        // Get local version
        Version localVersion = new();
        try { localVersion = Version.Parse(File.ReadAllText(VersionPath.FullName)); }
        catch { }


        // Get server version
        using var versonResponse = await httpClient.GetAsync(ServerVersionPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        versonResponse.EnsureSuccessStatusCode();
        Version serverVersion = Version.Parse(await versonResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));


        if (serverVersion > localVersion || !ExePath.Exists)
        {
            FileInfo tmpFile = new(Path.GetTempFileName());
            try
            {
                // Download
                using var downloadResponse = await httpClient.GetAsync(ServerZipPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                downloadResponse.EnsureSuccessStatusCode();
                double totalLength = downloadResponse.Content.Headers.ContentLength ?? -1;
                using (var downloadSrcStream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                using (var downloadDstStream = CreateFileStream(tmpFile))
                {
                    await CopyStreamAsync(downloadSrcStream, downloadDstStream, totalLength, 0, progress, 0, "Downloading", cancellationToken).ConfigureAwait(false);
                }
                progress?.Report(new InstallProgress("Downloading", 50));


                // Unzip
                using var zipArchive = ZipFile.OpenRead(tmpFile.FullName);
                totalLength = zipArchive.Entries.Sum(entry => entry.Length);
                double prevRead = 0;
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (!(entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\')))
                    {
                        string filePath = Path.Combine(ExeDir.FullName, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        using var dstFileStream = CreateFileStream(new FileInfo(filePath));
                        using var entryStream = entry.Open();
                        await CopyStreamAsync(entryStream, dstFileStream, totalLength, prevRead, progress, 50, "Installing", cancellationToken).ConfigureAwait(false);
                        prevRead += entry.Length;
                    }
                }


                // Write version
                File.WriteAllText(VersionPath.FullName, serverVersion.ToString());
                progress?.Report(new InstallProgress("Installing", 100));
            }
            finally
            {
                TryDeleteFile(tmpFile);
            }
        }
    }

    public void UnInstall()
    {
        TryDeleteFile(VersionPath);
        try { ExeDir.Delete(true); }
        catch { }
    }

    static void TryDeleteFile(FileInfo fileInfo)
    {
        try { fileInfo.Delete(); }
        catch { }
    }


    static FileStream CreateFileStream(FileInfo fileInfo)
    {
        fileInfo.Directory.Create();
        return new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, BUFFER_SIZE, true);
    }

    static async Task CopyStreamAsync(Stream src, Stream dst, double totalLength, double prevRead, IProgress<InstallProgress> progress, int preProgress, string status, CancellationToken cancellationToken)
    {
        var buffer = new byte[BUFFER_SIZE];
        double totalRead = 0;
        int lastDL = -1;

        while (true)
        {
            int read = await src.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read < 1)
                return;

            await dst.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);

            if (progress != null)
            {
                totalRead += read;
                var newDL = preProgress + Math.Max(0, Math.Min(49, Convert.ToInt32((prevRead + totalRead) / totalLength * 100 / 2)));
                if (newDL > lastDL)
                {
                    lastDL = newDL;
                    progress.Report(new(status, newDL));
                }
            }
        }
    }
}
