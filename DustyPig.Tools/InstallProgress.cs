namespace DustyPig.Tools;

public class InstallProgress
{
    internal InstallProgress(string status, int percent)
    {
        Status = status;
        Percent = percent;
    }

    public string Status { get; }

    public int Percent { get; }
}
