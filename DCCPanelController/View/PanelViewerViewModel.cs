using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.View.Properties;
using Microsoft.Extensions.Logging;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelViewerViewModel : Base.ConnectionViewModel {
    private Panel? _draggedPanel;
    private readonly ProfileService _profileService;

    [ObservableProperty] private bool _canZoomOut;
    [ObservableProperty] private bool _canZoomIn;
    [ObservableProperty] private bool _isPanelSelected;
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Panel? _selectedPanel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;

    public INavigation? NavigationService;
    public double ScreenHeight = 100;
    public double ScreenWidth = 100;
    public bool ShowThumbnail => false;
    public bool ShowLivePanel => !ShowThumbnail;
    public bool IsNotLoading => !IsLoading;

    private ILogger<PanelViewerViewModel> _logger;

    public PanelViewerViewModel(ILogger<PanelViewerViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        Panels = _profileService?.ActiveProfile?.Panels ?? throw new ArgumentNullException(nameof(profileService),"PanelViewerViewModel: Active profile is not defined.");
        PropertyChanged += OnPropertyChanged;
        SelectedPanel = Panels.FirstOrDefault();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(SelectedPanel)) {
            IsPanelSelected = SelectedPanel is not null;
        }
    }

    private async Task SaveAsync() {
        await _profileService.SaveActiveProfileAsync();
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
            var result = await DisplayAlertHelper.DisplayAlertYesNoAsync("Delete Panel?", $"Are you sure you want to delete the panel '{SelectedPanel.Id}'");
            if (!result) return; // Exit if the user cancels the delete operation
            Panels.Remove(SelectedPanel);
            RefreshSortOrder();
            SelectedPanel = null; //Panels.FirstOrDefault();
            await SaveAsync();
        }
        OnPropertyChanged(nameof(Panels));
    }

    [RelayCommand] private async Task DuplicatePanelAsync() {
        if (SelectedPanel != null) {
            var cloned = Panels.CreatePanelFrom(SelectedPanel);
            Panels.Add(cloned);
            RefreshSortOrder();
            await SaveAsync();
        }
        OnPropertyChanged(nameof(Panels));
    }

    [RelayCommand]
    public async Task DownloadPanelAsync() {
        try {
            if (SelectedPanel is { } panel) {
                var result = await DisplayAlertHelper.DisplayAlertAsync("Download Panel","This allows you to download a single Panel to local storage.", "Continue", "Cancel");
                if (result) {
                    var panelAsJson = panel.DownloadPanel();
                    var location = await FileHelper.SaveFileAsync("Save Panel", panelAsJson, $"{panel.Id}.panel.json");
                    if (!string.IsNullOrEmpty(location)) {
                        _logger.LogInformation(location);
                        await DisplayAlertHelper.DisplayToastAlert("Panel Saved");
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogCritical("Unable to save the panel: " + ex.Message);
        }
    }

    [RelayCommand]
    public async Task UploadPanelAsync() {
        try {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Upload Panel","This allows you to upload a previously downloaded panel.", "Continue", "Cancel");
            if (result) {
                var jsonString = await FileHelper.OpenFileAsync("Select a Panel File to upload");
                if (!string.IsNullOrEmpty(jsonString)) {
                    var panel = Panels.UploadPanel(jsonString);
                    if (panel is not null) {
                        await DisplayAlertHelper.DisplayToastAlert($"Uploaded Panel: {panel.Id ?? ""}");
                        await SaveAsync();
                    } else {
                        await DisplayAlertHelper.DisplayOkAlertAsync("Error", "Unable to upload the provided file as a Panel.");
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogCritical("Unable to upload the panel: " + ex.Message);
        }
    }

    [RelayCommand]
    public async Task EditPanelAsync() {
        try {
            if (SelectedPanel is { } panel && NavigationService is { } navigation) {
                IsLoading = true;
                await Task.Delay(100);
                var editorPage = new PanelEditor(LogHelper.CreateLogger<PanelEditor>(),panel,_profileService);
                await navigation.PushAsync(editorPage);
                await editorPage.PageClosed;
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error loading Panel Editor: {Message}",ex.Message);
        } finally {
            SelectedPanel = null;
            IsLoading = false;
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
        await _profileService.SaveActiveProfileAsync();
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