using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;
using DCCPanelController.View.Properties.PanelProperties;
using DCCPanelController.View.Properties.TileProperties;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;
using Syncfusion.Maui.Toolkit.Popup;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    private readonly PanelEditor? _panelEditor;

    private readonly ILogger<PanelEditor> _logger;
    private readonly ControlPanelView _panelView;
    private readonly ProfileService _profileService;
    
    [ObservableProperty]
    private EditModeEnum _editMode = EditModeEnum.Move;

    [ObservableProperty]
    private bool _gridVisible;

    [ObservableProperty] private bool _havePropertiesChanged;

    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    [NotifyPropertyChangedFor(nameof(CanSetModes))]
    [NotifyPropertyChangedFor(nameof(CanRotateTiles))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTiles))]
    [NotifyPropertyChangedFor(nameof(CanSavePanel))]
    [NotifyPropertyChangedFor(nameof(CanToggleGrid))]
    [ObservableProperty] private bool _isNavigationDrawerOpen;

    private Panel? _original;
    private IPropertyPage? _propertyPage;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Panel? _panel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedEntities))]
    [NotifyPropertyChangedFor(nameof(HasSelectedEntities))]
    [NotifyPropertyChangedFor(nameof(MultipleEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SingleEntitySelected))]
    [NotifyPropertyChangedFor(nameof(SingleOrNoEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SelectedEntity))]
    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    [NotifyPropertyChangedFor(nameof(CanSetModes))]
    [NotifyPropertyChangedFor(nameof(CanRotateTiles))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTiles))]
    [NotifyPropertyChangedFor(nameof(CanSavePanel))]
    [NotifyPropertyChangedFor(nameof(HavePropertiesChanged))]
    private ObservableCollection<ITile> _selectedTiles = [];

    public double ScreenHeight = 100;
    public double ScreenWidth = 100;

    public PanelEditorViewModel(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService, ControlPanelView panelView, PanelEditor panelEditor) {
        _profileService = profileService;
        _logger = logger;
        _original = panel;
        _panel = panel.Clone(false); // Make a clone so we are working on a clone
        _panelView = panelView;
        _panelEditor = panelEditor;

        // Pre-build the palette cache
        TileSelectorPaletteCache.Prebuild(_panel);

        CheckIfPanelChanged();
        if (HavePropertiesChanged) {
            _logger.LogDebug("Property comparison should NOT return true at this point.");
        }
    }

    public bool CanEditProperties => SetCanEditProperties() && !IsNavigationDrawerOpen;
    public bool CanSetModes => SingleOrNoEntitiesSelected && !IsNavigationDrawerOpen;
    public bool CanRotateTiles => HasSelectedEntities && !IsNavigationDrawerOpen;
    public bool CanDeleteTiles => HasSelectedEntities && !IsNavigationDrawerOpen;
    public bool CanSavePanel => HavePropertiesChanged && !IsNavigationDrawerOpen;
    public bool CanToggleGrid => !IsNavigationDrawerOpen;
    public bool CanPressBackButton => !IsNavigationDrawerOpen;

    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();
    public bool SingleOrNoEntitiesSelected => SelectedEntitiesCount is 1 or 0;
    public int SelectedEntitiesCount => SelectedEntities.Count;
    public bool HasSelectedEntities => SelectedEntitiesCount > 0;
    public bool MultipleEntitiesSelected => SelectedEntitiesCount > 1;
    public bool SingleEntitySelected => SelectedEntitiesCount == 1;
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();
    public string Title => Panel?.Title ?? "Panel";

    public event Action? ForcePanelRefresh;

    public void UpdateOriginalFromCopy() {
        ArgumentNullException.ThrowIfNull(_original, "Original Panel should never be undefined.");
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
        ArgumentNullException.ThrowIfNull(_original, "Original Panel should never be undefined.");
        ArgumentNullException.ThrowIfNull(Panel, "Panel should never be undefined.");
        HavePropertiesChanged = !_original.IsEqualTo(Panel);
    }

    [RelayCommand]
    private async Task BackButtonPressedAsync() {
        if (HavePropertiesChanged) {
            var result = await DisplayAlertHelper.DisplayAlertAsync("Unsaved Changes", null, "Save & Leave", "Discard Changes");
            if (result) await SaveAsync();
        }
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync() {
        if (Panel?.Panels?.Profile is { } profile) {
            // Take an image (thumbnail) of the Panel 
            // Currently turned off as we render panels directly
            // Panel.Base64Image = await GetThumbnailImage();

            // If we are saving, then we need to update the original item
            // but still work on the copied item. So just make a clone of 
            // the editing panel and make the original point to this new clone. 
            // ----------------------------------------------------------
            UpdateOriginalFromCopy();
            await _profileService.SaveAsync();
            await DisplayAlertHelper.DisplayToastAlert("Changes Saved");
            CheckIfPanelChanged();
        }
    }

    [RelayCommand]
    private async Task RotateTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            _panelView.UnMarkRotatedSelectedTiles();
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
            _panelView.ReMarkRotatedSelectedTiles();
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
            await EditTilePropertiesPopupAsync();
        } else {
            await EditPanelPropertiesPopupAsync();
        }
    }

    private async Task EditPanelPropertiesAsync() {
        try {
            if (Panel is { } panel && _panelEditor is not null) {
                var propertiesViewModel = new PanelPropertyViewModel(panel);
                var propertiesPage = new PanelPropertyPage(propertiesViewModel);
                ShowBottomSheet(propertiesPage, "Panel Properties");
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }
    }

    private async Task EditTilePropertiesAsync() {
        try {
            var title = SelectedEntities.Count switch {
                0 => "Unknown Entity",
                1 => SelectedEntity?.EntityName + " Properties",
                _ => "Multiple Entities Properties"
            };
            
            if (Panel is { } panel && SelectedEntities?.Count > 0 && _panelEditor is not null) {
                var propertiesViewModel = new DynamicPropertyPageViewModel(SelectedEntities);
                var propertiesPage = propertiesViewModel.CreatePropertiesView();
                ShowBottomSheet(propertiesPage, title);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Tile Properties Page: " + ex.Message);
        }
    }

    private async Task EditPanelPropertiesPopupAsync() {
        try {
            if (Panel is { } panel && _panelEditor is not null) {
                var propertiesViewModel = new PanelPropertyViewModel(panel);
                var propertiesPage = new PanelPropertyPage(propertiesViewModel);
                ShowPropertyPopup("", propertiesViewModel, propertiesPage);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }
    }
    
    private async Task EditTilePropertiesPopupAsync() {
        try {
            var title = SelectedEntities.Count switch {
                0 => "Unknown Entity",
                1 => SelectedEntity?.EntityName + " Properties",
                _ => "Multiple Entities Properties"
            };
            
            if (Panel is { } panel && SelectedEntities?.Count > 0 && _panelEditor is not null) {
                var propertiesViewModel = new DynamicPropertyPageViewModel(SelectedEntities);
                var propertiesPage = propertiesViewModel.CreatePropertiesView();
                ShowPropertyPopup(title, propertiesViewModel, propertiesPage);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Tile Properties Page: " + ex.Message);
        }
    }

    private void ShowPropertyPopup(string title, IPropertyPage propertyPage, Microsoft.Maui.Controls.View content) {
        _propertyPage = propertyPage;
        content.Margin = new Thickness(20);
        var propertySize = MauiViewSizeCalculator.CalculateTotalSize(content, ScreenWidth, ScreenHeight);
        Console.WriteLine($"Property Page Size: {propertySize.Width} x {propertySize.Height}");
        
        var popup = new SfPopup {
            ContentTemplate = new DataTemplate(() => content),
            HeaderTitle = title,
            ShowHeader = true,
            ShowFooter = true,
            BackgroundColor = Colors.WhiteSmoke,
            PopupStyle = new PopupStyle {
                CornerRadius = 10,
                HasShadow = false,
                BlurIntensity = PopupBlurIntensity.Light,
                HeaderBackground = Colors.WhiteSmoke,
                FooterBackground = Colors.LightGray,
                MessageBackground = Colors.WhiteSmoke,
                AcceptButtonBackground = Colors.White,
                DeclineButtonBackground = Colors.White,
                AcceptButtonTextColor = Colors.Black,
                DeclineButtonTextColor = Colors.Black,
            },
            AppearanceMode = PopupButtonAppearanceMode.TwoButton,
            ShowCloseButton = false,
            StaysOpen = true,
            IsFullScreen = true,
            AcceptButtonText = "Save",
            DeclineButtonText = "Cancel",
            Padding = new Thickness(20),
            Margin = new Thickness(20),
            AutoSizeMode = PopupAutoSizeMode.None,
            AnimationMode = PopupAnimationMode.Zoom,
            AnimationDuration = 300,
            OverlayMode = PopupOverlayMode.Transparent, 
            AcceptCommand = AcceptPopupCommand,
            DeclineCommand = DeclinePopupCommand
        };
        if (string.IsNullOrEmpty(title)) popup.ShowHeader = false;
        popup.Show();
    } 
    
    [RelayCommand]
    private async Task AcceptPopupAsync() {
        if (_propertyPage is not null) {
            await _propertyPage.ApplyChangesAsync();
        }
    }

    [RelayCommand]
    private async Task DeclinePopupAsync() {
        _propertyPage = null;
    }
    
    [RelayCommand]
    private async Task CloseBottomSheetAsync() {
        if (_panelEditor?.BottomSheet is { } sfBottomSheet) {
            sfBottomSheet.Close();
        }
    }
    
    private void ShowBottomSheet(Microsoft.Maui.Controls.View propertiesPage, string title = "Properties") {
        if (_panelEditor?.BottomSheet is { } sfBottomSheet) {
            sfBottomSheet.BottomSheetContent = propertiesPage;
            sfBottomSheet.Background = Colors.WhiteSmoke;
            sfBottomSheet.ShowGrabber = true;
            sfBottomSheet.EnableSwiping = true;
            sfBottomSheet.CollapseOnOverlayTap = true;
            sfBottomSheet.CollapsedHeight = 0;
            sfBottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Full;
            sfBottomSheet.State = BottomSheetState.HalfExpanded;
            sfBottomSheet.IsModal = true;
            sfBottomSheet.Show();
        }
    }

    public void BottomSheetOnStateChanged(object? sender, StateChangedEventArgs e) {
        if (e.NewState is BottomSheetState.Hidden or BottomSheetState.Collapsed) {
            if (sender is SfBottomSheet bottomSheet) bottomSheet.Content = null!;
            Console.WriteLine($"Panel Bottom Sheet State Changed: Hidden => {e.NewState}");
            IsNavigationDrawerOpen = false;
            CheckIfPanelChanged();
            ForcePanelRefresh?.Invoke();
        } else {
            Console.WriteLine($"Panel Bottom Sheet State Changed: Not Hidden => {e.NewState}");
            IsNavigationDrawerOpen = true;
        }
    }
}