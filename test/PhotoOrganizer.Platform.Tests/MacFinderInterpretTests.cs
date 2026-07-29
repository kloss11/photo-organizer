using PhotoOrganizer.Platform.Abstractions;
using PhotoOrganizer.Platform.MacOS;

namespace PhotoOrganizer.Platform.Tests;

/// <summary>
/// Testy czystej interpretacji wyjścia osascript (bez uruchamiania procesu) — logika mapowania
/// stdout/stderr Findera na status jest testowalna niezależnie od macOS.
/// </summary>
public sealed class MacFinderInterpretTests
{
    [Fact]
    public void Automation_denied_maps_to_permission_denied()
    {
        var result = MacFinderLocationReader.Interpret("", "execution error: Not authorized to send Apple events (-1743)");
        Assert.Equal(LocationReadStatus.PermissionDenied, result.Status);
    }

    [Fact]
    public void No_window_marker_maps_to_no_window()
    {
        var result = MacFinderLocationReader.Interpret("NO_WINDOW", "");
        Assert.Equal(LocationReadStatus.NoWindowAtPoint, result.Status);
    }

    [Fact]
    public void Not_folder_marker_maps_to_not_a_file_manager()
    {
        var result = MacFinderLocationReader.Interpret("NOT_FOLDER", "");
        Assert.Equal(LocationReadStatus.NotAFileManager, result.Status);
    }

    [Fact]
    public void Empty_output_maps_to_error()
    {
        var result = MacFinderLocationReader.Interpret("", "");
        Assert.Equal(LocationReadStatus.Error, result.Status);
    }

    [Fact]
    public void Existing_path_maps_to_success()
    {
        var existing = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        var result = MacFinderLocationReader.Interpret(existing, "");

        Assert.Equal(LocationReadStatus.Success, result.Status);
        Assert.Equal(existing, result.Path);
    }

    [Fact]
    public void Nonexistent_path_maps_to_not_a_file_manager()
    {
        var result = MacFinderLocationReader.Interpret("/nie/istnieje/taka/sciezka-12345", "");
        Assert.Equal(LocationReadStatus.NotAFileManager, result.Status);
    }
}
