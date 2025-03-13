using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : BaseViewModel {
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Panel? _selectedPanel;

    [NotifyPropertyChangedFor(nameof(IsPanelSelected))]
    [NotifyPropertyChangedFor(nameof(NoPanelSelected))]
    [NotifyPropertyChangedFor(nameof(PanelTitle))]
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;

    [ObservableProperty] private bool _designMode = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotFullScreen))]
    private bool _isFullScreen = false;

    private readonly Profile Profile;
    private Panel? _draggedPanel;

    public string PanelTitle => SelectedPanel?.Id ?? "Panel Editor";
    public bool IsNotFullScreen => !IsFullScreen;
    public bool IsPanelSelected => SelectedPanel is not null;
    public bool NoPanelSelected => !IsPanelSelected;
    
    public PanelEditorViewModel(Profile profile) {
        ArgumentNullException.ThrowIfNull(profile, "Profile Service should be provided by the DI.");
        Profile = profile;
        Panels = Profile.Panels;
        SelectedPanel = Panels.First();
        IsFullScreen = false;
    }

    public bool IsTileSelected => SelectedPanel?.SelectedTiles.Count > 0;

    public void AddPanel() {
        Console.WriteLine($"AddPanelAsync:");
        SelectedPanel = Panels?.CreatePanel();
        Profile.Save();
    }

    public async void DeletePanel() {
        Console.WriteLine($"DeletePanel:");
        if (SelectedPanel is not null) {
            try {
                var result = await AskUserToConfirm("Delete Panel?", $"Are you sure you want to delete the panel '{SelectedPanel.Id}'");
                if (!result) return; // Exit if the user cancels the delete operation
                Panels.Remove(SelectedPanel);
                RefreshSortOrder();
                SelectedPanel = Panels.First();
                Profile.Save();
            } catch (Exception ex) {
                Console.WriteLine($"Error Deleting Panel: {ex.Message}");
            }
        }
    }

    public void EditPanel() {
        Console.WriteLine($"EditPanel: ");
        DesignMode = true;
    }

    public void ExitEditMode() {
        Console.WriteLine($"ExitEditMode: ");
        DesignMode = false;
        Profile.Save();
    }

    public void ToggleFullscreen() {
        Console.WriteLine($"ToggleFullscreen: ");
        IsFullScreen = !IsFullScreen;
    }

    public void ToggleMode() {
        Console.WriteLine($"ToggleMode:");
        EditMode = EditMode switch {
            EditModeEnum.Copy   => EditModeEnum.Move,
            EditModeEnum.Move   => EditModeEnum.Size,
            EditModeEnum.Size   => EditModeEnum.Rotate,
            EditModeEnum.Rotate => EditModeEnum.Copy,
            _                   => EditModeEnum.Copy
        };
    }

    public void DeleteTile() {
        Console.WriteLine($"DeleteTile:");
    }

    public void EditProperties() {
        Console.WriteLine($"EditProperties: ");
        Profile.Save();
    }

    public void DuplicatePanel() {
        Console.WriteLine($"Duplicate Panel:: ");
        if (SelectedPanel != null && Panels != null) {
            Panels.CreatePanelFrom(SelectedPanel);
            OnPropertyChanged(nameof(Panels));
        }
    }

    [RelayCommand]
    private async Task AddNewPanel() {
        AddPanel();
    }

    [RelayCommand]
    private async Task DragPanelAsync(Panel? panel) {
        _draggedPanel = panel;
    }

    [RelayCommand]
    private async Task DropPanelAsync(Panel? panel) {
        if (_draggedPanel == null) return;
        SelectedPanel = _draggedPanel;
        _draggedPanel = null;
        RefreshSortOrder();
        OnPropertyChanged(nameof(Panels));
        OnPropertyChanged(nameof(SelectedPanel));
        Profile.Save();
    }

    [RelayCommand]
    private async Task DragPanelOverAsync(Panel? panel) {
        if (_draggedPanel == null) return;
        if (panel == null || panel == _draggedPanel) return;

        var draggedIndex = Panels.IndexOf(_draggedPanel);
        var targetIndex = Panels.IndexOf(panel);
        if (targetIndex == -1) return;

        if (targetIndex == Panels.Count - 1 && draggedIndex != Panels.Count - 1) {
            // If dragging over the last item, simulate dropping at the end of the list
            Panels.Move(draggedIndex, targetIndex);

            //Panels.Remove(_draggedPanel);
            //Panels.Add(_draggedPanel);
        } else if (draggedIndex != targetIndex) {
            // If dragging over a different panel, reorder panels by moving
            Panels.Move(draggedIndex, targetIndex);
            Panels.Remove(_draggedPanel);
            Panels.Insert(targetIndex, _draggedPanel);
        }
    }

    [RelayCommand]
    private async Task DragPanelLeaveAsync(Panel? panel) {
        if (_draggedPanel == null) return;
        SelectedPanel = _draggedPanel;
        RefreshSortOrder();
    }

    public void RefreshSortOrder() {
        // Update the SortOrder of all panels based on their current position in the collection
        for (var i = 0; i < Panels.Count; i++) {
            Panels[i].SortOrder = i + 1;
        }
        OnPropertyChanged(nameof(Panels));
        OnPropertyChanged(nameof(PanelTitle));
    }

    [RelayCommand]
    private async Task SelectionChangedAsync() {
        Console.WriteLine($"SelectionChangedAsync: {SelectedPanel?.Id ?? "NONE"}");
        OnPropertyChanged(nameof(DesignMode));
        OnPropertyChanged(nameof(PanelTitle));
    }

    private async Task<bool> AskUserToConfirm(string title, string message) {
        if (App.Current.Windows[0].Page is { } window) {
            var result = await window.DisplayAlert(
                title,
                message,
                "Yes",
                "No"
            );
            return result;
        }
        return false;
    }
}