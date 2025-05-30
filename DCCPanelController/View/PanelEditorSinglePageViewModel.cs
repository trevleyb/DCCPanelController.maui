using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorSinglePageViewModel : ConnectionViewModel {
    [ObservableProperty] private bool _designMode;
    private Panel? _draggedPanel;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    [ObservableProperty] private bool _gridVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotFullScreen))]
    private bool _isFullScreen;

    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private bool _propertiesChanged;

    [NotifyPropertyChangedFor(nameof(IsPanelSelected))]
    [NotifyPropertyChangedFor(nameof(NoPanelSelected))]
    [NotifyPropertyChangedFor(nameof(PanelTitle))]
    [ObservableProperty] private Panel? _selectedPanel;

    public PanelEditorSinglePageViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        ArgumentNullException.ThrowIfNull(Profile, "Profile Service should be provided by the DI.");
        Panels = Profile.Panels;
        SelectedPanel = Panels.FirstOrDefault();
        IsFullScreen = false;
    }

    public HashSet<ITile> SelectedTiles { get; set; } = new();
    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();
    public int SelectedEntitiesCount => SelectedEntities.Count;
    public bool HasSelectedEntities => SelectedEntitiesCount > 0;
    public bool MultipleEntitiesSelected => SelectedEntitiesCount > 1;
    public bool SingleEntitySelected => SelectedEntitiesCount == 1;
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();

    public string PanelTitle => SelectedPanel?.Id ?? "Panel Editor";
    public bool IsNotFullScreen => !IsFullScreen;
    public bool IsPanelSelected => SelectedPanel is not null;
    public bool NoPanelSelected => !IsPanelSelected;
    public ControlPanelView? SelectedView { get; set; }

    private async Task SaveAsync() {
        await Profile.SaveAsync();
    }

    /// <summary>
    ///     Adds a new panel to the collection. The newly created panel becomes the selected panel.
    ///     Updates the profile to persist the changes.
    /// </summary>
    [RelayCommand] private async Task AddPanelAsync() {
        AddPanel();
    }

    public void AddPanel() {
        SelectedPanel = Panels.CreatePanel();
        Panels.Add(SelectedPanel);
        SaveAsync();
    }

    /// <summary>
    ///     Deletes the currently selected panel from the application. Prompts the user for confirmation
    ///     before performing the deletion. If confirmed, removes the selected panel from the collection,
    ///     updates the panel order, selects the next available panel, and saves the updated profile.
    ///     Logs any errors encountered during the operation.
    /// </summary>
    [RelayCommand] private async Task DeletePanelAsync() {
        DeletePanel();
    }

    public async void DeletePanel() {
        try {
            Console.WriteLine("DeletePanel:");
            if (SelectedPanel is not null) {
                var result = await AskUserToConfirm("Delete Panel?", $"Are you sure you want to delete the panel '{SelectedPanel.Id}'");
                if (!result) return; // Exit if the user cancels the delete operation
                Panels.Remove(SelectedPanel);
                RefreshSortOrder();
                SelectedPanel = Panels.First();
                await SaveAsync();
            }
        } catch (Exception e) {
            Console.WriteLine($"Error Deleting Panel: {e.Message}");
        }
    }

    /// <summary>
    ///     Duplicates the currently selected panel by creating a new panel based on the
    ///     properties of the selected panel. If no panel is currently selected, the
    ///     operation is not performed. After duplication, notifies the system of the
    ///     updated panel collection.
    /// </summary>
    [RelayCommand] private async Task DuplicatePanelAsync() {
        DuplicatePanel();
    }

    public void DuplicatePanel() {
        if (SelectedPanel != null) {
            var cloned = Panels.CreatePanelFrom(SelectedPanel);
            Panels.Add(cloned);
            OnPropertyChanged(nameof(Panels));
        }
    }

    public void ToggleFullscreen() {
        IsFullScreen = !IsFullScreen;
    }

    public void EditPanel() {
        DesignMode = true;
        GridVisible = true;
    }

    public async void ExitEditMode() {
        try {
            GridVisible = false;
            DesignMode = false;
            if (SelectedPanel is { } panel && SelectedView is { } view) {
                panel.Base64Image = await view.GetThumbnailAsync();
            }
        } catch {
            // ignored
        }
        await SaveAsync();
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
        if (HasSelectedEntities && SelectedPanel is not null) {
            foreach (var entity in SelectedEntities) {
                SelectedPanel.Entities.Remove(entity);
            }
            SelectedEntities.Clear();
            OnPropertyChanged(nameof(Panels));
        }
    }

    public void RotateSelectedTile() {
        if (HasSelectedEntities && SelectedPanel is not null) {
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
        }
    }

    public void EditProperties() {
        if (SelectedPanel is not null) {
            if (HasSelectedEntities) {
                EditTileProperties(SelectedEntities);
            } else {
                EditPanelProperties();
            }
        }
    }

    [RelayCommand]
    public async Task DownloadPanelAsync() {
        try {
            if (SelectedPanel is { } panel) {
                var panelAsJson = panel.DownloadPanel();
                var location = await FileHelper.SaveFileAsync("Save Panel", panelAsJson, $"{panel.Id}.panel.json");
                Console.WriteLine(location);
                await DisplayAlert("Panel Saved", location ?? "");
            }
        } catch (Exception ex) {
            Console.WriteLine("Unable to save the panel: " + ex.Message);
        }
    }

    [RelayCommand]
    public async Task UploadPanelAsync() {
        try {
            var jsonString = await FileHelper.OpenFileAsync("Select a Panel File");
            if (!string.IsNullOrEmpty(jsonString)) {
                var panel = Panels.UploadPanel(jsonString);
                if (panel is not null) {
                    await DisplayAlert("Success", $"Uploaded Panel: {panel.Id ?? ""}");
                    await SaveAsync();
                } else {
                    await DisplayAlert("Error", "Unable to upload the provided file as a Panel.");
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Unable to upload the panel: " + ex.Message);
        }
    }

    public void EditTileProperties() {
        EditTileProperties(SelectedEntities);
    }

    public async Task EditTileProperties(Entity? entity) {
        try {
            entity ??= SelectedEntity;
            if (entity is not null) await EditTilePropertiesAsync(entity);
        } catch (Exception e) {
            Console.WriteLine("Unable to launch the edit tile panel: " + e.Message);
        }
    }

    public async void EditTileProperties(List<Entity> entities) {
        try {
            await EditTilePropertiesAsync(entities);
        } catch (Exception e) {
            Console.WriteLine("Unable to launch the edit tile panel: " + e.Message);
        }
    }

    public async Task EditTilePropertiesAsync(Entity entity) {
        Console.WriteLine($"Launching the Properties page for '{entity.EntityName}'");

        //await DynamicPageLauncher.ShowDynamicPropertyPageAsync([entity]);
        //await SaveAsync();
    }

    public async Task EditTilePropertiesAsync(List<Entity> entities) {
        Console.WriteLine("Launching the Properties page for multiple Entities");

        //await DynamicPageLauncher.ShowDynamicPropertyPageAsync(entities);
        //await SaveAsync();
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
            //await PropertyPageLauncher.ShowPanelPropertyPageAsync(panel);
            //await SaveAsync();
            PropertiesChanged = true;
        }
    }

    public new async Task ToggleConnectionAsync() {
        await base.ToggleConnectionAsync();
    }

    [RelayCommand]
    private async Task SelectionChangedAsync() {
        Console.WriteLine($"SelectionChangedAsync: {SelectedPanel?.Id ?? "NONE"}");
        OnPropertyChanged(nameof(DesignMode));
        OnPropertyChanged(nameof(PanelTitle));
        OnPropertyChanged(nameof(SelectedPanel));
        OnPropertyChanged(nameof(IsPanelSelected));
        OnPropertyChanged(nameof(NoPanelSelected));
    }

    private async Task DisplayAlert(string title, string message) {
        if (App.Current.Windows[0].Page is { } window) {
            await window.DisplayAlert(title, message, "OK");
        }
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
        await Profile.SaveAsync();
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