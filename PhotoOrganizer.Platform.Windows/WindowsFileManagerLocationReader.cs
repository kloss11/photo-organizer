using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using PhotoOrganizer.Platform.Abstractions;

namespace PhotoOrganizer.Platform.Windows;

/// <summary>
/// Odczytuje ścieżkę folderu z okna Eksploratora pod kursorem: <c>WindowFromPoint</c> →
/// <c>GetAncestor(GA_ROOT)</c> → dopasowanie uchwytu w kolekcji <c>Shell.Application.Windows()</c> →
/// <c>Document.Folder.Self.Path</c>. COM wołane na dedykowanym wątku STA.
/// (COM przez late-binding/dynamic — dla AOT/trim można to później zastąpić [GeneratedComInterface].)
/// </summary>
[SupportedOSPlatform("windows")]
public sealed partial class WindowsFileManagerLocationReader : IFileManagerLocationReader
{
    private const uint GaRoot = 2;

    public bool IsSupported => OperatingSystem.IsWindows();

    public Task<FolderLocationResult> ReadFolderAtAsync(ScreenPoint point, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<FolderLocationResult>();
        var thread = new Thread(() =>
        {
            try
            {
                tcs.SetResult(ReadCore(point));
            }
            catch (Exception ex)
            {
                tcs.SetResult(FolderLocationResult.Fail(LocationReadStatus.Error, ex.Message));
            }
        })
        {
            IsBackground = true,
            Name = "PhotoOrganizer-ShellCom"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
    }

    private static FolderLocationResult ReadCore(ScreenPoint point)
    {
        var hwnd = WindowFromPoint(new POINT { X = point.X, Y = point.Y });
        if (hwnd == IntPtr.Zero)
            return FolderLocationResult.Fail(LocationReadStatus.NoWindowAtPoint);

        var root = GetAncestor(hwnd, GaRoot);
        if (root == IntPtr.Zero)
            root = hwnd;

        var shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null)
            return FolderLocationResult.Fail(LocationReadStatus.Error, "Shell.Application niedostępne.");

        dynamic? shell = null;
        try
        {
            shell = Activator.CreateInstance(shellType);
            dynamic windows = shell!.Windows();
            int count = windows.Count;

            for (var i = 0; i < count; i++)
            {
                dynamic? window = windows.Item(i);
                if (window is null)
                    continue;

                try
                {
                    long handle = (long)window.HWND;
                    if (handle != root.ToInt64())
                        continue;

                    string path = window.Document.Folder.Self.Path;
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                        return FolderLocationResult.Fail(LocationReadStatus.NotAFileManager,
                            "Okno nie wskazuje na zwykły folder plikowy.");

                    return FolderLocationResult.Ok(path, "explorer");
                }
                finally
                {
                    Marshal.FinalReleaseComObject(window);
                }
            }

            return FolderLocationResult.Fail(LocationReadStatus.NotAFileManager,
                "Kliknięte okno to nie Eksplorator plików.");
        }
        finally
        {
            if (shell is not null)
                Marshal.FinalReleaseComObject(shell);
        }
    }

    [LibraryImport("user32.dll")]
    private static partial IntPtr WindowFromPoint(POINT point);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetAncestor(IntPtr hwnd, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
