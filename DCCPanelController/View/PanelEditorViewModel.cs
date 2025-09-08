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
using DCCPanelController.View.Properties;
using DCCPanelController.View.Properties.DynamicProperties;
using DCCPanelController.View.Properties.PanelProperties;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Popup;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    private readonly ILogger<PanelEditor> _logger;
    private readonly PanelEditor? _panelEditor;
    private readonly ControlPanelView _panelView;
    private readonly Microsoft.Maui.Controls.View _panelEditorContainer;
    private readonly ProfileService _profileService;
    
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    [ObservableProperty] private bool _gridVisible;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private bool _havePropertiesChanged;

    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    [NotifyPropertyChangedFor(nameof(CanSetModes))]
    [NotifyPropertyChangedFor(nameof(CanRotateTiles))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTiles))]
    [NotifyPropertyChangedFor(nameof(CanToggleGrid))]
    [ObservableProperty] private bool _isNavigationDrawerOpen;
    
    [NotifyPropertyChangedFor(nameof(Title))]
    [ObservableProperty] private Panel _panel;
    private Panel _original;
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
    public double ScreenWidth = 100;
    public bool ExitViaBackButton { get; set; }

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
                    using var ms = new MemoryStream(capacity: 64 * 1024); // small initial capacity; grows as needed
                    await result.CopyToAsync(ms, ScreenshotFormat.Jpeg, quality: 75);
                    var length = (int)ms.Length;
                    return Convert.ToBase64String(ms.GetBuffer(), 0, length);
                }).ConfigureAwait(false);
                return base64;            }
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
                _                 => EditModeEnum.Move
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
    private async Task ToggleGridAsync() {
        GridVisible = !GridVisible;
    }

    [RelayCommand]
    public async Task EditPropertiesAsync() {
        if (SelectedEntities.Count > 0) {
            await EditTilePropertiesPopupAsync();
        } else {
            await EditPanelPropertiesPopupAsync();
        }
    }

    private async Task EditPanelPropertiesPopupAsync() {
        try {
            if (Panel is { } panel && _panelEditor is not null) {
                var propertiesViewModel = new PanelPropertyViewModel(panel);
                var propertiesPage = new PanelPropertyPage(propertiesViewModel);
                ShowPropertyPopup("", propertiesViewModel, propertiesPage, AcceptPanelPropertiesPopupCommand);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Panel Properties Page: " + ex.Message);
        }
    }
    
    private async Task EditTilePropertiesPopupAsync() {
        try {
            var title = SelectedEntities.Count switch {
                0 => "Unknown Entity",
                1 => $"{SelectedEntity?.EntityName} ({SelectedEntity?.EntityDescription}) properties.",
                _ => AreAllTilesTheSame() ? $"Multiple {SelectedEntity?.EntityName} ({SelectedEntity?.EntityDescription}) properties." : "Multiple Selected Entities"
            };
            
            if (SelectedEntities?.Count > 0 && _panelEditor is not null) {
                await DynamicTilePropertyPopupAsync(title);
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Tile Properties Page: " + ex.Message);
        }
    }

    private bool AreAllTilesTheSame() {
        if (SelectedEntities.Count <= 1) return true;
        var first = SelectedEntities.FirstOrDefault();
        return SelectedEntities.All(entity => entity.EntityName == first?.EntityName);
    }

    private async Task DynamicTilePropertyPopupAsync(string title) {
        IsProcessing = true;
        try {
            var scrollContent = new ScrollView();
            var content = new DynamicTilePropertyPopupContent {
                Title = title,
                TilesSource = SelectedTiles
            };
            content.Applied += ContentOnApplied;
            content.Cancelled += ContentOnClosed;
            scrollContent.Content = content;

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
                HeaderHeight = 60,
                AutoSizeMode = PopupAutoSizeMode.None,
                AnimationMode = PopupAnimationMode.Zoom,
                AnimationDuration = 300,
                OverlayMode = PopupOverlayMode.Transparent,
                AcceptCommand = content.ApplyCommand,
                DeclineCommand = content.CancelCommand
            };
            if (string.IsNullOrEmpty(title)) popup.ShowHeader = false;
            popup.Show();
        } finally {
            IsProcessing = false;
        }
    }

    private void ContentOnClosed(object? sender, EventArgs e) {
        Console.WriteLine("Content Closed");
    }

    private void ContentOnApplied(object? sender, EventArgs e) {
        Console.WriteLine("Content Applied");
    }

    private void ShowPropertyPopup(string title, PanelPropertyViewModel propertyPage, Microsoft.Maui.Controls.View content, ICommand acceptPopupCommand) {
        _propertyPage = propertyPage;
        content.Margin = new Thickness(20,10,20,0);
        var propertySize = MauiViewSizeCalculator.CalculateTotalSize(content, ScreenWidth, ScreenHeight);
        Console.WriteLine($"Property Page Size: {propertySize.Width} x {propertySize.Height}");

        var scrollContent = new ScrollView();
        scrollContent.Content = content;
        
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
                HeaderBackground = Colors.WhiteSmoke,
                FooterBackground = Colors.DarkGrey,
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
            HeaderHeight = 60,
            AutoSizeMode = PopupAutoSizeMode.None,
            AnimationMode = PopupAnimationMode.Zoom,
            AnimationDuration = 300,
            OverlayMode = PopupOverlayMode.Transparent, 
            AcceptCommand = acceptPopupCommand,
            DeclineCommand = DeclinePopupCommand
        };
        if (string.IsNullOrEmpty(title)) popup.ShowHeader = false;
        popup.Show();
    } 
    
    [RelayCommand]
    private async Task AcceptPanelPropertiesPopupAsync() {
        if (_propertyPage is not null) {
            try {
                IsProcessing = true;
                await LetUICatchUpAsync();
                await _propertyPage.ApplyChangesAsync();
                await _panelView.ForceRefresh();
            } finally {
                IsProcessing = false;
            }
        }
    }

    [RelayCommand]
    private async Task AcceptTilePropertiesPopupAsync() {
        if (_propertyPage is not null) {
            try {
                IsProcessing = true;
                await LetUICatchUpAsync();
                await _propertyPage.ApplyChangesAsync();
            } finally {
                IsProcessing = false;
            }
        }
    }

    [RelayCommand]
    private async Task DeclinePopupAsync() {
        _propertyPage = null;
    }
    
    public string GetEditModeIconFilename =>
        EditMode switch {
            EditModeEnum.Copy => "copy.png",
            EditModeEnum.Move => "move.png",
            EditModeEnum.Size => "crop.png",
            _                 => "move.png"
        };
    
    private async Task LetUICatchUpAsync() {
        await Task.Yield();
        await Task.Delay(10);
    }
}