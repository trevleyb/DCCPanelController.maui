using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;
using DynamicPropertyPageViewModel = DCCPanelController.View.Properties.TileProperties.DynamicPropertyPageViewModel;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    private readonly INavigation _navigation;
    private PanelChangeDetector? _detector;
    private Func<Task<string>>? ThumbnailCallback { get; set; }

    [ObservableProperty] private bool _gridVisible;
    [ObservableProperty] private bool _havePropertiesChanged;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;

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
    [NotifyPropertyChangedFor(nameof(EditPropertiesIcon))]
    [NotifyPropertyChangedFor(nameof(HavePropertiesChanged))]
    private ObservableCollection<ITile> _selectedTiles = [];

    public bool CanEditProperties => SetCanEditProperties();
    public double ScreenHeight = 100;
    public double ScreenWidth = 100;

    public PanelEditorViewModel(Panel panel, INavigation navigation, Func<Task<string>>? thumbnailCallback) {
        _panel = panel;
        _navigation = navigation;
        ThumbnailCallback = thumbnailCallback;
        ResetPanelChanges();
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

    public bool SetCanEditProperties() {
        if (SelectedEntities.Count is 0 or 1) return true;

        // If more than 1 selected entity and any are IEntityID, return false
        if (SelectedEntities.Any(entity => entity is IEntityID)) return false;

        // If they are track entities (but not IEntityID), return true
        if (SelectedEntities.All(entity => entity is TrackEntity)) return true;

        return false;
    }

    public void CheckIfPanelChanged() {
        HavePropertiesChanged = _detector?.HasPanelChanged(Panel) ?? false;
    }
    
    public void ResetPanelChanges() {
        _detector = new PanelChangeDetector(Panel, new PanelChangeDetectorOptions {
            MaxDepth = 5,
            SkipProperties = new HashSet<string> { "Parent", "Navigation", "CustomProperty" },
            IncludePrivateProperties = false
        });
        HavePropertiesChanged = false;
    }
    
    [RelayCommand]
    public async Task SaveAsync() {
        if (Panel?.Panels?.Profile is { } profile) {
            Panel.Base64Image = await GetThumbnailAsync();
            
            // So that we trigger a refresh on other screens, such as the operate
            // page screen, we need to remove the panel and then re-add it to 
            // the collection. 
            var position = profile.Panels.IndexOf(Panel);
            profile.Panels.RemoveAt(position);
            profile.Panels.Insert(position, Panel);
            
            await profile.SaveAsync();
            ResetPanelChanges();
        }
    }

    public async Task<string> GetThumbnailAsync() {
        if (ThumbnailCallback != null) {
            return await ThumbnailCallback();
        }
        return "";
    }

    [RelayCommand]
    private async Task RotateTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
            CheckIfPanelChanged();
        }
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
            CheckIfPanelChanged();
            SelectedEntities.Clear();
            OnPropertyChanged(nameof(Panels));
        }
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
            Console.WriteLine("Error Launching Panel Properties Page: " + ex.Message);
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
            if (layer != SelectedEntities?.First().Layer) {
                Console.WriteLine("Layer was changed, so forcing a Panel redraw");
                ForcePanelRefresh?.Invoke();
            }
        } catch (Exception ex) {
            Console.WriteLine("Error Launching Tile Properties Page: " + ex.Message);
        }
    }
}