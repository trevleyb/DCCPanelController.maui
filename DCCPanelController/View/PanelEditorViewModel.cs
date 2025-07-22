using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;
using DCCPanelController.View.Properties.TileProperties;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    private readonly INavigation _navigation;
    private readonly ContentPage _page;
    private ControlPanelView _panelView;
    private ProfileService _profileService;
    
    [ObservableProperty] private bool _gridVisible;
    [ObservableProperty] private bool _havePropertiesChanged;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Panel? _panel;
    private Panel? _original;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedEntities))]
    [NotifyPropertyChangedFor(nameof(HasSelectedEntities))]
    [NotifyPropertyChangedFor(nameof(MultipleEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SingleEntitySelected))]
    [NotifyPropertyChangedFor(nameof(SingleOrNoEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SelectedEntity))]
    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    [NotifyPropertyChangedFor(nameof(EditPropertiesIcon))]
    [NotifyPropertyChangedFor(nameof(HavePropertiesChanged))]
    private ObservableCollection<ITile> _selectedTiles = [];

    public bool CanEditProperties => SetCanEditProperties();
    public double ScreenHeight = 100;
    public double ScreenWidth = 100;

    private ILogger<PanelEditor> _logger;
    public PanelEditorViewModel(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService, ContentPage page, ControlPanelView panelView) {
        _profileService = profileService;
        _logger = logger;
        _original = panel;
        _panel = panel.Clone(false);     // Make a clone so we are working on a clone
        _navigation = page.Navigation;
        _page = page;
        _panelView = panelView;
        
        CheckIfPanelChanged();
        if (HavePropertiesChanged == true) {
            _logger.LogDebug($"Property comparison should NOT return true at this point.");
        }
    }

    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();
    public string EditPropertiesIcon => SelectedEntitiesCount > 0 ? "edit.png" : "properties.png"; 
    public int SelectedEntitiesCount => SelectedEntities.Count;
    public bool HasSelectedEntities => SelectedEntitiesCount > 0;
    public bool SingleOrNoEntitiesSelected => SelectedEntitiesCount is 1 or 0;
    public bool MultipleEntitiesSelected => SelectedEntitiesCount > 1;
    public bool SingleEntitySelected => SelectedEntitiesCount == 1;
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();
    public string Title => Panel?.Title ?? "Panel";

    public event Action? ForcePanelRefresh;
    public event Action? OnBeginPushModal;
    public event Action? OnBeginPopModal;

    public void UpdateOriginalFromCopy() {
        ArgumentNullException.ThrowIfNull(_original,"Original Panel should never be undefined.");
        if (_original.Panels != null && Panel != null) {
            var collection = _original.Panels;
            var index = collection.IndexOf(_original);
            
            if (index >= 0) {
                var newPanel = Panel.Clone(false);
                collection[index] = newPanel;
                _original = newPanel;
            }
        }
    }
    
    // This is a callback so that the Editor View Model can take a snapshot
    // of the design for the thumbnail on the Panel Viewer Page
    // ----------------------------------------------------------------------
    public async Task<string> GetThumbnailImage() {
        try {
            var showGrid = _panelView.ShowGrid;
            _panelView.ShowGrid = false;
            var result = await _panelView.CaptureAsync();
            _panelView.ShowGrid = showGrid;

            if (result == null) return string.Empty;
            using var memoryStream = new MemoryStream();
            await result.CopyToAsync(memoryStream, ScreenshotFormat.Jpeg);
            var imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        } catch (Exception ex) {
            _logger.LogDebug("Error Capturing Thumbnail Image: {Message}", ex.Message);
            return string.Empty;
        }
    }
    
    public bool SetCanEditProperties() {
        return true;
    }

    public void CheckIfPanelChanged() {
        ArgumentNullException.ThrowIfNull(_original,"Original Panel should never be undefined.");
        ArgumentNullException.ThrowIfNull(Panel, "Panel should never be undefined.");
        HavePropertiesChanged = !_original.IsEqualTo(Panel);
    }

    [RelayCommand]
    private async Task BackButtonPressedAsync() {
        if (HavePropertiesChanged) {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Unsaved Changes",null, "Save & Leave", "Discard Changes");
            if (result) await SaveAsync();
        }
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync() {
        if (Panel?.Panels?.Profile is { } profile) {
            // If we are saving, then we need to update the original item
            // but still work on the copied item. So just make a clone of 
            // the editing panel and make the original point to this new clone. 
            // ----------------------------------------------------------
            Panel.Base64Image = await GetThumbnailImage();
            UpdateOriginalFromCopy();
            await _profileService.SaveActiveProfileAsync();
            await DisplayAlertHelper.DisplayToastAlert("Changes Saved");
            CheckIfPanelChanged();
        }
    }

    [RelayCommand]
    private async Task RotateTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
        }
        CheckIfPanelChanged();
    }

    [RelayCommand]
    private async Task SwitchModeAsync() {
        EditMode = EditMode switch {
            EditModeEnum.Move => EditModeEnum.Copy,
            EditModeEnum.Copy => EditModeEnum.Size,
            EditModeEnum.Size => EditModeEnum.Move,
            _                 => EditModeEnum.Move
        };
    }

    [RelayCommand]
    private async Task DeleteTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            foreach (var entity in SelectedEntities) {
                Panel.Entities.Remove(entity);
            }
            SelectedEntities.Clear();
            OnPropertyChanged(nameof(Panels));
        }
        CheckIfPanelChanged();
    }

    [RelayCommand]
    private async Task ToggleGridAsync() {
        GridVisible = !GridVisible;
    }

    [RelayCommand]
    public async Task EditPropertiesAsync() {
        if (SelectedEntities?.Count > 0) {
            await EditTilePropertiesAsync();
        } else {
            await EditPanelPropertiesAsync();
        }
        CheckIfPanelChanged();
    }

    [RelayCommand]
    public async Task EditPanelPropertiesAsync() {
        try {
            if (Panel is { } panel && _navigation is { } navigation) {
                OnBeginPushModal?.Invoke();
                var propertiesViewModel = new PanelPropertyViewModel(panel);
                await PropertyDisplayService.ShowPropertiesAsync(navigation, propertiesViewModel, ScreenWidth, ScreenHeight);
                OnBeginPopModal?.Invoke();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task EditTilePropertiesAsync() {
        try {
            var layer = SelectedEntities.First().Layer;
            if (_navigation is { } navigation && SelectedEntities?.Count > 0) {
                OnBeginPushModal?.Invoke();
                var propertiesViewModel = new DynamicPropertyPageViewModel(SelectedEntities);
                await PropertyDisplayService.ShowPropertiesAsync(navigation, propertiesViewModel, ScreenWidth, ScreenHeight);
                OnBeginPopModal?.Invoke();
            }
            
            // if the layer of the item changed, then we need to force a refresh of the panel
            // ------------------------------------------------------------------------------
            if (layer != SelectedEntities?.First()?.Layer) {
                Console.WriteLine("Layer was changed, so forcing a Panel redraw");
                ForcePanelRefresh?.Invoke();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Tile Properties Page: " + ex.Message);
        }
    }
}