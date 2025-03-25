using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
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

    [ObservableProperty] private bool _gridVisible = false;
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
        SelectedPanel = Panels.FirstOrDefault();
        IsFullScreen = false;
    }

    public bool IsEntitySelected => SelectedEntity is not null;
    public Entity? SelectedEntity;

    /// <summary>
    /// Adds a new panel to the collection. The newly created panel becomes the selected panel.
    /// Updates the profile to persist the changes.
    /// </summary>
    [RelayCommand] private async Task AddPanelAsync() => AddPanel();

    public void AddPanel() {
        SelectedPanel = Panels.CreatePanel();
        Panels.Add(SelectedPanel);
        Profile.Save();
    }

    /// <summary>
    /// Deletes the currently selected panel from the application. Prompts the user for confirmation
    /// before performing the deletion. If confirmed, removes the selected panel from the collection,
    /// updates the panel order, selects the next available panel, and saves the updated profile.
    /// Logs any errors encountered during the operation.
    /// </summary>
    [RelayCommand] private async Task DeletePanelAsync() => DeletePanel();

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
    [RelayCommand] private async Task DuplicatePanelAsync() => DuplicatePanel();

    public void DuplicatePanel() {
        if (SelectedPanel != null) {
            var cloned = Panels.CreatePanelFrom(SelectedPanel);
            Panels.Add(cloned);
            OnPropertyChanged(nameof(Panels));
        }
    }

    public void ToggleFullscreen() => IsFullScreen = !IsFullScreen;

    public void EditPanel() {
        DesignMode = true;
        GridVisible = true;
    }

    public void ExitEditMode() {
        GridVisible = false;
        DesignMode = false;
        Profile.Save();
    }

    public void ToggleGrid() {
        GridVisible = !GridVisible;
    }

    public void ToggleMode() {
        EditMode = EditMode switch {
            EditModeEnum.Copy => EditModeEnum.Move,
            EditModeEnum.Move => EditModeEnum.Size,
            EditModeEnum.Size => EditModeEnum.Copy,
            _                 => EditModeEnum.Move
        };
        OnPropertyChanged(nameof(EditMode));
    }

    public void DeleteSelectedTile() {
        if (SelectedEntity is not null && SelectedPanel is not null) {
            SelectedPanel.Entities.Remove(SelectedEntity);
            SelectedEntity = null;
            OnPropertyChanged(nameof(Panels));
        }
    }
    public void RotateSelectedTile() {
        if (SelectedEntity is not null) {
            SelectedEntity.RotateRight();
        }
    }
    
    public void EditProperties() {
        if (SelectedPanel is not null) {
            if (SelectedEntity is not null) {
                EditTileProperties(SelectedEntity);
            } else {
                EditPanelProperties();
            }
        }
    }
    
    public void EditTileProperties() => EditTileProperties(SelectedEntity);
    public async void EditTileProperties(Entity? entity) {
        try {
            entity ??= SelectedEntity;
            if (entity is not null) await EditTilePropertiesAsync(entity);
        } catch (Exception e) {
            Console.WriteLine("Unable to launch the edit tile panel: " + e.Message);
        }
    }

    public async Task EditTilePropertiesAsync(Entity entity) {
        Console.WriteLine($"Launching the Properties page for '{entity.Name}'");
        await DynamicPageLauncher.ShowDynamicPropertyPageAsync(entity);
        Profile.Save();
    }

    public async void EditPanelProperties() {
        try {
            await EditPanelPropertiesAsync();
        } catch (Exception e) {
            Console.WriteLine("Unable to launch the edit panel: " + e.Message);
        }
    }

    public async Task EditPanelPropertiesAsync() {
        if (SelectedPanel is { } panel) {
            await PropertyPageLauncher.ShowPanelPropertyPageAsync(panel);
            Profile.Save();
            PropertiesChanged = true;
        }
    }

    [RelayCommand]
    private async Task SelectionChangedAsync() {
        Console.WriteLine($"SelectionChangedAsync: {SelectedPanel?.Id ?? "NONE"}");
        OnPropertyChanged(nameof(DesignMode));
        OnPropertyChanged(nameof(PanelTitle));
        OnPropertyChanged(nameof(SelectedPanel));
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

    #region Drag and Drop Support for Panels
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
    #endregion

}