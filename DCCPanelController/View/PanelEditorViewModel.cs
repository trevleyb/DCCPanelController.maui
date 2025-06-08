using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;
using DynamicPropertyPageViewModel = DCCPanelController.View.Properties.TileProperties.DynamicPropertyPageViewModel;
using PanelPropertyViewModel = DCCPanelController.View.Properties.PanelProperties.PanelPropertyViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    private readonly INavigation _navigation;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;

    [ObservableProperty] private bool _gridVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Panel? _panel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedEntities))]
    [NotifyPropertyChangedFor(nameof(HasSelectedEntities))]
    [NotifyPropertyChangedFor(nameof(MultipleEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SingleEntitySelected))]
    [NotifyPropertyChangedFor(nameof(SelectedEntity))]
    private ObservableCollection<ITile> _selectedTiles = [];

    public double ScreenHeight = 100;

    public double ScreenWidth = 100;

    public PanelEditorViewModel(Panel panel, INavigation navigation) {
        _panel = panel;
        _navigation = navigation;
    }

    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();
    public int SelectedEntitiesCount => SelectedEntities.Count;
    public bool HasSelectedEntities => SelectedEntitiesCount > 0;
    public bool MultipleEntitiesSelected => SelectedEntitiesCount > 1;
    public bool SingleEntitySelected => SelectedEntitiesCount == 1;
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();
    public string Title => Panel?.Title ?? "Panel";
    public event Action? OnBeginPushModal;
    public event Action? OnBeginPopModal;

    [RelayCommand]
    public async Task SaveAsync() {
        if (Panel?.Panels?.Profile is { } profile) {
            await profile.SaveAsync();
        }
    }

    [RelayCommand]
    private async Task RotateTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
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
            if (_navigation is { } navigation && SelectedEntities?.Count > 0) {
                OnBeginPushModal?.Invoke();
                var propertiesViewModel = new DynamicPropertyPageViewModel(SelectedEntities);
                await PropertyDisplayService.ShowPropertiesAsync(navigation, propertiesViewModel, ScreenWidth, ScreenHeight);
                OnBeginPopModal?.Invoke();
            }
        } catch (Exception ex) {
            Console.WriteLine("Error Launching Tile Properties Page: " + ex.Message);
        }
    }
}