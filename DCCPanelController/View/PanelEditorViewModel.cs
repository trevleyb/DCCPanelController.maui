using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.ControlPanel;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;
using DCCPanelController.View.Properties.PanelProperties;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Popup;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    //private DynamicTilePropertyPopupContent? _dynamicTileContent;
    public enum PopupAction { None, Accept, Cancel }

    private readonly ILogger<PanelEditor>         _logger;
    private readonly PanelEditor?                 _panelEditor;
    private readonly Microsoft.Maui.Controls.View _panelEditorContainer;
    private readonly ControlPanelView             _panelView;
    private readonly ProfileService               _profileService;

    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    [ObservableProperty] private bool         _gridVisible;
    [ObservableProperty] private bool         _havePropertiesChanged;

    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    [NotifyPropertyChangedFor(nameof(CanSetModes))]
    [NotifyPropertyChangedFor(nameof(CanRotateTiles))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTiles))]
    [NotifyPropertyChangedFor(nameof(CanToggleGrid))]
    [ObservableProperty] private bool _isNavigationDrawerOpen;

    [ObservableProperty] private bool  _isProcessing;
    private                      Panel _original;

    [NotifyPropertyChangedFor(nameof(Title))]
    [ObservableProperty] private Panel _panel;

    private PanelPropertyViewModel? _propertyPage;

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
    [NotifyPropertyChangedFor(nameof(HavePropertiesChanged))]
    [ObservableProperty] private ObservableCollection<ITile> _selectedTiles = [];

    public double ScreenHeight = 100;
    public double ScreenWidth  = 100;

    public PanelEditorViewModel(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService, PanelEditor panelEditor, ControlPanelView panelView, Microsoft.Maui.Controls.View panelEditorContainer) {
        _profileService = profileService;
        _logger = logger;
        _original = panel;
        _panel = panel.Clone(false); // Make a clone so we are working on a clone
        _panelView = panelView;
        _panelEditor = panelEditor;
        _panelEditorContainer = panelEditorContainer;

        // Pre-build the palette cache
        TileSelectorPaletteCache.Prebuild(_panel);

        ExitViaBackButton = false;
        CheckIfPanelChanged();
        if (HavePropertiesChanged) {
            _logger.LogDebug("Property comparison should NOT return true at this point.");
        }
    }

    public bool ExitViaBackButton { get; set; }
    public PopupAction LastAction { get; set; } = PopupAction.None;
    public bool AcceptIsValid { get; set; } = true; // assume true by default

    public string Title => Panel.Title ?? "Panel";
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();
    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();

    public bool CanEditProperties => SetCanEditProperties() && !IsNavigationDrawerOpen;
    public bool CanSetModes => !IsNavigationDrawerOpen;
    public bool CanRotateTiles => HasSelectedEntities && !IsNavigationDrawerOpen;
    public bool CanDeleteTiles => HasSelectedEntities && !IsNavigationDrawerOpen;
    public bool CanToggleGrid => !IsNavigationDrawerOpen;
    public bool CanPressBackButton => !IsNavigationDrawerOpen;

    public int SelectedEntitiesCount => SelectedEntities.Count;
    public bool SingleOrNoEntitiesSelected => SelectedEntitiesCount is 1 or 0;
    public bool HasSelectedEntities => SelectedEntitiesCount > 0;
    public bool MultipleEntitiesSelected => SelectedEntitiesCount > 1;
    public bool SingleEntitySelected => SelectedEntitiesCount == 1;

    public string GetEditModeIconFilename =>
        EditMode switch {
            EditModeEnum.Copy => "copy.png",
            EditModeEnum.Move => "move.png",
            EditModeEnum.Size => "crop.png",
            _                 => "move.png",
        };

    public bool SetCanEditProperties() => true;

    public void UpdateOriginalFromCopy() {
        ArgumentNullException.ThrowIfNull(_original, "Original Panel should never be undefined.");
        if (_original.Panels != null) {
            var collection = _original.Panels;
            var index = collection.IndexOf(_original);

            if (index >= 0) {
                var newPanel = Panel.Clone(false);
                collection[index] = newPanel;
                _original = newPanel;
            }
        }
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
        ExitViaBackButton = true;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync() {
        try {
            IsProcessing = true;
            if (Panel.Panels?.Profile is { } profile) {
                Panel.Base64Image = await GetThumbnailImageAsync(_panelView);
                UpdateOriginalFromCopy();
                await _profileService.SaveAsync();
                await DisplayAlertHelper.DisplayToastAlert("Changes Saved");
            }
        } finally {
            IsProcessing = false;
        }
    }

    // Take a snapshot/thumnail of the panel and return it as a base64 string.
    // Only call this on exiting this editor as it will turn off the Grid
    // ----------------------------------------------------------------------
    public async Task<string> GetThumbnailImageAsync(Microsoft.Maui.Controls.View panelView) {
        try {
            using (new CodeTimer("Capture to Thumbnail Image")) {
                if (_panelView.ShowGrid) _panelView.ShowGrid = false;
                _panelView.ClearAllSelectedTiles();
                await LetUICatchUpAsync();

                var result = await _panelEditorContainer.CaptureAsync();
                if (result == null) return string.Empty;

                //var base64 = Convert.ToBase64String(ms.GetBuffer(), 0, length);
                var base64 = await Task.Run(async () => {
                                            using var ms = new MemoryStream(64 * 1024); // small initial capacity; grows as needed
                                            await result.CopyToAsync(ms, ScreenshotFormat.Jpeg, 75);
                                            var length = (int)ms.Length;
                                            return Convert.ToBase64String(ms.GetBuffer(), 0, length);
                                        })
                                       .ConfigureAwait(false);
                return base64;
            }
        } catch (Exception ex) {
            _logger.LogDebug("Error Capturing Thumbnail Image: {Message}", ex.Message);
            return string.Empty;
        }
    }

    [RelayCommand]
    private async Task RotateTileAsync() {
        if (HasSelectedEntities) {
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
            _panelView.MarkAllSelectedTiles();
        }
        CheckIfPanelChanged();
    }

    [RelayCommand]
    private async Task SwitchModeAsync() {
        if (SingleOrNoEntitiesSelected) {
            EditMode = EditMode switch {
                EditModeEnum.Move => EditModeEnum.Copy,
                EditModeEnum.Copy => EditModeEnum.Size,
                EditModeEnum.Size => EditModeEnum.Move,
                _                 => EditModeEnum.Move,
            };
        } else {
            // If multiple items selected, then only MOVE or COPY allowed
            EditMode = EditMode == EditModeEnum.Move ? EditModeEnum.Copy : EditModeEnum.Move;
        }
    }

    [RelayCommand]
    private async Task DeleteTileAsync() {
        if (HasSelectedEntities) {
            foreach (var entity in SelectedEntities) {
                Panel.Entities.Remove(entity);
            }
            SelectedEntities.Clear();
            OnPropertyChanged(nameof(Panels));
        }
        CheckIfPanelChanged();
    }

    [RelayCommand]
    private async Task ToggleGridAsync() => GridVisible = !GridVisible;

    [RelayCommand]
    public async Task EditPropertiesAsync() {
        if (SelectedEntities.Count > 0) {
            await EditTilePropertiesPopupAsync();
        } else {
            await EditPanelPropertiesPopupAsync();
        }
        EditMode = EditModeEnum.Move;   // Reset the edit mode after editing properties
    }

    private async Task EditPanelPropertiesPopupAsync() {
        try {
            if (Panel is { } panel && _panelEditor is { }) {
                IsProcessing = true;
                await LetUICatchUpAsync();
                var propertiesViewModel = new PanelPropertyViewModel(panel);
                var propertiesPage = new PanelPropertyPage(propertiesViewModel);
                ShowPropertyPopup("Panel Properties", propertiesViewModel, propertiesPage, AcceptPanelPropertiesPopupCommand);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        } finally {
            IsProcessing = false;
        }
    }

    private async Task EditTilePropertiesPopupAsync() {
        try {
            IsProcessing = true;
            await LetUICatchUpAsync();
            if (SelectedEntities?.Count > 0 && _panelEditor is { }) {
                await DynamicTilePropertyPage.CreatePropertyPage(SelectedEntities, _panelView.Width, _panelView.Height);
            }
            _panelView.ClearAllSelectedTiles(); // Reset all selected tiles for clarity
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Tile Properties Page: " + ex.Message);
        } finally {
            IsProcessing = false;
            await LetUICatchUpAsync();
        }
    }

    private void ShowPropertyPopup(string title, PanelPropertyViewModel propertyPage, Microsoft.Maui.Controls.View content, ICommand acceptPopupCommand) {
        _propertyPage = propertyPage;
        content.Margin = new Thickness(20, 10, 20, 0);
        var scrollContent = new ScrollView { Content = content };

        var popup = new SfPopup {
            ContentTemplate = new DataTemplate(() => scrollContent),
            HeaderTitle = title,
            ShowHeader = true,
            ShowFooter = true,
            BackgroundColor = Colors.WhiteSmoke,
            PopupStyle = new PopupStyle {
                CornerRadius = 10,
                HasShadow = false,
                BlurIntensity = PopupBlurIntensity.Light,
                HeaderBackground = Colors.LightGrey,
                FooterBackground = Colors.LightGray,
                MessageBackground = Colors.WhiteSmoke,
                AcceptButtonBackground = Colors.White,
                DeclineButtonBackground = Colors.White,
                AcceptButtonTextColor = Colors.Black,
                DeclineButtonTextColor = Colors.Black,
            },
            AppearanceMode = PopupButtonAppearanceMode.OneButton,
            ShowCloseButton = false,
            StaysOpen = true,
            IsFullScreen = true,
            AcceptButtonText = "Close",
            Padding = new Thickness(20),
            Margin = new Thickness(20),
            HeaderHeight = 65,
            FooterHeight = 55,
            AutoSizeMode = PopupAutoSizeMode.Both,
            AnimationMode = PopupAnimationMode.None,
            OverlayMode = PopupOverlayMode.Transparent,
            AcceptCommand = acceptPopupCommand,
        };
        popup.Show();
    }

    [RelayCommand]
    private async Task AcceptPanelPropertiesPopupAsync() {
        if (_propertyPage is { }) {
            try {
                IsProcessing = true;
                await LetUICatchUpAsync();
                await _propertyPage.ApplyChangesAsync();
                await _panelView.ForceRefreshAsync();
            } finally {
                IsProcessing = false;
            }
        }
    }

    private async Task LetUICatchUpAsync() {
        await Task.Yield();
        await Task.Delay(10);
    }
}