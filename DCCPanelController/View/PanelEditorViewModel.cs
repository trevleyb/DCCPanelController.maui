using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Components;
using DCCPanelController.View.ControlPanel;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;
using DCCPanelController.View.Properties.PanelProperties;
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
    [NotifyPropertyChangedFor(nameof(CanEditTileProperties))]
    [NotifyPropertyChangedFor(nameof(CanSetModes))]
    [NotifyPropertyChangedFor(nameof(CanRotateTiles))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTiles))]
    [NotifyPropertyChangedFor(nameof(CanToggleGrid))]
    [ObservableProperty] private bool _isNavigationDrawerOpen;
    [ObservableProperty] private bool _isProcessing;

    [NotifyPropertyChangedFor(nameof(Title))]
    [ObservableProperty] private Panel _panel;

    private PanelPropertyViewModel? _propertyPage;
    private Panel                   _original;

    [NotifyPropertyChangedFor(nameof(SelectedEntities))]
    [NotifyPropertyChangedFor(nameof(HasSelectedEntities))]
    [NotifyPropertyChangedFor(nameof(MultipleEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SingleEntitySelected))]
    [NotifyPropertyChangedFor(nameof(SingleOrNoEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SelectedEntity))]
    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    [NotifyPropertyChangedFor(nameof(CanEditTileProperties))]
    [NotifyPropertyChangedFor(nameof(CanSetModes))]
    [NotifyPropertyChangedFor(nameof(CanRotateTiles))]
    [NotifyPropertyChangedFor(nameof(CanChangeLayers))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTiles))]
    [NotifyPropertyChangedFor(nameof(HavePropertiesChanged))]
    [ObservableProperty] private ObservableCollection<ITile> _selectedTiles = [];

    public  double ScreenHeight = 100;
    public  double ScreenWidth  = 100;
    private int    _cols;
    private int    _rows;

    public PanelEditorViewModel(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService, PanelEditor panelEditor, ControlPanelView panelView, Microsoft.Maui.Controls.View panelEditorContainer) {
        _profileService = profileService;
        _logger = logger;
        _original = panel;
        _panel = panel.Clone(false); // Make a clone so we are working on a clone
        _panelView = panelView;
        _panelEditor = panelEditor;
        _panelEditorContainer = panelEditorContainer;

        ExitViaBackButton = false;
        CheckIfPanelChanged();
        if (HavePropertiesChanged) {
            _logger.LogDebug("Property comparison should NOT return true at this point.");
        }
    }

    public bool ExitViaBackButton { get; set; }
    public PopupAction LastAction { get; set; } = PopupAction.None;
    public bool AcceptIsValid { get; set; } = true; // assume true by default

    public string Title => Panel.Title;
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();
    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();

    public bool CanLinkTiles { get; set; }
    public bool CanEditProperties => !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanEditTileProperties => HasSelectedEntities && !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanSetModes => !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanRotateTiles => HasSelectedEntities && !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanChangeLayers => HasSelectedEntities && !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanDeleteTiles => HasSelectedEntities && !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanToggleGrid => !IsNavigationDrawerOpen && !IsProcessing;
    public bool CanPressBackButton => !IsNavigationDrawerOpen && !IsProcessing;

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
        HavePropertiesChanged = HavePropertiesChanged || !_original.IsEqualTo(Panel);
    }

    public void CheckIfCanLinkTiles() {
        CanLinkTiles = false;
        if (SelectedEntities.Count == 2) {
            CanLinkTiles = (SelectedEntities[0] is TurnoutButtonEntity && SelectedEntities[1] is TurnoutEntity) ||
                           (SelectedEntities[1] is TurnoutButtonEntity && SelectedEntities[0] is TurnoutEntity);
        }
        OnPropertyChanged(nameof(CanLinkTiles));
    }
    
    [RelayCommand]
    private async Task LinkTilesAsync() {
        if (SelectedEntities.Count != 2) return;
        
        TurnoutButtonEntity? button = null;
        TurnoutEntity? turnout = null;
        
        if (SelectedEntities[0] is TurnoutButtonEntity && SelectedEntities[1] is TurnoutEntity) {
            button = (TurnoutButtonEntity)SelectedEntities[0];
            turnout = (TurnoutEntity)SelectedEntities[1];
        }
        if (SelectedEntities[1] is TurnoutButtonEntity && SelectedEntities[0] is TurnoutEntity) {
            button = (TurnoutButtonEntity)SelectedEntities[1];
            turnout = (TurnoutEntity)SelectedEntities[0];
        }

        button?.TurnoutID = turnout?.Turnout?.Id?? "";
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
    private async Task SaveButtonPressedAsync() {
        IsProcessing = true;
        await SaveAsync();
        ExitViaBackButton = true;
        await Shell.Current.GoToAsync("..");
        IsProcessing = false;
    }

    [RelayCommand]
    private async Task SaveAsync() {
        try {
            IsProcessing = true;
            if (Panel.Panels?.Profile is { }) {
                Panel.Base64Image = await GetThumbnailImageAsync(_panelView);
                UpdateOriginalFromCopy();
                await _profileService.SaveAsync();
                await DisplayAlertHelper.DisplayToastAlert("Changes Saved");
            }
        } finally {
            IsProcessing = false;
        }
    }

    // Take a snapshot/thumbnail of the panel and return it as a base64 string.
    // Only call this on exiting this editor as it will turn off the Grid
    // ----------------------------------------------------------------------
    public async Task<string> GetThumbnailImageAsync(Microsoft.Maui.Controls.View panelView) {
        try {
            _panelView.ClearAllSelectedTiles();
            if (_panelView.ShowGrid) _panelView.ShowGrid = false;
            await LetUiCatchUpAsync(16);

            //var result = await CaptureHelper.CaptureStableAsync(_panelEditorContainer, 16, true);
            var shot = await CleanCapture.CaptureAsync(_panelEditorContainer);
            if (shot == null) return string.Empty;

            var base64 = await Task.Run(async () => {
                                        using var ms = new MemoryStream(64 * 1024); // small initial capacity; grows as needed
                                        await shot.CopyToAsync(ms, ScreenshotFormat.Jpeg, 75);
                                        var length = (int)ms.Length;
                                        return Convert.ToBase64String(ms.GetBuffer(), 0, length);
                                    })
                                   .ConfigureAwait(false);
            return base64;
        } catch (Exception ex) {
            _logger.LogDebug("Error Capturing Thumbnail Image: {Message}", ex.Message);
            return string.Empty;
        }
    }

    [RelayCommand]
    private async Task ChangeLayersAsync() {
        if (HasSelectedEntities) {
            LayerCyclerByLocation.CycleLayersAtSelectedLocations(SelectedEntities, Panel.Entities);
        }
        CheckIfPanelChanged();
    }

    
    [RelayCommand]
    private async Task RotateTileLeftAsync() {
        if (HasSelectedEntities) {
            foreach (var entity in SelectedEntities) {
                entity.RotateLeft();
            }
            _panelView.MarkAllSelectedTiles();
        }
        CheckIfPanelChanged();
    }

    [RelayCommand]
    private async Task RotateTileRightAsync() {
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
        CheckIfPanelChanged();
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
    private async Task EditPanelPropertiesPopupAsync() {
        try {
            if (Panel is { } panel && _panelEditor is { }) {
                IsProcessing = true;
                await LetUiCatchUpAsync();
                _cols = Panel.Cols;
                _rows = Panel.Rows;
                var propertiesViewModel = new PanelPropertyViewModel(panel, _original);
                var propertiesPage = new PanelPropertyPage(propertiesViewModel);
                ShowPropertyPopup("Panel Properties", propertiesViewModel, propertiesPage, AcceptPanelPropertiesPopupCommand);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        } finally {
            IsProcessing = false;
        }
        EditMode = EditModeEnum.Move; // Reset the edit mode after editing properties
        CheckIfPanelChanged();
    }

    [RelayCommand]
    private async Task EditTilePropertiesPopupAsync() {
        try {
            IsProcessing = true;
            await LetUiCatchUpAsync();
            if (SelectedEntities.Count > 0 && _panelEditor is { }) {
                var page = await DynamicTilePropertyPage.CreatePropertyPage(SelectedEntities, _panelView.Width, _panelView.Height);
                if (page is { }) {
                    page.Applied += PageOnApplied;
                    await DynamicTilePropertyPage.CreatePropertyPagePopup(page);
                }
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Tile Properties Page: " + ex.Message);
        } finally {
            IsProcessing = false;
            await LetUiCatchUpAsync();
        }
        EditMode = EditModeEnum.Move; // Reset the edit mode after editing properties
        CheckIfPanelChanged();
    }

    private void PageOnApplied(object? sender, DynamicTilePropertyPageEventArgs e) {
        e.Page.Applied -= PageOnApplied;
        _panelView.ClearAllSelectedTiles(); // Reset all selected tiles for clarity
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
                await LetUiCatchUpAsync();
                await _propertyPage.ApplyChangesAsync();
                if (_cols != Panel.Cols || _rows != Panel.Rows) {
                    await _panelView.ForceRefreshAsync();
                }
            } finally {
                IsProcessing = false;
            }
        }
    }

    private static async Task LetUiCatchUpAsync(int ms = 10) {
        await Task.Yield();
        await Task.Delay(ms);
    }
}