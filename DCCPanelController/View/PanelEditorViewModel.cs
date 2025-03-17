using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.DynamicProperties;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.PanelProperties;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : BaseViewModel {
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Panel? _selectedPanel;

    [NotifyPropertyChangedFor(nameof(IsPanelSelected))]
    [NotifyPropertyChangedFor(nameof(NoPanelSelected))]
    [NotifyPropertyChangedFor(nameof(PanelTitle))]
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;

    [ObservableProperty] private bool _designMode = false;
    [ObservableProperty] private bool _propertiesChanged = false;

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

    /// <summary>
    /// Adds a new panel to the collection. The newly created panel becomes the selected panel.
    /// Updates the profile to persist the changes.
    /// </summary>
    public void AddPanel() {
        SelectedPanel = Panels?.CreatePanel();
        Profile.Save();
    }

    /// <summary>
    /// Deletes the currently selected panel from the application. Prompts the user for confirmation
    /// before performing the deletion. If confirmed, removes the selected panel from the collection,
    /// updates the panel order, selects the next available panel, and saves the updated profile.
    /// Logs any errors encountered during the operation.
    /// </summary>
    public async void DeletePanel() {
        try {
            Console.WriteLine($"DeletePanel:");
            if (SelectedPanel is not null) {
                var result = await AskUserToConfirm("Delete Panel?", $"Are you sure you want to delete the panel '{SelectedPanel.Id}'");
                if (!result) return; // Exit if the user cancels the delete operation
                Panels.Remove(SelectedPanel);
                RefreshSortOrder();
                SelectedPanel = Panels.First();
                Profile.Save();
            }
        } catch (Exception e) {
            Console.WriteLine($"Error Deleting Panel: {e.Message}");
        }
    }

    /// <summary>
    /// Duplicates the currently selected panel by creating a new panel based on the
    /// properties of the selected panel. If no panel is currently selected, the
    /// operation is not performed. After duplication, notifies the system of the
    /// updated panel collection.
    /// </summary>
    public void DuplicatePanel() {
        if (SelectedPanel != null) {
            Panels.CreatePanelFrom(SelectedPanel);
            OnPropertyChanged(nameof(Panels));
        }
    }

    public void ToggleFullscreen() => IsFullScreen = !IsFullScreen;
    public void EditPanel() => DesignMode = true;

    public void ExitEditMode() {
        DesignMode = false;
        Profile.Save();
    }

    public void ToggleMode() {
        EditMode = EditMode switch {
            EditModeEnum.Copy   => EditModeEnum.Move,
            EditModeEnum.Move   => EditModeEnum.Size,
            EditModeEnum.Size   => EditModeEnum.Rotate,
            EditModeEnum.Rotate => EditModeEnum.Delete,
            EditModeEnum.Delete => EditModeEnum.Select,
            EditModeEnum.Select => EditModeEnum.Copy,
            _                   => EditModeEnum.Copy
        };
        OnPropertyChanged(nameof(EditMode));
    }

    public void EditTileProperties(ITile tile) => EditTilePropertiesAsync(tile);
    public async Task EditTilePropertiesAsync(ITile tile) {
        Console.WriteLine($"Launching the Properties page for '{tile.Entity.Name}'");
        DynamicPageLauncher.ShowDynamicPropertyPageAsync(tile);
        Profile.Save();
    }

    public void EditPanelProperties() => EditPanelPropertiesAsync();    
    public async Task EditPanelPropertiesAsync() {
        if (SelectedPanel is { } panel) {
            Console.WriteLine($"Launching the Properties page for the Panel '{panel.Id}");
            await PropertyPageLauncher.ShowPanelPropertyPageAsync(panel);
            Console.WriteLine($"Properties page closed");
            Profile.Save();
            PropertiesChanged = true;
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
            Panels.Move(draggedIndex, targetIndex);
        } else if (draggedIndex != targetIndex) {
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