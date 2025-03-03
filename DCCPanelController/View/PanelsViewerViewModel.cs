using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View.PropertyPages;
#if IOS || MACCATALYST
#endif

namespace DCCPanelController.View;

public partial class PanelsViewerViewModel : BaseViewModel {
    private readonly SettingsService _settingsService;
    [ObservableProperty] private bool _canExpandCollapse = true;
    [ObservableProperty] private Panel? _draggedPanel;
    [ObservableProperty] private bool _showPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidePanelClosed))]
    [NotifyPropertyChangedFor(nameof(SidePanelWidth))]
    [NotifyPropertyChangedFor(nameof(ShouldShowPanelView))]
    private bool _isSidePanelOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWideMode))]
    private bool _isThinMode;

    [ObservableProperty] private Panels _panels;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelSelected))]
    [NotifyPropertyChangedFor(nameof(NoPanelSelected))]
    private Panel? _selectedPanel;

    [ObservableProperty] private int _sidePanelWidth = 300;

    public PanelsViewerViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        Panels = _settingsService.Panels;
        SidePanelWidth = 300;
        IsSidePanelOpen = true;
    }

    public bool IsWideMode => !IsThinMode;
    public bool IsSidePanelClosed => !IsSidePanelOpen;
    public bool IsPanelSelected => SelectedPanel != null;
    public bool NoPanelSelected => SelectedPanel == null;
    public bool ShouldShowPanelView => CanExpandCollapse && IsPanelSelected;

    public void SelectPanel(Panel? panel = null) {
        SelectedPanel = null;
        SelectedPanel = panel ?? Panels.FirstOrDefault();
        OnPropertyChanged(nameof(IsPanelSelected));
        OnPropertyChanged(nameof(Panels));
        OnPropertyChanged(nameof(SelectedPanel));
    }

    public async void Save() {
        _settingsService?.Save();
    }

    [RelayCommand]
    private async Task DragPanelAsync(Panel? panel) {
        DraggedPanel = panel;
    }

    [RelayCommand]
    private async Task DropPanelAsync(Panel? panel) {
        if (DraggedPanel == null) return;
        RefreshSortOrder();
        Save();
        DraggedPanel = null;
    }

    [RelayCommand]
    private async Task DragPanelOverAsync(Panel? panel) {
        if (DraggedPanel == null) return;
        if (panel == null || panel == DraggedPanel) return;

        var draggedIndex = Panels.IndexOf(DraggedPanel);
        var targetIndex = Panels.IndexOf(panel);
        if (targetIndex == -1) return;

        if (targetIndex == Panels.Count - 1 && draggedIndex != Panels.Count - 1) {
            // If dragging over the last item, simulate dropping at the end of the list
            Panels.Remove(DraggedPanel);
            Panels.Add(DraggedPanel);
        } else if (draggedIndex != targetIndex) {
            // If dragging over a different panel, reorder panels by moving
            Panels.Remove(DraggedPanel);
            Panels.Insert(targetIndex, DraggedPanel);
        }

        //RefreshSortOrder();
    }

    [RelayCommand]
    private async Task DragPanelLeaveAsync(Panel? panel) {
        if (DraggedPanel == null) return;
        RefreshSortOrder();
    }

    public void RefreshSortOrder() {
        // Update the SortOrder of all panels based on their current position in the collection
        for (var i = 0; i < Panels.Count; i++) {
            Panels[i].SortOrder = i + 1;
        }
    }

    [RelayCommand]
    private Task SelectionChangedAsync() {
        return IsThinMode ? EditPanelAsync(SelectedPanel) : Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DuplicatePanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;

        var newPanel = SelectedPanel.Clone();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        newPanel.Name = "Panel " + maxSort;
        newPanel.SortOrder = maxSort;
        Panels.Add(newPanel);
        Save();
        SelectPanel(newPanel);
    }

    [RelayCommand]
    private async Task AddPanelAsync() {
        var newPanel = Panels.CreatePanel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        newPanel.Name = "Panel " + maxSort;
        newPanel.SortOrder = maxSort;
        Save();
        SelectPanel(newPanel);
    }

    [RelayCommand]
    private async Task EditPanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;

        // Always Save but we have the option to not as result
        // will be Save=True, Back=False
        var result = await LaunchEditPanelAsync(SelectedPanel);
        Save();
        SelectPanel(SelectedPanel);
    }

    public async Task<bool> LaunchEditPanelAsync(Panel panel) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");

        var tcs = new TaskCompletionSource<bool>();

        var panelEditorViewModel = new PanelEditorViewModel(panel, completed => {
            tcs.SetResult(completed); // Return the edited panel;
            mainPage.Navigation.PopAsync();
        });

        var editorPage = new PanelEditorPage(panelEditorViewModel);
        await mainPage.Navigation.PushAsync(editorPage);
        return await tcs.Task;
    }

    [RelayCommand]
    private async Task DeletePanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;

        try {
            var result = await AskUserToConfirmDelete(SelectedPanel);
            if (!result) return; // Exit if the user cancels the delete operation

            Panels.Remove(SelectedPanel);

            for (var index = 0; index < Panels.Count; index++) {
                Panels[index].SortOrder = index + 1;
            }

            Save();
            SelectPanel();
        } catch (Exception ex) {
            Console.WriteLine($"Failed to delete panel {SelectedPanel.Name} due to: {ex.Message}");
        }

        async Task<bool> AskUserToConfirmDelete(Panel panel) {
            // Replace this code with the appropriate logic to display a confirmation dialog in your app
            if (App.Current.Windows[0].Page is { } window) {
                var result = await window.DisplayAlert(
                    "Confirm Delete",
                    $"Are you sure you want to delete the panel \"{panel.Name}\"?",
                    "Yes",
                    "No"
                );

                return result;
            }

            return false;
        }
    }

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}