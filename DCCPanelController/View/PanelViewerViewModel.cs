using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.DynamicProperties;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelViewerViewModel : ConnectionViewModel {
    private Panel? _draggedPanel;
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Panel? _selectedPanel;
    [ObservableProperty] private bool _isPanelSelected;
    
    public INavigation? NavigationService;
    public double ScreenWidth   = 100;
    public double ScreenHeight  = 100;
    
    public PanelViewerViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        ArgumentNullException.ThrowIfNull(Profile, "Profile Service should be provided by the DI.");
        Panels = Profile.Panels;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(SelectedPanel)) {
            IsPanelSelected = SelectedPanel is not null;
        }
    }

    private async Task SaveAsync() {
        await Profile.SaveAsync();
    }
    
    [RelayCommand] 
    private async Task AddPanelAsync() {
        SelectedPanel = Panels.CreatePanel();
        Panels.Add(SelectedPanel);
        await SaveAsync();
        OnPropertyChanged(nameof(Panels));
    }

    [RelayCommand] 
    private async Task DeletePanelAsync() {
        if (SelectedPanel is not null) {
            var result = await AskUserToConfirm("Delete Panel?", $"Are you sure you want to delete the panel '{SelectedPanel.Id}'");
            if (!result) return; // Exit if the user cancels the delete operation
            Panels.Remove(SelectedPanel);
            RefreshSortOrder();
            SelectedPanel = Panels.First();
            await SaveAsync();
        }
        OnPropertyChanged(nameof(Panels));
    } 
    
    [RelayCommand] private async Task DuplicatePanelAsync() {
        if (SelectedPanel != null) {
            var cloned = Panels.CreatePanelFrom(SelectedPanel);
            Panels.Add(cloned);
            OnPropertyChanged(nameof(Panels));
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

    [RelayCommand]
    public async Task EditPanelAsync() {
        if (SelectedPanel is { } panel && NavigationService is { } navigation) {
            var editorPage = new PanelEditor(panel);
            await navigation.PushAsync(editorPage);
            await editorPage.PageClosed;
        }
    }

    [RelayCommand]
    public async Task EditPanelPropertiesAsync() {
        if (NavigationService is null) return;
        if (SelectedPanel is null) return;
        
        var panelViewModel = new PanelPropertyViewModel(SelectedPanel);
        bool result = await PropertyDisplayService.ShowPropertiesAsync(
            NavigationService, panelViewModel, ScreenWidth, ScreenHeight);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            System.Diagnostics.Debug.WriteLine("Properties applied successfully.");
            await SaveAsync();
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            System.Diagnostics.Debug.WriteLine("Properties view dismissed.");
        }
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
        SelectedPanel = null;
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
        for (var i = 0; i < Panels.Count; i++) {
            Panels[i].SortOrder = i + 1;
        }
        OnPropertyChanged(nameof(Panels));
    }
    #endregion

}