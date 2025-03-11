using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    [ObservableProperty] private bool _designMode = false;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotFullScreen))]
    private bool _isFullScreen = false;
    private Panel? _draggedPanel;
    public bool IsNotFullScreen => !IsFullScreen;
    
    public PanelEditorViewModel(Panels panels) {
        Panels = panels;
        IsFullScreen = false;
    }
    
    public bool IsTileSelected => SelectedPanel?.SelectedTiles.Count > 0;

    public void AddPanel() {
        Console.WriteLine($"AddPanelAsync:");
        SelectedPanel = Panels?.CreatePanel();
    }
    
    public void DeletePanel() { 
        Console.WriteLine($"DeletePanel:");
        SelectedPanel = null;
    }

    public void EditPanel() {
        Console.WriteLine($"EditPanel: ");
        DesignMode = true;
    }

    public void ExitEditMode() {
        Console.WriteLine($"ExitEditMode: ");
        DesignMode = false;
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
            _                    => EditModeEnum.Copy
        };
    }

    public void DeleteTile() { 
        Console.WriteLine($"DeleteTile:");
    }

    public void EditProperties() {
        Console.WriteLine($"EditProperties: ");
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
        RefreshSortOrder();
        //Save();
        _draggedPanel = null;
    }

    [RelayCommand]
    private async Task DragPanelOverAsync(Panel? panel) {
        if (_draggedPanel == null) return;
        if (panel == null || panel == _draggedPanel) return;

        if (Panels != null) {
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
        }
        //RefreshSortOrder();
    }

    [RelayCommand]
    private async Task DragPanelLeaveAsync(Panel? panel) {
        if (_draggedPanel == null) return;
        RefreshSortOrder();
    }

    public void RefreshSortOrder() {
        // Update the SortOrder of all panels based on their current position in the collection
        if (Panels is not null) {
            for (var i = 0; i < Panels.Count; i++) {
                Panels[i].SortOrder = i + 1;
            }
        }
    }

    [RelayCommand]
    private async Task SelectionChangedAsync() {
        DesignMode = false;    
        OnPropertyChanged(nameof(DesignMode));
    }
}