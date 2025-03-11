using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private Panels? _panels;
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _designMode = false;
    [ObservableProperty] private bool _showPanels = true;
    [ObservableProperty] private bool _isFullScreen = false;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;

    public PanelEditorViewModel(Panels panels) {
        Panels = panels;
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
}