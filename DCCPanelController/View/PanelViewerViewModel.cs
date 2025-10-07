using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class PanelViewerViewModel : ConnectionViewModel {
    private readonly ILogger<PanelViewerViewModel> _logger;
    private readonly ProfileService                _profileService;
    private readonly ConnectionService             _connectionService;

    [ObservableProperty] private bool   _canZoomIn;
    [ObservableProperty] private bool   _canZoomOut;
    private                      Panel? _draggedPanel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;

    [ObservableProperty] private bool    _isPanelSelected;
    [ObservableProperty] private Panels? _panels;
    [ObservableProperty] private Panel?  _selectedPanel;

    public INavigation? NavigationService;
    public double       ScreenHeight = 100;
    public double       ScreenWidth  = 100;

    public PanelViewerViewModel(ILogger<PanelViewerViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _connectionService = connectionService;
        
        Panels = _profileService?.ActiveProfile?.Panels ?? throw new ArgumentNullException(nameof(profileService), "PanelViewerViewModel: Active profile is not defined.");
        PropertyChanged += OnPropertyChanged;
        SelectedPanel = Panels.FirstOrDefault();

        // Refresh on the change of Panels - but do it on the main thread
        // ------------------------------------------------------------
        _profileService.ActiveProfileChanged += (_, __) =>
            MainThread.BeginInvokeOnMainThread(() => {
                Panels = null;
                Panels = _profileService.ActiveProfile?.Panels ?? throw new ArgumentNullException(nameof(profileService), "PanelViewerViewModel: Active profile is not defined.");
                SelectedPanel = Panels.FirstOrDefault();
            });
    }

    public bool ShowThumbnail => false;
    public bool ShowLivePanel => !ShowThumbnail;
    public bool IsNotLoading => !IsLoading;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => IsPanelSelected = SelectedPanel is { };

    [RelayCommand]
    private async Task AddPanelAsync() {
        ArgumentNullException.ThrowIfNull(Panels);
        SelectedPanel = Panels.CreatePanel();
        Panels.Add(SelectedPanel);
        await _profileService.SaveAsync();
    }

    [RelayCommand]
    private async Task DeletePanelAsync() {
        ArgumentNullException.ThrowIfNull(Panels);
        if (SelectedPanel is { }) {
            var result = await DisplayAlertHelper.DisplayAlertYesNoAsync("Delete Panel?", $"Are you sure you want to delete the panel '{SelectedPanel.Id}'");
            if (!result) return; // Exit if the user cancels the delete operation
            Panels.Remove(SelectedPanel);
            RefreshSortOrder();
            SelectedPanel = null; //Panels.FirstOrDefault();
            await _profileService.SaveAsync();
        }
    }

    [RelayCommand] private async Task DuplicatePanelAsync() {
        ArgumentNullException.ThrowIfNull(Panels);
        if (SelectedPanel != null) {
            var cloned = Panels.CreatePanelFrom(SelectedPanel);
            Panels.Add(cloned);
            RefreshSortOrder();
            await _profileService.SaveAsync();
        }
    }

    [RelayCommand]
    public async Task DownloadPanelAsync() {
        ArgumentNullException.ThrowIfNull(Panels);
        try {
            if (SelectedPanel is { } panel) {
                var result = await DisplayAlertHelper.DisplayAlertAsync("Download Panel", "This allows you to download a single Panel to local storage.", "Continue", "Cancel");
                if (result) {
                    var panelAsJson = panel.DownloadPanel();
                    var fileName = string.IsNullOrEmpty(panel.Id) ? "Panel.panel.json" : $"{panel.Id}.panel.json";
                    var saveResult = await FileHelper.SaveFileAsync("Save Panel", panelAsJson, fileName);
                    if (saveResult.IsOk) {
                        await DisplayAlertHelper.DisplayToastAlert("Panel Downloaded");
                        Debug.WriteLine($"Panel Saved to: {saveResult.Value}");
                    } else {
                        await DisplayAlertHelper.DisplayToastAlert($"{saveResult.Message}");
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogCritical("Unable to save the panel: " + ex.Message);
        }
    }

    [RelayCommand]
    public async Task UploadPanelAsync() {
        ArgumentNullException.ThrowIfNull(Panels);
        try {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Upload Panel", "This allows you to upload a previously downloaded panel.", "Continue", "Cancel");
            if (result) {
                var loadResult = await FileHelper.LoadFileAsync("Select a Panel File to upload");
                if (loadResult is { IsOk: true, Value: { } jsonString }) {
                    if (!string.IsNullOrEmpty(jsonString)) {
                        var panel = Panels.UploadPanel(jsonString);
                        if (panel is { }) {
                            await DisplayAlertHelper.DisplayToastAlert($"Uploaded Panel: {panel.Id ?? ""}");
                            await _profileService.SaveAsync();
                        } else {
                            await DisplayAlertHelper.DisplayOkAlertAsync("Error", "Unable to upload the provided file as a Panel.");
                        }
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogCritical("Unable to upload the panel: " + ex.Message);
        }
    }

    [RelayCommand]
    public async Task EditPanelAsync() {
        ArgumentNullException.ThrowIfNull(Panels);
        try {
            if (SelectedPanel is { } panel && NavigationService is { } navigation) {
                IsLoading = true;
                StepTimer.Start();
                await Task.Yield();
                await Task.Delay(10);
                var editorPage = new PanelEditor(LogHelper.CreateLogger<PanelEditor>(), panel, _profileService, _connectionService);
                await navigation.PushAsync(editorPage);
                await editorPage.PageClosed;
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error loading Panel Editor: {Message}", ex.Message);
        } finally {
            SelectedPanel = null;
            IsLoading = false;
        }
    }

    #region Drag and Drop Support for Panels
    [RelayCommand]
    private async Task DragPanelAsync(Panel? panel) => _draggedPanel = panel;

    [RelayCommand]
    private async Task DropPanelAsync(Panel? panel) {
        if (_draggedPanel == null) return;
        SelectedPanel = null;
        _draggedPanel = null;
        RefreshSortOrder();
        OnPropertyChanged(nameof(Panels));
        OnPropertyChanged(nameof(SelectedPanel));
        await _profileService.SaveAsync();
    }

    [RelayCommand]
    private async Task DragPanelOverAsync(Panel? panel) {
        ArgumentNullException.ThrowIfNull(Panels);
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
        ArgumentNullException.ThrowIfNull(Panels);
        for (var i = 0; i < Panels.Count; i++) {
            Panels[i].SortOrder = i + 1;
        }
    }
    #endregion
}