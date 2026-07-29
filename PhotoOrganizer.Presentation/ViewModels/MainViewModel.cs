using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Presentation.Localization;

namespace PhotoOrganizer.Presentation.ViewModels;

/// <summary>
/// Model widoku głównego okna. Rozmawia wyłącznie z portami warstwy Application. Praca I/O w tle,
/// UI aktualizowane po powrocie. Teksty tłumaczone przez <see cref="Localizer"/>.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPhotoOrganizer _organizer;
    private readonly ISettingsStore _settingsStore;
    private readonly IUndoLogStore _undoStore;

    private OrganizeSettings _loadedSettings = new();
    private OrganizePlan? _currentPlan;
    private UndoLog? _lastLog;
    private bool _initializing;

    private static Localizer Loc => Localizer.Instance;

    public MainViewModel(IPhotoOrganizer organizer, ISettingsStore settingsStore, IUndoLogStore undoStore)
    {
        _organizer = organizer;
        _settingsStore = settingsStore;
        _undoStore = undoStore;

        GranularityOptions = Enum.GetValues<DateGranularity>().Select(v => new LocalizedOption(v, EnumKeys.Of(v))).ToArray();
        CollisionOptions = Enum.GetValues<CollisionPolicy>().Select(v => new LocalizedOption(v, EnumKeys.Of(v))).ToArray();
        ScanScopeOptions = Enum.GetValues<ScanScope>().Select(v => new LocalizedOption(v, EnumKeys.Of(v))).ToArray();
        UndatedOptions = Enum.GetValues<UndatedPolicy>().Select(v => new LocalizedOption(v, EnumKeys.Of(v))).ToArray();
        Languages = Translations.Languages.Select(l => new LanguageChoice(l.Code, l.Name)).ToArray();

        Localizer.Instance.LanguageChanged += (_, _) => OnLanguageChanged();
    }

    public IReadOnlyList<LanguageChoice> Languages { get; }
    public IReadOnlyList<LocalizedOption> GranularityOptions { get; }
    public IReadOnlyList<LocalizedOption> CollisionOptions { get; }
    public IReadOnlyList<LocalizedOption> ScanScopeOptions { get; }
    public IReadOnlyList<LocalizedOption> UndatedOptions { get; }

    public ObservableCollection<PlannedMoveRow> PreviewRows { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviewCommand))]
    private string? _workingAreaPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private bool _hasActionablePlan;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    private bool _canUndoOperation;

    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _summary = string.Empty;

    [ObservableProperty] private LanguageChoice? _selectedLanguage;
    [ObservableProperty] private LocalizedOption? _selectedGranularity;
    [ObservableProperty] private LocalizedOption? _selectedCollision;
    [ObservableProperty] private LocalizedOption? _selectedScanScope;
    [ObservableProperty] private LocalizedOption? _selectedUndated;
    [ObservableProperty] private bool _zeroPadded = true;

    public async Task InitializeAsync()
    {
        _initializing = true;
        _loadedSettings = await _settingsStore.LoadAsync();

        Localizer.Instance.SetLanguage(_loadedSettings.LanguageCode);
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == _loadedSettings.LanguageCode) ?? Languages[0];
        SelectedGranularity = Find(GranularityOptions, _loadedSettings.Granularity);
        SelectedCollision = Find(CollisionOptions, _loadedSettings.CollisionPolicy);
        SelectedScanScope = Find(ScanScopeOptions, _loadedSettings.ScanScope);
        SelectedUndated = Find(UndatedOptions, _loadedSettings.UndatedPolicy);
        ZeroPadded = _loadedSettings.ZeroPadded;

        StatusMessage = Loc["Status_Initial"];
        _initializing = false;
    }

    partial void OnSelectedLanguageChanged(LanguageChoice? value)
    {
        if (_initializing || value is null)
            return;

        Localizer.Instance.SetLanguage(value.Code);
        _ = PersistAsync();
    }

    private void OnLanguageChanged()
    {
        foreach (var option in GranularityOptions.Concat(CollisionOptions).Concat(ScanScopeOptions).Concat(UndatedOptions))
            option.Refresh();

        if (string.IsNullOrWhiteSpace(WorkingAreaPath))
            StatusMessage = Loc["Status_Initial"];
    }

    public void SetWorkingArea(string path)
    {
        WorkingAreaPath = path;
        _currentPlan = null;
        HasActionablePlan = false;
        PreviewRows.Clear();
        Summary = string.Empty;
        StatusMessage = Loc.Format("WorkingArea_Set", path);
    }

    private OrganizeSettings BuildSettings() => _loadedSettings with
    {
        LanguageCode = SelectedLanguage?.Code ?? _loadedSettings.LanguageCode,
        Granularity = Value(SelectedGranularity, _loadedSettings.Granularity),
        CollisionPolicy = Value(SelectedCollision, _loadedSettings.CollisionPolicy),
        ScanScope = Value(SelectedScanScope, _loadedSettings.ScanScope),
        UndatedPolicy = Value(SelectedUndated, _loadedSettings.UndatedPolicy),
        ZeroPadded = ZeroPadded
    };

    private async Task PersistAsync()
    {
        _loadedSettings = BuildSettings();
        await _settingsStore.SaveAsync(_loadedSettings);
    }

    private static LocalizedOption? Find(IEnumerable<LocalizedOption> options, object value) =>
        options.FirstOrDefault(o => o.Value.Equals(value));

    private static T Value<T>(LocalizedOption? option, T fallback) => option?.Value is T typed ? typed : fallback;

    private bool CanPreview => !IsBusy && !string.IsNullOrWhiteSpace(WorkingAreaPath);

    [RelayCommand(CanExecute = nameof(CanPreview))]
    private async Task PreviewAsync()
    {
        if (!FilePath.TryCreate(WorkingAreaPath, out var area))
            return;

        IsBusy = true;
        StatusMessage = Loc["Status_Scanning"];
        try
        {
            await PersistAsync();
            var plan = await Task.Run(() => _organizer.PreviewAsync(area, _loadedSettings));
            _currentPlan = plan;

            PreviewRows.Clear();
            foreach (var move in plan.Moves)
                PreviewRows.Add(PlannedMoveRow.From(move));

            HasActionablePlan = plan.HasActionableMoves;
            Summary =
                $"{Loc["Sum_ToMove"]}: {plan.WillMoveCount} • {Loc["Sum_Overwrite"]}: {plan.OverwriteCount} • " +
                $"{Loc["Sum_InPlace"]}: {plan.AlreadyInPlaceCount} • {Loc["Sum_Collision"]}: {plan.CollisionCount} • " +
                $"{Loc["Sum_Undated"]}: {plan.UndatedSkippedCount} • {Loc["Sum_Online"]}: {plan.OnlineOnlyCount}";
            StatusMessage = plan.HasActionableMoves ? Loc["Status_PreviewReady"] : Loc["Status_PreviewEmpty"];
        }
        catch (Exception ex)
        {
            StatusMessage = Loc.Format("Err_PreviewFmt", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanApply => !IsBusy && HasActionablePlan;

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (_currentPlan is null || !FilePath.TryCreate(WorkingAreaPath, out var area))
            return;

        IsBusy = true;
        StatusMessage = Loc["Status_Applying"];
        try
        {
            var run = await Task.Run(() => _organizer.ApplyAsync(_currentPlan));
            _lastLog = await _undoStore.LoadLatestAsync(area);
            CanUndoOperation = _lastLog is not null;

            HasActionablePlan = false;
            _currentPlan = null;
            PreviewRows.Clear();
            Summary = string.Empty;
            StatusMessage = Loc.Format("Status_AppliedFmt", run.MovedCount, run.SkippedCount, run.FailedCount)
                            + (CanUndoOperation ? Loc["Status_UndoAvailable"] : string.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = Loc.Format("Err_ApplyFmt", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanUndo => !IsBusy && CanUndoOperation;

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private async Task UndoAsync()
    {
        if (_lastLog is null)
            return;

        IsBusy = true;
        StatusMessage = Loc["Status_Undoing"];
        try
        {
            var run = await Task.Run(() => _organizer.UndoAsync(_lastLog));
            StatusMessage = Loc.Format("Status_UndoneFmt", run.MovedCount, run.SkippedCount);
            _lastLog = null;
            CanUndoOperation = false;
        }
        catch (Exception ex)
        {
            StatusMessage = Loc.Format("Err_UndoFmt", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
