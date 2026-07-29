using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using PhotoOrganizer.Application.Abstractions;
using PhotoOrganizer.Domain.Model;
using PhotoOrganizer.Domain.ValueObjects;
using PhotoOrganizer.Presentation.Localization;
using PhotoOrganizer.Presentation.ViewModels;
using PhotoOrganizer.Presentation.Views;

namespace PhotoOrganizer.Presentation.Tests;

/// <summary>
/// Test headless renderujący realne okno i sprawdzający, że przełączenie języka faktycznie
/// zmienia tekst etykiet w UI (a nie tylko wartości w logice).
/// </summary>
public sealed class LanguageSwitchUiTests
{
    [AvaloniaFact]
    public void Switching_language_updates_button_label_in_the_rendered_window()
    {
        var viewModel = new MainViewModel(new FakeOrganizer(), new FakeSettingsStore(), new FakeUndoStore());
        var window = new MainWindow { DataContext = viewModel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var apply = window.FindControl<Button>("ApplyButton");
            Assert.NotNull(apply);
            Assert.Equal("Zastosuj", apply!.Content as string); // domyślnie polski

            viewModel.SelectedLanguage = viewModel.Languages.First(l => l.Code == "en");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Apply", apply.Content as string);

            viewModel.SelectedLanguage = viewModel.Languages.First(l => l.Code == "de");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Anwenden", apply.Content as string);

            viewModel.SelectedLanguage = viewModel.Languages.First(l => l.Code == "fr");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Appliquer", apply.Content as string);
        }
        finally
        {
            // Zamknięcie okna odpina bindingi od singletona Localizer — inaczej późniejsze testy
            // (na innym wątku) wywołałyby aktualizację kontrolek z niewłaściwego wątku.
            window.Close();
            Dispatcher.UIThread.RunJobs();
            Localizer.Instance.SetLanguage("pl");
        }
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        public Task<OrganizeSettings> LoadAsync(CancellationToken ct = default) => Task.FromResult(new OrganizeSettings());
        public Task SaveAsync(OrganizeSettings settings, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeOrganizer : IPhotoOrganizer
    {
        public Task<OrganizePlan> PreviewAsync(FilePath workingArea, OrganizeSettings settings, IProgress<ScanProgress>? p = null, CancellationToken ct = default)
            => Task.FromResult(new OrganizePlan(workingArea, settings, []));
        public Task<OrganizeRun> ApplyAsync(OrganizePlan plan, IProgress<MoveProgress>? p = null, CancellationToken ct = default)
            => Task.FromResult(new OrganizeRun(Guid.Empty, default, plan.WorkingArea, []));
        public Task<OrganizeRun> UndoAsync(UndoLog log, IProgress<MoveProgress>? p = null, CancellationToken ct = default)
            => Task.FromResult(new OrganizeRun(log.RunId, default, log.WorkingArea, []));
    }

    private sealed class FakeUndoStore : IUndoLogStore
    {
        public Task<IUndoSession> BeginAsync(FilePath workingArea, Guid runId, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task<UndoLog?> LoadLatestAsync(FilePath workingArea, CancellationToken ct = default) => Task.FromResult<UndoLog?>(null);
        public Task<IReadOnlyList<UndoLog>> LoadAllAsync(FilePath workingArea, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<UndoLog>>([]);
    }
}
