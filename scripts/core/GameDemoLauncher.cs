namespace ChronoShift;

/// <summary>Launches the external FTPS_Online aircraft demo as a separate OS process.</summary>
public static class GameDemoLauncher
{
    public const string FtpsPath = "res://Z_AircraftSystem_demo/FTPS_Online.exe";

    public static bool IsAvailable() => Godot.FileAccess.FileExists(FtpsPath);

    public static void Launch()
    {
        if (!IsAvailable())
        {
            return;
        }

        string abs = Godot.ProjectSettings.GlobalizePath(FtpsPath);
        Godot.OS.CreateProcess(abs, System.Array.Empty<string>());
    }
}
