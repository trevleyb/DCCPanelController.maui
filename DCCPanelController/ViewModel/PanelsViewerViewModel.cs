using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View;
using DCCPanelController.View.PropertPages;

namespace DCCPanelController.ViewModel;

public partial class PanelsViewerViewModel : BaseViewModel {
    private readonly SettingsService _settingsService;
    private readonly NavigationService _navigationService;
    private Panel? _draggedPanel;

    [ObservableProperty] private int _sidePanelWidth = 300;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelSelected))]
    [NotifyPropertyChangedFor(nameof(NoPanelSelected))]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Panel? _selectedPanel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidePanelClosed))]
    [NotifyPropertyChangedFor(nameof(SidePanelWidth))]
    [NotifyPropertyChangedFor(nameof(ShouldShowPanelView))]
    private bool _isSidePanelOpen;

    public bool IsSidePanelClosed => !IsSidePanelOpen;

    public string Title => SelectedPanel == null ? "DCC Panel Controller" : SelectedPanel.Name;
    public bool IsPanelSelected => SelectedPanel != null;
    public bool NoPanelSelected => SelectedPanel == null;

    public ObservableCollection<Panel> Panels { get; set; }
    [ObservableProperty] private bool _canExpandCollapse = true;

    public bool ShouldShowPanelView => CanExpandCollapse && IsPanelSelected;

    public PanelsViewerViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
        Panels = _settingsService.Panels;
        SidePanelWidth = 300;
        IsSidePanelOpen = true;
    }

    public void SelectPanel(Panel? panel = null) {
        SelectedPanel = null;
        SelectedPanel = panel ?? Panels.FirstOrDefault();
    }

    public async void Save() {
        _settingsService?.Save();
    }

    [RelayCommand]
    private async Task DragPanelAsync(Panel? panel) {
        Console.WriteLine("Drag Panel: " + panel?.Name);
        _draggedPanel = panel;
    }

    [RelayCommand]
    private async Task DropPanelAsync(Panel? panel) {
        Console.WriteLine("Drop Panel: " + panel?.Name);
        if (_draggedPanel == null) return;

        RefreshSortOrder();
        Save();
        _draggedPanel = null;
    }

    [RelayCommand]
    private async Task DragPanelOverAsync(Panel? panel) {
        Console.WriteLine("Drag Over: " + panel?.Name);

        if (_draggedPanel == null) return;
        if (panel == null || panel == _draggedPanel) return; // Ignore invalid or redundant scenarios

        var draggedIndex = Panels.IndexOf(_draggedPanel);
        var targetIndex = Panels.IndexOf(panel);
        if (targetIndex == -1) return;

        if (targetIndex == Panels.Count - 1 && draggedIndex != Panels.Count - 1) {
            // If dragging over the last item, simulate dropping at the end of the list
            Panels.Remove(_draggedPanel);
            Panels.Add(_draggedPanel);
        } else if (draggedIndex != targetIndex) {
            // If dragging over a different panel, reorder panels by moving
            Panels.Remove(_draggedPanel);
            Panels.Insert(targetIndex, _draggedPanel);
        }
        RefreshSortOrder();
    }

    [RelayCommand]
    private async Task DragPanelLeaveAsync(Panel? panel) {
        Console.WriteLine("Drag Leave: " + panel?.Name);
        if (_draggedPanel == null) return;
        RefreshSortOrder();
    }

    private void RefreshSortOrder() {
        // Update the SortOrder of all panels based on their current position in the collection
        for (int i = 0; i < Panels.Count; i++) {
            Panels[i].SortOrder = i + 1;
        }
    }

    [RelayCommand]
    private async Task SelectionChangedAsync() { }

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
        var newPanel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        newPanel.Name = "Panel " + maxSort;
        newPanel.SortOrder = maxSort;
        Panels.Add(newPanel);
        Save();
        SelectPanel(newPanel);
    }

    [RelayCommand]
    private async Task EditPanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;

        var tempPanel = SelectedPanel.Clone();
        var result = await LaunchEditPanelAsync(tempPanel);
        if (result) {
            // If we got a Panel Back, it is a COPY of the panel
            // we passed through, and, it has likely been edited. So we
            // need to update the original panel in the Panels
            // collection to be this new Panel. 
            // ------------------------------------------------------------------
            var index = Panels.IndexOf(SelectedPanel);
            if (index >= 0) Panels[index] = tempPanel;
            SelectedPanel = Panels[index];
            Save();
        } 
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
        }
        catch (Exception ex) {
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

    [RelayCommand] 
    private async Task PanelPropertiesAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;
        try {
            await NavigationService.NavigateToPopupWindow(new PanelPropertyPage(SelectedPanel));
            Save();
        } catch {
            Console.WriteLine($"Failed to delete panel {SelectedPanel.Name}");
        }
    }

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}